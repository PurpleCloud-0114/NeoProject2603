using UnityEngine;
using Mirror;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class MapFloor : NetworkBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float _spawnProbability = 0.5f;

    [SerializeField] private List<GameObject> _attachedObstacles;

    [SyncVar(hook = nameof(OnMaskChanged))]
    private uint _activeMask = 0;

    // ----- [에디터 전용] 프리팹 메뉴 추가 -----
#if UNITY_EDITOR
    [ContextMenu("Setup and Save Obstacle Indices")] // 이 줄이 있어야 인스펙터 메뉴에 나옵니다!
    public void SetupAndSaveIndices()
    {
        for (int i = 0; i < _attachedObstacles.Count; i++)
        {
            if (_attachedObstacles[i] == null) continue;

            if (!_attachedObstacles[i].TryGetComponent(out ObstacleIdentity id))
            {
                // 프리팹 모드에서도 컴포넌트가 추가되도록 처리
                id = _attachedObstacles[i].AddComponent<ObstacleIdentity>();
            }

            id.obstacleIndex = i;
            id.parentFloor = this;

            // 변경사항이 유니티 에디터에 기록되도록 설정
            EditorUtility.SetDirty(id);
        }

        EditorUtility.SetDirty(this);

        // 프리팹 스테이지(편집 모드)라면 해당 씬을 더티 상태로 마킹하여 저장 가능하게 함
        var stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage != null)
        {
            EditorSceneManager.MarkSceneDirty(stage.scene);
        }

        Debug.Log("<color=cyan>[MapFloor]</color> 인덱스 설정이 완료되었습니다. 프리팹을 저장(Ctrl+S)하세요!");
    }
#endif

    [Server]
    public void ResetObstacles()
    {
        _activeMask = 0;
        for (int i = 0; i < _attachedObstacles.Count; i++)
        {
            if (_attachedObstacles[i] != null)
                _attachedObstacles[i].SetActive(true);
        }
    }

    [Server]
    public void RandomizeAttachedObstacles()
    {
        uint mask = 0;
        for (int i = 0; i < _attachedObstacles.Count; i++)
        {
            if (Random.value < _spawnProbability)
                mask |= (1u << i);
        }

        _activeMask = mask;
        Apply(mask);
    }

    // 런타임에 혹시 몰라 한 번 더 실행해주는 용도
    [Server]
    public void SetupIndices()
    {
        for (int i = 0; i < _attachedObstacles.Count; i++)
        {
            if (_attachedObstacles[i] == null) continue;

            if (!_attachedObstacles[i].TryGetComponent(out ObstacleIdentity id))
            {
                id = _attachedObstacles[i].AddComponent<ObstacleIdentity>();
            }

            id.obstacleIndex = i;
            id.parentFloor = this;
        }
    }

    void OnMaskChanged(uint oldMask, uint newMask)
    {
        Apply(newMask);
    }

    void Apply(uint mask)
    {
        for (int i = 0; i < _attachedObstacles.Count; i++)
        {
            if (_attachedObstacles[i] == null) continue;

            bool active = (mask & (1u << i)) != 0;
            if (_attachedObstacles[i].activeSelf != active)
                _attachedObstacles[i].SetActive(active);
        }
    }

    [Server]
    public void DisableByIndex(int index)
    {
        if (index < 0 || index >= _attachedObstacles.Count) return;

        uint oldMask = _activeMask;
        uint newMask = oldMask & ~(1u << index);

        if (oldMask == newMask)
            _activeMask = oldMask ^ (1u << 31);

        _activeMask = newMask;
        Apply(_activeMask);
    }
}