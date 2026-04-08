using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using DG.Tweening;

public class PlayerState : NetworkBehaviour {
	private PlayerMovement _playerMovement;
	private PlayerUIController _playerUIController;

	private Rigidbody _rigidbody;

	[Header("날개")]
	[SerializeField, Range(0, 500)] private float _dropWingSpeed = 30f;
	private float _wingTime;

	[Header("도착 속도 판정 (Death)")]
	[SerializeField, Range(5f, 50f)] private float _deathOverSpeed = 30f;


	//----- 메서드
	private void Awake() {
		TryGetComponent(out _rigidbody);
		TryGetComponent(out _playerMovement);
		TryGetComponent(out _playerUIController);
	}

	private void OnCollisionEnter(Collision collision) {
		if (collision.transform.CompareTag("EndPoint")) {
			


			//TODO : 추후 서버한테 도착했으니 1등이라고 알리는 이벤트 메시지 추가.
		}
	}

	[Command]
	//서버에게 보내는 도착 신호. (도착 성공 여부 / 시간,
	private void SendArriveResult(bool result) {

	}


	private void OnTriggerEnter(Collider other) {
		if (other.transform.CompareTag("Obstacle")) {

		}
		if (other.transform.CompareTag("DangerZone")) {
			_playerUIController.ActivateWingBtn();
		}
	}

	public void SetDecreaseDropSpeedTimeOnWing(float mapRedZone) {
		//_dropSmoothOnWing = mapRedZone / _dropMaxSpeed * 1.5f; 
		_wingTime = (3f * mapRedZone) / (_playerMovement.drop_max_speed + 2f * _dropWingSpeed);
	}

	/* Ease.OutQuad의 수학적 접근.
	Distance = Time * (Vstart + 2 * Vtarget) / 3
	-> Time = 3 * Distance / (Vstart + 2 * Vtarget) 이 된다.
	즉, 변수를 대입한다면
	mapRedZone = _dropSmoothOnWing * (_DropMaxSpeed + 2f * _dropSpeedOnWing) / 3
	_dropSmoothOnWing = 3 * mapRedZone / (_DropMaxSpeed + 2f * _dropSpeedOnWing)
	 */
	public void OpenWing() {
		DOTween.To(() => _playerMovement.drop_max_speed, x => _playerMovement.drop_max_speed = x, _dropWingSpeed, _wingTime).SetEase(Ease.OutQuad);
	}
}
