using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItemController : MonoBehaviour {
	private PlayerUIController _playerUIController;

	public IUseable currentItem = null;

	private void Awake() {
		TryGetComponent(out _playerUIController);
	}

	public void GetItem(IUseable newItem) {
		currentItem = newItem;
		SetItemNameOnUI(currentItem.Name);
	}

	public void UseItem() {
		if(currentItem != null) {
			currentItem.Use(gameObject);
			currentItem = null;
			SetItemNameOnUI(string.Empty);
		}
	}

	private void SetItemNameOnUI(string name) => _playerUIController.SetItemNameOnUI(name);
}
