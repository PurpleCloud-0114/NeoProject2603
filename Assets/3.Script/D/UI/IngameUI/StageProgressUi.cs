using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StageProgressUi : NetworkBehaviour {
	[SerializeField] private Transform _player;

	[SerializeField] private RectTransform _curPointBar;
	[SerializeField] private RectTransform _redZone;

	[SerializeField] private float _mapHeight;
	[SerializeField] private float _progressPercent;

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

	public void BindPlayer(Transform player) => player = _player;

	private void Update() {
		if (_player == null) return;
		if (_player.position.y > _mapHeight) _progressPercent = 100;
		else _progressPercent = (_player.position.y / _mapHeight) * 100;
		UpdateProgress();
	}

	private void UpdateProgress() {
		Vector3 Progress = new Vector3(0, _progressPercent * 18 , 0);
		_curPointBar.anchoredPosition = Progress;
	}
}
