using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
	[SerializeField] private InputController _inputController;

	private Rigidbody _rigidBody;

	[Header("속도 조절")]
	[SerializeField, Range(0,100)] public float _moveSpeed = 25f;				//임시 퍼블릭
	[SerializeField, Range(-100f, -25f)] public float _dropLimitSpeed = -70f;	//임시 퍼블릭

	[Header("Debug용 감도 조절")]
	[SerializeField, Range(0.0001f, 0.01f)] private float _moveDebugSensitive = 0.001f;

	[Header("Mobile용 감도 조절")]
	[Range(0.1f, 1.9f)] public float MoveMobileSensitive = 1f;

	private Vector2 _moveVector;
	private Vector3 _moveDir;
	private Vector2 _moveNomal;
	private float _moveValue;
	private float _dropSpeed;

	[Header("VelocityTracker")]
	[SerializeField] private Vector3 _velocity;
	
	//------

	private void Awake() {
		TryGetComponent(out _rigidBody);
	}

	private void Update() {
		_velocity = _rigidBody.linearVelocity;
		Move();
	}

	private void Move() {
		//방향 구하기.
		_moveVector = _inputController.move_point - _inputController.base_point;
		_moveNomal = _moveVector.normalized;

		//방향에 따른 속도 구하기. (조금 기울이면 속도 느리게, 많이 기울이면 최대치 제한 속도로.)
		//+ 허용거리 비율 조절
#if UNITY_EDITOR
		_moveValue = Mathf.Clamp01(_moveVector.magnitude * _moveDebugSensitive);
#elif UNITY_ANDROID && !UNITY_EDITOR
		_moveValue = Mathf.Clamp01(_moveVector.magnitude * MoveMobileSensitive);
#endif

		//낙하 가속도도 구해야함.
		_dropSpeed = _rigidBody.linearVelocity.y;
		_dropSpeed = Mathf.Clamp(_dropSpeed, _dropLimitSpeed, 0);

		//방향 + 낙하
		_moveDir = new Vector3(_moveNomal.x, 0, _moveNomal.y) * _moveValue * _moveSpeed + new Vector3(0, _dropSpeed, 0);

		//반영
		_rigidBody.linearVelocity = _moveDir;
	}
}
