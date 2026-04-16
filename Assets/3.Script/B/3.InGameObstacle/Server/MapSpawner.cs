using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class MapSpawner : NetworkBehaviour
{
    [Header("연동 스크립트")]
    [SerializeField] private ObstacleSpawner _obstacleSpawner;

    [Header("Tower Settings")]
    [SerializeField] private Vector3 _towerCenter = Vector3.zero;
    [SerializeField] private float _startY = 0f;
    [SerializeField] private float _endY = -500f;
    [SerializeField] private float _heightStep = 40f;

    [Header("Scale Settings")]
    [Tooltip("이 Y값보다 낮아지면 스케일 변화가 시작됩니다.")]
    [SerializeField] private float _scaleVariationThresholdY = -120f;
    [SerializeField] private float _minScale = 0.6f;
    [SerializeField] private float _maxScale = 1.0f;
    [SerializeField] private float _scaleStep = 0.1f;

    [Header("Map Floor Templates")]
    [SerializeField] private List<GameObject> _floorPrefabs;

    private List<GameObject> _mapPool = new List<GameObject>();
    public List<MapFloor> SpawnedFloors { get; private set; } = new List<MapFloor>();

    [Server]
    public void FullGenerate()
    {
        ReturnMapToPool();
        SpawnedFloors.Clear();

        float currentY = _startY;
        // 시작 스케일은 1.0으로 초기화
        float currentFloorScale = 1.0f;

        while (currentY > _endY)
        {
            if (_floorPrefabs == null || _floorPrefabs.Count == 0) break;

            GameObject prefab = _floorPrefabs[Random.Range(0, _floorPrefabs.Count)];

            // 1. 스케일 계산 로직
            if (currentY <= _scaleVariationThresholdY)
            {
                // -0.1, 0, +0.1 중 하나를 랜덤하게 결정
                int randomChoice = Random.Range(-1, 2); // -1, 0, 1 반환
                float change = randomChoice * _scaleStep;

                // 이전 층 스케일에 변화량을 더하고 최소/최대값으로 제한(Clamp)
                currentFloorScale = Mathf.Clamp(currentFloorScale + change, _minScale, _maxScale);
            }
            else
            {
                // 임계값보다 높은 구간은 무조건 1.0
                currentFloorScale = 1.0f;
            }

            float randomRotationY = Random.Range(0, 12) * 30f;
            Vector3 spawnPos = new Vector3(_towerCenter.x, currentY, _towerCenter.z);
            Quaternion spawnRot = Quaternion.Euler(0, randomRotationY, 0);

            // 2. 풀에서 가져올 때 계산된 스케일 적용
            Vector3 targetScale = new Vector3(currentFloorScale, 1f, currentFloorScale);
            GameObject floorObj = GetFromPool(prefab, spawnPos, spawnRot, targetScale);

            if (!floorObj.activeSelf) floorObj.SetActive(true);

            MapFloor mapFloor = floorObj.GetComponent<MapFloor>();
            if (mapFloor != null)
            {
                mapFloor.RandomizeAttachedObstacles();
                SpawnedFloors.Add(mapFloor);
            }

            NetworkServer.Spawn(floorObj);
            currentY -= _heightStep;
        }

        if (_obstacleSpawner != null)
        {
            _obstacleSpawner.GenerateFloatingObstacles();
        }

        Debug.Log("[Server] 전체 맵 생성 완료 (스케일 변화 적용됨)");
    }

    [Server]
    private GameObject GetFromPool(GameObject prefab, Vector3 pos, Quaternion rot, Vector3 scale)
    {
        GameObject obj = _mapPool.Find(x => !x.activeSelf && x.name.Equals(prefab.name + "(Clone)"));
        if (obj == null)
        {
            obj = Instantiate(prefab, pos, rot);
            _mapPool.Add(obj);
        }
        else
        {
            obj.transform.position = pos;
            obj.transform.rotation = rot;
        }

        // 스케일 적용 (새로 생성하든 풀에서 꺼내든 타겟 스케일로 고정)
        obj.transform.localScale = scale;

        return obj;
    }

    [Server]
    public void ReturnMapToPool()
    {
        foreach (GameObject obj in _mapPool)
        {
            if (obj != null && obj.activeSelf)
            {
                NetworkServer.UnSpawn(obj);
                obj.SetActive(false);
            }
        }
    }
}