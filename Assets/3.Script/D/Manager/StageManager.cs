using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StageManager : NetworkBehaviour {
	public static StageManager Instance = null;

	public MapSize map_size;

	public StageProgressUi stageProgressUi;

	[SyncVar(hook = nameof(OnStageDataChanged))] public StageData stage_data_sync;
	[SerializeField] private Transform _redzoneTrigger;

	private void Awake() {
		if (Instance == null) Instance = this;
		else Destroy(gameObject);
	}

	[Server]
	public void SetStage() {
		float rnd_RedZone = Random.Range(250f, 400f);
		float rnd_RedZoneHeight = Random.Range(50f, 300f);
		stage_data_sync = new StageData(3000f, rnd_RedZone, rnd_RedZoneHeight);
	}

	private void OnStageDataChanged(StageData oldData, StageData newData) {
		stageProgressUi.Initialize();
		if (_redzoneTrigger != null) {
			_redzoneTrigger.position = new Vector3(0, newData.map_redzone_height_Y, 0);
		}
	}
}
