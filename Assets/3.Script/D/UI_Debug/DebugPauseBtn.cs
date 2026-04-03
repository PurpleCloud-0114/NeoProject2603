using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugPauseBtn : MonoBehaviour {

	public void OnClickBtn() {
		if (Time.timeScale == 0f) {
			Time.timeScale = 1f;
		} else {
			Time.timeScale = 0f;
		}
	}
}
