using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WingBtn : MonoBehaviour {
	[SerializeField] private PlayerController _player;

	public void OnBtnClick() {
		_player.OnWingOpen();
		if(TryGetComponent(out Button btn)) {
			btn.interactable = false;
		}
	}
}
