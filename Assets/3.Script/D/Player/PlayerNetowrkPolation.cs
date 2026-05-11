using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*
 네트워크 상에서, 플레이어들의 위치를 보간하기 위해 작성하는 스크립트.
- Interpolation (보간)
보간으로 랜덤하게 도착하는 패킷을 통하여 위치를 부드럽게 표현하기 위함.
하지만 단순히 이 방법으로는, 플레이어들의 위치가 실시간처럼 동기화되는게 아닌것처럼 보임.

- Extrapolation (외삽)
따라서 외삽을 통하여 외부의 플레이어들의 위치를 보간예측한다.
이 방법은 직진의 경우 확실하지만, 플레이어들이 좌우로 움직일 경우
키 입력은 예측의 영역 밖이므로 끊기는 것처럼 보일 수 있다.

- HardSnap
따라서 이를 HardSnap으로 어쩌구 한다는데, 일단 이건 더 봐야할듯.
 */

public class PlayerNetowrkPolation : NetworkBehaviour {
	public struct SyncState {
		public Vector3 position;
		public Quaternion rotation;
		public Vector3 velocity;
		public double time_stamp;
	}

	[Header("Sync Settings")]
	[Tooltip("초당 패킷 전송 횟수 (NetworkTransform의 SendRate와 유사")]
	public float send_interval = 0.033f;
	private float _lastSendTime;

	[Header("Extrapolation Settings")]
	[Tooltip("이 거리 이상 차이나면 Lerp를 무시하고 즉시 순간이동 (Hard Snap)")]
	public float hardsnap_threshold = 30.0f;
	[Tooltip("부드럽게 따라가는 보간 속도")]
	public float smooth_speed = 20;
	[Tooltip("외삽 제한 시간 (초). 너무 길면 예측 오차가 커짐")]
	public float max_extrapolation_time = 0.15f;

	[SyncVar(hook = nameof(OnStateChanged))]
	private SyncState _latestStateSync;

	private Rigidbody _rigidbody;

	private Vector3 _ghostPosition;

	[Header("SmoothDamp Settings")]
	[Tooltip("목표 위치에 도달하는 예상 시간 (작을수록 빠름, 추천: 0.05~0.1)")]
	public float smooth_time = 0.06f;
	// SmoothDamp 내부 캐싱용 변수 (인스펙터 노출 X)
	private Vector3 _currentVelocity;

	private void Awake() {
		TryGetComponent(out _rigidbody);
	}

	private void FixedUpdate() {
		if (isLocalPlayer) {
			// 로컬 플레이어 전송 로직 (물리 연산이므로 FixedUpdate 유지)
			if (Time.time - _lastSendTime > send_interval) {
				CmdSendState(transform.position, transform.rotation, _rigidbody.linearVelocity, NetworkTime.time);
				_lastSendTime = Time.time;
			}
		}
	}

	private void LateUpdate() {
		if (isServerOnly || isLocalPlayer || _latestStateSync.time_stamp == 0) return;

		//[핵심] 통신 지연(Ping)과 상관없이, 로컬 프레임(Time.deltaTime)에 맞춰 타겟을 직접 낙하시킴!
		//이렇게 하면 타겟 자체가 위아래로 떨리는 현상이 100% 차단됨.
		_ghostPosition.y += _latestStateSync.velocity.y * Time.deltaTime;

		// Hard Snap 체크
		if (Vector3.Distance(transform.position, _ghostPosition) > hardsnap_threshold) {
			transform.position = _ghostPosition;
			return;
		}

		// 부드럽게 추적 (제한 속도를 무한대(Mathf.Infinity)로 풀어주기)
		transform.position = Vector3.SmoothDamp(transform.position, _ghostPosition, ref _currentVelocity, smooth_time, Mathf.Infinity, Time.deltaTime);

		// 회전 동기화
		transform.rotation = Quaternion.Slerp(transform.rotation, _latestStateSync.rotation, Time.deltaTime * smooth_speed);
	}

	//channel = Channels.Unreliable
	//게임에서 사용하는 통신(네트워크)에서 TCP와 UDP가 있습니다.
	//TCP는 연결(connect)라는 함수가 있다. ACK가 있어서 응답이 있다.
	//UDP는 응답 없음.

