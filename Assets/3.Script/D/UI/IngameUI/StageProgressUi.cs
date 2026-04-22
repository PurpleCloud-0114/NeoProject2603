using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class StageProgressUi : NetworkBehaviour {
	[Header("맵	데이터")]
	[SerializeField] private RectTransform _redZone;
	[SerializeField] private float _mapHeight;

	[Header("플레이어 UI 마커")]
	[SerializeField] private GameObject _playerMarker;


	private Dictionary<Transform, RectTransform> MappingData = new Dictionary<Transform, RectTransform>();

	private void OnEnable() {
		RaceManager.Instance.on_any_rank_changed += UpdateMarkerSorting;
	}

	private void OnDisable() {
		RaceManager.Instance.on_any_rank_changed -= UpdateMarkerSorting;
	}


	public void Initialize() {
		_mapHeight = StageManager.Instance.stage_data_sync.map_height;
		float _uiBarHeight = 0f;
		if (TryGetComponent(out RectTransform transform)) {
			_uiBarHeight = transform.sizeDelta.y;
		}

		float uiRatio = _uiBarHeight / _mapHeight;
		float calibrateRedZoneY = StageManager.Instance.stage_data_sync.map_redzone * uiRatio;
		float calibrateRedZoneHeight = StageManager.Instance.stage_data_sync.map_redzone_height * uiRatio;

		_redZone.anchoredPosition = new Vector3(0, calibrateRedZoneY, 0);
		//_dangerZone.sizeDelta= new Vector2(_dangerZone.sizeDelta.x, StageSystem.Instance.stage_data.map_redzone);
		_redZone.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, calibrateRedZoneHeight);
	}

	//플레이어들의 마커를 생성합니다.
	public void CreatePlayerMarker() {
		GameObject obj = Instantiate(_playerMarker, this.transform);
		if(!MappingData.ContainsKey(obj.transform)) {
			if(obj.TryGetComponent(out RectTransform rect)) {
				MappingData.Add(obj.transform, rect);
			}
		}
	}
	public void CreatePlayerMarker(Transform playerTransform, bool isLocal) {
		if (MappingData.ContainsKey(playerTransform)) return;

		GameObject obj = Instantiate(_playerMarker, this.transform);
		if (obj.TryGetComponent(out RectTransform rect)) {
			MappingData.Add(playerTransform, rect);
			if (isLocal) {
				Image[] img = obj.GetComponentsInChildren<Image>();
				img[1].color = Color.red;
				rect.SetAsLastSibling();
				rect.localScale = new Vector3(0.75f, 0.75f, 0.75f);
			}
		}
	}

	private void Update() {
		if (_mapHeight <= 0) return;

		foreach(var kvp in MappingData) {
			Transform targetPlayer = kvp.Key;
			RectTransform markerRect = kvp.Value;

			if (targetPlayer == null) continue;

			//진행도
			float percent = (targetPlayer.position.y / _mapHeight) * 100f;
			if (percent > 100f) percent = 100f;

			markerRect.anchoredPosition = new Vector3(0, percent * 18, 0);
		}
	}

	private void UpdateMarkerSorting() {
		// RaceManager에서 이미 정렬해둔 리스트 가져오기 (0번 인덱스가 1등)
		List<Transform> sortedPlayers = RaceManager.Instance.active_players;
		int totalPlayers = sortedPlayers.Count;

		// 내 로컬 플레이어 Transform 캐싱 (안전성을 위해 NetworkClient 활용)
		Transform localPlayerTransform = NetworkClient.localPlayer != null ? NetworkClient.localPlayer.transform : null;

		for (int i = 0; i < totalPlayers; i++) {
			Transform targetPlayer = sortedPlayers[i];

			// 딕셔너리에서 이 플레이어와 짝지어진 UI 마커를 찾습니다.
			if (MappingData.TryGetValue(targetPlayer, out RectTransform markerRect)) {

				// 마커 프리팹에 달아둔 Canvas 컴포넌트 가져오기
				if (markerRect.TryGetComponent(out Canvas markerCanvas)) {

					if (targetPlayer == localPlayerTransform) {
						//플레이어 본인 마커면, 가장 최상위 위치.
						markerCanvas.sortingOrder = 100;
					} else {
						// [타 유저] 1등(i=0)일수록 더 큰 숫자를 부여하여 아래 순위 마커를 덮게 함
						// (예: 총 10명일 때, 1등(0번)은 order가 10 / 꼴찌(9번)는 order가 1)
						markerCanvas.sortingOrder = totalPlayers - i;
					}
				}
			}
		}
	}
}
