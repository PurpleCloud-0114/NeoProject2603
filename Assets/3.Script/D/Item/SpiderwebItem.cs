using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderwebItem : IUseable {
	public string Name => "°Å¹ÌÁÙ ÅºÈ¯";
	public ItemType Type => ItemType.Spiderweb;

	public void Use(GameObject user) {
		if (user.TryGetComponent(out PlayerEffectController player)) {
			player.UseSpiderwebBulletItem();
		}
	}
}
