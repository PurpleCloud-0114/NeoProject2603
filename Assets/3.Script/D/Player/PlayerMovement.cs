using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using DG.Tweening;

public class PlayerMovement : NetworkBehaviour {
	public InputSystem _inputSystem;

	public PlayerState _playerState;

	private Rigidbody _rigidBody;

	public Vector3 VelocityTracker;

	[Header("조작 이동속도 조절")]
	[SerializeField, Range(0, 100)] public float _moveSpeed = 45f;              //임시 퍼블릭

	[Header("낙하 이동속도 조절")]
	[Range(1f, 200f)] public float _dropMaxSpeed = 100f;                        //임시 퍼블릭
	private float _dropSpeed;

	private Vector2 _moveVector;
	private Vector3 _moveDir;

	[Header("날개")]
	[SerializeField, Range(0, 500)] private float _dropWingSpeed = 30f;
	private float _wingTime;

	[Header("Mobile용 감도 조절")]
	[Range(1f, 10f)] public float MoveMobileSensitive = 1f;

	//----- 메서드

	private void Awake() {
		TryGetComponent(out _rigidBody);
		TryGetComponent(out _playerState);
	}

	private void Start() {
		if(isLocalPlayer) _inputSystem = FindAnyObjectByType<InputSystem>();
	}

	private void FixedUpdate() {
		if (!isLocalPlayer || _inputSystem == null) return;

		VelocityTracker = _rigidBody.linearVelocity;
		if(_playerState.state == State.Falling) Drop();
	}

	private void Update() {
		if (!isLocalPlayer || _inputSystem == null) return;

		_moveVector = _inputSystem.MovePoint * MoveMobileSensitive;
		_moveDir = new Vector3(Mathf.Clamp(_moveVector.x, -1, 1), 0, Mathf.Clamp(_moveVector.y, -1, 1));
	}

	private void Drop() {
		_dropSpeed = _rigidBody.linearVelocity.y;
		_dropSpeed = Mathf.Clamp(_dropSpeed, -1 * _dropMaxSpeed, 0);
		_rigidBody.linearVelocity = _moveDir * _moveSpeed + new Vector3(0, _dropSpeed, 0); //반영

		float targetX = 90f + (_moveDir.z * 35f);
		float targetZ = _moveDir.x * -35f;
		Quaternion targetRotation = Quaternion.Euler(targetX, 0f, targetZ);
		//transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 20f);
		_rigidBody.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 20f));
	}

	public void hitObstacle() {

	}

	public void SetDecreaseDropSpeedTimeOnWing() {
		//_dropSmoothOnWing = mapRedZone / _dropMaxSpeed * 1.5f; 
		_wingTime = (3f * StageManager.Instance.stage_data_sync.map_redzone) / (_dropMaxSpeed + 2f * _dropWingSpeed);
	}

	/* Ease.OutQuad의 수학적 접근.
	Distance = Time * (Vstart + 2 * Vtarget) / 3
	-> Time = 3 * Distance / (Vstart + 2 * Vtarget) 이 된다.
	즉, 변수를 대입한다면
	mapRedZone = _dropSmoothOnWing * (_DropMaxSpeed + 2f * _dropSpeedOnWing) / 3
	_dropSmoothOnWing = 3 * mapRedZone / (_DropMaxSpeed + 2f * _dropSpeedOnWing)
	 */
	public void OpenWing() {
		DOTween.To(() => _dropMaxSpeed, x => _dropMaxSpeed = x, _dropWingSpeed, _wingTime).SetEase(Ease.OutQuad);
	}
}
