using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct FloatingObstacleDatalocal
{
    public GameObject prefab;
    [Header("물리 체크 반경 (여유분 포함)")]
    public float checkRadius;
}

public class ObstacleSpawnerlocal : MonoBehaviour
{
    [Header("Spawn Area Settings")]
    [SerializeField] private Vector3 _towerCenter = Vector3.zero;
    [SerializeField] private float _maxRadius = 5.0f;
    [SerializeField] private float _startY = 0f;
    [SerializeField] private float _endY = -500f;

    [Header("--- Obstacle Settings ---")]
    [SerializeField] private int _totalFloatingObstacles = 50;
    [SerializeField] private List<FloatingObstacleDatalocal> _obstacleList;
    [SerializeField] private LayerMask _obstacleLayer; // 장애물 레이어

    [Header("--- Item Settings ---")]
    [SerializeField] private int _totalItems = 15;
    [SerializeField] private List<FloatingObstacleDatalocal> _itemList; // 아이템 프리팹 리스트
    [SerializeField] private LayerMask _itemLayer; // 아이템 전용 레이어

    private List<GameObject> _pool = new List<GameObject>(); // 장애물/아이템 통합 풀

    public void GenerateFloatingObstacles()
    {
        ReturnAllToPool();

        // 1. 장애물 먼저 생성 (장애물 레이어만 체크)
        SpawnObjects(_obstacleList, _totalFloatingObstacles, _obstacleLayer, "Obstacle");

        // 2. 아이템 생성 (장애물 레이어 + 아이템 레이어 둘 다 체크하여 빈 공간 탐색)
        // 아이템은 장애물 위나 다른 아이템 위에 겹치면 안 되기 때문
        LayerMask combinedLayer = _obstacleLayer | _itemLayer;
        SpawnObjects(_itemList, _totalItems, combinedLayer, "Item");
    }

    private void SpawnObjects(List<FloatingObstacleDatalocal> dataList, int totalCount, LayerMask checkLayer, string typeTag)
    {
        if (dataList == null || dataList.Count == 0) return;

        int spawnedCount = 0;
        int maxAttempts = 2000;
        int attempts = 0;

        while (spawnedCount < totalCount && attempts < maxAttempts)
        {
            attempts++;
            FloatingObstacleDatalocal data = dataList[Random.Range(0, dataList.Count)];

            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float randomDist = Random.Range(0f, _maxRadius);
            float randomY = Random.Range(_endY, _startY);

            Vector3 spawnPos = new Vector3(
                _towerCenter.x + Mathf.Cos(randomAngle) * randomDist,
                randomY,
                _towerCenter.z + Mathf.Sin(randomAngle) * randomDist
            );

            // 물리 체크: 설정된 레이어들과 겹치는지 확인
            if (!Physics.CheckSphere(spawnPos, data.checkRadius, checkLayer))
            {
                Quaternion randomRot = (typeTag == "Item") ? Quaternion.identity :
                    Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));

                GameObject obj = GetFromPool(data.prefab, spawnPos, randomRot);
                if (!obj.activeSelf) obj.SetActive(true);

                spawnedCount++;
                Physics.SyncTransforms(); // 생성 직후 위치 반영하여 다음 체크에 사용
            }
        }
        Debug.Log($"[Local] {typeTag} 생성 완료: {spawnedCount}개");
    }

    private GameObject GetFromPool(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        GameObject obj = _pool.Find(x => !x.activeSelf && x.name.Equals(prefab.name + "(Clone)"));
        if (obj == null)
        {
            obj = Instantiate(prefab, pos, rot);
            _pool.Add(obj);
        }
        else
        {
            obj.transform.position = pos;
            obj.transform.rotation = rot;
        }
        return obj;
    }

    public void ReturnAllToPool()
    {
        foreach (GameObject obj in _pool)
        {
            if (obj != null && obj.activeSelf) obj.SetActive(false);
        }
    }
}