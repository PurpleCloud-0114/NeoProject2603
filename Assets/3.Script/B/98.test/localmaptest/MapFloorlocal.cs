using UnityEngine;
using System.Collections.Generic;

public class MapFloorlocal : MonoBehaviour
{
    [Header("밸런싱 설정")]
    [Tooltip("장애물이 생성될 확률 (0: 없음, 1: 모두 생성)")]
    [Range(0f, 1f)]
    [SerializeField] private float _spawnProbability = 0.5f;

    [Header("프리팹에 미리 배치된 장애물들")]
    [SerializeField] private List<GameObject> _attachedObstacles;

    private uint _activeObstacleMask = 0;

    public void RandomizeAttachedObstacles()
    {
        uint mask = 0;
        for (int i = 0; i < _attachedObstacles.Count; i++)
        {
            // 고정값 0.5f 대신 설정한 확률 변수 사용
            if (Random.value < _spawnProbability)
            {
                mask |= (1u << i);
            }
        }
        _activeObstacleMask = mask;
        ApplyObstacleState(_activeObstacleMask);
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