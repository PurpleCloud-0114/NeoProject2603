using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using DG.Tweening;

public class PlayerMovement : NetworkBehaviour {
	private PlayerInputSystem _playerInputSystem;
	private PlayerCore _playerCore;
	private Rigidbody _rigidBody;
	private MapSize _mapSize;
	private Vector2 _moveVector;
	private Vector3 _moveDir;

	[SerializeField] private Vector3 _velocityTracker;

	[Header("조작 이동속도 조절")]
	[SerializeField, Range(0, 100)] public float _moveSpeed = 45f;              //임시 퍼블릭
	[Header("낙하 이동속도 조절")]
	[Range(1f, 200f)] public float drop_max_speed = 100f;
	[HideInInspector] public float base_drop_max_speed;
	[Header("날개")]
	[SerializeField, Range(0, 500)] private float _dropWingSpeed = 30f;
	private float _wingTime;
	[Header("Mobile용 감도 조절")]
	[Range(1f, 10f)] public float MoveMobileSensitive = 1f;

	private Tween _speedControlSequence;

	//----- 메서드

	private void Awake() {
		TryGetComponent(out _rigidBody);
		TryGetComponent(out _playerCore);

		base_drop_max_speed = drop_max_speed;
	}
	private void Start() {
		_rigidBody.isKinematic = true;

		if (!RaceManager.Instance.isSinglePlay && !isLocalPlayer) {
			this.enabled = false;
			return;
		}
		if (!_playerCore.is_dummy) {
			_playerInputSystem = FindAnyObjectByType<PlayerInputSystem>();
		}

		_mapSize = StageManager.Instance.map_size;

		if(RaceManager.Instance.isSinglePlay) StartFalling(PlayerState.Falling);
	}

	private void OnEnable() {
		_playerCore.on_player_state_change_requested += StartFalling;
		_playerCore.on_wing_button_clicked += OpenWing;
		_playerCore.on_max_drop_speed_change_requested += ApplySpeedChange;
		_playerCore.on_impulse_requested += ApplyImpulse;
	}
	private void OnDisable() {
		_playerCore.on_player_state_change_requested -= StartFalling;
		_playerCore.on_wing_button_clicked -= OpenWing;
		_playerCore.on_max_drop_speed_change_requested -= ApplySpeedChange;
		_playerCore.on_impulse_requested -= ApplyImpulse;
	}

	private void FixedUpdate() {
		if ((!RaceManager.Instance.isSinglePlay && !isLocalPlayer) || _playerInputSystem == null) return;
		if (_playerCore.player_state == PlayerState.Falling) {
			LimitDropSpeed();
			if (_playerCore.status_effect != StatusEffect.Stun) {
				Move();
				Rotate();
			}
		}

		ClampPositionToMapBounds();
		_velocityTracker = _rigidBody.linearVelocity;
	}
	private void Update() {
		if ((!RaceManager.Instance.isSinglePlay && !isLocalPlayer) || _playerInputSystem == null) return;

		_moveVector = _playerInputSystem.MovePoint * MoveMobileSensitive;
		_moveDir = new Vector3(Mathf.Clamp(_moveVector.x, -1, 1), 0, Mathf.Clamp(_moveVector.y, -1, 1));
	}

	private void StartFalling(PlayerState playerState) {
		if (isLocalPlayer) {
			if (playerState == PlayerState.Falling) {
				_rigidBody.isKinematic = false;
			} else {
				_rigidBody.isKinematic = true;
			}
		}
	}

