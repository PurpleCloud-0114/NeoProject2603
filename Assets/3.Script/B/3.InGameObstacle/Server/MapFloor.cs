using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class MapFloor : NetworkBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float _spawnProbability = 0.5f;

    [SerializeField] private List<GameObject> _attachedObstacles;

    [SyncVar(hook = nameof(OnMaskChanged))]
    private uint _activeMask = 0;

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

    [Server]
    public void DisableByIndex(int index)
    {
        if (index < 0 || index >= _attachedObstacles.Count) return;

        uint oldMask = _activeMask;
        uint newMask = oldMask & ~(1u << index);

        // SyncVar 강제 갱신
        if (oldMask == newMask)
            _activeMask = oldMask ^ (1u << 31);

        _activeMask = newMask;
        Apply(_activeMask);
    }

    [Server]
    public void DisableByObject(GameObject hitObj)
    {
        for (int i = 0; i < _attachedObstacles.Count; i++)
        {
            GameObject target = _attachedObstacles[i];
            if (target == null) continue;

            if (hitObj.transform.IsChildOf(target.transform))
            {
                DisableByIndex(i);
                return;
            }
        }

        Debug.LogWarning("[Fallback] 매칭 실패 → 해당 오브젝트만 비활성화");
        hitObj.SetActive(false);
    }
}