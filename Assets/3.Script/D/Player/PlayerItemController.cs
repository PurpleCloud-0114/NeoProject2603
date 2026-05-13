using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerItemController : NetworkBehaviour {
	private PlayerCore _playerCore;

	public IUseable current_item = null;

	[SerializeField] private AudioSource _player3DAudioSource;

	private void Awake() {
		TryGetComponent(out _playerCore);
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
		if(current_item != null) {
			CmdUseItem(current_item.Type);
		}
		current_item = null;
	}

	[Command]
	private void CmdUseItem(ItemType itemType) {
		IUseable itemToUse = ItemManager.Instance.GetItemUseable(itemType);

		if (itemToUse != null) {
			// 서버에 있는 이 플레이어 객체(gameObject)를 대상으로 Use 로직 실행
			itemToUse.Use(gameObject);

			//아이템 이름에 따른 사운드 이름 매칭
			string sfxName = GetItemActivationSFX(itemType);
			if (!string.IsNullOrEmpty(sfxName))
			{
				RpcPlayItemSFX(sfxName);
			}
		}
	}

	// -------------- 3D SOUND ---------------------

	[ClientRpc]
	private void RpcPlayItemSFX(string sfxName)
	{
		// 모든 클라이언트의 해당 플레이어 위치에서 소리가 남
		AudioClip clip = AudioManager.Instance.GetSFXClip(sfxName);
		if (clip != null && _player3DAudioSource != null)
		{
			_player3DAudioSource.PlayOneShot(clip);
		}
	}

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
