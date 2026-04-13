using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Option : MonoBehaviour {
	[SerializeField] private PlayerMovement _playerMovement;
	[SerializeField] private PlayerInputSystem _playerInputSystem;

	[Header("Sensitive")]
	[SerializeField] private Slider _sensitiveSlider;

	[Header("JoyStick")]
	[SerializeField] private GameObject _joyStick;

	public void OnSensitiveValueChange() {
		_playerMovement.MoveMobileSensitive = _sensitiveSlider.value;
	}

	public void OnJoyStickModChange() {
		_joyStick.SetActive(!_joyStick.activeSelf);
		_playerInputSystem.OnGravitySensorToggle();
	}
}
