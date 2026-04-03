using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestManager : MonoBehaviour {
	public static TestManager Instance = null;

	[SerializeField] private Transform _player;

	[SerializeField] private InputController _inputController;
	[SerializeField] private RandomSpawner _randomSpawner;

	[SerializeField] private StageProgressUi _stageProgressUi;


	private void Awake() {
		if (Instance == null) Instance = this;
		else Destroy(gameObject);

		Time.timeScale = 0f;
	}

	public void Initialize() {
		StageSystem.Instance.SetStage();


		_randomSpawner.SpawnObstacles();

		_player.transform.position = new Vector3(0, StageSystem.Instance.stage_data.map_height + 150f, 0);

		_inputController.Calibrate();

		_stageProgressUi.Initialize();

		Time.timeScale = 1f;
		Timer.Instance.StartStopwatch();
	}
}
