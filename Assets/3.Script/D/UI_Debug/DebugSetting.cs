using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugSetting : MonoBehaviour {
	[SerializeField] private PlayerController _playerController;
	[SerializeField] private DynamicFOVController _dynamicFOVController;

	[Space(30f)]
	[SerializeField] private Slider _debugMoveSpeed;
	[SerializeField] private Slider _debugDropSpeed;
	[SerializeField] private Slider _debugFOVcoefficient;
	[SerializeField] private Slider _debugFOVlimit;

	[Space(30f)]
	[SerializeField] private Text _debugMoveSpeedText;
	[SerializeField] private Text _debugDropSpeedText;
	[SerializeField] private Text _debugFOVcoefficientText;
	[SerializeField] private Text _debugFOVlimitText;

	private void Start() {
		_debugMoveSpeedText.text = _debugMoveSpeed.value.ToString();
		_debugDropSpeedText.text = _debugDropSpeed.value.ToString();
		_debugFOVcoefficientText.text = _debugFOVcoefficient.value.ToString();
		_debugFOVlimitText.text = _debugFOVlimit.value.ToString();
	}

	public void OnMoveSpeedValueChange() {
		_playerController._moveSpeed = _debugMoveSpeed.value;
		_debugMoveSpeedText.text = _debugMoveSpeed.value.ToString();
	}
	public void OnDropSpeedValueChange() {
		_playerController._dropLimitSpeed = _debugDropSpeed.value * -1;
		_debugDropSpeedText.text = _debugDropSpeed.value.ToString();
	}
	public void OnFOVcoefficientValueChange() {
		_dynamicFOVController._extendCoefficient = _debugFOVcoefficient.value;
		_debugFOVcoefficientText.text = _debugFOVcoefficient.value.ToString();
	}
	public void OnFOVlimitValueChange() {
		_dynamicFOVController._limitFOVValue = _debugFOVlimit.value;
		_debugFOVlimitText.text = _debugFOVlimit.value.ToString();
	}
}
