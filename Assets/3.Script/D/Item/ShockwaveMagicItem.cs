using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "ItemOption/ShockwaveMagicItem")]
public class ShockwaveMagicItem : ScriptableObject, IUseable {
	[SerializeField] private string _itemName = "충격파 마법";
	[SerializeField] private ItemType _itemType = ItemType.Shockwave;
	[SerializeField] private Sprite _itmeImage;

	public string Name => _itemName;
	public ItemType Type => _itemType;
	public Sprite Item_Image => _itmeImage;

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
