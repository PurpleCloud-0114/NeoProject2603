using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerController : MonoBehaviour {
	//임시 ui
	[Header("임시")]
	[SerializeField] private GameObject _endPointPopUpUI;
	[SerializeField] private Text _endPointPopUpUITitle;
	[SerializeField] private Button _wingBtn;

	[Header("Input Controller")]
	[SerializeField] private InputController _inputController;

	private Rigidbody _rigidBody;

	[Header("조작 이동속도 조절")]
	[SerializeField, Range(0,500)] public float _moveSpeed = 25f;				//임시 퍼블릭

	[Header("낙하 이동속도 조절")]
	[SerializeField, Range(1f, 200f)] public float _dropMaxSpeed = 70f;   //임시 퍼블릭
	private float _SaveDropMaxSpeed = 70f;   //임시 퍼블릭

	[SerializeField, Range(25, 100)] private float _decreaseDropSpeedFromHittingObstacle = 30f;

	[SerializeField, Range(5, 50f)] private float _dropSpeedOnWing = 15f;
	[SerializeField] private float _dropSmoothOnWing;




	[Header("도착 속도 판정 (Death)")]
	[SerializeField, Range(5f, 50f)] private float _deathSpeedCount = 30f;

	[Header("Mobile용 감도 조절")]
	[Range(1f, 10f)] public float MoveMobileSensitive = 5f;

	private Vector2 _moveVector;
	private Vector3 _moveDir;
	private float _dropSpeed;

	private bool _isArriveEndPoint = false;
	
	public Vector3 VelocityTracker;
	
	//------

	private void Awake() {
		TryGetComponent(out _rigidBody);
	}

	private void Start() {
		_SaveDropMaxSpeed = _dropMaxSpeed;
	}

	private void Update() {
		VelocityTracker = _rigidBody.linearVelocity;
		if(!_isArriveEndPoint) {
			Drop();
		}
	}

	public void Initialize() {
		_wingBtn.interactable = false;
		_isArriveEndPoint = false;
		transform.position = new Vector3(0, StageSystem.Instance.stage_data.map_height + 100f, 0);
		_dropMaxSpeed = _SaveDropMaxSpeed;
	}

	private void Drop() {
		_dropSpeed = _rigidBody.linearVelocity.y;
		_dropSpeed = Mathf.Clamp(_dropSpeed, -1 * _dropMaxSpeed, 0);
		_moveVector = _inputController.MovePoint * MoveMobileSensitive; //방향 구하기.
		_moveDir = new Vector3(Mathf.Clamp(_moveVector.x, -1, 1), 0, Mathf.Clamp(_moveVector.y, -1, 1)) * _moveSpeed; //방향 + 낙하
		_rigidBody.linearVelocity = _moveDir + new Vector3(0, _dropSpeed, 0);           //반영

		float targetX = 90f + (_moveVector.y * 20f);
		float targetZ = _moveVector.x * -20f;
		Quaternion targetRotation = Quaternion.Euler(targetX, 0f, targetZ);
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
			_rigidBody.linearVelocity = _rigidBody.linearVelocity + (Vector3.up * _decreaseDropSpeedFromHittingObstacle);
			Destroy(other.gameObject);
		}
		if(other.transform.CompareTag("DangerZone")) {
			ActivateWingBtn();
		}
	}

	private void ActivateWingBtn() {
		_wingBtn.interactable = true;
	}

	public void SetDecreaseDropSpeedTimeOnWing(float mapRedZone) {
		//_dropSmoothOnWing = mapRedZone / _dropMaxSpeed * 1.5f; 
		_dropSmoothOnWing = (3f * mapRedZone) / (_dropMaxSpeed + 2f * _dropSpeedOnWing);
	}

	public void OnWingOpen() {
		//_dropMaxSpeed
		DOTween.To(() => _dropMaxSpeed, x => _dropMaxSpeed = x, _dropSpeedOnWing, _dropSmoothOnWing).SetEase(Ease.OutQuad);
		/*
		 Ease.OutQuad의 수학적 접근.
		Distance = Time * (Vstart + 2 * Vtarget) / 3
		-> Time = 3 * Distance / (Vstart + 2 * Vtarget) 이 된다.
		즉, 변수를 대입한다면
		mapRedZone = _dropSmoothOnWing * (_DropMaxSpeed + 2f * _dropSpeedOnWing) / 3
		_dropSmoothOnWing = 3 * mapRedZone / (_DropMaxSpeed + 2f * _dropSpeedOnWing)
		 */
	}
}
