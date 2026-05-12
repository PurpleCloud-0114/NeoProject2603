using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class StageProgressUi : MonoBehaviour {
	[Header("맵	데이터")]
	[SerializeField] private RectTransform _redZone;
	[SerializeField] private float _mapHeight;

	[Header("플레이어 UI 마커")]
	[SerializeField] private GameObject _playerMarker;


	private Dictionary<Transform, RectTransform> MappingData = new Dictionary<Transform, RectTransform>();

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
}
