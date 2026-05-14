using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ItemOption/MagneticItem")]
public class MagneticItem : ScriptableObject, IUseable {
	[SerializeField] private string _itemName = "마그네틱";
	[SerializeField] private ItemType _itemType = ItemType.Magnetic;

	public string Name => _itemName;
	public ItemType Type => _itemType;

	public float duration = 1f;
	public float power = 3f;

	public void Use(GameObject user) {
		var players = RaceManager.Instance.active_players;
		int myIndex = players.FindIndex(p => p != null && p.gameObject == user);

		if (myIndex > 0) {
			GameObject target = players[myIndex - 1].gameObject;

			// 1. 공격자(나)의 컨트롤러를 가져와서 '나'를 움직이게 함
			if (user.TryGetComponent(out PlayerEffectController userController)) {
				userController.TargetApplyMagneticEffect(userController.connectionToClient, target, true, duration, power);
			}

			// 2. 피격자(상대)의 컨트롤러를 직접 가져와서 '상대'를 움직이게 함
			if (target.TryGetComponent(out PlayerEffectController targetController)) {
				// 타겟 본인의 컨트롤러에서 RPC를 쏴야, 타겟 클라이언트의 '본인 객체'가 이벤트를 발생시킴
				targetController.TargetApplyMagneticEffect(targetController.connectionToClient, user, false, duration, power);
			}
		}
	}
}
