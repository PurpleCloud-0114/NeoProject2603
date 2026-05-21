using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
	public string name;
	public double finishTime;
	public bool isDead;
}

public struct TotalScoreResult {
	public string name;
	public int totalScore;
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
	// private Dictionary<Transform, int> _previousRanks = new Dictionary<Transform, int>();

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
	}

	[Server]
	public void StartCountdown() {
		//씬 로드 끝나고, 플레이들이 모두 스폰되면 호출.
		current_state_sync = RaceState.Countdown;

		//NetworkTime.time = 서버 시간
		//5초 뒤 출발 하는거. (나중에 수정)
		race_start_time_sync = NetworkTime.time + 0.1f;

		//TODO - ClientRPC로 카운트다운 UI 넣을건가?

		if(RoundManager.Instance.current_round_sync == 0) {
			InitAllPlayerScores();//모든 플레이어 Dictionary에 등록
		}
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
	public void GetArriveResult(NetworkConnectionToClient sender, string name, float impactSpeed, double finishTime) {
		//TODO - 순위 리스트 업데이트 및 결과 RPC 전송
		if (current_state_sync != RaceState.Racing) return;
		bool isDead = impactSpeed > _deathOverSpeedSync;

		if (!_roundResults.ContainsKey(sender.identity)) {
			_roundResults.Add(sender.identity,
				new PlayerResult {
					player = sender.identity,
					name = name,
					finishTime = finishTime,
					isDead = isDead
				}
			);	
		}
		ReceiveArriveResult(sender, isDead, finishTime);

		if (isDead)
		{
			if (sender.identity.TryGetComponent(out PlayerTrigger trigger))
			{
				trigger.PlayHitEffect(5);
			}
		}
	}

	[Server]
	public void EndRaceCheck() {
		if (_roundResults.Count >= total_players) {
			EndRace();
		}
	}

	[TargetRpc]
	private void ReceiveArriveResult(NetworkConnectionToClient target, bool isDead, double finishTime) {
		//플레이어에게 결과값에 대한 반응 처리.
		UIManager.Instance.SetResult(isDead, finishTime);
		if (!isDead) StageManager.Instance.ChangeFloorTrigger(true);
	}

	//------------[ 레이스 종료 ] -----------------
	//------------[ 레이스 종료 ] -----------------

	[Server]
	private void EndRace() {
		if (current_state_sync == RaceState.Finished) return;

		current_state_sync = RaceState.Finished;

		List<PlayerResult> sortedResults = new List<PlayerResult>(_roundResults.Values);

		sortedResults.Sort((a, b) => {
			// 생존자(false)는 0, 사망자(true)는 1로 취급됨
			int deadCompare = a.isDead.CompareTo(b.isDead);
			if (deadCompare != 0) return deadCompare;

			// 생존 여부가 같다면(둘 다 성공 or 둘 다 실패) 시간이 빠른 순
			return a.finishTime.CompareTo(b.finishTime);
		});

		List<int> roundScores = ScoreCalculate(sortedResults);

		//컴파일 에러 방지용 임시.
		Dictionary<NetworkIdentity, int> externalTotalScores = new Dictionary<NetworkIdentity, int>();


		int[] previousScores = new int[sortedResults.Count];
		for (int i = 0; i < sortedResults.Count; i++) {
			NetworkIdentity p = sortedResults[i].player;
			// 이번 라운드 점수가 더해지기 전의 기존 점수를 저장
			//previousScores[i] = SQLManager.Instance.player_score.GetPlayerScore(p);
			previousScores[i] = SQLManager.Instance.player_score.GetPlayerScore(sortedResults[i].name);
		}

		//플레이어 UI 점수 전송.
		RpcShowRoundResult(sortedResults.ToArray(), previousScores, roundScores.ToArray());
		StartCoroutine(Co_RpcShowScoreResult(sortedResults, roundScores));

		RoundManager.Instance.RoundChanger();
	}

	private IEnumerator Co_RpcShowScoreResult(List<PlayerResult> sortedResults, List<int> roundScores) {
		yield return new WaitForSeconds(4f);
		//스코어 업데이트 메서드 (점수 추가)
		for (int i = 0; i < sortedResults.Count; i++) {
			//SQLManager.Instance.player_score.AddPlayerScore(sortedResults[i].player, roundScores[i]);
			SQLManager.Instance.player_score.AddPlayerScore(sortedResults[i].name, roundScores[i]);
		}

		//현재 토탈 스코어 랭크 정렬
		List<TotalScoreResult> totalList = new List<TotalScoreResult>();
		foreach (var res in sortedResults) {
			//int total = SQLManager.Instance.player_score.GetPlayerScore(res.player);
			int total = SQLManager.Instance.player_score.GetPlayerScore(res.name);
			totalList.Add(new TotalScoreResult {
				name = res.name,
				totalScore = total
			});
		}

		totalList.Sort((a, b) => b.totalScore.CompareTo(a.totalScore));
		RpcShowScoreResult(totalList.ToArray());
	}

	//각자 유저들에게 결과창 보여주기.
	[ClientRpc]
	private void RpcShowRoundResult(PlayerResult[] results, int[] previousScores, int[] roundScores) {
		// UI에서 results[i]의 점수는 roundScores[i]로 매칭하여 출력
		UIManager.Instance.ShowRoundResult(results, previousScores, roundScores);
		UIManager.Instance.HideUIforFinish();
	}

	[ClientRpc]
	private void RpcShowScoreResult(TotalScoreResult[] totalResults) {
		// 높은 점수 순으로 정렬된 토탈 순위 배열 전달
		UIManager.Instance.ShowScoreResult(totalResults);
	}

	[Server]
	public void ResetRoundState()  // <<--- 추가
	{
		_roundResults.Clear();          // 도착 결과 초기화
		_playersReadyCount = 0;         // 준비 카운트 초기화
		current_state_sync = RaceState.Waiting;  // 레이스 상태 초기화
		//Debug.Log("[RaceManager] 라운드 상태 초기화 완료");
	}


	// ==========================================
	// [ 점수 처리 ]
	// ==========================================
	//1위 50 / 2위 35 / 3위 25 / 4위 15 / 5위 5
	//6위 0 / 7위 -5 / 8위 -10 / 9위 -20 / 10위 -35
	private List<int> ScoreCalculate(List<PlayerResult> sortedResult) {
		int[] scores_value = { 50, 35, 25, 15, 5, 0, -5, -10, -20, -35 };
		List<int> scores = new List<int>();

		for (int i = 0; i < sortedResult.Count; i++) {
			if (sortedResult[i].isDead) {
				scores.Add(scores_value[9]); // 사망자 점수 (-35)
			} else {
				// 인원수가 많아 인덱스를 초과하는 경우 방어 코드 (마지막 등수 점수로 고정)
				int scoreIndex = Mathf.Min(i, 9);
				scores.Add(scores_value[scoreIndex]);
			}
		}
		return scores;
	}

	//토탈 랭크 업데이트
	public void UpdateRatingScore() {
		//List<NetworkIdentity> sortedPlayers = 
		//	SQLManager.Instance.player_score.player_score_management.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();

		////List<NetworkIdentity> netIDkeys = new List<NetworkIdentity>();
		////foreach(NetworkIdentity key in SQLManager.Instance.player_score.player_score_management.Keys) {
		////	netIDkeys.Add(key);
		////}
		//Dictionary<NetworkIdentity, string> player_names = new Dictionary<NetworkIdentity, string>();
		//foreach (NetworkIdentity key in SQLManager.Instance.player_score.player_score_management.Keys) {
		//	if (_roundResults.TryGetValue(key, out var value)) {
		//		player_names.Add(key, value.name);
		//	}
		//}

		//int[] RatingScroes = { 50, 40, 35, 30, 25, 20, 15, 10, 5, 0 };
		//for(int i = 0; i < sortedPlayers.Count; i++) {
		//	if(player_names.TryGetValue(sortedPlayers[i], out string nickname)) {
		//		SQLManager.Instance.AddScore(nickname, RatingScroes[i]);
		//	}
		//}

		// Key가 이미 string(닉네임)이므로 복잡한 변환 없이 바로 정렬 및 추출 가능
		List<string> sortedPlayers =
			SQLManager.Instance.player_score.player_score_management.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();

		int[] RatingScroes = { 50, 40, 35, 30, 25, 20, 15, 10, 5, 0 };

		for (int i = 0; i < sortedPlayers.Count; i++) {
			// 인원이 많아도 배열의 마지막 등수 점수(0점)로 안전하게 고정
			int scoreIndex = Mathf.Min(i, RatingScroes.Length - 1);

			SQLManager.Instance.AddScore(sortedPlayers[i], RatingScroes[scoreIndex]);
		}
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
		//Debug.Log($"플레이어 준비 완료: {_playersReadyCount} / {total_players}");

		//Int로 카운트만 한다면 악의적으로 혼자서 여러번 보낼 수 있음., 해시셋을 써서 검증하는게 안전.
		//HashSet<NetworkConnection>

		if (_playersReadyCount >= total_players && current_state_sync == RaceState.Waiting) {
			//if(_playersReadyCount >= START_MAX_PLAYER) { 
			//StartCountdown();
			CutsceneController.Instance.PlayIntro();
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
			//_previousRanks.Remove(player);
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
	[Server]
	private void InitAllPlayerScores()
	{
		if (SQLManager.Instance == null)
		{
			Debug.LogError("[RaceManager] SQLManager.Instance가 null → 점수 초기화 실패");
			return;
		}

		SQLManager.Instance.player_score.player_score_management.Clear();

		foreach (var conn in NetworkServer.connections.Values) {
			if (conn == null || conn.identity == null) continue;

			NetworkIdentity player = conn.identity;

			//SQLManager.Instance.player_score.InitPlayerScore(player);

			//Debug.Log($"[RaceManager] 점수 초기화 등록: {player.name} | 초기값: 0");

			// [수정] player.name(프리팹 이름) 대신, 플레이어 스크립트에서 진짜 닉네임을 가져옵니다.
			if (player.TryGetComponent(out PlayerDataSync playerData)) {
				SQLManager.Instance.player_score.InitPlayerScore(playerData.SyncNickname);
				Debug.Log($"[RaceManager] 점수 초기화 등록: {playerData.SyncNickname} | 초기값: 0");
			}
		}

		Debug.Log($"[RaceManager] 전체 플레이어 점수 초기화 완료 | 등록 수: " +
				  $"{SQLManager.Instance.player_score.player_score_management.Count}");
	}
}
