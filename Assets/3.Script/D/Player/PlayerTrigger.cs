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

        MapFloor floor = other.GetComponentInParent<MapFloor>();

        switch (other.tag)
        {
            case TAG_ITEMBOX:
                CmdDisableRoot(other.transform.root.gameObject);
                IUseable randomItem = ItemManager.Instance.RandomItem();
                _playerCore.on_item_acquired?.Invoke(randomItem);
                break;

            case TAG_OBSTACLE:
                if (floor != null)
                {
                    CmdHitObstacle(floor.gameObject, other.gameObject);
                }
                else
                {
                    CmdDisableRoot(other.gameObject);
                }

                _playerCore.on_obstacle_hit?.Invoke();
                break;

            case TAG_REDZONE:
                _playerCore.on_redzone_entered?.Invoke();
                break;

            case TAG_SPIDERWEB:
                if (_playerCore.status_effect == StatusEffect.Invinsible) return;
                _playerCore.on_spiderweb_hit?.Invoke(other);
                break;

            case TAG_PLAYER:
                PushPlayer(other);
                break;
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

    [Command]
    private void CmdDisableRoot(GameObject obj)
    {
        if (obj == null) return;

        NetworkServer.UnSpawn(obj);
        obj.SetActive(false);
    }

    [Command]
    private void CmdHitObstacle(GameObject floorObj, GameObject hitObj)
    {
        if (floorObj == null || hitObj == null) return;

        if (floorObj.TryGetComponent(out MapFloor floor))
        {
            floor.DisableByObject(hitObj);
        }
    }
}