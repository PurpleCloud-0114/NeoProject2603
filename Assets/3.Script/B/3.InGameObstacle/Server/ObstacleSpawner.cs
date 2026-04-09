using UnityEngine;
using Mirror;
using System.Collections.Generic;

[System.Serializable]
public struct ObstacleData
{
    public string obstacleName;
    public GameObject prefab;

    [Header("3D 크기 설정 (겹침 방지용)")]
    public float visualHeight;    // 수직(Y) 높이
    public float visualRadius;    // 수평(X, Z) 반지름 크기

    [Header("배치 개수 설정 (랜덤 범위)")]
    public int minCountPerFloor;
    public int maxCountPerFloor;

    [Header("여백 설정 (랜덤 범위)")]
    public float minGapY;         // 층간 최소 수직 여백
    public float maxGapY;         // 층간 최대 수직 여백
    public float angleOffset;     // 장애물 간 최소 각도 간격
}

public class ObstacleSpawner : NetworkBehaviour
{
    [Header("Tower Center Settings")]
    [SerializeField] private Vector3 _towerCenter = Vector3.zero;

    [Header("Spawn Range Settings")]
    [SerializeField] private float _minRadius = 2.0f;
    [SerializeField] private float _maxRadius = 5.0f;

    public float _startY = 0f;
    public float _endY = -500f;

    [Header("Obstacle Templates")]
    [SerializeField] private List<ObstacleData> _obstacleList;

    private List<GameObject> _obstaclePool = new List<GameObject>();

    public override void OnStartServer()
    {
        base.OnStartServer();
        GenerateCylindricalMap();
    }

    [Server]
    public void GenerateCylindricalMap()
    {
        // 1. 기존 장애물 풀로 회수
        ReturnAllToPool();

        Debug.Log("[Server] 가변 간격 및 3D 크기 기반 장애물 생성 시작...");
        float currentY = _startY;

        while (currentY > _endY)
        {
            // 장애물 종류 랜덤 선택
            int randomIndex = Random.Range(0, _obstacleList.Count);
            ObstacleData data = _obstacleList[randomIndex];
            if (data.prefab == null) continue;

            // 층당 생성 개수 랜덤 결정
            int spawnCount = Random.Range(data.minCountPerFloor, data.maxCountPerFloor + 1);

            // 수평 겹침 방지를 위한 각도 간격 계산
            float safeAngleStep = Mathf.Max(data.angleOffset, 360f / spawnCount);
            float baseAngle = Random.Range(0f, 360f);

            for (int i = 0; i < spawnCount; i++)
            {
                float finalAngle = baseAngle + (i * safeAngleStep);
                float radian = finalAngle * Mathf.Deg2Rad;
                float randomRadius = Random.Range(_minRadius, _maxRadius);

                Vector3 spawnPos = new Vector3(
                    _towerCenter.x + Mathf.Cos(radian) * randomRadius,
                    currentY,
                    _towerCenter.z + Mathf.Sin(radian) * randomRadius
                );

                Quaternion rotation = Quaternion.Euler(0, -finalAngle + 90f, 0);

                // 풀에서 가져오기
                GameObject obstacle = GetFromPool(data.prefab, spawnPos, rotation);

                if (!obstacle.activeSelf) obstacle.SetActive(true);
                NetworkServer.Spawn(obstacle);
            }

            // [수직 겹침 방지] 물체 높이 + 랜덤 여백 적용
            float randomGapY = Random.Range(data.minGapY, data.maxGapY);
            currentY -= (data.visualHeight + randomGapY);

            // 무한 루프 방지 안전장치
            if (data.visualHeight + randomGapY <= 0.1f) currentY -= 1.0f;
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