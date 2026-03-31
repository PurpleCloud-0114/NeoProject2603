using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicFOVController : MonoBehaviour {
	[SerializeField] private Camera _mainCamera;

	private Rigidbody _rigidBody;

	[Header("카메라 FOV 증가 계수 조절")]
	[SerializeField, Range(0f,1f)] public float _extendCoefficient = 0.5f;  //임시 퍼블릭
	[Header("카메라 FOV 증가 제한")]
	[SerializeField, Range(60,180)] public float _limitFOVValue = 90;	//임시 퍼블릭

	private float _defaultFOVvalue;
	private float _stretchFOVvalue;

	private void Awake() {
		TryGetComponent(out _rigidBody);
		_defaultFOVvalue = _mainCamera.fieldOfView;
	}

	private void Update() {
		ChangeFOV(); 
	}

	private void ChangeFOV() {
		_stretchFOVvalue = _defaultFOVvalue + Mathf.Abs(_rigidBody.linearVelocity.y * _extendCoefficient);
		_mainCamera.fieldOfView = Mathf.Clamp(_stretchFOVvalue, _defaultFOVvalue, _limitFOVValue);
	}
}
