using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugValue : MonoBehaviour {
	[SerializeField] private InputController _inputController;
	[SerializeField] private PlayerController _playerController;

	[SerializeField] private Text _moveSpeedText;
	[SerializeField] private Text _gyroPointText;
	[SerializeField] private Text _gyroVectorText;

	private void Update() {
		
		_moveSpeedText.text = $"이동 가속도 - x : {_playerController.VelocityTracker.x.ToString("F2")} y : {_playerController.VelocityTracker.y.ToString("F2")} z : {_playerController.VelocityTracker.z.ToString("F2")}";
		_gyroPointText.text = $"자이로 기준 - x : {_inputController.BasePoint.x.ToString("F2")} y : {_inputController.BasePoint.y.ToString("F2")}";
		_gyroVectorText.text = $"자이로 기준 - x : {_inputController.MovePoint.x.ToString("F2")} y : {_inputController.MovePoint.y.ToString("F2")}";
	}
}
