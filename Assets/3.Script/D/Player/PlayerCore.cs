using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public enum PlayerGameState {
	Wait,
	Falling,
	Stun,
	Finish
}

public class PlayerCore : NetworkBehaviour {
	private PlayerMovement _playerMovement;
	private PlayerUIController _playerUIController;
	private PlayerItemController _playerItemController;
	private PlayerEffectController _playerEffectController;

	private Rigidbody _rigidbody;
	private GameObject _mainCamera;

	public PlayerGameState playerGameState = PlayerGameState.Wait;

	//임시
	public bool is_dummy = false;
	public float player_number = 0;

	//----- 메서드
	private void Awake() {
		TryGetComponent(out _rigidbody);
		TryGetComponent(out _playerMovement);
		TryGetComponent(out _playerUIController);
		TryGetComponent(out _playerItemController);
		TryGetComponent(out _playerEffectController);
	}

	private void Start() {
		//int totalPlayer = RaceManager.Instance.total_players;
		Debug.Log("배치할게요");
		int totalPlayer = 10;

		float angle = player_number * Mathf.PI * 2f / totalPlayer;

		float x = Mathf.Cos(angle) * 5f;
		float z = Mathf.Sin(angle) * 5f;

		Vector3 spawnCenter = StageManager.Instance.map_size.map_center + Vector3.up * 3000f;
		Vector3 spawnPosition = spawnCenter + new Vector3(x, 0f, z);
		transform.position = spawnPosition;

		Vector3 directionToCenter = (spawnCenter - spawnPosition).normalized;
		transform.rotation = Quaternion.LookRotation(directionToCenter);
		Debug.Log("배치됨!");

		if ((isLocalPlayer || RaceManager.Instance.isSinglePlay) && !is_dummy) {
			_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			if (_mainCamera.TryGetComponent(out DynamicFOVController FOVController)) {
				//Debug.Log("Find Camera!");
				FOVController.BindPlayer(gameObject);
			} else {
				//Debug.Log("Can't Find Camera...");
			}
		}
	}

	public override void OnStartLocalPlayer() {
		RaceManager.Instance.CmdReportReady();
	}

	private void OnCollisionEnter(Collision collision) {
		if (!isLocalPlayer && !RaceManager.Instance.isSinglePlay) return;

		if (collision.transform.CompareTag("EndPoint")) {
			playerGameState = PlayerGameState.Finish;
			double myFinishTime = NetworkTime.time - RaceManager.Instance.race_start_time_sync;
			float impactSpeed = Mathf.Abs(collision.relativeVelocity.y);
			// Checked - TODO : 추후 서버한테 도착을 알리는 이벤트 메시지 추가.
			SendArriveResult(impactSpeed, myFinishTime);
		}
	}

	private void OnTriggerEnter(Collider other) {
		if (!isLocalPlayer && !RaceManager.Instance.isSinglePlay) return;
		switch (other.tag) {
			case "ItemBox":
				IUseable randomItem = new ShockwaveMagicItem();
				_playerItemController.GetItem(randomItem);
				_playerUIController.ActivateItemBtn();
				break;
			case "Obstacle":
				//_playerMovement.hitObstacle();
				break;
			case "Redzone":
				Debug.Log($"레드존 진입합 Y좌표 : {other.transform.position.y}");
				Debug.Log($"플레이어 현재 Y좌표 : {transform.position.y}");
				_playerUIController.ActivateWingBtn();
				break;
			case "Spiderweb":
				Debug.Log("거미줄 트리거 발동");
				_playerEffectController.HitSpiderweb(other);
				break;
		}
	}

	[Command]
	//서버에게 보내는 도착 신호. (도착 속도 / 시간,
	private void SendArriveResult(float impactSpeed, double finishTime) {
		RaceManager.Instance.GetArriveResult(connectionToClient, impactSpeed, finishTime);
	}

	public void UpdatePlayerStateByRace(RaceState raceState) {
		switch (raceState) {
			case RaceState.Waiting:
				playerGameState = PlayerGameState.Wait;
				_playerMovement._inputSystem.DisableInputSystem();
				break;
			case RaceState.Countdown:
				_playerMovement.SetDecreaseDropSpeedTimeOnWing();
				break;
			case RaceState.Racing:
				playerGameState = PlayerGameState.Falling;
				_playerMovement._inputSystem.EnableInputSystem();
				break;
			case RaceState.Finished:
				playerGameState = PlayerGameState.Finish;
				_playerMovement._inputSystem.DisableInputSystem();
				break;
		}
	}
}
