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

    [Header("Scale Dynamic Settings")]
    [Tooltip("몇 층부터 스케일 변동을 적용할지")]
    [SerializeField] private int _shrinkStartFloor = 5;
    [Tooltip("변동될 수치 (0.1)")]
    [SerializeField] private float _scaleStep = 0.1f;
    [Tooltip("최소 스케일")]
    [SerializeField] private float _minScale = 0.6f;
    [Tooltip("최대 스케일")]
    [SerializeField] private float _maxScale = 1.0f;

    [SerializeField] private List<GameObject> _floorPrefabs;
    private List<GameObject> _pool = new List<GameObject>();

    [Server]
    public void FullGenerate()
    {
        float y = _startY;
        int currentFloor = 0;

        // 시작 스케일은 기본 1.0f로 설정
        float currentScale = 1.0f;

        while (y > _endY)
        {
            // 1. 랜덤 프리팹 결정
            GameObject prefab = _floorPrefabs[Random.Range(0, _floorPrefabs.Count)];
            Vector3 pos = new Vector3(_center.x, y, _center.z);
            Quaternion rot = Quaternion.Euler(0, Random.Range(0, 360), 0);

            // 2. 풀에서 가져오기
            GameObject obj = GetFromPool(prefab, pos, rot);

            // 3. [핵심] 랜덤 축소/확대 기믹 적용
            if (currentFloor >= _shrinkStartFloor)
            {
                // 50% 확률로 증가(+) 또는 감소(-) 결정
                float change = (Random.value > 0.5f) ? _scaleStep : -_scaleStep;
                currentScale += change;

                // 범위 제한 (0.6 ~ 1.0)
                currentScale = Mathf.Clamp(currentScale, _minScale, _maxScale);

                // 부동 소수점 오차 보정 (0.700001 같은 값 방지)
                currentScale = Mathf.Round(currentScale * 10f) / 10f;
            }

            // 스케일 적용 (X, Z축만 변동, Y축은 두께 유지를 위해 1.0 고정)
            obj.transform.localScale = new Vector3(currentScale, 1f, currentScale);

            // 4. 활성화 및 스폰
            if (!obj.activeSelf)
                obj.SetActive(true);

            var identity = obj.GetComponent<NetworkIdentity>();
            if (identity != null && identity.netId == 0)
            {
                NetworkServer.Spawn(obj);
            }

            // 5. 장애물 상태 동기화
            MapFloor floorScript = obj.GetComponent<MapFloor>();
            if (floorScript != null)
            {
                floorScript.RandomizeAttachedObstacles();
            }

            // 6. 다음 층 준비
            y -= _step;
            currentFloor++;
        }

        // 공중 부유 장애물 생성
        if (_obstacleSpawner != null)
        {
            _obstacleSpawner.GenerateFloatingObstacles();
        }
    }

    [Server]
    private GameObject GetFromPool(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        // 이름에 프리팹 명칭이 포함된 비활성 오브젝트 탐색
        GameObject obj = _pool.Find(x => x != null && !x.activeSelf && x.name.Contains(prefab.name));

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
    public void ReturnMapToPool()
    {
        foreach (var obj in _pool)
        {
            if (obj != null && obj.activeSelf)
            {
                // [중요] 풀로 회수 시 스케일 초기화 (안 하면 다음 생성 시 찌그러진 상태 유지됨)
                obj.transform.localScale = Vector3.one;

                NetworkServer.UnSpawn(obj);
                obj.SetActive(false);
            }
        }
    }
}