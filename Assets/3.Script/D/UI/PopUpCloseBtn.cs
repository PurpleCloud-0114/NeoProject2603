using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpCloseBtn : MonoBehaviour {
	[SerializeField] private GameObject popup;

	public void OnClickBtn() {
		popup.SetActive(false);
	}
}
