using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RoundManager : NetworkBehaviour {
	public static RoundManager Instance = null;

	[Header("라운드 관리")]
	public int current_round_sync = 0;
	public int MAX_ROUND = 5;   //Const 임시 제거

	[Header("레이스 종료")]
	[SerializeField] private float returnDelay = 7.5f;
	private bool isSceneChanging = false;

	private void Awake() {
		if (Instance == null) {
			Instance = this;
			DontDestroyOnLoad(gameObject);
		} else if (Instance != this) {
			Destroy(gameObject);
		}
	}

	[Server]
	public void RoundChanger() {
		StartCoroutine(Co_ReturnToLobby());
	}

	private void OnDestroy() {
		if (Instance == this) {
			Instance = null;
		}
	}

	//결과창 7.5초
	private IEnumerator Co_ReturnToLobby() {
		if (isSceneChanging) yield break;
		isSceneChanging = true;

		Debug.Log($"시상식중...(7.5초 걸림) | 현재 라운드: {current_round_sync + 1} / {MAX_ROUND}");
		yield return new WaitForSeconds(returnDelay);

		current_round_sync++;  // <<--- 라운드 증가

		var roomManager = NetworkManager.singleton as NetworkRoomManager;
		if (roomManager == null) {
			//Debug.LogError("[RaceManager] RoomManager를 찾을 수 없습니다.");
			yield break;
		}

		if (current_round_sync < MAX_ROUND)  // <<--- 10라운드 미만 -> 게임 씬 재시작
		{
			Debug.Log($"[RaceManager] 라운드 {current_round_sync} / {MAX_ROUND} → 게임 씬 재시작");
			RaceManager.Instance.ResetRoundState();                                              // <<--- 상태 초기화

			isSceneChanging = false;

			roomManager.ServerChangeScene(roomManager.GameplayScene);      // <<--- 게임 씬 재시작
		} else                                 // <<--- 10라운드 완료 -> 로비 복귀
		  {
			//여기서 이제 점수 업데이트
			RaceManager.Instance.UpdateRatingScore();

			Debug.Log($"[RaceManager] {MAX_ROUND}라운드 완료 → 로비 복귀");
			current_round_sync = 0;                                        // <<--- 라운드 초기화
			roomManager.ServerChangeScene(roomManager.RoomScene);          // <<--- 로비 복귀

			if (NetworkServer.active) {
				NetworkServer.Destroy(gameObject);
			} else {
				Destroy(gameObject);
			}
		}
	}
}
