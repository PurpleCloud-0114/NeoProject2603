using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class MapFloor : NetworkBehaviour
{
    [Header("밸런싱 설정")]
    [Tooltip("장애물이 생성될 확률 (0: 없음, 1: 모두 생성)")]
    [Range(0f, 1f)]
    [SerializeField] private float _spawnProbability = 0.5f;

    [Header("프리팹에 미리 배치된 장애물들")]
    [SerializeField] private List<GameObject> _attachedObstacles;

    [SyncVar(hook = nameof(OnObstacleMaskChanged))]
    private uint _activeObstacleMask = 0;

    [Server]
    public void RandomizeAttachedObstacles()
    {
        uint mask = 0;
        for (int i = 0; i < _attachedObstacles.Count; i++)
        {
            // 서버에서 설정된 확률로 마스크 계산
            if (Random.value < _spawnProbability)
            {
                mask |= (1u << i);
            }
        }
        _activeObstacleMask = mask;
        ApplyObstacleState(_activeObstacleMask);
    }

    private void OnObstacleMaskChanged(uint oldMask, uint newMask)
    {
        ApplyObstacleState(newMask);
    }

    private void ApplyObstacleState(uint mask)
    {
        for (int i = 0; i < _attachedObstacles.Count; i++)
        {
            if (_attachedObstacles[i] != null)
            {
                bool isActive = (mask & (1u << i)) != 0;
                _attachedObstacles[i].SetActive(isActive);
            }
        }
    }

    [Server]
    public void Server_DisableObstacle(GameObject obstacle)
    {
        int index = _attachedObstacles.IndexOf(obstacle);
        if (index != -1)
        {
            // 해당 비트만 0으로 끔
            _activeObstacleMask &= ~(1u << index);

            // 주의: 서버에서는 Hook이 자동으로 안 불릴 수 있으므로 직접 적용
            ApplyObstacleState(_activeObstacleMask);
        }
    }
}