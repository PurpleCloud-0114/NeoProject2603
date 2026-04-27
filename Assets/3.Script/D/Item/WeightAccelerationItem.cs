using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeightAccelerationItem : IUseable {
	public string Name => "중량 가속";
	public ItemType Type => ItemType.WeightAcceleration;

	public float AmplifyGravity = 25f;
	public float expansion_drop_max_speed = 150f;
	public float duration = 1.5f;

	public void Use(GameObject user) {
		if (user.TryGetComponent(out PlayerEffectController player)) {
			player.UseWeightAccelerationItem(AmplifyGravity, expansion_drop_max_speed, duration);
		}
	}
}
