using UnityEngine;
using Mirror;
using System.Collections;
using System.Collections.Generic;

public class MapSpawner : NetworkBehaviour
{
    [SerializeField] private ObstacleSpawner _obstacleSpawner;

    [Header("Tower Settings")]
    [SerializeField] private Vector3 _center = Vector3.zero;
    [SerializeField] private float _startY = 0f;
    [SerializeField] private float _endY = -500f;
    [SerializeField] private float _step = 40f;

    [Header("Scale Settings")]
    [SerializeField] private int _noChangeFloorCount = 5;
    [SerializeField] private float _scaleStep = 0.1f;
    [SerializeField] private float _minScale = 0.6f;
    [SerializeField] private float _maxScale = 1.0f;

    [SerializeField] private List<GameObject> _floorPrefabs;

    private List<GameObject> _pool = new List<GameObject>();
    private readonly int[] _yRotations = { 0, 30, 60, 90, 120, 150 };

    [Server]
    public void FullGenerate()
    {
        float y = _startY;
        int floor = 0;
        float scale = _maxScale; // 시작은 1.0 (_maxScale)

        float redzoneY = -9999f;
        if (StageManager.Instance != null)
        {
            redzoneY = StageManager.Instance.stage_data_sync.map_redzone_height_Y;
        }

        while (y > _endY)
        {
            // 1. 프리팹 결정 로직
            GameObject prefab = (floor < 3) ? _floorPrefabs[0] : _floorPrefabs[Random.Range(0, _floorPrefabs.Count)];

            GameObject obj = GetFromPool(prefab);
            obj.SetActive(false);
            obj.transform.position = new Vector3(_center.x, y, _center.z);

            int angle = _yRotations[Random.Range(0, _yRotations.Length)];
            obj.transform.rotation = Quaternion.Euler(0, angle, 0);

            // _noChangeFloorCount(7)층부터 _scaleStep(0.02)씩 감소
            if (floor >= _noChangeFloorCount)
            {
                scale -= _scaleStep;
                scale = Mathf.Max(scale, _minScale); // _minScale(0.6)까지만 감소
            }

            obj.transform.localScale = new Vector3(scale, 1f, scale);
            // ---------------------------------------------

            obj.SetActive(true);

            var id = obj.GetComponent<NetworkIdentity>();
            if (id != null && id.netId == 0)
            {
                NetworkServer.Spawn(obj);
            }

            // 2. 장애물 설정 로직
            MapFloor mf = obj.GetComponent<MapFloor>();
            if (mf != null)
            {
                mf.ResetObstacles();
                if (floor >= 3 && y > redzoneY)
                {
                    mf.RandomizeAttachedObstacles();
                }
                mf.SetupIndices();
            }

            y -= _step;
            floor++;
        }

        if (_obstacleSpawner != null)
            _obstacleSpawner.GenerateFloatingObstacles();
    }

    [Server]
    private GameObject GetFromPool(GameObject prefab)
    {
        GameObject obj = _pool.Find(x => x != null && !x.activeSelf && x.name.Contains(prefab.name));
        if (obj == null)
        {
            obj = Instantiate(prefab);
            _pool.Add(obj);
        }
        return obj;
    }

    [Server]
    public void ReturnMapToPool()
    {
        foreach (var obj in _pool)
        {
            if (obj != null && obj.activeSelf)
            {
                NetworkServer.UnSpawn(obj);
                obj.SetActive(false);
                obj.transform.localScale = Vector3.one;
            }
        }
    }
}