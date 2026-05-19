using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum ItemType : byte
{
    None = 0,
    Spiderweb = 1,          // 거미줄
    Shockwave = 2,          // 충격파
    WeightAcceleration = 3, // 중량 가속
    Magnetic = 4,           // 자석
	AccelGate = 5,			// 가속 관문
    AntiMagic = 6           // 안티 매직 (예정)

	//아이템 이름 (이거 번호는 안적어도됨)
}

public class ItemManager : NetworkBehaviour {
	//TODO - Item Spawn 기능 / Item Object Pooling 기능
	public static ItemManager Instance;

	[SerializeField] private GameObject _spiderwebPrefab;

	[SerializeField] private List<ScriptableObject> _itemTemplates;
	private Dictionary<ItemType, IUseable> _itemDict = new();

	private void Awake() {
		if (Instance == null) Instance = this;
		else Destroy(gameObject);

		// 등록된 SO들을 타입별로 매핑
		foreach (var obj in _itemTemplates) {
			if (obj is IUseable item) {
				_itemDict[item.Type] = item;
			}
		}
	}

	public IUseable RandomItem(GameObject user) {
		if (!isServer) return null;
		int myIndex = RaceManager.Instance.active_players.FindIndex(p => p != null && p.gameObject == user);
		int myRank = myIndex + 1; // 인덱스는 0부터이므로 +1
		int totalPlayers = RaceManager.Instance.active_players.Count;

		ItemType type = ItemType.None;
		if (myRank == 1) {
			type = ItemType.Spiderweb;
		} else if (myRank > totalPlayers / 2) {
			type = (ItemType)Random.Range(2, 5);
		} else {
			type = (ItemType)Random.Range(1, 4);
		}
		return GetItemUseable(type);
	}

	public IUseable GetItemUseable(ItemType type) {
		// 복사본을 만들지 않고 원본 참조만 넘김
		return _itemDict.GetValueOrDefault(type);
	}

	public void SpanwSpiderweb(Vector3 position) {
		if (_spiderwebPrefab.TryGetComponent(out SpiderwebObstacle _spiderweb)) {
			Quaternion rotation = Quaternion.Euler(-90, 0, 0);
			float distance = _spiderweb.distance;

			Vector3 pos1 = position + ((Vector3.up + Vector3.forward) * distance);
			Vector3 pos2 = position + ((Vector3.up + Vector3.left + Vector3.back) * distance);
			Vector3 pos3 = position + ((Vector3.up + Vector3.right + Vector3.back) * distance);

			NetworkServer.Spawn(Instantiate(_spiderwebPrefab, pos1, rotation));
			NetworkServer.Spawn(Instantiate(_spiderwebPrefab, pos2, rotation));
			NetworkServer.Spawn(Instantiate(_spiderwebPrefab, pos3, rotation));
		}
	}
}
