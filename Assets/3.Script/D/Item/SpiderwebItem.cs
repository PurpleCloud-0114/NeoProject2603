using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderwebItem : IUseable {
	public string Name => "∞≈πÃ¡Ÿ ≈∫»Ø";

	public void Use(GameObject user) {
		if (user.TryGetComponent(out PlayerEffectController player)) {
			player.UseSpiderwebBulletItem();
		}
	}
}
