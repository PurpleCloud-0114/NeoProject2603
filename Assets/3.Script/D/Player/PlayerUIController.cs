using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerUIController : NetworkBehaviour {
	private PlayerCore _playerCore;
	private PlayerMovement _playerMovement;
	private PlayerItemController _playerItemController;

	[SerializeField] private Button _wingButton;
	[SerializeField] private Button _itemButton;
	[SerializeField] private Text _itemName;
	//[SerializeField] private Button _itemButton;

	//----- ¸Þ¼­µå
	private void Awake() {
		TryGetComponent(out _playerCore);
		TryGetComponent(out _playerMovement);
		TryGetComponent(out _playerItemController);
	}

	private void Start() {
		if (!isLocalPlayer && !RaceManager.Instance.isSinglePlay && !_playerCore.is_dummy) return;
		if(isLocalPlayer)BindButton();
		BindBtnAction();
		BindUI();
	}

	private void BindButton() {
		_wingButton = UIManager.Instance.BindWingButton();
		_itemButton = UIManager.Instance.BindItemButton();
		_itemName = UIManager.Instance.BindItemText();
	}

	private void BindBtnAction() {
		_wingButton.onClick.AddListener(_playerMovement.OpenWing);
		_wingButton.onClick.AddListener(DeActivateWingBtn);
		_itemButton.onClick.AddListener(_playerItemController.UseItem);
		_itemButton.onClick.AddListener(DeActivateItemBtn);
	}

	private void BindUI() {
		UIManager.Instance.BindStageProgressUI(transform);
	}

	public void ActivateWingBtn() => _wingButton.interactable = true;
	public void DeActivateWingBtn() => _wingButton.interactable = false;
	public void ActivateItemBtn() => _itemButton.interactable = true;
	public void DeActivateItemBtn() => _itemButton.interactable = false;
	public void SetItemNameOnUI(string name) => _itemName.text = name;
}
