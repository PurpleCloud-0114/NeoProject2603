using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerItemController : NetworkBehaviour {
	private PlayerCore _playerCore;

	public IUseable current_item = null;

    [Header("3D Audio")]
    [SerializeField] private AudioSource _player3DAudioSource;

    private void Awake() {
        TryGetComponent(out _playerCore);

        if (_player3DAudioSource == null) {
            _player3DAudioSource = GetComponentInChildren<AudioSource>();
        }
    }

    private void OnEnable() {
		_playerCore.on_item_acquired += GetItem;
		_playerCore.on_item_button_clicked += UseItem;
	}
	private void OnDisable() {
		_playerCore.on_item_acquired -= GetItem;
		_playerCore.on_item_button_clicked -= UseItem;
	}

	private void GetItem(IUseable newItem) {
		current_item = newItem;
	}

    public void UseItem() {
        if (current_item != null) {
            string sfxName = GetItemActivationSFX(current_item.Type);

            // 본인 즉시 재생
            Play3DSFXLocal(sfxName);

            // 서버 아이템 사용 요청
            CmdUseItem(current_item.Type, sfxName);
        }

        current_item = null;
    }

    [Command]
    private void CmdUseItem(ItemType itemType, string sfxName) {
        IUseable itemToUse = ItemManager.Instance.GetItemUseable(itemType);

        if (itemToUse != null) {
            itemToUse.Use(gameObject);
        }

        // 다른 클라이언트에게 사운드 전파
        RpcPlayItemSFX(sfxName, netId);
    }

    [ClientRpc]
    private void RpcPlayItemSFX(string sfxName, uint senderNetId) {
        // 본인은 이미 로컬 재생했으므로 제외
        if (NetworkClient.localPlayer != null &&
            NetworkClient.localPlayer.netId == senderNetId) {
            return;
        }

        Play3DSFXLocal(sfxName);
    }

    // ---------------- LOCAL 3D SOUND ----------------

    private void Play3DSFXLocal(string sfxName) {
        if (string.IsNullOrEmpty(sfxName))
            return;

        if (_player3DAudioSource == null) {
            Debug.LogWarning($"[{name}] AudioSource NULL");
            return;
        }

        if (AudioManager.Instance == null) {
            Debug.LogWarning($"[{name}] AudioManager NULL");
            return;
        }

        AudioClip clip = AudioManager.Instance.GetSFXClip(sfxName);

        if (clip == null) {
            Debug.LogWarning($"[{name}] SFX NOT FOUND : {sfxName}");
            return;
        }

        _player3DAudioSource.PlayOneShot(clip);
    }

    // ---------------- SFX NAME ----------------

    private string GetItemActivationSFX(ItemType type) {
        return type switch {
            ItemType.WeightAcceleration => "ItemWeightActivate",
            ItemType.Shockwave => "ItemShockwaveActivate",
            ItemType.Magnetic => "ItemMagneticActivate",
            ItemType.Spiderweb => "Jump",
            _ => ""
        };
    }
}