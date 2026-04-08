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

public class RoadManager : NetworkBehaviour {
	public static RoadManager Instance = null;

	//공통 데이터
	[SyncVar(hook = nameof(OnStateChanged))]
	public RaceState current_state_sync = RaceState.Waiting;

	[SyncVar] public double race_state_time_sync;

	[SyncVar] private int _playersReadyCount = 0;
	private int TotalPlayers => NetworkServer.connections.Count;

	private List<NetworkIdentity> finishers = new List<NetworkIdentity>();
	
	//----- 메서드
	private void Awake() {
		if (Instance == null) Instance = this;
		else Destroy(gameObject);
	}

	// ==========================================
	// [서버 영역] - 판정과 흐름 제어
	// ==========================================
	[Server]
	public void StartCountdown() {
		//씬 로드 끝나고, 플레이들이 모두 스폰되면 호출.
		current_state_sync = RaceState.Countdown;

		//NetworkTime.time = 서버 시간
		//5초 뒤 출발 하는거. (나중에 수정)
		race_state_time_sync = NetworkTime.time + 5.0;

		//TODO - ClientRPC로 카운트다운 UI 넣을건가?
	}

	[ServerCallback]
	//TODO - 코루틴으로 바꿀지 고민.
	private void Update() {
		if(current_state_sync == RaceState.Countdown) {
			if(NetworkTime.time >= race_state_time_sync) {
				current_state_sync = RaceState.Racing;
			}
		}
	}

	[Server]
	//서버 수신 - 클라이언트 통과 정보 받기
	public void GetArriveResult(bool result, NetworkIdentity player, double finishTime) {
		//TODO - 순위 리스트 업데이트 및 결과 RPC 전송
		if (current_state_sync != RaceState.Racing) return;

		if(!finishers.Contains(player)) {
			finishers.Add(player);

			if (finishers.Count >= TotalPlayers) {
				EndRace();
			}
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
		Debug.Log($"플레이어 준비 완료: {_playersReadyCount} / {TotalPlayers}");

		if(_playersReadyCount >= TotalPlayers && current_state_sync == RaceState.Waiting) {
			StartCountdown();
		}
	}
}
