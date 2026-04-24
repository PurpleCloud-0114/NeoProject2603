using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum ItemType { 
	None,
	WeightAcceleration,
	Spiderweb,
	Shockwave,
	Magnetic,
	AntiMagic
}

public class ItemManager : NetworkBehaviour {
	//TODO - Item Spawn 기능 / Item Object Pooling 기능
	public static ItemManager Instance;

	[SerializeField] private GameObject _spiderwebPrefab;

	private void Awake() {
		if (Instance == null) Instance = this;
		else Destroy(gameObject);
	}

	[Server]
	public void ExecuteItemLogic(ItemType type, GameObject user) {
		switch (type) {
			case ItemType.WeightAcceleration:
				ApplyWeightAcceleration(user);
				break;
			case ItemType.Spiderweb:

				break;
			case ItemType.Shockwave:

				break;
			case ItemType.Magnetic:

				break;
			case ItemType.AntiMagic:

				break;
		}
	}

	private void ApplyWeightAcceleration(GameObject user) {
		if(user.TryGetComponent(out PlayerEffectController effect)) {
			effect.UseWeightAccelerationItem(25f, 150f, 1.5f);
		}
	}

	public IUseable RandomItem() {
		int randomIndex = Random.Range(0, 0);

		switch(randomIndex) {
			case 0:
				return new WeightAccelerationItem();
			case 1:
				return new SpiderwebItem();
			case 2:
				return new ShockwaveMagicItem();
			default:
				return null;
		}
	}

	public void SpanwSpiderweb(Vector3 postion) {
		if(_spiderwebPrefab.TryGetComponent(out SpiderwebObstacle _spiderweb))
		Instantiate(_spiderwebPrefab, postion + Vector3.up * _spiderweb.distance, Quaternion.identity);
	}
}