	private void Move() {
		Vector3 currentVelocity = _rigidBody.linearVelocity;
		Vector3 targetVel = _moveDir * _moveSpeed;
		Vector3 velocityDiff = targetVel - new Vector3(currentVelocity.x, 0, currentVelocity.z);
		_rigidBody.AddForce(velocityDiff, ForceMode.VelocityChange);

		//_dropSpeed = _rigidBody.linearVelocity.y;
		//_dropSpeed = Mathf.Clamp(_dropSpeed, -1 * drop_max_speed, 0);
		//_rigidBody.linearVelocity = _moveDir * _moveSpeed + new Vector3(0, _dropSpeed, 0); //반영
	}
	private void LimitDropSpeed() {
		Vector3 currentVelocity = _rigidBody.linearVelocity;
		if (currentVelocity.y < -drop_max_speed) {
			_rigidBody.linearVelocity = new Vector3(_rigidBody.linearVelocity.x,
													-drop_max_speed,
													_rigidBody.linearVelocity.z);
		}
	}
	private void Rotate() {
		// 회전
		float targetX = 90f + (_moveDir.z * 35f);
		float targetZ = _moveDir.x * -35f;
		Quaternion targetRotation = Quaternion.Euler(targetX, 0f, targetZ);
		//transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 20f);
		_rigidBody.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 20f));
	}
	private void ClampPositionToMapBounds() {
		//Vector3 currentPos = transform.position;
		//Vector3 mapCenter = _mapSize.map_center;

		//// Y축 제외한 평면 거리 계산
		//Vector2 flatPos = new Vector2(currentPos.x, currentPos.z);
		//Vector2 flatCenter = new Vector2(mapCenter.x, mapCenter.z);
		//float distance = Vector2.Distance(flatPos, flatCenter);

		//if (distance > _mapSize.boundaryRadius) {
		//	// 1. 위치 보정 (떨림 방지를 위해 현재 위치를 경계선에 딱 붙임)
		//	Vector2 dir = (flatPos - flatCenter).normalized;
		//	Vector3 clampedPos = new Vector3(
		//		flatCenter.x + dir.x * _mapSize.boundaryRadius,
		//		currentPos.y,
		//		flatCenter.y + dir.y * _mapSize.boundaryRadius
		//	);
		//	_rigidBody.position = clampedPos; // MovePosition 대신 직접 대입 시도

		//	// 2. 속도 제어 (중요: 바깥으로 나가는 운동 에너지만 0으로 만듦)
		//	Vector3 velocity = _rigidBody.linearVelocity;
		//	Vector3 outDir = new Vector3(dir.x, 0, dir.y);

		//	float dot = Vector3.Dot(velocity, outDir);
		//	if (dot > 0) {
		//		// 바깥 방향 속도 성분을 뺌
		//		_rigidBody.linearVelocity -= outDir * dot;
		//	}
		//}

		Vector3 currentPos = transform.position;
		Vector3 mapCetner = _mapSize.map_center;
		currentPos.y = mapCetner.y;
		float distanceFromCenter = Vector3.Distance(currentPos, mapCetner);
		if (distanceFromCenter > _mapSize.boundaryRadius) {
			Vector3 direction = (currentPos - mapCetner).normalized;
			Vector3 clampedPosition = mapCetner + (direction * _mapSize.boundaryRadius);
			clampedPosition.y = transform.position.y;
			_rigidBody.MovePosition(clampedPosition);
			_rigidBody.linearVelocity = new Vector3(0, _rigidBody.linearVelocity.y, 0);
		}
	}

	/* Ease.OutQuad의 수학적 접근.
	Distance = Time * (Vstart + 2 * Vtarget) / 3
	-> Time = 3 * Distance / (Vstart + 2 * Vtarget) 이 된다.
	즉, 변수를 대입한다면
	mapRedZone = _dropSmoothOnWing * (drop_max_speed + 2f * _dropSpeedOnWing) / 3
	_dropSmoothOnWing = 3 * mapRedZone / (drop_max_speed + 2f * _dropSpeedOnWing)
	 */
	public void SetDecreaseDropSpeedTimeOnWing() {
		//_dropSmoothOnWing = mapRedZone / drop_max_speed * 1.5f; 
		_wingTime = (3f * StageManager.Instance.stage_data_sync.map_redzone) / (drop_max_speed + 2f * _dropWingSpeed);
	}
	private void OpenWing() {
		DOTween.To(() => drop_max_speed, x => drop_max_speed = x, _dropWingSpeed, _wingTime).SetEase(Ease.OutQuad);
	}

	private void ApplySpeedChange(float targetSpeed, float duration, float recoveryTime, StatusEffect statusEffect) {
		// 플레이어의 조작 제한 및 속도 조절 방식을 코루틴으로 하려고 했으나,
		// 만약 중첩되어 효과가 적용될 경우, 두번째 효과는 무시될 가능성이 높다.
		// 모바일 환경에서는 코루틴을 중단 실행과 같은 행위를 반복한다면 GC작동이 자주되어 성능이 떨어짐.
		// -> 이를 해결하기 위해서 DOTween 사용

		_speedControlSequence?.Kill();
		drop_max_speed = targetSpeed;
		if (statusEffect != StatusEffect.None) _playerCore.on_state_effect_change_requested?.Invoke(statusEffect);

		Sequence seq = DOTween.Sequence();
		seq.AppendInterval(duration);

		if (recoveryTime > 0f) {
			seq.Append(DOTween.To(() => drop_max_speed, x => drop_max_speed = x, base_drop_max_speed, recoveryTime)
			   .SetEase(Ease.OutQuad));
		} else {
			seq.AppendCallback(() => drop_max_speed = base_drop_max_speed);
		}

		seq.OnComplete(() => {
			if (statusEffect != StatusEffect.None) _playerCore.on_state_effect_change_requested?.Invoke(StatusEffect.None);
			_speedControlSequence = null;
		});

		_speedControlSequence = seq;
	}
	private void ApplyImpulse(Vector3 force) {
		_rigidBody.AddForce(force, ForceMode.VelocityChange);
	}
}
