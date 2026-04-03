using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageProgressUi : MonoBehaviour {
	[SerializeField] private Transform _player;

	[SerializeField] private RectTransform _curPointBar;
	[SerializeField] private RectTransform _dangerZone;

	[SerializeField] private float _mapHeight;
	[SerializeField] private float _progressPercent;

	public void Initialize() {
		_mapHeight = StageSystem.Instance.stage_data.map_height;
		_dangerZone.sizeDelta= new Vector2(_dangerZone.sizeDelta.x, StageSystem.Instance.stage_data.map_dangerzone);
	}

	private void Update() {
		if (_player.position.y > _mapHeight) _progressPercent = 100;
		else _progressPercent = (_player.position.y / _mapHeight) * 100;
		UpdateProgress();
	}

	private void UpdateProgress() {
		Vector3 Progress = new Vector3(0, _progressPercent * 18 , 0);
		_curPointBar.anchoredPosition = Progress;
	}
}
