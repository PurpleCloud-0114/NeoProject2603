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
	[SerializeField] private Transform _wingPointTrigger;
	[SerializeField] private Collider _floor;

	private void Awake() {
		if (Instance == null) Instance = this;
		else Destroy(gameObject);
	}

	[Server]
	public void SetStage() {
		float rnd_RedZone = Random.Range(250f, 500f);
		float rnd_RedZoneHeight = Random.Range(400f, 600f);
		stage_data_sync = new StageData(6000f, rnd_RedZone, rnd_RedZoneHeight);
	}

	private void OnStageDataChanged(StageData oldData, StageData newData) {
		stageProgressUi.Initialize();
		if (_redzoneTrigger != null) _redzoneTrigger.position = new Vector3(0, newData.map_redzone_height_Y, 0);
		if (_wingPointTrigger != null) _wingPointTrigger.position = new Vector3(0, newData.map_redzone_height_Y * 1.5f, 0);
	}

	public void ChangeFloorTrigger(bool isTrigger) {
		_floor.isTrigger = isTrigger;
	}
}
