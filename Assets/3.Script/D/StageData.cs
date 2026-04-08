using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct StageData {
	public float map_height { get; private set; }
	public float map_redzone { get; private set; }
	public float map_redzone_height { get; private set; }
	public float map_redzone_height_Y { get; private set; }

	public StageData(float mapHeight, float mapRedZone, float mapRedZoneHeight) {
		map_height = mapHeight;
		map_redzone = mapRedZone;
		map_redzone_height = mapRedZoneHeight;
		map_redzone_height_Y = mapRedZone + mapRedZoneHeight;
	}
}
