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

    [SerializeField] private AudioSource _player3DAudioSource;

    private void Awake()
    {
        TryGetComponent(out _playerCore);
    }

    private void OnCollisionEnter(Collision collision) {
        

        if (collision.transform.CompareTag("EndPoint")) {
            if (isLocalPlayer) {
                float impactSpeed = Mathf.Abs(collision.relativeVelocity.y);
                double myFinishTime = NetworkTime.time - RaceManager.Instance.race_start_time_sync;
                _playerCore.player_state = PlayerState.Finish;
                _playerCore.SendArriveResult(impactSpeed, myFinishTime);
                _playerCore.LandEndpoint();
			}
            gameObject.SetActive(false);
        }
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
                Play3DSFX("ItemWeightActivate");
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
                CmdPlaySFX("ObstacleBreak"); // 서버에 소리 재생 요청
                break;

            case TAG_REDZONE:
                _playerCore.on_redzone_entered?.Invoke();
                break;

            case TAG_SPIDERWEB:
                if (_playerCore.status_effect == StatusEffect.Invinsible) return;
                _playerCore.on_spiderweb_hit?.Invoke(other);
                CmdPlaySFX("ItemWebHit"); // 거미줄 걸림 소리
                Debug.Log("나 거미줄 걸렸어~");
                break;

            case TAG_PLAYER:
                PushPlayer(other);
                CmdPlaySFX("PlayerCollision"); // 플레이어 충돌 소리
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

    // ---------------- 3D SOUND ----------------
    [Command]
    private void CmdPlaySFX(string sfxName)
    {
        RpcPlay3DSFX(sfxName);
    }

    [ClientRpc]
    private void RpcPlay3DSFX(string sfxName)
    {
        Play3DSFX(sfxName);
    }

    private void Play3DSFX(string sfxName)
    {
        if (_player3DAudioSource == null) return;
        AudioClip clip = AudioManager.Instance.GetSFXClip(sfxName);
        if (clip != null)
        {
            _player3DAudioSource.PlayOneShot(clip);
        }
    }
}