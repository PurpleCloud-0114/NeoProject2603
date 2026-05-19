using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "ItemOption/SpiderwebItem")]
public class SpiderwebItem : ScriptableObject, IUseable {
	[SerializeField] private string _itemName = "°Å¹ÌÁÙ ÅºÈ¯";
	[SerializeField] private ItemType _itemType = ItemType.Spiderweb;
	[SerializeField] private Sprite _itmeImage;

	public string Name => _itemName;
	public ItemType Type => _itemType;
	public Sprite Item_Image => _itmeImage;

	public void Use(GameObject user) {
		if (user.TryGetComponent(out PlayerEffectController player)) {
			player.UseSpiderwebBulletItem();
		}
	}
}
