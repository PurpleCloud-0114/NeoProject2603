using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class MapSpawner : NetworkBehaviour
{
    [Header("Tower Settings")]
    [SerializeField] private Vector3 _towerCenter = Vector3.zero;
    [SerializeField] private float _startY = 0f;
    [SerializeField] private float _endY = -500f;
    [SerializeField] private float _heightStep = 40f;

    [Header("Map Floor Templates")]
    [SerializeField] private List<GameObject> _floorPrefabs;

    private List<GameObject> _mapPool = new List<GameObject>();

    // 생성된 맵 층들을 관리 (ObstacleSpawner가 겹침 체크할 때 사용)
    public List<MapFloor> SpawnedFloors { get; private set; } = new List<MapFloor>();

    [Server]
    public void GenerateMap()
    {
        ReturnMapToPool();
        SpawnedFloors.Clear();

        float currentY = _startY;
        while (currentY > _endY)
        {
            if (_floorPrefabs == null || _floorPrefabs.Count == 0) break;
            GameObject prefab = _floorPrefabs[Random.Range(0, _floorPrefabs.Count)];

            float randomRotationY = Random.Range(0, 12) * 30f;
            Vector3 spawnPos = new Vector3(_towerCenter.x, currentY, _towerCenter.z);
            Quaternion spawnRot = Quaternion.Euler(0, randomRotationY, 0);

            GameObject floorObj = GetFromPool(prefab, spawnPos, spawnRot);
            if (!floorObj.activeSelf) floorObj.SetActive(true);

            // [추가됨] 프리팹 내 장애물 랜덤 On/Off 설정
            MapFloor mapFloor = floorObj.GetComponent<MapFloor>();
            if (mapFloor != null)
            {
                mapFloor.RandomizeAttachedObstacles();
                SpawnedFloors.Add(mapFloor);
            }

            NetworkServer.Spawn(floorObj);
            currentY -= _heightStep;
        }
    }

    [Server]
    private GameObject GetFromPool(GameObject prefab, Vector3 pos, Quaternion rot)
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