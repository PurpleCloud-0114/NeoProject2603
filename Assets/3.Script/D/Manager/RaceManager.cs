using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum RaceState {
	Waiting,
	Countdown,
	Racing,
	Finished
}

public class RaceManager : NetworkBehaviour {
	public static RaceManager Instance = null;

	public bool isSinglePlay = false;

	//공통 데이터
	[SyncVar(hook = nameof(OnStateChanged))]
	public RaceState current_state_sync = RaceState.Waiting;

	[SyncVar] public double race_start_time_sync;

	[SyncVar] private int _playersReadyCount = 0;
	public int total_players => NetworkServer.connections.Count;

	[Header("도착 속도 판정 (Death)")]
	[SyncVar, SerializeField, Range(5f, 50f)] private float _deathOverSpeedSync = 30f;

	private List<NetworkIdentity> finishers = new List<NetworkIdentity>();
	
	public int START_MAX_PLAYER = 10;

	//----- 메서드
	private void Awake() {
		if (Instance == null) Instance = this;
		else Destroy(gameObject);
	}

	// ==========================================
	// [서버 영역] - 판정과 흐름 제어
	// ==========================================
	//[ServerCallback]
	private void Start() {
		if(isServer || isSinglePlay) StageManager.Instance.SetStage();
		if (isSinglePlay) RandomSpawner.Instance.SetObstacles();
	}

	[Server]
	public void StartCountdown() {
		//씬 로드 끝나고, 플레이들이 모두 스폰되면 호출.
		current_state_sync = RaceState.Countdown;

		//NetworkTime.time = 서버 시간
		//5초 뒤 출발 하는거. (나중에 수정)
		race_start_time_sync = NetworkTime.time + 5.0;

		//TODO - ClientRPC로 카운트다운 UI 넣을건가?
	}

	[ServerCallback]
	//TODO - 코루틴으로 바꿀지 고민.
	private void Update() {
		if(current_state_sync == RaceState.Countdown) {
			if(NetworkTime.time >= race_start_time_sync) {
				current_state_sync = RaceState.Racing;
				race_start_time_sync = NetworkTime.time;
			}
		}
	}

	[Server]
	//서버 수신 - 클라이언트 통과 정보 받기
	public void GetArriveResult(NetworkConnectionToClient sender, float impactSpeed, double finishTime) {
		//TODO - 순위 리스트 업데이트 및 결과 RPC 전송
		if (current_state_sync != RaceState.Racing) return;
		bool result = (impactSpeed > _deathOverSpeedSync) ? true : false;
		//나중에 결과 알려주기.


		if(!finishers.Contains(sender.identity)) {
			finishers.Add(sender.identity);
			if (finishers.Count >= total_players) {
				EndRace();
			}
			//들어온 시간 보고 순위 정렬?
		}
	}

	[Server]
	private void EndRace() {
		current_state_sync = RaceState.Finished;
	}

	// ==========================================
	// [클라이언트 영역] - 연출 및 입력 제어
	// ==========================================
	private void OnStateChanged(RaceState raceState, RaceState newState) {
		if (NetworkClient.localPlayer != null && NetworkClient.localPlayer.TryGetComponent(out PlayerCore playerCore)) {
			playerCore.UpdatePlayerStateByRace(newState);
		}
		switch (newState) {
			case RaceState.Waiting:
				//없음.
				break;
			case RaceState.Countdown:
				//UI 매니저한테 카운트다운 연출 지시?
				//New Input Ststem 액션 맵 비활성화 (Disable)
				break;
			case RaceState.Racing:
				//New Input Ststem 액션 맵 활성화 (Enable)
				break;
			case RaceState.Finished:
				//New Input Ststem 액션 맵 비활성화 (Disable)
				break;
		}
	}

	[Command(requiresAuthority = false)]
	public void CmdReportReady() {
		_playersReadyCount++;
		Debug.Log($"플레이어 준비 완료: {_playersReadyCount} / {total_players}");

		//if(_playersReadyCount >= total_players && current_state_sync == RaceState.Waiting) {
		if(_playersReadyCount >= START_MAX_PLAYER) { 
			StartCountdown();
		}
	}
}
