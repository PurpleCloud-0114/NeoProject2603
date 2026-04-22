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

        switch (other.tag)
        {
            case TAG_ITEMBOX:
                CmdDisableRoot(other.transform.root.gameObject);
                IUseable randomItem = ItemManager.Instance.RandomItem();
                _playerCore.on_item_acquired?.Invoke(randomItem);
                break;

            case TAG_OBSTACLE:
                // 1. 맵 모듈의 자식 장애물인지 확인 (Identity 스크립트 기반)
                if (other.TryGetComponent(out ObstacleIdentity obsID))
                {
                    if (obsID.parentFloor != null)
                    {
                        CmdHitModuleObstacle(obsID.parentFloor.gameObject, obsID.obstacleIndex);
                    }
                }
                // 2. 맵 모듈의 일부지만 스크립트가 없는 경우를 위한 안전장치
                else if (other.GetComponentInParent<MapFloor>() != null)
                {
                    // 맵 전체 삭제를 방지하기 위해 root 삭제를 실행하지 않음
                    Debug.Log("MapFloor 자식 장애물이지만 식별자가 없습니다.");
                }
                // 3. 순수 독립 장애물 (Spawner에 의해 개별 생성된 경우)
                else
                {
                    CmdDisableRoot(other.transform.root.gameObject);
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

    [Command]
    private void CmdHitModuleObstacle(GameObject floorObj, int index)
    {
        if (floorObj != null && floorObj.TryGetComponent(out MapFloor floor))
        {
            floor.DisableByIndex(index);
        }
    }

    [Command]
    private void CmdDisableRoot(GameObject obj)
    {
        if (obj == null) return;
        NetworkServer.UnSpawn(obj);
        obj.SetActive(false);
    }

    private void PushPlayer(Collider other)
    {
        Vector3 pushDir = transform.position - other.transform.position;
        pushDir.y = 0;
        if (pushDir.sqrMagnitude > 0.001f)
            _playerCore.on_impulse_requested?.Invoke(pushDir.normalized * _hitPlayerImpulsePower);
    }
}