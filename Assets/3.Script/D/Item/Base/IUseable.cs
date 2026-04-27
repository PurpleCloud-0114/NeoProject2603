using UnityEngine;

public interface IUseable {
	string Name { get; }
	ItemType Type { get; }

	void Use(GameObject user);
}
