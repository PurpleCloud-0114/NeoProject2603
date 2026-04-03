using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Option : MonoBehaviour {
	[SerializeField] private PlayerController _playerController;
	[SerializeField] private InputController _inputController;

	[Header("Sensitive")]
	[SerializeField] private Slider _sensitiveSlider;

	[Header("JoyStick")]
	[SerializeField] private GameObject _joyStick;

	public void OnSensitiveValueChange() {
		_playerController.MoveMobileSensitive = _sensitiveSlider.value;
	}

	public void OnJoyStickModChange() {
		_joyStick.SetActive(!_joyStick.activeSelf);
		_inputController.OnGravitySensorToggle();
	}
}
