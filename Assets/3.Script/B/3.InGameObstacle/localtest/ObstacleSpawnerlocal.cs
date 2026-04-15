using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct FloatingObstacleDatalocal
{
    public GameObject prefab;
    [Header("물리 체크 반경 (장애물 크기 + 여유분)")]
    public float checkRadius;
}

public class ObstacleSpawnerlocal : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Vector3 _towerCenter = Vector3.zero;
    [SerializeField] private float _maxRadius = 5.0f; // 중심에서 생성될 최대 반경
    [SerializeField] private float _startY = 0f;
    [SerializeField] private float _endY = -500f;

    [Header("생성 개수")]
    [SerializeField] private int _totalFloatingObstacles = 50;

    [Header("Templates")]
    [SerializeField] private List<FloatingObstacleData> _obstacleList;

    [Header("Collision Settings")]
    [SerializeField] private LayerMask _obstacleLayer; // 장애물 레이어(예: Obstacle) 설정 필수

    private List<GameObject> _obstaclePool = new List<GameObject>();

    /// <summary>
    /// 공중 장애물을 생성합니다.
    /// 맵 타일과 벽 장애물이 생성된 이후에 호출되어야 합니다.
    /// </summary>
    public void GenerateFloatingObstacles()
    {
        // 1. 기존 장애물 모두 풀로 반환
        ReturnAllToPool();

        int spawnedCount = 0;
        int maxAttempts = 2000; // 빈 공간을 찾기 위한 최대 시도 횟수
        int attempts = 0;

        Debug.Log("[Local] 공중 장애물 생성 시작...");

        while (spawnedCount < _totalFloatingObstacles && attempts < maxAttempts)
        {
            attempts++;

            // 랜덤 프리팹 데이터 선택
            FloatingObstacleData data = _obstacleList[Random.Range(0, _obstacleList.Count)];

            // 2. 원기둥 형태의 완전 랜덤 3D 좌표 생성
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float randomDist = Random.Range(0f, _maxRadius);
            float randomY = Random.Range(_endY, _startY);

            Vector3 spawnPos = new Vector3(
                _towerCenter.x + Mathf.Cos(randomAngle) * randomDist,
                randomY,
                _towerCenter.z + Mathf.Sin(randomAngle) * randomDist
            );

            // 3. 물리 체크 (Physics.CheckSphere)
            // 해당 위치에 이미 다른 장애물이 있는지 부피 단위로 체크
            if (!Physics.CheckSphere(spawnPos, data.checkRadius, _obstacleLayer))
            {
                // 랜덤 회전값 결정
                Quaternion randomRot = Quaternion.Euler(
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f)
                );

                // 풀에서 가져오거나 생성
                GameObject obstacle = GetFromPool(data.prefab, spawnPos, randomRot);

                if (!obstacle.activeSelf) obstacle.SetActive(true);

                spawnedCount++;

                // 방금 생성된 장애물의 위치 정보를 물리 엔진에 즉시 반영
                Physics.SyncTransforms();
            }
        }

        Debug.Log($"[Local] 공중 장애물 생성 완료: {spawnedCount}개 배치됨 (시도 횟수: {attempts})");
    }

    private GameObject GetFromPool(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        // 이름 뒤에 (Clone)이 붙는 것을 고려하여 탐색
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

    public void ReturnAllToPool()
    {
        foreach (GameObject obj in _obstaclePool)
        {
            if (obj != null && obj.activeSelf)
            {
                obj.SetActive(false);
            }
        }
    }
}