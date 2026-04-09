using System;
using UnityEngine;

[Serializable]
public struct StageData {
	[field: SerializeField] public float map_height;
	[field: SerializeField] public float map_redzone;
	[field: SerializeField] public float map_redzone_height;
	[field: SerializeField] public float map_redzone_height_Y;

	public StageData(float mapHeight, float mapRedZone, float mapRedZoneHeight) {
		map_height = mapHeight;
		map_redzone = mapRedZone;
		map_redzone_height = mapRedZoneHeight;
		map_redzone_height_Y = mapRedZone + mapRedZoneHeight;
	}
}
