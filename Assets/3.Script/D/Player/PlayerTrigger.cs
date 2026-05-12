using System.Collections;
using System.Collections.Generic;
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
    private const string TAG_FINISHLINE = "FinishLine";
    private const string TAG_ENDPOINT = "EndPoint";

    [SerializeField] private GameObject _characterModel;
    [SerializeField] private float _hitPlayerImpulsePower = 25f;
    [SerializeField] private float _hitObstacleInvincibilityDuration = 1f;
    private bool Invincibility = false;
    private WaitForSeconds wfs;
    private WaitForSeconds wfs2;

    private void Awake()
    {
        TryGetComponent(out _playerCore);
        wfs = new WaitForSeconds(_hitObstacleInvincibilityDuration);
        wfs2 = new WaitForSeconds(_hitObstacleInvincibilityDuration * 0.1f);
    }

	private void OnCollisionEnter(Collision collision) {
		if(collision.transform.CompareTag(TAG_ENDPOINT)) {
            _characterModel.SetActive(false);
            UIManager.Instance.ShowPersonalResult();
            _playerCore.SendEndpoint();
        }
	}

	private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case TAG_ITEMBOX:
                if (!isLocalPlayer) return;
                CmdDisableRoot(other.transform.root.GetComponent<NetworkIdentity>());
                _playerCore.on_item_acquired?.Invoke(ItemManager.Instance.RandomItem());
                break;

            case TAG_OBSTACLE:
                ObstacleIdentity obs = other.GetComponentInParent<ObstacleIdentity>();

                // 1. MapFloor에 속한 모듈형 장애물인 경우
                if (isLocalPlayer) {
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
				}
                if (!Invincibility) {
                    StartCoroutine(Co_Invincibility());

                    _playerCore.on_obstacle_hit?.Invoke(); 
                }
                break;

            case TAG_REDZONE:
                if (!isLocalPlayer) return;
                _playerCore.on_redzone_entered?.Invoke();
                break;

            case TAG_SPIDERWEB:
                if (!isLocalPlayer) return;
                if (_playerCore.status_effect == StatusEffect.Invinsible) return;
                _playerCore.on_spiderweb_hit?.Invoke(other);
                break;

            case TAG_PLAYER:
                if (!isLocalPlayer) return;
                PushPlayer(other);
                break;
            case TAG_FINISHLINE:
                if (!isLocalPlayer) return;
                float impactSpeed = 0f;
                if (TryGetComponent(out Rigidbody rb)) impactSpeed = Mathf.Abs(rb.linearVelocity.y);
                double myFinishTime = NetworkTime.time - RaceManager.Instance.race_start_time_sync;
                _playerCore.player_state = PlayerState.Finish;
                _playerCore.SpawnPortal();
                _playerCore.SendArriveResult(impactSpeed, myFinishTime);
                break;
        }
    }

    private IEnumerator Co_Invincibility() {
        Invincibility = true;
        StartCoroutine(Co_InvincibilityVisual());
        yield return wfs;
        Invincibility = false;
    }

    private IEnumerator Co_InvincibilityVisual() {
        while(Invincibility) {
            _characterModel.SetActive(!_characterModel.activeSelf);
            yield return wfs2;
        }
        _characterModel.SetActive(true);
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