using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockwaveMagicItem : IUseable {
	public string Name => "충격파 마법";
	public float range = 10f;
	public float push_force = 25f;


	public void Use(GameObject user) {
		Collider[] hits = Physics.OverlapSphere(user.transform.position, range);
		foreach(Collider hit in hits) {
			if (hit.gameObject == user) continue;
			if(hit.TryGetComponent(out PlayerEffectController _playerEffectController)) {
				Vector3 normal = (hit.transform.position - user.transform.position).normalized;
				_playerEffectController.HitShockwave(normal * push_force);
			}
		}
	}
}
