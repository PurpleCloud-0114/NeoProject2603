using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum ItemType : byte
{
    None = 0,
    WeightAcceleration = 1, // 중량 가속
    Spiderweb = 2,          // 거미줄
    Shockwave = 3,          // 충격파
    Magnetic = 4,           // 자석 (예정)
    AntiMagic = 5           // 안티 매직 (예정)

	//아이템 이름 (이거 번호는 안적어도됨)
}

public class ItemManager : NetworkBehaviour {
	//TODO - Item Spawn 기능 / Item Object Pooling 기능
	public static ItemManager Instance;

	[SerializeField] private GameObject _spiderwebPrefab;

	private void Awake() {
		if (Instance == null) Instance = this;
		else Destroy(gameObject);
	}

	public IUseable RandomItem() {
		ItemType type = ItemType.None;
		if (UIManager.Instance.myRank == 1) {
			type = (ItemType)2;
		} else if(UIManager.Instance.myRank > RaceManager.Instance.total_players/2) {
			type = (ItemType)Random.Range(1, 4);
		} else {
			type = (ItemType)Random.Range(1, 1);
		}
		switch (type) {
			case ItemType.WeightAcceleration: return new WeightAccelerationItem();
			case ItemType.Spiderweb: return new SpiderwebItem();
			case ItemType.Shockwave: return new ShockwaveMagicItem();
			default: return null;
		}
	}

	public IUseable GetItemUseable(ItemType type) {
		switch (type) {
			case ItemType.WeightAcceleration: return new WeightAccelerationItem();
			case ItemType.Spiderweb: return new SpiderwebItem();
			case ItemType.Shockwave: return new ShockwaveMagicItem();
			default: return null;
		}
	}

	public void SpanwSpiderweb(Vector3 postion) {
		if (_spiderwebPrefab.TryGetComponent(out SpiderwebObstacle _spiderweb)) {
			Quaternion rotation = Quaternion.Euler(-90, 0, 0);
			GameObject webInst = Instantiate(_spiderwebPrefab, postion + Vector3.up * _spiderweb.distance, rotation);
			NetworkServer.Spawn(webInst);
		}
	}
}
