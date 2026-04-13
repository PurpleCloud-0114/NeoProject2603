using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerTrigger : NetworkBehaviour {
	private PlayerCore _playerCore;

	private const string TAG_ITEMBOX = "ItemBox";
	private const string TAG_REDZONE = "Redzone";
	private const string TAG_SPIDERWEB = "Spiderweb";

	private void Awake() {
		TryGetComponent(out _playerCore);
	}

	private void OnCollisionEnter(Collision collision) {
		if (!isLocalPlayer && !RaceManager.Instance.isSinglePlay) return;

		if (collision.transform.CompareTag("EndPoint")) {
			//playerGameState = PlayerGameState.Finish;
			double myFinishTime = NetworkTime.time - RaceManager.Instance.race_start_time_sync;
			float impactSpeed = Mathf.Abs(collision.relativeVelocity.y);
			// Checked - TODO : 추후 서버한테 도착을 알리는 이벤트 메시지 추가.
			//SendArriveResult(impactSpeed, myFinishTime);
		}
	}

	private void OnTriggerEnter(Collider other) {
		if (!isLocalPlayer && !RaceManager.Instance.isSinglePlay) return;
		Debug.Log("뭔가 닿음.");
		switch (other.tag) {
			case TAG_ITEMBOX:
				Debug.Log("아이템 먹음");
				IUseable randomItem = ItemManager.Instance.RandomItem();
				Debug.Log("아이템 생성");
				_playerCore.on_item_acquired?.Invoke(randomItem);   //아이템 획득 이벤트 호출
				Debug.Log("아이템 로직 끝");
				break;
			case "Obstacle":
				//_playerMovement.hitObstacle();
				break;
			case TAG_REDZONE:
				Debug.Log($"레드존 진입합 Y좌표 : {other.transform.position.y}");
				Debug.Log($"플레이어 현재 Y좌표 : {transform.position.y}");
				//_playerUIController.ActivateWingBtn();
				break;
			case TAG_SPIDERWEB:
				if (_playerCore.status_effect == StatusEffect.Invinsible) return;

				Debug.Log("거미줄 트리거 발동");
				_playerCore.on_spiderweb_hit?.Invoke(other); //거미줄 충돌 이벤트 호출
				break;
		}
	}
}
