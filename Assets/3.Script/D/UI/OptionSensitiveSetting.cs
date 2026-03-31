using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionSensitiveSetting : MonoBehaviour {
	[SerializeField] private PlayerController _playerController;

	[Space(30f)]
	[SerializeField] private Slider _sensitiveSlider;

	public void OnSensitiveValueChange() {
		_playerController.MoveMobileSensitive = _sensitiveSlider.value;
	}
}
