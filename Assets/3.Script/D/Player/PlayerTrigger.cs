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

    private void OnCollisionEnter(Collision collision)
    {
        if (!isLocalPlayer && !RaceManager.Instance.isSinglePlay) return;

        if (collision.transform.CompareTag("EndPoint"))
        {
            double myFinishTime = NetworkTime.time - RaceManager.Instance.race_start_time_sync;
            float impactSpeed = Mathf.Abs(collision.relativeVelocity.y);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isLocalPlayer && !RaceManager.Instance.isSinglePlay) return;

        Debug.Log("뭔가 닿음.");

        MapFloor floor = other.GetComponentInParent<MapFloor>();
        Debug.Log($"[Trigger] 감지: {other.name} | Tag: {other.tag} | Floor 존재: {floor != null}");

        switch (other.tag)
        {
            case TAG_ITEMBOX:
                CmdReportCollision(other.transform.root.gameObject, other.name);
                Debug.Log("아이템 먹음");
                IUseable randomItem = ItemManager.Instance.RandomItem();
                _playerCore.on_item_acquired?.Invoke(randomItem);
                break;

            case TAG_OBSTACLE:
                if (floor != null)
                {
                    int index = floor.GetObstacleIndex(other.gameObject);
                    // 로그 2: 인덱스 탐색 결과
                    Debug.Log($"[Obstacle] Floor: {floor.name}에서 Index 탐색 결과: {index}");

                    if (index != -1)
                    {
                        CmdReportCollisionByIndex(floor.gameObject, index);
                    }
                    else
                    {
                        Debug.LogWarning($"[Obstacle] {other.name}이 {floor.name}의 리스트에 등록되어 있지 않습니다!");
                    }
                }
                else
                {
                    Debug.Log($"[Obstacle] 독립 오브젝트 처리: {other.name}");
                    CmdReportCollision(other.gameObject, other.name);
                }
                _playerCore.on_obstacle_hit?.Invoke();
                break;

            case TAG_REDZONE:
                Debug.Log($"레드존 진입 Y좌표 : {other.transform.position.y}");
                Debug.Log($"플레이어 현재 Y좌표 : {transform.position.y}");
                _playerCore.on_redzone_entered?.Invoke();
                break;

            case TAG_SPIDERWEB:
                if (_playerCore.status_effect == StatusEffect.Invinsible) return;
                Debug.Log("거미줄 트리거 발동");
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
    private void CmdReportCollision(GameObject rootObj, string targetName)
    {
        if (rootObj == null) return;

        if (rootObj.TryGetComponent(out MapFloor floor))
        {
            floor.Server_DisableObstacleByName(targetName);
        }
        else
        {
            NetworkServer.UnSpawn(rootObj);
            rootObj.SetActive(false);
        }
    }

    [Command]
    private void CmdReportCollisionByIndex(GameObject floorObj, int index)
    {
        if (floorObj == null) return;

        if (floorObj.TryGetComponent(out MapFloor floor))
        {
            floor.Server_DisableObstacleByIndex(index);
        }
    }
}