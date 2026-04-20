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
	private PlayerCore _playerCore;
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
		TryGetComponent(out _playerCore);
	}

	private void OnEnable() {
		_playerCore.on_spiderweb_hit += HitSpiderweb;
		//_playerCore.OnShockwaveHit += HitShockwave;
	}
	private void OnDisable() {
		_playerCore.on_spiderweb_hit -= HitSpiderweb;
	}

	public void UseWeightAccelerationItem(float force, float expansionValue, float duration) {
		_playerCore.on_max_drop_speed_change_requested(expansionValue, duration, 1.5f, StatusEffect.None);
		_playerCore.on_impulse_requested(Vector3.down * force);
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

	//이제 맞는 판정
	public void HitSpiderweb(Collider spiderweb) {
		if (spiderweb.TryGetComponent(out SpiderwebObstacle _spiderweb)) {
			_playerCore.on_max_drop_speed_change_requested(_spiderweb.SetPlayerVelocity, _spiderweb.duration, 0f, StatusEffect.Stun);
		}
	}
	public void HitShockwave(Vector3 force) {
		//_rigidbody.AddForce(force, ForceMode.VelocityChange);
	}
	public void HitMagnetic() {
		//조작 불가 및 그 사람한테 쭉 달려간다.
		//일단 미구현
	}
}
