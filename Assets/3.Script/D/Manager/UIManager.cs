using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class UIManager : MonoBehaviour {
	public static UIManager Instance = null;


	[SerializeField] private Button _wingButton;
	[SerializeField] private Button _itemButton;
	
	[SerializeField] private TextMeshProUGUI _itemText;
	[SerializeField] private TextMeshProUGUI _rankText;
	[SerializeField] private TextMeshProUGUI _ResultTextLog;

	[SerializeField] private StageProgressUi _stageProgressUI;
	[SerializeField] private Option _option;
	[SerializeField] private TimerUI _timerUI;

	[Header("최종 결과 UI")]
	[SerializeField] private GameObject _resultWindow;    // 결과창 부모 오브젝트
	[SerializeField] private RectTransform _resultContainer; // RankPrefab이 생성될 부모 (Content)
	[SerializeField] private GameObject _rankPrefab;

	private void Awake() {
		if (Instance == null) Instance = this;
		else Destroy(gameObject);
	}

	private void OnEnable() {
		RaceManager.Instance.on_any_rank_changed += UpdateRankUI;
	}

	private void OnDisable() {
		RaceManager.Instance.on_any_rank_changed -= UpdateRankUI;
	}

	/*
	 TODO List

	 [ 인게임 ]
	 * 1. 순위 UI
	 * 2. 시간 UI
	 * 3. Progress UI
	 * 4. Wing 버튼
	 * 5. 아이템 버튼
	 * 6. 조이스틱
	 * 
	 [ 팝업 ]
	 * 1. 게임 종료 시 - 결과 창
	 * 2. 라운드 결과
	 * 3. 다시하기?	 
	  
	 [ 메서드 기능 ] 
	 레드존 
	 */

	public void CreatePlayerMarker(Transform playerTransform, bool isLocal) {
		_stageProgressUI.CreatePlayerMarker(playerTransform, isLocal);
	}

	public void UpdateRankUI() {
		// 1. 미러에서 제공하는 '내 로컬 플레이어'의 Transform을 즉시 가져옵니다.
		// 아직 스폰 전이거나 로컬 플레이어가 없으면 안전하게 리턴
		if (NetworkClient.localPlayer == null) return;
		Transform myTransform = NetworkClient.localPlayer.transform;

		// 2. RaceManager의 정렬된 리스트를 가져옵니다.
		List<Transform> sortedList = RaceManager.Instance.active_players;

		// 3. 리스트에서 내 캐릭터(로컬)가 몇 번째인지 찾습니다.
		int myRank = sortedList.IndexOf(myTransform) + 1;

		// 4. 내 화면의 순위 텍스트 갱신
		if (_rankText != null) {
			_rankText.text = $"{myRank} / {sortedList.Count}";
		}
	}

	public void UpdateResultTextLog(bool result) {
		if(result) _ResultTextLog.text = "성공";
		else _ResultTextLog.text = "실패";
	}

	public void StopTimer() {
		_timerUI.isStop = true;
	}

	public void ShowFinalResult(PlayerResult[] results) {
		// 1. 기존에 생성된 리스트가 있다면 제거 (초기화)
		foreach (Transform child in _resultContainer) {
			Destroy(child.gameObject);
		}

		_resultWindow.SetActive(true);

		for (int i = 0; i < results.Length; i++) {
			// 2. 프리팹 생성 및 부모 설정
			GameObject go = Instantiate(_rankPrefab, _resultContainer);
			RectTransform rect = go.GetComponent<RectTransform>();

			// 3. 위치 배치 (위에서부터 150 간격으로 하단 배치)
			// anchoredPosition의 Y값을 -150 * i 로 설정하여 아래로 나열
			rect.anchoredPosition = new Vector2(0, -150 * i);

			// 4. 텍스트 데이터 바인딩
			var rankTexts = go.GetComponentsInChildren<TextMeshProUGUI>();

			// 프리팹 구조에 따른 순서 (Rank, Name, Time)
			// 인덱스는 하이어라키 순서에 따라 다를 수 있으니 확인 필요
			foreach (var tmp in rankTexts) {
				if (tmp.name == "Rank") {
					if(results[i].isDead) {
						tmp.color = Color.red;
						tmp.text = "사망";
					} else {
						tmp.text = (i + 1).ToString();
					}
				} else if (tmp.name == "Name") {
					// NetworkIdentity를 통해 해당 오브젝트의 이름을 가져옴
					tmp.text = "플레이어";
				} else if (tmp.name == "Time") {
					// 사망자(Retire) 처리
					if (results[i].isDead) {
						tmp.text = "--:--.--";
					} else {
						// 시간을 "00:00.00" 형식으로 포맷팅
						TimeSpan time = TimeSpan.FromSeconds(results[i].finishTime);
						tmp.text = string.Format($"{time.Minutes:00}:{time.Seconds:00}:{time.Milliseconds / 10:00}");
					}
				}
			}
		}
	}

	// ==========================================
	// [ 클라이언트 UI 바인드 ]
	// ==========================================
	[Client] public Button BindWingButton() => _wingButton;
	[Client] public Button BindItemButton() => _itemButton;
	[Client] public TextMeshProUGUI BindItemText() => _itemText;
	public void BindJoystick(Transform player) => _option.BindPlayer(player);



}
