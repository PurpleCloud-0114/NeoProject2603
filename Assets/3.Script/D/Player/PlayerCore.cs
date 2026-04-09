using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public enum PlayerGameState {
	Wait,
	Falling,
	Finish
}

public class PlayerCore : NetworkBehaviour {
	private PlayerMovement _playerMovement;
	private PlayerUIController _playerUIController;
	private PlayerItemController _playerItemController;
	private PlayerEffectController playerEffectController;

	private Rigidbody _rigidbody;

	private GameObject _mainCamera;

	public PlayerGameState playerGameState = PlayerGameState.Wait;

	//----- 메서드
	private void Awake() {
		TryGetComponent(out _rigidbody);
		TryGetComponent(out _playerMovement);
		TryGetComponent(out _playerUIController);
		TryGetComponent(out _playerItemController);
	}

	private void Start() {
		if (isLocalPlayer || RaceManager.Instance.isSinglePlay) {
			_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			if (_mainCamera.TryGetComponent(out DynamicFOVController FOVController)) {
				Debug.Log("Find Camera!");
				FOVController.BindPlayer(gameObject);
			} else {
				Debug.Log("Can't Find Camera...");
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
		if (other.transform.CompareTag("ItemBox")) {
			IUseable randomItem = new WeightAccelerationItem();
			_playerItemController.GetItem(randomItem);
			_playerUIController.ActivateItemBtn();
		}

		if (other.transform.CompareTag("Obstacle")) {
			_playerMovement.hitObstacle();
		}
		if (other.transform.CompareTag("Redzone")) {
			Debug.Log($"레드존 진입합 Y좌표 : {other.transform.position.y}");
			Debug.Log($"플레이어 현재 Y좌표 : {transform.position.y}");
			_playerUIController.ActivateWingBtn();
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
