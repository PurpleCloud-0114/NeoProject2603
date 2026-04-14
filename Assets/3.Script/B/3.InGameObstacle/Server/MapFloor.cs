using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class MapFloor : NetworkBehaviour
{
    [Header("프리팹에 미리 배치된 장애물들")]
    [SerializeField] private List<GameObject> _attachedObstacles;

    // 최대 32개의 장애물 On/Off 상태 동기화
    [SyncVar(hook = nameof(OnObstacleMaskChanged))]
    private uint _activeObstacleMask = 0;

    [Server]
    public void RandomizeAttachedObstacles()
    {
        uint mask = 0;
        for (int i = 0; i < _attachedObstacles.Count; i++)
        {
            // 기획: 수량이 많아질수록 어려워짐 (여기서 확률이나 개수를 조절 가능)
            if (Random.value > 0.5f)
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
}