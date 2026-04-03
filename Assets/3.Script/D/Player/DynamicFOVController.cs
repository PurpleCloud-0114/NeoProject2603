using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicFOVController : MonoBehaviour {
	[SerializeField] private GameObject _player;
	
	private Camera _mainCamera;
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
		TryGetComponent(out _mainCamera);
		_player.TryGetComponent(out _rigidBody);
		_defaultFOVvalue = _mainCamera.fieldOfView;
	}

	private void Update() {
		TrackingPlayer();
		ChangeFOV(); 
	}

	private void TrackingPlayer() {
		transform.position = _player.transform.position + Vector3.up * 15f + Vector3.back;
	}

	private void ChangeFOV() {
		float targetFOV = _defaultFOVvalue + Mathf.Abs(_rigidBody.linearVelocity.y * _extendCoefficient);
		targetFOV = Mathf.Clamp(targetFOV, _defaultFOVvalue, _limitFOVValue);

		_mainCamera.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, targetFOV, Time.deltaTime * _smoothSpeed);
	}
}
