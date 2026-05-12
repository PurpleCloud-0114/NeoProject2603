using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ItemOption/SpiderwebItem")]
public class SpiderwebItem : ScriptableObject, IUseable {
	[SerializeField] private string _itemName = "°Å¹ÌÁÙ ÅºÈ¯";
	[SerializeField] private ItemType _itemType = ItemType.Spiderweb;

	public string Name => _itemName;
	public ItemType Type => _itemType;

	public void Use(GameObject user) {
		if (user.TryGetComponent(out PlayerEffectController player)) {
			player.UseSpiderwebBulletItem();
		}
	}
}
