using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTPBtn : MonoBehaviour {
	public Transform player;

	public void OnClickBtn() {
		player.transform.position = new Vector3(player.transform.position.x,
												StageManager.Instance.stage_data_sync.map_redzone_height_Y + 300f,
												player.transform.position.z);
	}
}
