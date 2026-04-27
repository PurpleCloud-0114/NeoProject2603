using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerItemController : NetworkBehaviour {
	private PlayerCore _playerCore;

	public IUseable current_item = null;

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
		}
	}
}
