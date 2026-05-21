using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using DG.Tweening;

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

	[SerializeField] private RectTransform _warningText;
	[Header("Punch Settings")]
	[SerializeField] private float punchDuration = 0.4f;
	[SerializeField] private Vector3 punchScale = new Vector3(0.2f, 0.2f, 0); // 기존 크기에서 얼마나 더 커질지

	[Header("Sway Settings")]
	[SerializeField] private float swayAngle = 10f; // 좌우 기울기 각도
	[SerializeField] private float swayDuration = 2f; // 한 세트 걸리는 시간


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
	private float blinkInterval = 0.15f;
	private WaitForSeconds blinkWfs;
	private Coroutine blinkCoroutine;
	private Sequence activeSequence;

	[Header("최종 결과 UI")]
	[SerializeField] private GameObject _finalResultWindow;    // 결과창 부모 오브젝트
	[SerializeField] private RectTransform _finalResultContainer; // RankPrefab이 생성될 부모 (Content)
	
	[SerializeField] private GameObject _rountRankPrefab;
	[SerializeField] private GameObject _totalRankPrefab;



	[Header("컷신용 UI")]
	[SerializeField] private TextMeshProUGUI uiText;
	[SerializeField] private RectTransform rectTransform;

	[Header("설정값")]
	[SerializeField] private float fadeInDuration = 0.5f;
	[SerializeField] private float waitTimeA = 3.33f;
	[SerializeField] private float pullDownDistance = 30f; // 아래로 튕기는 거리
	[SerializeField] private float pullDownDuration = 0.15f;
	[SerializeField] private float rollUpDuration = 0.4f;

	private Vector2 originalPosition;
	private Sequence rollScreenSequence; // 필드로 선언




	private void Awake() {
		if (Instance == null) Instance = this;
		else Destroy(gameObject);

		originalPosition = rectTransform.anchoredPosition;
	}

	private void Start() {
		blinkWfs = new WaitForSeconds(blinkInterval);
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

	[ClientRpc]
	public void PlayTextEffect() {
		// 기존 패턴과 동일하게 필드로 Kill
		if (rollScreenSequence != null) {
			rollScreenSequence.Kill();
			rollScreenSequence = null;
		}

		uiText.text = $"{RoundManager.Instance.current_round_sync + 1} Round";
		rectTransform.anchoredPosition = originalPosition;


		Color c = uiText.color;
		c.a = 0f;
		uiText.color = c;

		rollScreenSequence = DOTween.Sequence();
		rollScreenSequence
			.Append(uiText.DOColor(new Color(c.r, c.g, c.b, 1f), fadeInDuration))
			.AppendInterval(waitTimeA)
			.Append(rectTransform.DOAnchorPosY(originalPosition.y - pullDownDistance, pullDownDuration).SetEase(Ease.InBack))
			.Append(rectTransform.DOAnchorPosY(originalPosition.y + 1000f, rollUpDuration).SetEase(Ease.InQuad))
			.Join(uiText.DOColor(new Color(c.r, c.g, c.b, 0f), rollUpDuration))
			.OnComplete(() => rollScreenSequence = null); // 완료 후 자동 정리
	}



	public void ToggleWingButtons(bool isTrue) {
		_wingButtons.gameObject.SetActive(isTrue);
	}
	public void StartBlinking() {
		if (blinkCoroutine != null) return;
		blinkCoroutine = StartCoroutine(BlinkRoutine());
	}
	public void StopBlinking() {
		if (blinkCoroutine != null) {
			StopCoroutine(blinkCoroutine);
			blinkCoroutine = null;
		}

		if (_wingButtons != null) {
			ToggleWingButtons(true); // 항상 켜진 상태로 복구
		}
	}
	private IEnumerator BlinkRoutine() {
		if (_wingButtons == null) yield break;

		while (true) {
			// 현재 상태의 반대로 토글
			ToggleWingButtons(!_wingButtons.gameObject.activeSelf);
			yield return blinkWfs;
		}
	}
	public void PlayIntroSequence() {
		if (_warningText == null) return;

		// 기존 트윈이 돌고 있다면 초기화
		KillSequence();

		// 시작 상태 세팅 (크기 0, 회전 0)
		_warningText.localScale = Vector3.zero;
		_warningText.localRotation = Quaternion.identity;
		_warningText.gameObject.SetActive(true);

		// 시퀀스 생성
		activeSequence = DOTween.Sequence();

		// [단계 1] 크기 0에서 1로 커지며 펀치 효과 (부르르 떨리는 연출)
		activeSequence.Append(_warningText.DOScale(Vector3.one, punchDuration).From(Vector3.zero));
		activeSequence.Append(_warningText.DOPunchScale(punchScale, punchDuration, vibrato: 5, elasticity: 0.5f));

		// [단계 2] 펀치 끝나고 무한 좌우 시소 흔들기
		activeSequence.AppendCallback(() => {
			_warningText.localRotation = Quaternion.Euler(0, 0, -swayAngle);
			_warningText.DORotate(new Vector3(0, 0, swayAngle), swayDuration)
					 .SetLoops(-1, LoopType.Yoyo)
					 .SetEase(Ease.InOutSine)
					 .SetId(_warningText); // 나중에 끄기 편하게 ID 부여
		});
	}
	public void PlayOutroSequence() {
		if (_warningText == null) return;

		// 기존 흔들림 및 시퀀스 정지
		KillSequence();

		// 크기 0으로 줄어들고 비활성화
		_warningText.DOScale(Vector3.zero, 0.3f)
				 .SetEase(Ease.InBack) // 살짝 커졌다가 쏙 사라지는 느낌
				 .OnComplete(() => _warningText.gameObject.SetActive(false));
	}
	private void KillSequence() {
		if (activeSequence != null) {
			activeSequence.Kill();
			activeSequence = null;
		}
		DOTween.Kill(_warningText); // 해당 UI에 걸린 회전 트윈 제거
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
	public void ShowRoundResult(PlayerResult[] results, int[] previousScores, int[] roundScores) {
		HideUIforEndRace();
		// 1. 기존에 생성된 리스트가 있다면 제거 (초기화)
		foreach (Transform child in _finalResultContainer) {
			Destroy(child.gameObject);
		}

		_finalResultWindow.SetActive(true);

		for (int i = 0; i < results.Length; i++) {
			// 2. 프리팹 생성 및 부모 설정
			GameObject go = Instantiate(_rountRankPrefab, _finalResultContainer);
			RectTransform rect = go.GetComponent<RectTransform>();

			// 3. 위치 배치 (위에서부터 150 간격으로 하단 배치)
			// anchoredPosition의 Y값을 -150 * i 로 설정하여 아래로 나열
			rect.anchoredPosition = new Vector2(0, -160 * i);

			// 4. 텍스트 데이터 바인딩
			var rankTexts = go.GetComponentsInChildren<TextMeshProUGUI>();

			// 프리팹 구조에 따른 순서 (Rank, Name, Time)
			// 인덱스는 하이어라키 순서에 따라 다를 수 있으니 확인 필요
			foreach (var tmp in rankTexts) {
				if (tmp.name == "Rank") {
					if(results[i].isDead) {
						tmp.color = Color.red;
						tmp.text = "DEAD";
					} else {
						tmp.text = (i + 1).ToString();
					}
				} else if (tmp.name == "Name") {
					// NetworkIdentity를 통해 해당 오브젝트의 이름을 가져옴
					tmp.text = results[i].name;
				} else if (tmp.name == "Time") {
					// 사망자(Retire) 처리
					if (results[i].isDead) {
						tmp.text = "--:--.--";
					} else {
						// 시간을 "00:00.00" 형식으로 포맷팅
						TimeSpan time = TimeSpan.FromSeconds(results[i].finishTime);
						tmp.text = string.Format($"{time.Minutes:00}:{time.Seconds:00}:{time.Milliseconds / 10:00}");
					}
				} else if (tmp.name == "Score") {
					int previousScore = previousScores[i];
					int roundScore = roundScores[i];
					if (roundScore > 0) {
						tmp.text = $"{previousScore} <color=green>+ {roundScore}</color>";
					} else if (roundScore < 0) {
						tmp.text = $"{previousScore} <color=red>- {Mathf.Abs(roundScore)}</color>";
					} else {
						tmp.text = $"{previousScore} + {roundScore}";
					}
				}
			}
		}
	}
	public void ShowScoreResult(TotalScoreResult[] totalResults) {
		// 1. 기존에 생성된 리스트가 있다면 제거 (초기화)
		foreach (Transform child in _finalResultContainer) {
			Destroy(child.gameObject);
		}

		_finalResultWindow.SetActive(true);
		for (int i = 0; i < totalResults.Length; i++) {
			// 2. 프리팹 생성 및 부모 설정
			GameObject go = Instantiate(_totalRankPrefab, _finalResultContainer);
			RectTransform rect = go.GetComponent<RectTransform>();

			// 3. 위치 배치 (위에서부터 150 간격으로 하단 배치)
			// anchoredPosition의 Y값을 -150 * i 로 설정하여 아래로 나열
			rect.anchoredPosition = new Vector2(0, -160 * i);

			// 4. 텍스트 데이터 바인딩
			var rankTexts = go.GetComponentsInChildren<TextMeshProUGUI>();

			// 프리팹 구조에 따른 순서 (Rank, Name, Time)
			// 인덱스는 하이어라키 순서에 따라 다를 수 있으니 확인 필요
			foreach (var tmp in rankTexts) {
				if (tmp.name == "Rank") {
					tmp.text = (i+1).ToString();
				} else if (tmp.name == "Name") {
					tmp.text = totalResults[i].name;
				} else if (tmp.name == "Score") {
					tmp.text = totalResults[i].totalScore.ToString();
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
