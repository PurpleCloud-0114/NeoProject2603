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
    public void Server_DisableObstacleByName(string targetName)
    {
        int index = _attachedObstacles.FindIndex(obj => obj != null &&
            (obj.name == targetName || targetName.StartsWith(obj.name)));

        if (index != -1)
        {
            // 이미 꺼진 경우 무시
            if ((_activeObstacleMask & (1u << index)) == 0) return;

            _activeObstacleMask &= ~(1u << index);
            ApplyObstacleState(_activeObstacleMask);
            Debug.Log($"[Server] {targetName} 비활성화 성공 (인덱스: {index})");
        }
        else
        {
            Debug.LogWarning($"[Server] {targetName}을 리스트에서 찾을 수 없습니다.");
        }
    }

    [Server]
    public void Server_DisableObstacleByIndex(int index)
    {
        // index가 유효한지 먼저 체크
        if (index < 0 || index >= _attachedObstacles.Count) return;

        // 현재 마스크 상태 로그 (서버 콘솔에서 확인용)
        // Debug.Log($"[Server] 현재 마스크: {_activeObstacleMask}, 끌 인덱스: {index}");

        // 비트마스크에서 해당 인덱스를 끄는 가장 확실한 방법
        uint bit = (1u << index);

        // 이미 꺼져있더라도 다시 한번 확실히 끄고 상태를 동기화합니다.
        _activeObstacleMask &= ~bit;

        // [중요] SyncVar 후크가 작동하지만, 서버에서도 즉시 반영되도록 명시적 호출
        ApplyObstacleState(_activeObstacleMask);
    }

    public int GetObstacleIndex(GameObject contactObj)
    {
        Transform contact = contactObj.transform;
        for (int i = 0; i < _attachedObstacles.Count; i++)
        {
            if (_attachedObstacles[i] == null) continue;
            Transform obstacle = _attachedObstacles[i].transform;

            Transform check = contact;
            while (check != null)
            {
                if (check == obstacle) return i;
                check = check.parent;
            }
        }
        // 로그 4: 탐색 실패 시 상세 경로 출력 (구조 파악용)
        // Debug.Log($"[Index] 리스트에 없음: {contactObj.name}");
        return -1;
    }
}