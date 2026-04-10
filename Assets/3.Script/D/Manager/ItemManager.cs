using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour {
	//TODO - Item Spawn 기능 / Item Object Pooling 기능
	public static ItemManager Instance;

	[SerializeField] private GameObject _spiderwebPrefab;

	private void Awake() {
		if (Instance == null) Instance = this;
		else Destroy(gameObject);
	}

	public IUseable RandomItem() {
		int randomIndex = Random.Range(0, 3);

		switch(randomIndex) {
			case 1:
				return new WeightAccelerationItem();
			case 2:
				return new SpiderwebItem();
			case 3:
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
