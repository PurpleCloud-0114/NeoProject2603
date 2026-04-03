using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartBtn : MonoBehaviour {
	[SerializeField] private PlayerController _player;

	public void OnClickBtn() {
		_player.Initialize();
	}
}
