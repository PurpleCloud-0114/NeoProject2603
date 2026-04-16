using UnityEngine;

public class Obstacle : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			Collect(other.gameObject);
		}
	}

	public void Collect(GameObject collector)
	{

		//TODO - s사운드 및 이펙트 재생

		gameObject.SetActive(false);
	}
}
