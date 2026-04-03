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
	[Range(1f, 10f)] public float MoveMobileSensitive = 5f;

	private Vector2 _moveVector;
	private Vector3 _moveDir;
	private float _dropSpeed;

	private float _lastAngleY = 0f;

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
		_moveVector = _inputController.MovePoint * MoveMobileSensitive; //방향 구하기.
		_moveDir = new Vector3(Mathf.Clamp(_moveVector.x, -1, 1), 0, Mathf.Clamp(_moveVector.y, -1, 1)) * _moveSpeed; //방향 + 낙하

		//반영
		_rigidBody.linearVelocity = _moveDir + new Vector3(0, _dropSpeed, 0);

		// 3. 틸트(기울기) 회전 처리
		// 입력값(_moveVector)은 보통 -1 ~ 1 사이의 값입니다.

		// 앞뒤 조작(y)에 따라 X축(Pitch) 각도 계산
		// 방향키 위를 눌렀을 때 고개를 더 숙이게 하려면 + 기호를 사용합니다. (반대면 - 로 변경)
		float targetX = 90f + (_moveVector.y * 20f);

		// 좌우 조작(x)에 따라 Z축(Roll) 각도 계산
		// 유니티에서는 오른쪽으로 기울어질 때 Z값이 음수(-)가 되어야 자연스럽습니다.
		float targetZ = _moveVector.x * -20f;

		// Y축(바라보는 방향)은 0으로 고정하고, X와 Z축만 기울입니다.
		Quaternion targetRotation = Quaternion.Euler(targetX, 0f, targetZ);

		// 현재 상태에서 목표 기울기로 부드럽게 Slerp
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
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
