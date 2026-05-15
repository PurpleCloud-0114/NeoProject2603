using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Mirror;

/// <summary>
/// 아이템으로 인한 상호작용 효과들을 다루는 컨트롤러입니다.
/// 즉, 아이템으로 인한 시각적 효과가 아니라는 말입니다.
/// </summary>
public class PlayerEffectController : NetworkBehaviour {
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


	// ======================
	// [ 중량 가속 ] - 자버프
	// ======================
	public void UseWeightAccelerationItem(float force, float expansionValue, float duration) {
		if (isServer) {
			TargetApplyWeightAcceleration(netIdentity.connectionToClient, force, expansionValue, duration);
		}
	}
	[TargetRpc]
	private void TargetApplyWeightAcceleration(NetworkConnectionToClient target, float force, float expansionValue, float duration) {
		// 실제 물리는 권한을 가진 클라이언트 로컬에서 실행!
		_playerCore.on_max_drop_speed_change_requested(expansionValue, duration, 1.5f, StatusEffect.None);
		_playerCore.on_impulse_requested(Vector3.down * force);
	}


	// ======================
	// [ 거미줄 탄환 ] - 소환
	// ======================
	public void UseSpiderwebBulletItem() {
		if (isServer) {
			ItemManager.Instance.SpanwSpiderweb(transform.position);
		}
	}
	//맞는 판정
	public void HitSpiderweb(Collider spiderweb) {
		if (spiderweb.TryGetComponent(out SpiderwebObstacle _spiderweb)) {
			_playerCore.on_max_drop_speed_change_requested(_spiderweb.SetPlayerVelocity, _spiderweb.duration, 0f, StatusEffect.Stun);
			_playerCore.on_stun_requested?.Invoke(_spiderweb.duration);
		}
	}


	// =========================
	// [ 충격파 마법 ] - 피격시.
	// =========================
	public void HitShockwave(Vector3 force, float stunDuration) {
		// 이 함수는 서버에서 OverlapSphere로 찾아낸 '맞은 플레이어'의 컨트롤러에서 실행됨
		if (isServer) {
			// 맞은 플레이어의 클라이언트에게 날아가라고 명령 전달
			TargetApplyShockwave(netIdentity.connectionToClient, force, stunDuration);
		}
	}
	[TargetRpc]
	private void TargetApplyShockwave(NetworkConnectionToClient target, Vector3 force, float stunDuration) {
		// 맞은 클라이언트가 스스로 날아감
		_playerCore.on_impulse_requested?.Invoke(force);
		_playerCore.on_stun_requested?.Invoke(stunDuration);
	}


	// =========================
	// [ 마 그 네 틱 ] - 유유유유유유유유
	// =========================
	public void UseMagneticItem(GameObject target, float duration, float power) {
		if (!isServer) return;

		TargetApplyMagneticEffect(netIdentity.connectionToClient, target, true, duration, power);
		if(target.TryGetComponent(out NetworkIdentity targetIdentity)) {
			TargetApplyMagneticEffect(targetIdentity.connectionToClient, gameObject, false, duration, power);
		}
	}
	[TargetRpc]
	public void TargetApplyMagneticEffect(NetworkConnectionToClient target, GameObject opponent, bool isAttacker, float duration, float power) {
		if (opponent == null) return;
		if (_playerCore.player_state != PlayerState.Falling) return;
		// 1. 공격자가 아닌 '당한 사람'일 경우에만 UI 표시 및 파티클 재생
		if (UIManager.Instance != null)
		{
			UIManager.Instance.ShowMagneticIndicator(isAttacker, 2.5f);
		}
		// 2. 각자 역할에 맞는 파티클 재생
		if (TryGetComponent(out PlayerTrigger trigger))
		{
			trigger.PlayHitEffect(isAttacker ? 4 : 6);
		}
		StartCoroutine(Co_MagneticForce(opponent, isAttacker, duration, power));
	}
	private IEnumerator Co_MagneticForce(GameObject opponent, bool isAttacker, float duration, float power) {
		float elapsed = 0f;
		while (elapsed < duration) {
			if (opponent == null) break;

			// 상대방을 향한 방향 벡터
			Vector3 dir = (opponent.transform.position - transform.position).normalized;

			// PlayerCore의 impulse 이벤트를 통해 물리 힘 전달
			_playerCore.on_impulse_requested?.Invoke(dir * power * Time.deltaTime * 60f);

			elapsed += Time.deltaTime;
			yield return null;
		}
	}



	// =========================
	// [ 가속 게이트 ] - 통과시
	// =========================
	public void UseAntiMagicItem(float duration) {
		StopCoroutine("Invinsibling_co");
		StartCoroutine("Invinsibling_co",duration);
	}

	// =========================
	// [ 텔레포트 게이트 ] - 통과시
	// =========================

}
