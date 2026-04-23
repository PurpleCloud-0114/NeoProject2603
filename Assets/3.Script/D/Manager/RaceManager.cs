using System;
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

public struct PlayerResult {
	public NetworkIdentity player;
	public double finishTime;
	public bool isDead;
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

	public int START_MAX_PLAYER = 10;


	// 참가자 리스트 (순위)
	public List<Transform> active_players = new List<Transform>();
	// 이전 순위 기록용 딕셔너리
	private Dictionary<Transform, int> _previousRanks = new Dictionary<Transform, int>();
	public event Action on_any_rank_changed;

	// 도착 순위
	//private List<PlayerResult> final_results = new List<PlayerResult>();
	private Dictionary<NetworkIdentity, PlayerResult> _roundResults = new Dictionary<NetworkIdentity, PlayerResult>();
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
		race_start_time_sync = NetworkTime.time + 4.0;

		//TODO - ClientRPC로 카운트다운 UI 넣을건가?
	}

	[ServerCallback]
	//TODO - 코루틴으로 바꿀지 고민.
	//-> 코루틴으로 하면, 정말 미세하게 어긋나는 경우가 있을 수 있다고 함.
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
		bool isDead = impactSpeed > _deathOverSpeedSync;

		_roundResults.Add(sender.identity,
			new PlayerResult {
				player = sender.identity,
				finishTime = finishTime,
				isDead = isDead
			}
		);

		ReceiveArriveResult(sender, isDead);

		if(_roundResults.Count >= active_players.Count) {
			EndRace();
		}
	}

	[TargetRpc]
	private void ReceiveArriveResult(NetworkConnectionToClient target, bool result) {
		UIManager.Instance.UpdateResultTextLog(result);
	}

	[Server]
	private void EndRace() {
		current_state_sync = RaceState.Finished;

		List<PlayerResult> sortedResults = new List<PlayerResult>(_roundResults.Values);
		sortedResults.Sort((a, b) => a.finishTime.CompareTo(b.finishTime));
		RpcShowFinalResult(sortedResults.ToArray());
	}

	[ClientRpc]
	private void RpcShowFinalResult(PlayerResult[] results) {
		UIManager.Instance.ShowFinalResult(results);
	}

	// ==========================================
	// [클라이언트 영역] - 연출 및 입력 제어
	// ==========================================
	private void OnStateChanged(RaceState oldState, RaceState newState) {
		PlayerState playerNewState = PlayerState.Wait;
		switch (newState) {
			case RaceState.Waiting:
				playerNewState = PlayerState.Wait;
				break;
			case RaceState.Racing:
				playerNewState = PlayerState.Falling;
				StartRankTracking();
				break;
			case RaceState.Finished:
				playerNewState = PlayerState.Finish;
				break;
		}
		if (NetworkClient.localPlayer != null && NetworkClient.localPlayer.TryGetComponent(out PlayerCore playerCore)) {
			playerCore.on_player_state_change_requested?.Invoke(playerNewState);
		}

	}

	[Command(requiresAuthority = false)]
	public void CmdReportReady() {
		_playersReadyCount++;
		Debug.Log($"플레이어 준비 완료: {_playersReadyCount} / {total_players}");

		if(_playersReadyCount >= total_players && current_state_sync == RaceState.Waiting) {
		//if(_playersReadyCount >= START_MAX_PLAYER) { 
			StartCountdown();
		}
	}

	//플레이어 생성시, PlayerCore에서 호출됨.
	public void RegisterPlayer(Transform player) {
		if (!active_players.Contains(player)) {
			active_players.Add(player);
		}
	}
	public void UnregisterPlayer(Transform player) {
		if (active_players.Contains(player)) {
			active_players.Remove(player);
			_previousRanks.Remove(player);
		}
	}

	private void StartRankTracking() {
		StartCoroutine(Co_TrackRankingRoutine());
	}

	private IEnumerator Co_TrackRankingRoutine() {
		WaitForSeconds wfs = new WaitForSeconds(0.2f);

		while (current_state_sync == RaceState.Racing) {
			if (active_players.Count > 1) {
				CalculateRanks();
			}
			yield return wfs;
		}
	}

	private void CalculateRanks() {
		bool isRankChangedThisTick = false;

		active_players.RemoveAll(p => p == null);   //null이 된 플레이어 리스트에서 제거 (튕긴 플레이어 예외처리)
		active_players.Sort((a, b) => a.position.y.CompareTo(b.position.y));    //Y값 기준 오름차순 정렬
		for (int i = 0; i < active_players.Count; i++) {
			Transform player = active_players[i];
			int currentRank = i + 1;

			//사전에 등록되지 않았거나, 순위가 달라졌을 경우.
			if (!_previousRanks.ContainsKey(player) || _previousRanks[player] != currentRank) {
				_previousRanks[player] = currentRank;
				isRankChangedThisTick = true;

				//TODO : 개별 유저 순위 UI 업데이트 필요하다면 여기서 호출.
				UIManager.Instance.UpdateRankUI();
			}
		}

		if (isRankChangedThisTick) {
			on_any_rank_changed?.Invoke();
		}
	}
}
