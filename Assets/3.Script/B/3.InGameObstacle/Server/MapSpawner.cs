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

    [SerializeField] private List<GameObject> _floorPrefabs;

    private List<GameObject> _pool = new List<GameObject>();

    [Server]
    public void FullGenerate()
    {
        float y = _startY;

        while (y > _endY)
        {
            GameObject prefab = _floorPrefabs[Random.Range(0, _floorPrefabs.Count)];

            Vector3 pos = new Vector3(_center.x, y, _center.z);
            Quaternion rot = Quaternion.Euler(0, Random.Range(0, 360), 0);

            GameObject obj = GetFromPool(prefab, pos, rot);

            if (!obj.activeSelf)
                obj.SetActive(true);

            var identity = obj.GetComponent<NetworkIdentity>();

            if (identity.netId == 0)
                NetworkServer.Spawn(obj);

            y -= _step;
        }

        _obstacleSpawner.GenerateFloatingObstacles();
    }

    [Server]
    private GameObject GetFromPool(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        GameObject obj = _pool.Find(x => !x.activeSelf && x.name.Contains(prefab.name));

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
                NetworkServer.UnSpawn(obj);
                obj.SetActive(false);
            }
        }
    }
}