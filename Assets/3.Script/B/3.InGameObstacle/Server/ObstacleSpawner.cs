using UnityEngine;
using Mirror;
using System.Collections.Generic;

[System.Serializable]
public struct FloatingObstacleData
{
    public GameObject prefab;
    [Header("물리 체크 반경 (장애물 크기 + 여유분)")]
    public float checkRadius;
}

public class ObstacleSpawner : NetworkBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Vector3 _towerCenter = Vector3.zero;
    [SerializeField] private float _maxRadius = 5.0f;
    [SerializeField] private float _startY = 0f;
    [SerializeField] private float _endY = -500f;

    [Header("--- Obstacle Settings ---")]
    [SerializeField] private int _totalFloatingObstacles = 50;
    [SerializeField] private List<FloatingObstacleData> _obstacleList;
    [SerializeField] private LayerMask _obstacleLayer; // 장애물 레이어

    [Header("--- Item Settings ---")]
    [SerializeField] private int _totalItems = 15;
    [SerializeField] private List<FloatingObstacleData> _itemList; // 아이템 프리팹 리스트
    [SerializeField] private LayerMask _itemLayer; // 아이템 레이어

    private List<GameObject> _pool = new List<GameObject>();

    [Server]
    public void GenerateFloatingObstacles()
    {
        // 1. 기존 모든 오브젝트 언스폰 및 풀 회수
        ReturnAllToPool();

        // 2. 장애물 먼저 생성 (장애물끼리만 겹치지 않게)
        SpawnObjects(_obstacleList, _totalFloatingObstacles, _obstacleLayer, "Obstacle");

        // 3. 아이템 생성 (장애물 + 아이템 레이어 모두 체크하여 빈 공간에 생성)
        LayerMask combinedLayer = _obstacleLayer | _itemLayer;
        SpawnObjects(_itemList, _totalItems, combinedLayer, "Item");
    }

    [Server]
    private void SpawnObjects(List<FloatingObstacleData> dataList, int totalCount, LayerMask checkLayer, string typeTag)
    {
        if (dataList == null || dataList.Count == 0) return;

        int spawnedCount = 0;
        int maxAttempts = 2000;
        int attempts = 0;

        while (spawnedCount < totalCount && attempts < maxAttempts)
        {
            attempts++;
            FloatingObstacleData data = dataList[Random.Range(0, dataList.Count)];

            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float randomDist = Random.Range(0f, _maxRadius);
            float randomY = Random.Range(_endY, _startY);

            Vector3 spawnPos = new Vector3(
                _towerCenter.x + Mathf.Cos(randomAngle) * randomDist,
                randomY,
                _towerCenter.z + Mathf.Sin(randomAngle) * randomDist
            );

            // 물리 체크 (서버의 물리 연산 사용)
            if (!Physics.CheckSphere(spawnPos, data.checkRadius, checkLayer))
            {
                // 아이템은 보통 회전을 고정(identity)하거나 특정 방향만 랜덤으로 줌
                Quaternion randomRot = (typeTag == "Item") ? Quaternion.identity :
                    Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));

                GameObject obj = GetFromPool(data.prefab, spawnPos, randomRot);

                if (!obj.activeSelf) obj.SetActive(true);

                // [중요] 미러 네트워크 스폰
                NetworkServer.Spawn(obj);

                spawnedCount++;
                Physics.SyncTransforms();
            }
        }
        Debug.Log($"[Server] {typeTag} 생성 완료: {spawnedCount}개");
    }

    [Server]
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

    [Server]
    public void ReturnAllToPool()
    {
        foreach (GameObject obj in _pool)
        {
            if (obj != null && obj.activeSelf)
            {
                // [중요] 클라이언트들에서 제거하도록 언스폰 호출
                NetworkServer.UnSpawn(obj);
                obj.SetActive(false);
            }
        }
    }
}