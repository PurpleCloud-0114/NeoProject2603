using UnityEngine;
using Mirror;
using System.Collections.Generic;

[System.Serializable]
public struct FloatingObstacleData
{
    public GameObject prefab;
    public float checkRadius;
}

public class ObstacleSpawner : NetworkBehaviour
{
    [SerializeField] private Vector3 _center = Vector3.zero;
    [SerializeField] private float _radius = 5f;
    [SerializeField] private float _startY = 0f;
    [SerializeField] private float _endY = -500f;

    [SerializeField] private int _count = 50;
    [SerializeField] private List<FloatingObstacleData> _list;
    [SerializeField] private LayerMask _layer;

    private List<GameObject> _pool = new List<GameObject>();

    [Server]
    public void GenerateFloatingObstacles()
    {
        int spawned = 0;
        int attempts = 0;

        while (spawned < _count && attempts < 2000)
        {
            attempts++;

            var data = _list[Random.Range(0, _list.Count)];

            Vector3 pos = new Vector3(
                _center.x + Random.Range(-_radius, _radius),
                Random.Range(_endY, _startY),
                _center.z + Random.Range(-_radius, _radius)
            );

            if (!Physics.CheckSphere(pos, data.checkRadius, _layer))
            {
                GameObject obj = GetFromPool(data.prefab, pos, Quaternion.identity);

                if (!obj.activeSelf)
                    obj.SetActive(true);

                var identity = obj.GetComponent<NetworkIdentity>();

                if (identity.netId == 0)
                    NetworkServer.Spawn(obj);

                spawned++;
            }
        }

        Debug.Log($"[Obstacle] 생성 완료: {spawned}");
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
    public void ReturnAllToPool()
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