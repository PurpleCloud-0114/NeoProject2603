using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItemController : MonoBehaviour {
	private PlayerCore _playerCore;

	public IUseable currentItem = null;

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
		currentItem = newItem;
	}

	public void UseItem() {
		if(currentItem != null) {
			currentItem.Use(gameObject);
			currentItem = null;
		}
	}
}
