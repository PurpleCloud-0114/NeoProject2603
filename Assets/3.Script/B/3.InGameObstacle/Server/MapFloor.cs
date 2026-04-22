using UnityEngine;
using Mirror;

public class MapFloor : NetworkBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float _spawnProbability = 0.5f;

    private ObstacleIdentity[] _obstacles;

    [SyncVar(hook = nameof(OnMaskChanged))]
    private uint _mask;

    // =========================
    // 서버 초기화
    // =========================
    [Server]
    public void Initialize()
    {
        _obstacles = GetComponentsInChildren<ObstacleIdentity>(true);

        Debug.Log($"[MapFloor] 장애물 수: {_obstacles.Length}");

        ResetObstacles();
        RandomizeAttachedObstacles();
    }

    // =========================
    // 클라 초기화 보장
    // =========================
    public override void OnStartClient()
    {
        base.OnStartClient();
        EnsureInit();
    }

    private void EnsureInit()
    {
        if (_obstacles == null || _obstacles.Length == 0)
        {
            _obstacles = GetComponentsInChildren<ObstacleIdentity>(true);
        }
    }

    // =========================
    // 전체 활성화
    // =========================
    [Server]
    public void ResetObstacles()
    {
        uint mask = 0;

        for (int i = 0; i < _obstacles.Length; i++)
        {
            if (_obstacles[i] != null)
                mask |= (1u << i);
        }

        _mask = mask;
        Apply(mask);
    }

    // =========================
    // 랜덤 활성화
    // =========================
    [Server]
    public void RandomizeAttachedObstacles()
    {
        uint mask = 0;

        for (int i = 0; i < _obstacles.Length; i++)
        {
            if (_obstacles[i] == null) continue;

            if (Random.value < _spawnProbability)
                mask |= (1u << i);
        }

        _mask = mask;
        Apply(mask);
    }

    // =========================
    // 장애물 제거 (index 기반)
    // =========================
    [Server]
    public void DisableByIndex(int index)
    {
        if (index < 0 || index >= _obstacles.Length) return;

        Debug.Log($"[MapFloor] 제거 index: {index}");

        uint newMask = _mask & ~(1u << index);

        if (newMask == _mask)
            _mask ^= (1u << 31);

        _mask = newMask;

        Apply(_mask);
    }

    // =========================
    // SyncVar
    // =========================
    private void OnMaskChanged(uint oldMask, uint newMask)
    {
        Apply(newMask);
    }

    private void Apply(uint mask)
    {
        EnsureInit();

        if (_obstacles == null) return;

        for (int i = 0; i < _obstacles.Length; i++)
        {
            if (_obstacles[i] == null) continue;

            bool active = (mask & (1u << i)) != 0;
            _obstacles[i].gameObject.SetActive(active);
        }
    }

    // =========================
    // index 찾기
    // =========================
    public int GetIndex(GameObject obj)
    {
        Transform t = obj.transform;

        for (int i = 0; i < _obstacles.Length; i++)
        {
            if (_obstacles[i] == null) continue;

            Transform root = _obstacles[i].transform;

            Transform check = t;
            while (check != null)
            {
                if (check == root)
                    return i;

                check = check.parent;
            }
        }

        return -1;
    }
}