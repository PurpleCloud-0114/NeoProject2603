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

    // ---------------- [추가됨] 런타임 인덱스 설정 ----------------
    [Server]
    public void SetupIndices()
    {
        for (int i = 0; i < _attachedObstacles.Count; i++)
        {
            if (_attachedObstacles[i] == null) continue;

            if (!_attachedObstacles[i].TryGetComponent(out ObstacleIdentity id))
                id = _attachedObstacles[i].AddComponent<ObstacleIdentity>();

            id.obstacleIndex = i;
            id.parentFloor = this;
        }
    }

    #region Editor Setup
#if UNITY_EDITOR
    [ContextMenu("Setup and Save Obstacle Indices")]
    public void SetupAndSaveIndices()
    {
        for (int i = 0; i < _attachedObstacles.Count; i++)
        {
            if (_attachedObstacles[i] == null) continue;

            if (!_attachedObstacles[i].TryGetComponent(out ObstacleIdentity id))
                id = _attachedObstacles[i].AddComponent<ObstacleIdentity>();

            id.obstacleIndex = i;
            id.parentFloor = this;

            EditorUtility.SetDirty(id);
        }

        EditorUtility.SetDirty(this);

        var stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage != null)
            EditorSceneManager.MarkSceneDirty(stage.scene);

        Debug.Log("[MapFloor] 인덱스 설정 완료");
    }
#endif
    #endregion

    public override void OnStartClient()
    {
        base.OnStartClient();
        Apply(_activeMask);
    }

    // ---------------- SERVER ----------------

    [Server]
    public void ResetObstacles()
    {
        _activeMask = 0;
        Apply(_activeMask);
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
        Apply(_activeMask);
    }

    [Server]
    public void DisableByIndex(int index)
    {
        if (index < 0 || index >= _attachedObstacles.Count)
        {
            Debug.LogWarning($"[MapFloor] 잘못된 index: {index}");
            return;
        }

        uint newMask = _activeMask & ~(1u << index);
        _activeMask = newMask;

        Debug.Log($"[MapFloor Server] 장애물 OFF Index: {index}");

        // 서버(호스트)에서도 즉시 반영하고 클라이언트로 전파
        Apply(_activeMask);
        RpcForceDisable(index);
    }

    // 연속 충돌 시 SyncVar 유실 방지를 위한 보조 RPC
    [ClientRpc]
    private void RpcForceDisable(int index)
    {
        if (index >= 0 && index < _attachedObstacles.Count)
        {
            if (_attachedObstacles[index] != null)
                _attachedObstacles[index].SetActive(false);
        }
    }

    // ---------------- SYNC ----------------

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
            _attachedObstacles[i].SetActive(active);
        }
    }
}