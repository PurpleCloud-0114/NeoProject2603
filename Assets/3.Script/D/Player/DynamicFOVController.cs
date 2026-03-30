using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicFOVController : MonoBehaviour {
	[SerializeField] private Camera _mainCamera;

	private Rigidbody _rigidBody;

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
		_stretchFOVvalue = _defaultFOVvalue + Mathf.Abs(_rigidBody.linearVelocity.y * 0.5f);
		_mainCamera.fieldOfView = Mathf.Clamp(_stretchFOVvalue, _defaultFOVvalue, _defaultFOVvalue + 30);
	}
}
