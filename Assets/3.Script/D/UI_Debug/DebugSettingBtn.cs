sing System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugSettingBtn : MonoBehaviour {
	[SerializeField] private GameObject _debugPanel;
	
	public void OnBtnClick() {
		_debugPanel.SetActive(!_debugPanel.activeSelf);
	}
}
