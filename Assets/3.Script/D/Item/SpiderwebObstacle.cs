using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderwebObstacle : MonoBehaviour {
	public float SetPlayerVelocity = 0f;
	public float distance = 2.5f;
	public float duration = 0.5f;

	private void OnTriggerEnter(Collider other) {
		if (other.CompareTag("Player")) {
			StartCoroutine("DeleteSelf_co");
		}
	}

	private void OnDisable() {
		StopCoroutine("DeleteSelf_co");
	}

	private IEnumerator DeleteSelf_co() {
		yield return new WaitForSeconds(duration);
		Destroy(gameObject);
		//gameObject.SetActive(false);
	}
}
