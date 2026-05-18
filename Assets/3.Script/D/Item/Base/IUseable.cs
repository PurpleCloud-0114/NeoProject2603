using UnityEngine;
using UnityEngine.UI;

public interface IUseable {
	string Name { get; }
	ItemType Type { get; }
	Sprite Item_Image { get; }

	void Use(GameObject user);
}
