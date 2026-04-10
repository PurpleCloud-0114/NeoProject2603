using UnityEngine;

public interface IUseable {
	string Name { get; }

	void Use(GameObject user);
}
