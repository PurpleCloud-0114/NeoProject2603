using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageSystem : MonoBehaviour {
	public static StageSystem Instance = null;

	public StageData stage_data { get; private set; }

	[SerializeField] private Transform _dangerZoneTrigger;

	[Header("Debug")]
	public float rnd_RZY;
	public float rnd_RZH;
	public float rnd_RZHY;

	private void Awake() {
		if (Instance == null) Instance = this;
		else Destroy(gameObject);
	}

	public void SetStage() {
		float rnd_RedZone = Random.Range(250f, 400f);
		float rnd_RedZoneHeight = Random.Range(50f, 300f);
		float set_RedZoneTrigger = rnd_RedZoneHeight + rnd_RedZone;
		stage_data = new StageData(3000f, rnd_RedZone, rnd_RedZoneHeight);
		_dangerZoneTrigger.position = new Vector3(0, stage_data.map_redzone_height_Y, 0);
		rnd_RZY = rnd_RedZone;
		rnd_RZH = rnd_RedZoneHeight;
		rnd_RZHY = stage_data.map_redzone_height_Y;
	}
}
