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
	[SerializeField] private StageProgressUi _stageProgressUI;
	[SerializeField] private Option _option;

	[SerializeField] private TextMeshProUGUI _rankText;

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


	// ==========================================
	// [ 클라이언트 UI 바인드 ]
	// ==========================================
	[Client] public Button BindWingButton() => _wingButton;
	[Client] public Button BindItemButton() => _itemButton;
	[Client] public TextMeshProUGUI BindItemText() => _itemText;
	public void BindJoystick(Transform player) => _option.BindPlayer(player);



}
