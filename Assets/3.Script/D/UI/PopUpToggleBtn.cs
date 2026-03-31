using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpToggleBtn : MonoBehaviour {
	[SerializeField] private GameObject[] _popupList;

	public void OnClickBtn() {
		foreach(GameObject popup in _popupList) {
			popup.SetActive(!popup.activeSelf);
		}
	}
}
