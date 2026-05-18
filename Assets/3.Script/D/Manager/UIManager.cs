using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class UIManager : MonoBehaviour {
	public static UIManager Instance = null;

	public int myRank = 1;

	[Header("Player Button Bind")]
	[SerializeField] private Button _wingButton;
	[SerializeField] private Button _itemButton;

	[Header("Player Text Bind")]
	[SerializeField] private TextMeshProUGUI _rankText;
	//[SerializeField] private TextMeshProUGUI _ResultTextLog;

	[Header("Player Image Bind")]
	[SerializeField] private Image _itemImage;

	[Header("대상 UI (배치 변경될 UI)")]
	[SerializeField] private GameObject _touchzone;
	[SerializeField] private RectTransform _wingButtons;
	[SerializeField] private RectTransform _itemButtons;

	[Header("위치 프리셋 (기준점 트랜스폼)")]
	[SerializeField] private RectTransform _lefttouchzone;
	[SerializeField] private RectTransform _righttouchzone;
	[Space]
	[SerializeField] private RectTransform _itemLeftRef;
	[SerializeField] private RectTransform _itemRightRef;
	[Space]
	[SerializeField] private RectTransform _wingLeftRef;
	[SerializeField] private RectTransform _wingRightRef;

	[Header("Game UI")]
	[SerializeField] private StageProgressUi _stageProgressUI;
	[SerializeField] private TimerUI _timerUI;
	[SerializeField] private GameObject _playUI;
	[SerializeField] private GameObject _specUI;

	[Header("자석 상태 알림 UI")]
	[SerializeField] private GameObject _magneticWarningUI; // 피격자용 (붉은색/경고)
	[SerializeField] private GameObject _magneticAttackUI;  // 공격자용 (푸른색/활성)

	[Header("개인 결과 UI")]
	[SerializeField] private GameObject _personalResultWindow;
	[SerializeField] private TextMeshProUGUI _personalTitleText;
	[SerializeField] private TextMeshProUGUI _personalResultText;
	[SerializeField] private TextMeshProUGUI _personalRecordText;

	private bool SetResultBool;
	private double SetResultTime;

	[Header("최종 결과 UI")]
	[SerializeField] private GameObject _finalResultWindow;    // 결과창 부모 오브젝트
	[SerializeField] private RectTransform _finalResultContainer; // RankPrefab이 생성될 부모 (Content)
	[SerializeField] private GameObject _finalRankPrefab;

	private void Awake() {
		if (Instance == null) Instance = this;
		else Destroy(gameObject);
	}

	private void Start() {
		ApplySettings();
	}

	public void CreatePlayerMarker(Transform playerTransform, bool isLocal) {
		_stageProgressUI.CreatePlayerMarker(playerTransform, isLocal);
	}

	// 저장된 데이터를 바탕으로 UI 레이아웃 및 입력 방식 적용
	private void ApplySettings() {
		// PlayerPrefs에서 프리셋 정보 로드 (0:기본, 1:왼손, 2:자이로, 3:자이로+왼손 등)
		int preset = PlayerPrefs.GetInt("ControlPreset", 0);
		bool isLeft = (preset == 1 || preset == 3);
		bool isGyro = (preset == 2 || preset == 3);

		if (_touchzone != null)
			SetLayout(_touchzone.GetComponent<RectTransform>(), isLeft ? _lefttouchzone : _righttouchzone, !isGyro);

		SetLayout(_itemButtons, isLeft ? _itemLeftRef : _itemRightRef, true);
		SetLayout(_wingButtons, isLeft ? _wingLeftRef : _wingRightRef, true);
	}

	// 레퍼런스(기준점)를 기반으로 대상 RectTransform의 좌표 및 앵커 복사
	private void SetLayout(RectTransform target, RectTransform reference, bool active) {
		if (target == null || reference == null) return;

		target.gameObject.SetActive(active);

		// 앵커, 피벗, 위치값을 기준점과 동일하게 일치시킴
		target.anchorMin = reference.anchorMin;
		target.anchorMax = reference.anchorMax;
		target.pivot = reference.pivot;
		target.anchoredPosition = reference.anchoredPosition;
	}

	public void UpdateMyRank(int newRank) {
		myRank = newRank;
		if (_rankText != null) {
			switch(myRank) {
				case 1:
					_rankText.text = $"{myRank}st";
					break;
				case 2:
					_rankText.text = $"{myRank}nd";
					break;
				case 3:
					_rankText.text = $"{myRank}rd";
					break;
				default:
					_rankText.text = $"{myRank}th";
					break;
			}
		}
	}


	// ==========================================
	// [ 도착 시 결과 집계 ]
	// ==========================================
	public void StopTimer() {
		_timerUI.isStop = true;
	}

	public void SetResult(bool isDead, double finishTime) {
		SetResultBool = isDead;
		SetResultTime = finishTime;
	}

	public void ShowPersonalResult() {
		HideUIforFinish();
		_personalResultWindow.SetActive(true);
		if(SetResultBool) {
			_personalTitleText.text = "탈출 실패...";
			_personalResultText.text = $"결과 : <color=red>사망</color>";
		} else {
			_personalTitleText.text = "탈출 성공!";
			_personalResultText.text = $"결과 : 생존";
		}

		TimeSpan time = TimeSpan.FromSeconds(SetResultTime);
		_personalRecordText.text = string.Format($"{time.Minutes:00}:{time.Seconds:00}:{time.Milliseconds / 10:00}");   
	}
	public void ShowFinalResult(PlayerResult[] results) {
		HideUIforEndRace();
		// 1. 기존에 생성된 리스트가 있다면 제거 (초기화)
		foreach (Transform child in _finalResultContainer) {
			Destroy(child.gameObject);
		}

		_finalResultWindow.SetActive(true);

		for (int i = 0; i < results.Length; i++) {
			// 2. 프리팹 생성 및 부모 설정
			GameObject go = Instantiate(_finalRankPrefab, _finalResultContainer);
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
	// [ 도착 및 관전 시 UI 활성화/비활성화 ]
	// ==========================================
	public void PlayerUISetActive(bool isTrue) {
		// 1. _playUI 체크
		if (_playUI != null) {
			_playUI.SetActive(isTrue);
		}

		// 2. _stageProgressUI 및 그 GameObject 체크
		if (_stageProgressUI != null && _stageProgressUI.gameObject != null) {
			_stageProgressUI.gameObject.SetActive(isTrue);
		}
	}
	public void HideUIforFinish() {
		_playUI.SetActive(false);
		_specUI.SetActive(false);
		_personalResultWindow.SetActive(false);
		_stageProgressUI.gameObject.SetActive(false);
	}
	public void ShowUIforSpectator() {
		_personalResultWindow.SetActive(false);
		_specUI.SetActive(true);
		_stageProgressUI.gameObject.SetActive(true);
	}
	public void HideUIforEndRace() {
		HideUIforFinish();
		_specUI.SetActive(false);
	}
	//---------- 자석 UI ------------
	public void ShowMagneticIndicator(bool isAttacker, float duration = 2.5f)
	{
		GameObject targetUI = isAttacker ? _magneticAttackUI : _magneticWarningUI;
		if (targetUI == null) return;

		CancelInvoke(nameof(HideAllMagneticUI));
		_magneticAttackUI?.SetActive(false);
		_magneticWarningUI?.SetActive(false);
		targetUI.SetActive(true);

		Invoke(nameof(HideAllMagneticUI), duration);
	}

	private void HideAllMagneticUI()
	{
		_magneticAttackUI?.SetActive(false);
		_magneticWarningUI?.SetActive(false);
	}

	// ==========================================
	// [ 클라이언트 UI 바인드 ]
	// ==========================================
	[Client] public Button BindWingButton() => _wingButton;
	[Client] public Button BindItemButton() => _itemButton;
	[Client] public Image BindItemImage() => _itemImage;
}
