using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerTrigger : NetworkBehaviour
{
    private PlayerCore _playerCore;

    private const string TAG_ITEMBOX = "ItemBox";
    private const string TAG_OBSTACLE = "Obstacle";
    private const string TAG_REDZONE = "Redzone";
    private const string TAG_SPIDERWEB = "Spiderweb";
    private const string TAG_PLAYER = "Player";

    private float _hitPlayerImpulsePower = 25f;

    private void Awake()
    {
        TryGetComponent(out _playerCore);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isLocalPlayer && !RaceManager.Instance.isSinglePlay) return;

        Debug.Log($"[Trigger] 충돌: {other.name}");

        // 맵 장애물 처리 (최우선)
        var obstacle = other.GetComponentInParent<ObstacleIdentity>();

        if (obstacle != null)
        {
            MapFloor floor = obstacle.GetComponentInParent<MapFloor>();

            if (floor != null)
            {
                int index = floor.GetIndex(obstacle.gameObject);

                if (index != -1)
                {
                    Debug.Log($"[Hit] index: {index}");

                    CmdDisableObstacle(floor.netIdentity, index);
                }
                else
                {
                    Debug.LogError("index 못찾음");
                }
            }

            _playerCore.on_obstacle_hit?.Invoke();
            return;
        }

        // ===== 일반 처리 =====

        if (other.CompareTag(TAG_ITEMBOX))
        {
            CmdDisableRoot(other.transform.root.gameObject);
            _playerCore.on_item_acquired?.Invoke(ItemManager.Instance.RandomItem());
            return;
        }

        if (other.CompareTag(TAG_REDZONE))
        {
            _playerCore.on_redzone_entered?.Invoke();
            return;
        }

        if (other.CompareTag(TAG_SPIDERWEB))
        {
            if (_playerCore.status_effect == StatusEffect.Invinsible) return;
            _playerCore.on_spiderweb_hit?.Invoke(other);
            return;
        }

        if (other.CompareTag(TAG_PLAYER))
        {
            PushPlayer(other);
        }
    }

    private void PushPlayer(Collider other)
    {
        Vector3 pushDir = transform.position - other.transform.position;
        pushDir.y = 0;

        if (pushDir.sqrMagnitude > 0.001f)
        {
            _playerCore.on_impulse_requested?.Invoke(pushDir.normalized * _hitPlayerImpulsePower);
        }
    }

    // =========================
    // 공중 장애물 / 아이템
    // =========================
    [Command]
    private void CmdDisableRoot(GameObject obj)
    {
        if (obj == null) return;

        if (obj.GetComponent<MapFloor>() != null)
        {
            Debug.LogError("MapFloor 삭제 시도 차단");
            return;
        }

        Debug.Log($"[Cmd] 공중 제거: {obj.name}");

        NetworkServer.UnSpawn(obj);
        obj.SetActive(false);
    }

    // =========================
    // 맵 장애물 제거 (index 기반)
    // =========================
    [Command]
    private void CmdDisableObstacle(NetworkIdentity floorId, int index)
    {
        if (floorId == null) return;

        var floor = floorId.GetComponent<MapFloor>();

        if (floor != null)
        {
            floor.DisableByIndex(index);
        }
    }
}