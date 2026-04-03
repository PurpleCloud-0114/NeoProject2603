using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {
	//임시 ui
	[Header("임시")]
	[SerializeField] private GameObject _endPointPopUpUI;
	[SerializeField] private Text _endPointPopUpUITitle;
	[SerializeField] private Button _wingBtn;

	[Header("Input Controller")]
	[SerializeField] private InputController _inputController;

	private Rigidbody _rigidBody;

	[Header("속도 조절")]
	[SerializeField, Range(0,500)] public float _moveSpeed = 25f;				//임시 퍼블릭
	[SerializeField, Range(25f, 100f)] public float _dropLimitSpeed = 70f;   //임시 퍼블릭

	[Header("장애물 hit시 속도 감소량")]
	[SerializeField, Range(25, 100)] private float _decreaseDropSpeed = 30f;

	[Header("날개")]
	[SerializeField, Range(5, 50f)] private float _dropSpeedOnWing = 15f;
	[SerializeField, Range(1f, 10f)] private float _dropSmoothOnWing = 3f;

	[Header("도착 속도 판정 (Death)")]
	[SerializeField, Range(5f, 50f)] private float _deathSpeedCount = 30f;

	[Header("Mobile용 감도 조절")]
	[Range(0.1f, 3f)] public float MoveMobileSensitive = 1f;

	private Vector2 _moveVector;
	private Vector3 _moveDir;
	private float _dropSpeed;

	private bool _isWingOpened = false;
	private bool _isArriveEndPoint = false;
	
	public Vector3 VelocityTracker;
	
	//------

	private void Awake() {
		TryGetComponent(out _rigidBody);
	}

	private void Update() {
		VelocityTracker = _rigidBody.linearVelocity;
		if(!_isArriveEndPoint) {
			Drop();
		}
	}

	public void Initialize() {
		_wingBtn.interactable = false;
		_isWingOpened = false;
		_isArriveEndPoint = false;
		transform.position = new Vector3(0, StageSystem.Instance.stage_data.map_height + 100f, 0);
	}

	private void Drop() {
		//낙하 가속도도 구해야함.
		_dropSpeed = _rigidBody.linearVelocity.y;

		if (_isWingOpened) {
			//날개를 폈다면: 현재 떨어지던 엄청난 속도에서 -> 목표 글라이딩 속도(_glideDropSpeed)로 부드럽게 감속
			_dropSpeed = Mathf.Lerp(_dropSpeed, -1 * _dropSpeedOnWing, Time.deltaTime * _dropSmoothOnWing);
		}

		_dropSpeed = Mathf.Clamp(_dropSpeed, -1 * _dropLimitSpeed, 0);

		//방향 구하기.
		_moveVector = _inputController.MovePoint;

		//방향 + 낙하
		_moveDir = new Vector3(_moveVector.x, 0, _moveVector.y) * _moveSpeed;

		//반영
		_rigidBody.linearVelocity = _moveDir + new Vector3(0, _dropSpeed, 0);

		if (_moveDir.sqrMagnitude > 0.01f) {
			//Atan2를 사용하여 X, Z 평면에서의 이동 방향 각도를 구함 (라디안을 각도로 변환)
			float targetAngleY = Mathf.Atan2(_moveDir.x, _moveDir.z) * Mathf.Rad2Deg;

			//현재 X축 회전값 유지, Y축은 목표 방향, Z축은 0으로 설정한 목표 회전값
			Quaternion targetRotation = Quaternion.Euler(165, targetAngleY, 0f);
			//10f는 회전 속도입니다. 모바일 조작감에 맞춰 값을 조절해 보세요.
			//transform.rotation = targetRotation;
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
		}
	}

	private void OnCollisionEnter(Collision collision) {
		if(collision.transform.CompareTag("EndPoint")) {
			_isArriveEndPoint = true;
			Time.timeScale = 0f;

			float impactSpeed = Mathf.Abs(collision.relativeVelocity.y);
			Debug.Log(impactSpeed);
			if (impactSpeed > _deathSpeedCount) _endPointPopUpUITitle.text = "사망...";
			else _endPointPopUpUITitle.text = "도착 성공!";

			_endPointPopUpUI.SetActive(true);
			Timer.Instance.EndStopwatch();

			//TODO : 추후 서버한테 도착했으니 1등이라고 알리는 이벤트 메시지 추가.
		}
	}

	private void OnTriggerEnter(Collider other) {
		if(other.transform.CompareTag("Obstacle")) {
			_rigidBody.linearVelocity = _rigidBody.linearVelocity + (Vector3.up * _decreaseDropSpeed);
			Destroy(other.gameObject);
		}
		if(other.transform.CompareTag("DangerZone")) {
			ActivateWingBtn();
		}
	}

	private void ActivateWingBtn() {
		_wingBtn.interactable = true;
	}

	public void OnWingOpen() {
		//TODO : 쭉 미끄러지듯 낙하속도 감소. - Checked
		_isWingOpened = true;
	}
}
