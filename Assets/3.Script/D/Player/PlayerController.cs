using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
	//임시 ui
	[Header("임시")]
	[SerializeField] private GameObject _EndPointPopUpUI;

	[Header("Input Controller")]
	[SerializeField] private InputController _inputController;

	private Rigidbody _rigidBody;

	[Header("속도 조절")]
	[SerializeField, Range(0,500)] public float _moveSpeed = 25f;				//임시 퍼블릭
	[SerializeField, Range(-100f, -25f)] public float _dropLimitSpeed = -70f;	//임시 퍼블릭

	[Header("Debug용 감도 조절")]
	[SerializeField, Range(0.001f, 0.01f)] private float _moveDebugSensitive = 0.001f;

	[Header("Mobile용 감도 조절")]
	[Range(0.1f, 3f)] public float MoveMobileSensitive = 1f;

	private Vector2 _moveVector;
	private Vector3 _moveDir;
	private Vector2 _moveNomal;
	private float _moveValue;
	private float _dropSpeed;

	public bool IsArriveEndPoint = false;

	//디버그
	[Space(30f), Header("VelocityTracker")]
	public Vector3 Velocity;
	
	//------

	private void Awake() {
		TryGetComponent(out _rigidBody);
	}

	private void Update() {
		Velocity = _rigidBody.linearVelocity;
		if(!IsArriveEndPoint) {
			Move();
		}
	}

	private void Move() {
		

		//낙하 가속도도 구해야함.
		_dropSpeed = _rigidBody.linearVelocity.y;
		_dropSpeed = Mathf.Clamp(_dropSpeed, _dropLimitSpeed, 0);
#if UNITY_EDITOR
		//방향 구하기.
		_moveVector = _inputController.MovePoint - _inputController.BasePoint;
		_moveNomal = _moveVector.normalized;

		//방향에 따른 속도 구하기. (조금 기울이면 속도 느리게, 많이 기울이면 최대치 제한 속도로.)
		//+ 허용거리 비율 조절
		_moveValue = Mathf.Clamp01(_moveVector.magnitude * _moveDebugSensitive);

		//방향 + 낙하
		_moveDir = new Vector3(_moveNomal.x, 0, _moveNomal.y) * _moveValue * _moveSpeed + new Vector3(0, _dropSpeed, 0);
#elif UNITY_ANDROID && !UNITY_EDITOR
		//방향 구하기.
		_moveVector = _inputController.MovePoint;
		_moveNomal = _moveVector.normalized;

		//방향 + 낙하
		_moveDir = new Vector3(_moveVector.x, 0, _moveVector.y) * _moveSpeed + new Vector3(0, _dropSpeed, 0);
#endif

		//반영
		_rigidBody.linearVelocity = _moveDir;
	}

	private void OnCollisionEnter(Collision collision) {
		if(collision.transform.CompareTag("EndPoint")) {
			IsArriveEndPoint = true;
			Time.timeScale = 0f;
			_EndPointPopUpUI.SetActive(true);
			//TODO : 추후 서버한테 도착했으니 1등이라고 알리는 이벤트 메시지 추가.
		}
	}
}
