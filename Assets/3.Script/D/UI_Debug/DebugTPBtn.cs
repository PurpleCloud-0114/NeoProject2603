using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTPBtn : MonoBehaviour {
	public Transform player;

	public void OnClickBtn() {
		player.transform.position = new Vector3(player.transform.position.x,
												StageSystem.Instance.stage_data.map_redzone_height_Y + 300f,
												player.transform.position.z);
	}
}
