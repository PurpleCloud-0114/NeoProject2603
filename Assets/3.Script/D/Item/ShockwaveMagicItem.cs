using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ItemOption/ShockwaveMagicItem")]
public class ShockwaveMagicItem : ScriptableObject, IUseable {
	[SerializeField] private string _itemName = "충격파 마법";
	[SerializeField] private ItemType _itemType = ItemType.Shockwave;

	public string Name => _itemName;
	public ItemType Type => _itemType;

	public float range = 15f;
	public float push_force = 25f;
	public float stun_duration = 2f;

	public void Use(GameObject user) {
		Collider[] hits = Physics.OverlapSphere(user.transform.position, range);
		foreach(Collider hit in hits) {
			if (hit.gameObject == user) continue;
			if(hit.TryGetComponent(out PlayerEffectController _playerEffectController)) {
				Vector3 normal = (hit.transform.position - user.transform.position).normalized;
				_playerEffectController.HitShockwave(normal * push_force, stun_duration);
			}
		}
	}
}
