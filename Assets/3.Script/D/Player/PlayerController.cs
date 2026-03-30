using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
	[SerializeField] private InputController _inputController;

	private Rigidbody _rigidBody;

	[SerializeField, Range(0,100)] private float _moveSpeed;
	[SerializeField, Range(0.001f, 0.1f)] private float _moveSensitive;


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
		_moveValue = Mathf.Clamp01(_moveVector.magnitude);
//#if UNITY_EDITOR
//		_moveValue = Mathf.Clamp01(_moveVector.magnitude * _moveSensitive);
//#elif UNITY_ANDROID && !UNITY_EDITOR
//#endif

		//낙하 가속도도 구해야함.
		_dropSpeed = _rigidBody.linearVelocity.y;

		//방향 + 낙하
		_moveDir = new Vector3(_moveNomal.x, 0, _moveNomal.y) * _moveValue * _moveSpeed + new Vector3(0, _dropSpeed, 0);

		//반영
		_rigidBody.linearVelocity = _moveDir;
	}
}
