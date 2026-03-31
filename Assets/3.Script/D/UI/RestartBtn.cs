using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartBtn : MonoBehaviour {
	[SerializeField] private GameObject _player;

	public void OnClickBtn() {
		_player.transform.position = new Vector3(0, 1000f, 0);
		if(_player.TryGetComponent(out PlayerController playerController)) {
			playerController.IsArriveEndPoint = false;
		}
	}
}
