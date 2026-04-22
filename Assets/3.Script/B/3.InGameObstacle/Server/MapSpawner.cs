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
        float scale = 1f;

        while (y > _endY)
        {
            GameObject prefab = _floorPrefabs[Random.Range(0, _floorPrefabs.Count)];
            GameObject obj = GetFromPool(prefab);

            obj.SetActive(false);
            obj.transform.localScale = Vector3.one;
            obj.transform.position = new Vector3(_center.x, y, _center.z);

            int angle = _yRotations[Random.Range(0, _yRotations.Length)];
            obj.transform.rotation = Quaternion.Euler(0, angle, 0);

            obj.SetActive(true);

            var id = obj.GetComponent<NetworkIdentity>();
            if (id != null && id.netId == 0)
            {
                NetworkServer.Spawn(obj);
            }

            // 스케일 계산 로직
            if (floor >= _noChangeFloorCount)
            {
                float delta = (floor == _noChangeFloorCount)
                    ? -_scaleStep
                    : (Random.value > 0.5f ? _scaleStep : -_scaleStep);

                scale += delta;
                scale = Mathf.Clamp(scale, _minScale, _maxScale);
            }
            obj.transform.localScale = new Vector3(scale, 1f, scale);

            // 맵 구성요소(장애물) 자동 설정
            MapFloor mf = obj.GetComponent<MapFloor>();
            if (mf != null)
            {
                mf.ResetObstacles();
                mf.RandomizeAttachedObstacles();
                // 생성 직후 인덱스와 부모 참조를 장애물들에게 주입
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