	//TCP = Reliable / UDP = Unreliable
	//-> TCP는 느리다. 반응이 없기 때문

	//NGO가 되던 포톤이 되던 미러가되던 Unreliable UDP라는 알고리즘을 만든다.
	//UDP 자체에는 신뢰성이 없기 때문에, 신뢰성 검증하는걸 따로 만든다는것.

	//따라서 Unreliable = UDP인데, 신뢰성 검증이 추가된 버전.

	//소켓 알면 해결할게 많긴 한데.
	//전 팀이 UDP 사용해서 정보를 못받는 줄 알았지만, 사실 다른게 문제였을 가능성이 높을듯 하다.

	[Command(channel = Channels.Unreliable)]
	private void CmdSendState(Vector3 pos, Quaternion rot, Vector3 vel, double timeStamp) {
		//TODO : 서버 검증 로직 추가하기. (핵 방지)
		transform.position = pos;
		transform.rotation = rot;
		_latestStateSync = new SyncState {
			position = pos,
			rotation = rot,
			velocity = vel,
			time_stamp = timeStamp
		};
	}

	private void OnStateChanged(SyncState oldState, SyncState newState) {
		//새로운 데이터가 들어올 때마다, 한 번씩 타켓 위치를 갱신할 수 있고,
		//Update문에서 실시간 Time을 반영해 매 프레임 타겟을 갱신할 수도 있습니다.
		if (isLocalPlayer) return;

		// 패킷이 도착한 시점의 지연 시간 계산 (딱 한 번만)
		double latency = Math.Max(0, NetworkTime.time - newState.time_stamp);
		//float extrapolatedTime = Mathf.Min((float)latency, max_extrapolation_time);
		float extrapolatedTime = Mathf.Min((float)latency, max_extrapolation_time) + smooth_time;

		// X, Z는 최신 위치(보간) / Y는 지연된 시간만큼만 미래로 땡겨옴(외삽)
		_ghostPosition = new Vector3(
			newState.position.x,
			newState.position.y + (newState.velocity.y * extrapolatedTime),
			newState.position.z
		);
	}

	private void ApplyExtrapolationAndInterpolation() {
		//서버 최신 정보가 없으면 리턴
		if (_latestStateSync.time_stamp == 0) return;

		//지연 시간 계산 (Ping)
		double latency = NetworkTime.time - _latestStateSync.time_stamp;
		float extrapolatedTime = Mathf.Min((float)latency, max_extrapolation_time);

		// [핵심 1] 축 분리 (Axis Separation)
		// Y축(낙하): 중력의 영향을 받으므로 외삽(Extrapolation)을 적용하여 실시간 위치 예측
		float predictedY = _latestStateSync.position.y + (_latestStateSync.velocity.y * extrapolatedTime);

		// X, Z축(이동): 플레이어 입력으로 급변하므로 외삽을 배제하고 서버의 최신 확정 위치만 사용 (보간)
		float targetX = _latestStateSync.position.x;
		float targetZ = _latestStateSync.position.z;

		// 분리된 축을 결합한 최종 타겟 위치
		Vector3 targetPos = new Vector3(targetX, predictedY, targetZ);
		Quaternion targetRot = _latestStateSync.rotation;

		// Hard Snap (순간이동) : 오차가 너무 클 경우 강제 교정
		float distanceError = Vector3.Distance(transform.position, targetPos);
		if (distanceError > hardsnap_threshold) {
			transform.position = targetPos;
			transform.rotation = targetRot;
			return;
		}

		// [핵심 2] Jitter 해결을 위한 SmoothDamp 적용
		// Lerp 대신 SmoothDamp를 사용하여 패킷 딜레이 사이의 멈칫거림(Jitter)을 스프링처럼 부드럽게 연결
		//transform.position = Vector3.SmoothDamp(transform.position,	targetPos, ref _currentVelocity, smooth_time);
		// 변경 (MaxSpeed를 무한대로 열어주기)
		transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _currentVelocity, smooth_time, Mathf.Infinity, Time.deltaTime);

		// 회전은 플레이어 입력에 즉각 반응해야 하므로 기존의 Slerp 방식을 유지하거나 필요에 따라 조절
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * smooth_speed);
	}
}