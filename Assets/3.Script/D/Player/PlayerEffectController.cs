using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 아이템으로 인한 상호작용 효과들을 다루는 컨트롤러입니다.
/// 즉, 아이템으로 인한 시각적 효과가 아니라는 말입니다.
/// </summary>
public class PlayerEffectController : MonoBehaviour {
	private PlayerMovement _playerMovement;
	private PlayerCore _playerCore;
	private Rigidbody _rigidbody;
	private Tween _itemBuffTween;

	private bool _isInvinsible = false;

	/*
	 아이템 효과 발동할 내용들
	[ 가속 아이템 ]
	- 나만 빨라짐

	[ 거미줄 아이템 ]
	- 나만 느려짐

	[ 충격파 아이템 ]
	- 나는 주변 사람들에게 효과를 부여함 대충 서클캐스팅?
	- 맞은 나는 밀려남 ADDforce
	 
	[ 마그내틱 아이템 ]
	- 나는 A한테 끌려감
	- A는 나한테 끌려감
	 
	 */
	private void Awake() {
		TryGetComponent(out _playerMovement);
		TryGetComponent(out _playerCore);
		TryGetComponent(out _rigidbody);
	}

	public void UseWeightAccelerationItem(float force, float expansionValue, float duration) {
		_itemBuffTween?.Kill();
		_playerMovement.drop_max_speed += expansionValue;
		_rigidbody.AddForce(0, -force, 0, ForceMode.VelocityChange);
		//Vector3 currentVelocity = _rigidbody.linearVelocity;
		//currentVelocity.y -= force;
		//_rigidbody.linearVelocity = currentVelocity;



		_itemBuffTween = DOTween.To(() => _playerMovement.drop_max_speed, x => _playerMovement.drop_max_speed = x, _playerMovement.base_drop_max_speed, 0.5f)
			.SetDelay(duration) // 지속시간만큼 대기
			.SetEase(Ease.InOutQuad); // 부드럽게 감속
	}
	public void UseSpiderwebBulletItem() {
		ItemManager.Instance.SpanwSpiderweb(transform.position);
	}
	public void UseShockwaveMagicItem() {
		//주변 사람들한테 범위 지정 및 거리 계산하여 일정 파워 날리기.
	}
	public void UseMagneticMagicItem() {
		//일단 미구현
	}
	public void UseAntiMagicItem(float duration) {
		StopCoroutine("Invinsibling_co");
		StartCoroutine("Invinsibling_co",duration);
	}

	private IEnumerator Invinsibling_co(float duration) {
		_isInvinsible = true;
		yield return new WaitForSeconds(duration);
		_isInvinsible = false;
	}

	//이제 맞는 판정
	public void HitSpiderweb(Collider spiderweb) {
		if (_isInvinsible) return;
		if(spiderweb.TryGetComponent(out SpiderwebObstacle _spiderweb)) {
			StopCoroutine("CantMove_co");
			_playerMovement.drop_max_speed = _spiderweb.SetPlayerVelocity;
			StartCoroutine("CantMove_co", _spiderweb.duration);
		}
	}
	public void HitShockwave(Vector3 force) {
		if (_isInvinsible) return;
		StopCoroutine("CantMove_co");
		_rigidbody.AddForce(force, ForceMode.VelocityChange);
		StartCoroutine("CantMove_co", 1f);
	}
	public void HitMagnetic() {
		if (_isInvinsible) return;
		//조작 불가 및 그 사람한테 쭉 달려간다.
		//일단 미구현
	}

	private IEnumerator CantMove_co(float duration) {
		_playerCore.playerGameState = PlayerGameState.Stun;
		yield return new WaitForSeconds(duration);
		_playerMovement.drop_max_speed = _playerMovement.base_drop_max_speed;
		_playerCore.playerGameState = PlayerGameState.Falling;
	}
}
