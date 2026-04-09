using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItemController : MonoBehaviour {
	public IUseable currentItem = null;

	public void GetItem(IUseable newItem) {
		currentItem = newItem;
	}

	public void UseItem() {
		if(currentItem != null) {
			currentItem.Use(gameObject);
			currentItem = null;
		}
	}
}
