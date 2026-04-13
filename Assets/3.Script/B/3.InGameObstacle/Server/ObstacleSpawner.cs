using UnityEngine;
using Mirror;
using System.Collections.Generic;

[System.Serializable]
public struct FloatingObstacleData
{
    public GameObject prefab;
    [Header("겹침 체크 반경 (이 수치만큼 다른 물체와 떨어짐)")]
    public float safeRadius;
}

public class ObstacleSpawner : NetworkBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Vector3 _towerCenter = Vector3.zero;
    [SerializeField] private float _maxRadius = 5.0f; // 중심에서 생성될 최대 반경 (0 ~ 5 완전 랜덤)
    public float _startY = 0f;
    public float _endY = -500f;

    [Header("생성 개수 (전체 맵 기준)")]
    [SerializeField] private int _totalFloatingObstacles = 50;

    [Header("Templates")]
    [SerializeField] private List<FloatingObstacleData> _obstacleList;

    private List<GameObject> _obstaclePool = new List<GameObject>();

    // 맵 생성이 끝난 직후에 호출하세요.
    [Server]
    public void GenerateFloatingObstacles(List<MapFloor> mapFloors)
    {
        ReturnAllToPool();

        // 1. 맵에 이미 '켜진' 부착형 장애물들의 위치를 모두 수집
        List<Vector3> avoidPositions = new List<Vector3>();
        foreach (var floor in mapFloors)
        {
            avoidPositions.AddRange(floor.GetActiveObstaclePositions());
        }

        Debug.Log($"[Server] 공중부양 장애물 생성 시작... (피해야 할 맵 장애물 수: {avoidPositions.Count})");

        int spawnedCount = 0;
        int maxAttempts = 1000; // 무한 루프 방지
        int attempts = 0;

        while (spawnedCount < _totalFloatingObstacles && attempts < maxAttempts)
        {
            attempts++;

            FloatingObstacleData data = _obstacleList[Random.Range(0, _obstacleList.Count)];

            // 완전 랜덤 3D 좌표 생성 (원기둥 형태)
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float randomDist = Random.Range(0f, _maxRadius);
            float randomY = Random.Range(_endY, _startY);

            Vector3 spawnPos = new Vector3(
                _towerCenter.x + Mathf.Cos(randomAngle) * randomDist,
                randomY,
                _towerCenter.z + Mathf.Sin(randomAngle) * randomDist
            );

            // 겹침 체크 (거리 계산)
            bool isPositionSafe = true;
            foreach (Vector3 posToAvoid in avoidPositions)
            {
                if (Vector3.Distance(spawnPos, posToAvoid) < data.safeRadius)
                {
                    isPositionSafe = false;
                    break;
                }
            }

            // 자리가 안전하면 스폰
            if (isPositionSafe)
            {
                Quaternion randomRot = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
                GameObject obstacle = GetFromPool(data.prefab, spawnPos, randomRot);

                if (!obstacle.activeSelf) obstacle.SetActive(true);
                NetworkServer.Spawn(obstacle);

                // 방금 스폰한 장애물 위치도 '피해야 할 위치' 리스트에 추가 (공중 장애물끼리 겹침 방지)
                avoidPositions.Add(spawnPos);
                spawnedCount++;
            }
        }

        if (attempts >= maxAttempts)
        {
            Debug.LogWarning("[Server] 너무 좁아서 설정한 개수만큼 스폰하지 못했습니다. (공간 부족)");
        }
    }

    [Server]
    private GameObject GetFromPool(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        GameObject obj = _obstaclePool.Find(x => !x.activeSelf && x.name.Equals(prefab.name + "(Clone)"));
        if (obj == null)
        {
            obj = Instantiate(prefab, pos, rot);
            _obstaclePool.Add(obj);
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
        foreach (GameObject obj in _obstaclePool)
        {
            if (obj != null && obj.activeSelf)
            {
                NetworkServer.UnSpawn(obj);
                obj.SetActive(false);
            }
        }
    }
}