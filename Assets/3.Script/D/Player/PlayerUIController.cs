using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class PlayerUIController : NetworkBehaviour {
	private PlayerCore _playerCore;

	[SerializeField] private Button _wingButton;
	[SerializeField] private Button _itemButton;
	[SerializeField] private TextMeshProUGUI _itemName;

	//----- 메서드
	private void Awake() {
		TryGetComponent(out _playerCore);
	}
	private void Start() {
		if (!isLocalPlayer && !RaceManager.Instance.isSinglePlay && !_playerCore.is_dummy) return;

		if(isLocalPlayer)BindButton();
		BindBtnAction();
		BindUI();
	}

	public override void OnStartClient() {
		base.OnStartClient();

		// 내 캐릭터든 남의 캐릭터든, 화면에 스폰될 때 UI 매니저에게 마커 생성 요청
		UIManager.Instance.CreatePlayerMarker(this.transform, isLocalPlayer);
	}

	private void OnEnable() {
		_playerCore.on_redzone_entered += ActivateWingBtn;
		_playerCore.on_item_acquired += ActivateItemBtn;
		_playerCore.on_item_acquired += SetItemNameOnUI;
	}
	private void OnDisable() {
		_playerCore.on_redzone_entered -= ActivateWingBtn;
		_playerCore.on_item_acquired -= ActivateItemBtn;
		_playerCore.on_item_acquired -= SetItemNameOnUI;
	}

	private void BindButton() {
		_wingButton = UIManager.Instance.BindWingButton();
		_itemButton = UIManager.Instance.BindItemButton();
		_itemName = UIManager.Instance.BindItemText();
	}

	private void BindBtnAction() {
		_wingButton.onClick.AddListener(() => _playerCore.on_wing_button_clicked());
		_wingButton.onClick.AddListener(DeActivateWingBtn);
		_itemButton.onClick.AddListener(() => _playerCore.on_item_button_clicked());
		_itemButton.onClick.AddListener(DeActivateItemBtn);
	}

	private void BindUI() {
		UIManager.Instance.BindJoystick(transform);
	}

	public void ActivateWingBtn() => _wingButton.interactable = true;
	public void DeActivateWingBtn() => _wingButton.interactable = false;
	public void ActivateItemBtn(IUseable _item) => _itemButton.interactable = true;
	public void DeActivateItemBtn() => _itemButton.interactable = false;
	public void SetItemNameOnUI(IUseable _item) => _itemName.text = _item.Name;
}
