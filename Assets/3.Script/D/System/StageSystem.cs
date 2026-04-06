using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageSystem : MonoBehaviour {
	public static StageSystem Instance = null;

	public StageData stage_data { get; private set; }

	[SerializeField] private Transform _dangerZoneTrigger;

	private void Awake() {
		if (Instance == null) Instance = this;
		else Destroy(gameObject);
	}

	public void SetStage() {
		float rnd_dangerzone = Random.Range(300f, 500f);
		stage_data = new StageData(3000f, rnd_dangerzone);
		_dangerZoneTrigger.position = new Vector3(0, rnd_dangerzone, 0);
	}
}
