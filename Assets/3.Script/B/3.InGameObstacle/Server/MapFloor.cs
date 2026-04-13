using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class MapFloor : NetworkBehaviour
{
    [Header("프리팹에 미리 배치된 장애물들")]
    [SerializeField] private List<GameObject> _attachedObstacles;

    // 최대 32개의 장애물 On/Off 상태를 동기화하기 위한 비트마스크
    [SyncVar(hook = nameof(OnObstacleMaskChanged))]
    private uint _activeObstacleMask = 0;

    // 서버가 맵을 생성할 때 호출 (랜덤 On/Off 결정)
    [Server]
    public void RandomizeAttachedObstacles()
    {
        uint mask = 0;
        for (int i = 0; i < _attachedObstacles.Count; i++)
        {
            if (Random.value > 0.5f) // 50% 확률로 켬 (원하는 확률로 조정 가능)
            {
                mask |= (1u << i);
            }
        }
        _activeObstacleMask = mask;
        ApplyObstacleState(_activeObstacleMask);
    }

    // 클라이언트가 서버의 상태를 전달받을 때 실행
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

    // 공중부양 장애물이 위치를 피하기 위해, 현재 '켜져 있는' 장애물들의 위치를 반환
    public List<Vector3> GetActiveObstaclePositions()
    {
        List<Vector3> activePositions = new List<Vector3>();
        for (int i = 0; i < _attachedObstacles.Count; i++)
        {
            if (_attachedObstacles[i] != null && _attachedObstacles[i].activeSelf)
            {
                activePositions.Add(_attachedObstacles[i].transform.position);
            }
        }
        return activePositions;
    }
}