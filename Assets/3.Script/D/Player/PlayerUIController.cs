using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour {
	private PlayerState _playerState;

	[SerializeField] private Button _wingButton;
	//[SerializeField] private Button _itemButton;

	//----- ¸Þ¼­µå
	private void Awake() {
		TryGetComponent(out _playerState);
	}

	private void Start() {
		BindBtnAction();
	}

	private void BindBtnAction() {
		_wingButton.onClick.AddListener(_playerState.OpenWing);
		_wingButton.onClick.AddListener(DeActivateWingBtn);
	}

	public void ActivateWingBtn() => _wingButton.interactable = true;
	public void DeActivateWingBtn() => _wingButton.interactable = false;
}
