using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerMovement : NetworkBehaviour {
	private InputSystem _inputSystem;

	private Rigidbody _rigidBody;

	public Vector3 VelocityTracker;

	[Header("조작 이동속도 조절")]
	[SerializeField, Range(0, 100)] public float _moveSpeed = 45f;              //임시 퍼블릭

	[Header("낙하 이동속도 조절")]
	[Range(1f, 200f)] public float drop_max_speed = 100f;
	private float _dropSpeed;

	private Vector2 _moveVector;
	private Vector3 _moveDir;

	[Header("Mobile용 감도 조절")]
	[Range(1f, 10f)] public float MoveMobileSensitive = 1f;

	private bool _isCutScene = false;

	//----- 메서드

	private void Awake() {
		TryGetComponent(out _rigidBody);
		_inputSystem = FindAnyObjectByType<InputSystem>();
	}

	private void Update() {
		//if (!isLocalPlayer) return;

		VelocityTracker = _rigidBody.linearVelocity;
		if(!_isCutScene) {
			Drop();
		}
	}

	private void Drop() {
		_dropSpeed = _rigidBody.linearVelocity.y;
		_dropSpeed = Mathf.Clamp(_dropSpeed, -1 * drop_max_speed, 0);
		_moveVector = _inputSystem.MovePoint * MoveMobileSensitive; //방향 구하기.
		_moveDir = new Vector3(Mathf.Clamp(_moveVector.x, -1, 1), 0, Mathf.Clamp(_moveVector.y, -1, 1)); //방향
		_rigidBody.linearVelocity = _moveDir * _moveSpeed + new Vector3(0, _dropSpeed, 0); //반영

		float targetX = 90f + (_moveDir.z * 35f);
		float targetZ = _moveDir.x * -35f;
		Quaternion targetRotation = Quaternion.Euler(targetX, 0f, targetZ);
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 20f);
	}

	public void CutsceneTest(bool cutsceneState) {
		if(cutsceneState) {
			_isCutScene = true;
			_inputSystem.DisableInputSystem();
		} else {
			_isCutScene = false;
			_inputSystem.EnableInputSystem();
		}
	}
}
