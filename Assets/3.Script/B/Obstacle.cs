using UnityEngine;
using Mirror;

public class Obstacle : NetworkBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (!NetworkServer.active) return;

		if (other.CompareTag("Player"))
		{
			Collect(other.gameObject);
		}
	}

	public void Collect(GameObject collector)
	{

		// TODO: 여기서 플레이어에게 데미지를 주거나(장애물), 점수를 추가(아이템) 하세요.
		Debug.Log($"{gameObject.name}이(가) {collector.name}에 의해 처리됨");

		// 2. 사운드나 이펙트를 모든 클라이언트에서 재생하고 싶다면 
		// 여기서 Rpc 함수를 호출할 수 있습니다 (아래 3번 참고).

		// 3. 서버에서 오브젝트를 네트워크상에서 제거 (풀링을 위해 UnSpawn 사용)
		// NetworkServer.Destroy(gameObject) 대신 UnSpawn을 사용하면 풀로 돌아갑니다.
		NetworkServer.UnSpawn(gameObject);

		// 4. 실제 오브젝트 비활성화
		gameObject.SetActive(false);
	}
}
