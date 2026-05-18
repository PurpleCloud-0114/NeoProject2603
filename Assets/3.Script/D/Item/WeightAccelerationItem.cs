using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "ItemOption/WeightAccelerationItem")]
public class WeightAccelerationItem : ScriptableObject, IUseable {
	[SerializeField] private string _itemName = "중량 가속";
	[SerializeField] private ItemType _itemType = ItemType.WeightAcceleration;
	[SerializeField] private Sprite _itmeImage;

	public string Name => _itemName;
	public ItemType Type => _itemType;
	public Sprite Item_Image => _itmeImage;

	public float AmplifyGravity = 25f;
	public float expansion_drop_max_speed = 150f;
	public float duration = 1.5f;

	public void Use(GameObject user) {
		if (user.TryGetComponent(out PlayerEffectController player)) {
			player.UseWeightAccelerationItem(AmplifyGravity, expansion_drop_max_speed, duration);
		}
	}
}
