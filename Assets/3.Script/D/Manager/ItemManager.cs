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

	public void SpanwSpiderweb(Vector3 postion) {
		Instantiate(_spiderwebPrefab, postion, Quaternion.identity);
	}
}
