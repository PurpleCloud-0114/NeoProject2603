using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageData {
	public float map_height { get; private set; }
	public float map_dangerzone { get; private set; }

	public StageData(float mapHeight, float mapDangerzone) {
		map_height = mapHeight;
		map_dangerzone = mapDangerzone;
	}
}
