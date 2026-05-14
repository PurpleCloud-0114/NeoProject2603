using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerItemController : NetworkBehaviour
{
    private PlayerCore _playerCore;

    public IUseable current_item = null;

    [Header("3D Audio")]
    [SerializeField] private AudioSource _player3DAudioSource;

    private void Awake()
    {
        TryGetComponent(out _playerCore);

        if (_player3DAudioSource == null)
        {
            _player3DAudioSource = GetComponentInChildren<AudioSource>(true);
        }

        Debug.Log($"[{name}] AUDIO SOURCE FOUND : " +
                  $"{(_player3DAudioSource != null ? _player3DAudioSource.name : "NULL")}");

        if (_player3DAudioSource != null)
        {
            Debug.Log(
                $"[{name}] AUDIO INFO | " +
                $"enabled={_player3DAudioSource.enabled} | " +
                $"activeInHierarchy={_player3DAudioSource.gameObject.activeInHierarchy} | " +
                $"volume={_player3DAudioSource.volume} | " +
                $"mute={_player3DAudioSource.mute} | " +
                $"spatialBlend={_player3DAudioSource.spatialBlend}"
            );
        }
    }

    private void OnEnable()
    {
        _playerCore.on_item_acquired += GetItem;
        _playerCore.on_item_button_clicked += UseItem;
    }

    private void OnDisable()
    {
        _playerCore.on_item_acquired -= GetItem;
        _playerCore.on_item_button_clicked -= UseItem;
    }

    private void GetItem(IUseable newItem)
    {
        current_item = newItem;
    }

    public void UseItem()
    {
        if (current_item != null)
        {
            string sfxName = GetItemActivationSFX(current_item.Type);

            Debug.Log($"[{name}] USE ITEM : {current_item.Type} / SFX : {sfxName}");

            // 서버 요청
            CmdUseItem(current_item.Type, sfxName);
        }

        current_item = null;
    }

    [Command]
    private void CmdUseItem(ItemType itemType, string sfxName)
    {
        Debug.Log($"[SERVER] CmdUseItem : {itemType} / {sfxName}");

        IUseable itemToUse = ItemManager.Instance.GetItemUseable(itemType);

        if (itemToUse != null)
        {
            Debug.Log($"[SERVER] ITEM USE SUCCESS");

            itemToUse.Use(gameObject);
        }
        else
        {
            Debug.LogError($"[SERVER] ITEM USEABLE NULL");
        }

        // 모든 클라이언트 재생
        RpcPlayItemSFX(sfxName);
    }

    [ClientRpc]
    private void RpcPlayItemSFX(string sfxName)
    {
        Debug.Log($"[CLIENT RPC] ITEM RPC SFX : {sfxName} / {gameObject.name}");

        Play3DSFXLocal(sfxName);
    }

    // ---------------- LOCAL 3D SOUND ----------------

    private void Play3DSFXLocal(string sfxName)
    {
        Debug.Log($"[{name}] TRY PLAY SFX : {sfxName}");

        if (string.IsNullOrEmpty(sfxName))
        {
            Debug.LogWarning($"[{name}] SFX NAME EMPTY");
            return;
        }

        if (_player3DAudioSource == null)
        {
            Debug.LogError($"[{name}] AudioSource NULL");
            return;
        }

        Debug.Log(
            $"[{name}] SOURCE INFO | " +
            $"source={_player3DAudioSource.name} | " +
            $"enabled={_player3DAudioSource.enabled} | " +
            $"active={_player3DAudioSource.gameObject.activeInHierarchy} | " +
            $"volume={_player3DAudioSource.volume} | " +
            $"mute={_player3DAudioSource.mute}"
        );

        if (AudioManager.Instance == null)
        {
            Debug.LogError($"[{name}] AudioManager NULL");
            return;
        }

        AudioClip clip = AudioManager.Instance.GetSFXClip(sfxName);

        if (clip == null)
        {
            Debug.LogError($"[{name}] SFX NOT FOUND : {sfxName}");
            return;
        }

        Debug.Log(
            $"[{name}] CLIP FOUND | " +
            $"clip={clip.name} | " +
            $"length={clip.length}"
        );

        _player3DAudioSource.PlayOneShot(clip);

        Debug.Log($"[{name}] PlayOneShot CALLED");

        // 1. 오디오 리스너 존재 확인
        if (FindObjectOfType<AudioListener>() == null)
        {
            Debug.LogError("[CRITICAL] 씬에 AudioListener가 없습니다! 소리가 들릴 수 없는 상태입니다.");
        }
        else
        {
            AudioListener listener = FindObjectOfType<AudioListener>();
            Debug.Log($" Listener Found on: {listener.gameObject.name} | Enabled: {listener.enabled}");
        }

        _player3DAudioSource.PlayOneShot(clip);

        // 2. PlayOneShot 직후 재생 상태 로그 (PlayOneShot은 isPlaying을 True로 만들지 않으므로 수동 체크 필요)
        Debug.Log($" [{name}] PlayOneShot executed for clip: {clip.name}");
    }

    // ---------------- SFX NAME ----------------

    private string GetItemActivationSFX(ItemType type)
    {
        return type switch
        {
            ItemType.WeightAcceleration => "ItemWeightActivate",
            ItemType.Shockwave => "ItemShockwaveActivate",
            ItemType.Magnetic => "ItemMagneticActivate",
            ItemType.Spiderweb => "Jump",
            _ => ""
        };
    }
}