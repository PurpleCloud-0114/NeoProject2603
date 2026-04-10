using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinglePlayerActive : MonoBehaviour {
	public GameObject[] players;

	private void Start() {
		StartCoroutine(test_co());
	}

	private IEnumerator test_co() {
		yield return new WaitForSeconds(1f);
		foreach(GameObject player in players) {
			player.SetActive(true);
		}
	}
}
