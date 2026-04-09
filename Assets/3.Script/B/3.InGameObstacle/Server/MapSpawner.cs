using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class MapSpawner : NetworkBehaviour
{
    [Header("Tower Settings")]
    [SerializeField] private Vector3 _towerCenter = Vector3.zero;
    [SerializeField] private float _startY = 0f;
    [SerializeField] private float _endY = -500f;
    [SerializeField] private float _heightStep = 40f; // 층간 높이 고정 40

    [Header("Map Floor Templates")]
    [SerializeField] private List<GameObject> _floorPrefabs; // 사용할 층 프리펩 리스트

    // 맵 오브젝트 풀링 리스트
    private List<GameObject> _mapPool = new List<GameObject>();

    public override void OnStartServer()
    {
        base.OnStartServer();
        GenerateMap();
    }

    /// <summary>
    /// 매 판 새로운 맵을 생성할 때 호출
    /// </summary>
    [Server]
    public void GenerateMap()
    {
        // 1. 기존 맵 조각들 회수
        ReturnMapToPool();

        Debug.Log("[Server] 맵 생성 시작 (층별 합체 구조)...");
        float currentY = _startY;

        while (currentY > _endY)
        {
            // 2. 프리펩 리스트에서 랜덤 선택
            if (_floorPrefabs == null || _floorPrefabs.Count == 0) break;
            GameObject prefab = _floorPrefabs[Random.Range(0, _floorPrefabs.Count)];

            // 3. 30도 단위 랜덤 회전 계산 (0, 30, 60 ... 330)
            int randomStep = Random.Range(0, 12); // 0 ~ 11
            float randomRotationY = randomStep * 30f;

            Vector3 spawnPos = new Vector3(_towerCenter.x, currentY, _towerCenter.z);
            Quaternion spawnRot = Quaternion.Euler(0, randomRotationY, 0);

            // 4. 풀에서 가져오기
            GameObject floor = GetFromPool(prefab, spawnPos, spawnRot);

            // 5. 활성화 및 네트워크 스폰
            if (!floor.activeSelf) floor.SetActive(true);
            NetworkServer.Spawn(floor);

            // 6. 40만큼 아래로 이동
            currentY -= _heightStep;
        }

        Debug.Log("[Server] 맵 생성 완료!");
    }

    [Server]
    private GameObject GetFromPool(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        // 이름 대조 시 (Clone) 포함 여부 확인
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