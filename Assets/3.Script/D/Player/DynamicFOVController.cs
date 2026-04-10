using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class DynamicFOVController : MonoBehaviour {
	private GameObject _player;

	private CinemachineCamera _mainCamera;
	private Rigidbody _rigidBody;

	[Header("카메라 FOV 증가 계수 조절")]
	[SerializeField, Range(0f,1f)] public float _extendCoefficient = 0.5f;  //임시 퍼블릭
	[Header("카메라 FOV 증가 제한")]
	[SerializeField, Range(60,180)] public float _limitFOVValue = 90f;   //임시 퍼블릭

	[Header("멀미 방지 (부드러움 조절)")]
	[SerializeField, Range(1f, 20f)] public float _smoothSpeed = 5f; //값이 작을수록 부드럽고 늦게 따라감

	private float _defaultFOVvalue;
	private float _stretchFOVvalue;

	private void Awake() {
		if (TryGetComponent(out _mainCamera)) {
			_defaultFOVvalue = _mainCamera.Lens.FieldOfView;
		}		
	}

	public void BindPlayer(GameObject player) { 
		_player = player;
		_player.TryGetComponent(out _rigidBody);
		_mainCamera.Follow = _player.transform;
		_mainCamera.LookAt = _player.transform;
	}

	private void Update() {
		if(_player != null) {
			ChangeFOV(); 
		}
	}

	private void ChangeFOV() {
		float targetFOV = _defaultFOVvalue + Mathf.Abs(_rigidBody.linearVelocity.y * _extendCoefficient);
		targetFOV = Mathf.Clamp(targetFOV, _defaultFOVvalue, _limitFOVValue);

		_mainCamera.Lens.FieldOfView = Mathf.Lerp(_mainCamera.Lens.FieldOfView, targetFOV, Time.deltaTime * _smoothSpeed);
	}
}
