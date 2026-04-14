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

    [Header("Map Floor Templates")]
    [SerializeField] private List<GameObject> _floorPrefabs;

    private List<GameObject> _mapPool = new List<GameObject>();

    // 생성된 맵 층들을 관리
    public List<MapFloor> SpawnedFloors { get; private set; } = new List<MapFloor>();

    /// <summary>
    /// 게임 시작 시 또는 맵 재생성 시 호출하는 메인 함수
    /// </summary>
    [Server]
    public void FullGenerate()
    {
        // 1. 기존 맵 제거 및 풀링 반환
        ReturnMapToPool();
        SpawnedFloors.Clear();

        // 2. 층(Floor) 생성 및 1번(벽 부착형) 장애물 랜덤화
        float currentY = _startY;
        while (currentY > _endY)
        {
            if (_floorPrefabs == null || _floorPrefabs.Count == 0) break;

            // 랜덤 프리팹 선택
            GameObject prefab = _floorPrefabs[Random.Range(0, _floorPrefabs.Count)];

            // 기획: 0, 30, 60... 330도 중 랜덤 로테이션 (12각형 대응)
            float randomRotationY = Random.Range(0, 12) * 30f;
            Vector3 spawnPos = new Vector3(_towerCenter.x, currentY, _towerCenter.z);
            Quaternion spawnRot = Quaternion.Euler(0, randomRotationY, 0);

            GameObject floorObj = GetFromPool(prefab, spawnPos, spawnRot);
            if (!floorObj.activeSelf) floorObj.SetActive(true);

            // [1번 장애물] 프리팹 내 장애물 랜덤 On/Off 설정
            MapFloor mapFloor = floorObj.GetComponent<MapFloor>();
            if (mapFloor != null)
            {
                mapFloor.RandomizeAttachedObstacles();
                SpawnedFloors.Add(mapFloor);
            }

            // 서버 스폰 (클라이언트들에게 전송)
            NetworkServer.Spawn(floorObj);
            currentY -= _heightStep;
        }

        // 3. [2번 장애물] 공중 장애물 생성 (1번 장애물들이 배치된 이후 실행)
        if (_obstacleSpawner != null)
        {
            // Physics.CheckSphere가 현재 배치된 1번 장애물들을 감지할 수 있도록 실행
            _obstacleSpawner.GenerateFloatingObstacles();
        }

        Debug.Log("[Server] 전체 맵 및 장애물 생성 완료.");
    }

    [Server]
    private GameObject GetFromPool(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        // 이름에 (Clone)이 붙으므로 이를 고려하여 풀에서 탐색
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