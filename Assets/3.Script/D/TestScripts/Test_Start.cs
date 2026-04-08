using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_Start : MonoBehaviour {
	public void OnBtnClick() {
		gameObject.SetActive(false);
		TestManager.Instance.Initialize();
		CutsceneManager.Instance.PlayStartCutscene();
	}
}
