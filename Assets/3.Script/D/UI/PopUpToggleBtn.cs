using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpToggleBtn : MonoBehaviour {
	[SerializeField] private GameObject popup;

	public void OnClickBtn() {
		popup.SetActive(!popup.activeSelf);
	}
}
