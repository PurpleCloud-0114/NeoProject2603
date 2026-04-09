using UnityEngine;
using Mirror;
using System.Collections.Generic;

[System.Serializable]
public struct ObstacleData
{
    public string obstacleName;
    public GameObject prefab;

    [Header("배치 설정")]
    public float floorHeight;     // 층간 높이 간격
    [Range(1, 8)]
    public int countPerFloor;     // 층당 생성 개수
    public float angleOffset;     // 장애물 간 각도
}

public class ObstacleSpawner : NetworkBehaviour
{
    [Header("Tower Center Settings")]
    [SerializeField] private Vector3 _towerCenter = Vector3.zero;

    [Header("Spawn Range Settings")]
    [SerializeField] private float _minRadius = 2.0f; // 타워 중심에서 가장 가까운 거리
    [SerializeField] private float _maxRadius = 5.0f; // 타워 중심에서 가장 먼 거리 (벽면 포함)
    
    //시작 높이와 끝 높이. 데드존이 있으니 확인
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
        // 1. 기존 장애물 전부 비활성화 후 풀로 회수
        ReturnAllToPool();

        Debug.Log("[Server] 원통형 공중 맵 생성 시작...");
        float currentY = _startY;

        while (currentY > _endY)
        {
            int randomIndex = Random.Range(0, _obstacleList.Count);
            ObstacleData data = _obstacleList[randomIndex];
            if (data.prefab == null) continue;

            float baseAngle = Random.Range(0f, 360f);

            for (int i = 0; i < data.countPerFloor; i++)
            {
                float finalAngle = baseAngle + (i * data.angleOffset);
                float radian = finalAngle * Mathf.Deg2Rad;

                // [수정 포인트] 벽에 붙지 않고 떠 있을 수 있도록 반지름을 범위 내에서 랜덤 결정
                float randomRadius = Random.Range(_minRadius, _maxRadius);

                Vector3 spawnPos = new Vector3(
                    _towerCenter.x + Mathf.Cos(radian) * randomRadius,
                    currentY,
                    _towerCenter.z + Mathf.Sin(radian) * randomRadius
                );

                // 장애물이 중심을 바라보게 설정 (90도 보정은 프리펩 방향에 따라 수정)
                Quaternion rotation = Quaternion.Euler(0, -finalAngle + 90f, 0);

                // 2. 풀링 시스템 적용
                GameObject obstacle = GetFromPool(data.prefab, spawnPos, rotation);

                // 3. 네트워크 스폰 활성화
                if (!obstacle.activeSelf) obstacle.SetActive(true);
                NetworkServer.Spawn(obstacle);
            }

            currentY -= data.floorHeight;
            if (data.floorHeight <= 0) currentY -= 1f;
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