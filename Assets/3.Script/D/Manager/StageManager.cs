using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StageManager : NetworkBehaviour {
	public static StageManager Instance = null;

	[SyncVar] public StageData stage_data_sync;
	[SerializeField] private Transform _dangerZoneTrigger;

	private void Awake() {
		if (Instance == null) Instance = this;
		else Destroy(gameObject);
	}

	[Server]
	public void SetStage() {
		float rnd_RedZone = Random.Range(250f, 400f);
		float rnd_RedZoneHeight = Random.Range(50f, 300f);
		stage_data_sync = new StageData(3000f, rnd_RedZone, rnd_RedZoneHeight);
		SetRedzonePosition();
	}

	[ClientRpc]
	private void SetRedzonePosition() {
		_dangerZoneTrigger.position = new Vector3(0, stage_data_sync.map_redzone_height_Y, 0);
	}
}
