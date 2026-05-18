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
	public float charge_duration = 2f; // 차지 시간 추가

	public void Use(GameObject user) {
		// 유저의 PlayerEffectController를 찾아 충격파 차지 시퀀스를 시작합니다.
		if (user.TryGetComponent(out PlayerEffectController effectController)) {
			effectController.UseShockwaveItem(range, push_force, stun_duration, charge_duration);
		}
	}
}
