using UnityEngine;
using Mirror;
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

    [Header("Spawn Chance")]
    [Range(0, 100)]
    [SerializeField] private int _3F_Chance = 20;

    [Header("Prefabs")]
    [SerializeField] private List<GameObject> _1F_Prefabs;
    [SerializeField] private List<GameObject> _3F_Prefabs;

    private List<GameObject> _pool = new List<GameObject>();
    private readonly int[] _yRotations = { 0, 30, 60, 90, 120, 150 };

    [Server]
    public void FullGenerate()
    {
        float y = _startY;
        int floor = 0;
        float scale = _maxScale;

        float redzoneY = -9999f;
        if (StageManager.Instance != null)
        {
            redzoneY = StageManager.Instance.stage_data_sync.map_redzone_height_Y;
        }

        while (y > _endY)
        {
            // ---------------------------
            // 1. 프리팹 + 높이 결정
            // ---------------------------
            GameObject prefab;
            int heightMultiplier;

            if (floor < 3)
            {
                prefab = _1F_Prefabs[Random.Range(0, _1F_Prefabs.Count)];
                heightMultiplier = 1;
            }
            else
            {
                bool spawn3F = (Random.Range(0, 100) < _3F_Chance) && _3F_Prefabs.Count > 0;

                if (spawn3F)
                {
                    prefab = _3F_Prefabs[Random.Range(0, _3F_Prefabs.Count)];
                    heightMultiplier = 3;
                }
                else
                {
                    prefab = _1F_Prefabs[Random.Range(0, _1F_Prefabs.Count)];
                    heightMultiplier = 1;
                }
            }

            // ---------------------------
            float nextY = y - (_step * heightMultiplier);

            GameObject obj = GetFromPool(prefab);

            obj.SetActive(false);

            obj.transform.position = new Vector3(_center.x, nextY, _center.z);

            int angle = _yRotations[Random.Range(0, _yRotations.Length)];
            obj.transform.rotation = Quaternion.Euler(0, angle, 0);

            // ---------------------------
            // 스케일
            // ---------------------------
            if (floor >= _noChangeFloorCount)
            {
                scale -= (_scaleStep * heightMultiplier);
                scale = Mathf.Max(scale, _minScale);
            }

            obj.transform.localScale = new Vector3(scale, 1f, scale);

            obj.SetActive(true);

            // ---------------------------
            // 네트워크 스폰
            // ---------------------------
            var id = obj.GetComponent<NetworkIdentity>();
            if (id != null && id.netId == 0)
            {
                NetworkServer.Spawn(obj);
            }

            // ---------------------------
            // 장애물 설정
            // ---------------------------
            MapFloor mf = obj.GetComponent<MapFloor>();
            if (mf != null)
            {
                mf.SetupIndices();
                mf.ResetObstacles();

                if (floor >= 3 && nextY > redzoneY)
                {
                    mf.RandomizeAttachedObstacles();
                }
            }

            y = nextY;
            floor += heightMultiplier;
        }

        if (_obstacleSpawner != null)
            _obstacleSpawner.GenerateFloatingObstacles();
    }

    // ---------------- 풀링 ----------------

    [Server]
    private GameObject GetFromPool(GameObject prefab)
    {
        GameObject obj = _pool.Find(x => x != null && !x.activeSelf && x.name == prefab.name);

        if (obj == null)
        {
            obj = Instantiate(prefab);
            obj.name = prefab.name;
            _pool.Add(obj);
        }

        obj.SetActive(true);

        foreach (Transform child in obj.transform)
            child.gameObject.SetActive(true);

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