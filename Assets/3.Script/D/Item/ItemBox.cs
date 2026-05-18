using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ItemBox : NetworkBehaviour {
	private ParticleSystem _particle;
	[SerializeField] private GameObject _cube;
	private bool _isCooldown = false;

	private WaitForSeconds wfs = new WaitForSeconds(3f);

	private void Awake() {
		TryGetComponent(out _particle);
	}

	private void OnTriggerEnter(Collider other) {
		if (other.CompareTag("Player") && !_isCooldown) {
			_isCooldown = true;
			_particle.Stop();
			_cube.SetActive(false);
			StartCoroutine("Recreate_co");
		}
	}

	private IEnumerator Recreate_co() {
		yield return wfs;
		_particle.Play();
		_isCooldown = false;
		_cube.SetActive(true);
	}
}
