using UnityEngine;
using Mirror;

public class PlayerTrigger : NetworkBehaviour
{
    private PlayerCore _playerCore;

    private const string TAG_OBSTACLE = "Obstacle";
    private const string TAG_ITEMBOX = "ItemBox";
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
        if (!isLocalPlayer && !RaceManager.Instance.isSinglePlay)
            return;

        switch (other.tag)
        {
            case TAG_ITEMBOX:
                CmdDisableRoot(other.transform.root.GetComponent<NetworkIdentity>());
                _playerCore.on_item_acquired?.Invoke(ItemManager.Instance.RandomItem());
                break;

            case TAG_OBSTACLE:
                ObstacleIdentity obs = other.GetComponentInParent<ObstacleIdentity>();

                // 1. MapFloor에 속한 모듈형 장애물인 경우
                if (obs != null && obs.parentFloor != null)
                {
                    CmdHitModuleObstacle(obs.parentFloor.netIdentity, obs.obstacleIndex);
                }
                // 2. MapFloor가 없는 독립형(공중) 장애물인 경우 (아이템박스처럼 처리)
                else
                {
                    // root에서 NetworkIdentity를 찾아 서버에서 UnSpawn 및 삭제 요청
                    NetworkIdentity identity = other.transform.root.GetComponent<NetworkIdentity>();
                    if (identity != null)
                    {
                        CmdDisableRoot(identity);
                    }
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

    // ---------------- COMMANDS ----------------

    [Command]
    private void CmdHitModuleObstacle(NetworkIdentity floorId, int index)
    {
        if (floorId != null && floorId.TryGetComponent(out MapFloor floor))
        {
            floor.DisableByIndex(index);
        }
    }

    [Command]
    private void CmdDisableRoot(NetworkIdentity identity)
    {
        if (identity == null) return;

        NetworkServer.UnSpawn(identity.gameObject);
        identity.gameObject.SetActive(false);
    }

    // ---------------- PUSH ----------------

    private void PushPlayer(Collider other)
    {
        Vector3 dir = transform.position - other.transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude > 0.001f)
            _playerCore.on_impulse_requested?.Invoke(dir.normalized * _hitPlayerImpulsePower);
    }
}