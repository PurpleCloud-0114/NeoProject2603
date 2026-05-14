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

	// 도착 순위
	//private List<PlayerResult> final_results = new List<PlayerResult>();
	private Dictionary<NetworkIdentity, PlayerResult> _roundResults = new Dictionary<NetworkIdentity, PlayerResult>();

	[Header("레이스 종료")]
	[SerializeField] private float returnDelay = 7.5f;

	[Header("라운드 관리")]
	[SyncVar] public int current_round_sync = 0;
	public const int MAX_ROUND = 10;

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
				StartRankTracking();
			}
		}
	}

	[Server]
	//서버 수신 - 클라이언트 통과 정보 받기
	public void GetArriveResult(NetworkConnectionToClient sender, float impactSpeed, double finishTime) {
		//TODO - 순위 리스트 업데이트 및 결과 RPC 전송
		if (current_state_sync != RaceState.Racing) return;
		bool isDead = impactSpeed > _deathOverSpeedSync;

		if (!_roundResults.ContainsKey(sender.identity)) {
			_roundResults.Add(sender.identity,
				new PlayerResult {
					player = sender.identity,
					finishTime = finishTime,
					isDead = isDead
				}
			);	
		}
		ReceiveArriveResult(sender, isDead, finishTime);
	}

	[Server]
	public void EndRaceCheck() {
		if (_roundResults.Count >= total_players) {
			EndRace();
		}
	}

	[TargetRpc]
	private void ReceiveArriveResult(NetworkConnectionToClient target, bool isDead, double finishTime) {
		UIManager.Instance.SetResult(isDead, finishTime);
		if (!isDead) StageManager.Instance.ChangeFloorTrigger(true);
	}

	//------------[ 레이스 종료 ] -----------------
	//------------[ 레이스 종료 ] -----------------

	[Server]
	private void EndRace() {
		current_state_sync = RaceState.Finished;
		List<PlayerResult> sortedResults = new List<PlayerResult>(_roundResults.Values);
		sortedResults.Sort((a, b) => {
			// 생존자(false)는 0, 사망자(true)는 1로 취급됨
			int deadCompare = a.isDead.CompareTo(b.isDead);
			if (deadCompare != 0) return deadCompare;

			// 생존 여부가 같다면(둘 다 성공 or 둘 다 실패) 시간이 빠른 순
			return a.finishTime.CompareTo(b.finishTime);
		});

		RpcShowFinalResult(sortedResults.ToArray());

		StartReturnToLobby();
	}

	//각자 유저들에게 결과창 보여주기.
	[ClientRpc]
	private void RpcShowFinalResult(PlayerResult[] results) {
		UIManager.Instance.ShowFinalResult(results);
		UIManager.Instance.HideUIforFinish();
	}

	[Server]
	private void StartReturnToLobby() {
		StartCoroutine(Co_ReturnToLobby());
	}

	//결과창 7.5초
	private IEnumerator Co_ReturnToLobby()
	{
		Debug.Log($"시상식중...(7.5초 걸림) | 현재 라운드: {current_round_sync + 1} / {MAX_ROUND}");
		yield return new WaitForSeconds(returnDelay);

		current_round_sync++;  // <<--- 라운드 증가

		var roomManager = NetworkManager.singleton as NetworkRoomManager;
		if (roomManager == null)
		{
			Debug.LogError("[RaceManager] RoomManager를 찾을 수 없습니다.");
			yield break;
		}

		if (current_round_sync < MAX_ROUND)  // <<--- 10라운드 미만 -> 게임 씬 재시작
		{
			Debug.Log($"[RaceManager] 라운드 {current_round_sync} / {MAX_ROUND} → 게임 씬 재시작");
			ResetRoundState();                                              // <<--- 상태 초기화
			roomManager.ServerChangeScene(roomManager.GameplayScene);      // <<--- 게임 씬 재시작
		}
		else                                 // <<--- 10라운드 완료 -> 로비 복귀
		{
			Debug.Log($"[RaceManager] {MAX_ROUND}라운드 완료 → 로비 복귀");
			current_round_sync = 0;                                        // <<--- 라운드 초기화
			roomManager.ServerChangeScene(roomManager.RoomScene);          // <<--- 로비 복귀
		}
	}
	[Server]
	private void ResetRoundState()  // <<--- 추가
	{
		_roundResults.Clear();          // 도착 결과 초기화
		_playersReadyCount = 0;         // 준비 카운트 초기화
		current_state_sync = RaceState.Waiting;  // 레이스 상태 초기화
		Debug.Log("[RaceManager] 라운드 상태 초기화 완료");
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
		if (!isServer) return;
		if (!active_players.Contains(player)) {
			active_players.Add(player);
		}
	}
	public void UnregisterPlayer(Transform player) {
		if (!isServer) return;
		if (active_players.Contains(player)) {
			active_players.Remove(player);
			_previousRanks.Remove(player);
		}
	}

	private void StartRankTracking() {
		StartCoroutine(Co_TrackRankingRoutine());
	}

	private IEnumerator Co_TrackRankingRoutine() {
		WaitForSeconds wfs = new WaitForSeconds(0.1f); // 0.01초는 너무 잦아 성능에 부하를 줄 수 있습니다.

		do {
			if (active_players.Count > 1) {
				CalculateRanks();
			}
			yield return wfs;
		} while (current_state_sync == RaceState.Racing);
	}

	private void CalculateRanks() {
		if (!isServer) return;

		active_players.RemoveAll(p => p == null);
		active_players.Sort((a, b) => a.position.y.CompareTo(b.position.y));

		for (int i = 0; i < active_players.Count; i++) {
			int rank = i + 1;
			if (active_players[i].TryGetComponent(out PlayerCore pc)) {
				// 서버에서 각 클라이언트의 PlayerCore에 등수 업데이트 명령
				pc.TargetUpdateRank(pc.connectionToClient, rank);
			}
		}
	}
}
