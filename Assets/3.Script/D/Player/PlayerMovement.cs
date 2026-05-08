using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using DG.Tweening;

public class PlayerMovement : NetworkBehaviour {
	private const string _SENSITIVITY_KEY = "GyroSensitivity";
	private PlayerInputSystem _playerInputSystem;
	private PlayerCore _playerCore;
	private Rigidbody _rigidBody;
	private MapSize _mapSize;
	private Vector2 _moveVector;
	private Vector3 _moveDir;

	//[SerializeField] private Vector3 _velocityTracker;

	[Header("조작 이동속도 조절")]
	[SerializeField, Range(0, 100)] public float _moveSpeed = 45f;              //임시 퍼블릭
	[Header("낙하 이동속도 조절")]
	[Range(1f, 200f)] public float drop_max_speed = 100f;
	[HideInInspector] public float base_drop_max_speed;
	[Header("날개")]
	[SerializeField, Range(0, 500)] private float _dropWingSpeed = 30f;
	[SerializeField] private float _wingTime;
	[Header("Mobile용 감도 조절")]
	[Range(2f, 10f)] public float MoveMobileSensitive = 6f;

	private Tween _speedControlSequence;
	private Tween _stunSequence;

	//----- 메서드

	private void Awake() {
		TryGetComponent(out _rigidBody);
		TryGetComponent(out _playerCore);

		base_drop_max_speed = drop_max_speed;
		MoveMobileSensitive = PlayerPrefs.GetFloat(_SENSITIVITY_KEY);
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

		if (RaceManager.Instance.isSinglePlay) StartFalling(PlayerState.Falling);
	}

	private void OnEnable() {
		_playerCore.on_player_state_change_requested += StartFalling;
		_playerCore.on_wing_button_clicked += CancleSequence;
		_playerCore.on_wing_button_clicked += OpenWing;
		_playerCore.on_max_drop_speed_change_requested += ApplySpeedChange;
		_playerCore.on_impulse_requested += ApplyImpulse;
		_playerCore.on_obstacle_hit += HitObstacle;
		_playerCore.on_redzone_entered += SetDecreaseDropSpeedTimeOnWing;
		_playerCore.on_stun_requested += ApplyStun;
		_playerCore.on_race_start += SetBasePoint;
	}
	private void OnDisable() {
		_playerCore.on_player_state_change_requested -= StartFalling;
		_playerCore.on_wing_button_clicked -= CancleSequence;
		_playerCore.on_wing_button_clicked -= OpenWing;
		_playerCore.on_max_drop_speed_change_requested -= ApplySpeedChange;
		_playerCore.on_impulse_requested -= ApplyImpulse;
		_playerCore.on_obstacle_hit -= HitObstacle;
		_playerCore.on_redzone_entered -= SetDecreaseDropSpeedTimeOnWing;
		_playerCore.on_stun_requested -= ApplyStun;
		_playerCore.on_race_start -= SetBasePoint;
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
		//_velocityTracker = _rigidBody.linearVelocity;
	}
	private void Update() {
		if ((!RaceManager.Instance.isSinglePlay && !isLocalPlayer) || _playerInputSystem == null) return;

		_moveVector = _playerInputSystem.MovePoint;
		if (!_playerInputSystem.is_joystick) _moveVector *= MoveMobileSensitive;
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
		//float targetX = 90f + (_moveDir.z * 35f);
		//float targetZ = _moveDir.x * -35f;
		//Quaternion targetRotation = Quaternion.Euler(targetX, 0f, targetZ);
		////transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 20f);
		//_rigidBody.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 20f));

		// 1. 기본적으로 90도 누워 있는 기본 회전값 (기준점)
		Quaternion baseRotation = Quaternion.Euler(90f, 0f, 0f);

		// 2. 상하 입력(_moveDir.z)에 따른 X축 회전 (앞뒤 굽히기)
		Quaternion xRotation = Quaternion.AngleAxis(_moveDir.z * 35f, Vector3.right);

		// 3. 좌우 입력(_moveDir.x)에 따른 Y축 회전 (옆으로 기울이기)
		// 누워 있는 상태에서는 로컬 Y축을 돌려야 '기울어짐'이 표현됩니다.
		Quaternion yRotation = Quaternion.AngleAxis(_moveDir.x * -35f, Vector3.up);

		// 4. 모든 회전을 조합 (순서 중요: 기본 상태 * 상하 * 좌우)
		Quaternion targetRotation = baseRotation * xRotation * yRotation;

		// 적용
		_rigidBody.MoveRotation(Quaternion.Slerp(_rigidBody.rotation, targetRotation, Time.fixedDeltaTime * 20f));
	}
	private void ClampPositionToMapBounds() {
		Vector3 currentPos = transform.position;
		Vector3 mapCetner = _mapSize.map_center;
		currentPos.y = mapCetner.y;
		float distanceFromCenter = Vector3.Distance(currentPos, mapCetner);

		//최저-25 | 최고-90 => 75를 백분율화.
		//높이 3000
		float percent = 100f;
		if (transform.position.y > 3000f) percent = 100f;
		else {
			percent = transform.position.y * 0.0003f;
		}
		float boundary = _mapSize.boundaryRadius + (75f * percent);
		if (distanceFromCenter > boundary) {
			Vector3 direction = (currentPos - mapCetner).normalized;
			Vector3 clampedPosition = mapCetner + (direction * boundary);
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
	private void SetBasePoint() {
		if (_playerInputSystem.is_joystick) _playerInputSystem.SetBasePoint(Vector2.zero);
		else _playerInputSystem.Calibrate();
	}

	private void HitObstacle() {
		Vector3 currentVelocity = _rigidBody.linearVelocity;
		float nextYVelocity = currentVelocity.y + 30f;
		currentVelocity.y = Mathf.Min(nextYVelocity, 0f);
		_rigidBody.linearVelocity = currentVelocity;
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
	private void ApplyStun(float duration) {
		_stunSequence?.Kill();
		_playerCore.on_state_effect_change_requested?.Invoke(StatusEffect.Stun);
		if (_playerInputSystem != null) {
			_playerInputSystem.DisableInputSystem();
		}
		_stunSequence = DOVirtual.DelayedCall(duration, () => {
			_playerCore.on_state_effect_change_requested?.Invoke(StatusEffect.None);
			if (_playerInputSystem != null) {
				_playerInputSystem.EnableInputSystem();
			}
			_stunSequence = null;
		});
	}
	private void CancleSequence() {
		_speedControlSequence?.Kill();
	}
}
