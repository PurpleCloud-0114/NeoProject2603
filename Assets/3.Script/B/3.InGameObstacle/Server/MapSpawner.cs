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

    [Server]
    public void GenerateMap()
    {
        ReturnMapToPool();

        float currentY = _startY;
        while (currentY > _endY)
        {
            if (_floorPrefabs == null || _floorPrefabs.Count == 0) break;
            GameObject prefab = _floorPrefabs[Random.Range(0, _floorPrefabs.Count)];

            // 30도 단위 랜덤 회전
            int randomStep = Random.Range(0, 12);
            float randomRotationY = randomStep * 30f;

            Vector3 spawnPos = new Vector3(_towerCenter.x, currentY, _towerCenter.z);
            Quaternion spawnRot = Quaternion.Euler(0, randomRotationY, 0);

            GameObject floor = GetFromPool(prefab, spawnPos, spawnRot);

            if (!floor.activeSelf) floor.SetActive(true);
            NetworkServer.Spawn(floor);

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