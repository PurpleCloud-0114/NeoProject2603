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
	[SerializeField] private Image _itemImg;
	[SerializeField] private Sprite _noneImg;

	//----- 메서드
	private void Awake() {
		TryGetComponent(out _playerCore);
	}
	private void Start() {
		if (!isLocalPlayer && !RaceManager.Instance.isSinglePlay && !_playerCore.is_dummy) return;

		if(isLocalPlayer)BindButton();
		BindBtnAction();
	}

	public override void OnStartClient() {
		base.OnStartClient();

		// 내 캐릭터든 남의 캐릭터든, 화면에 스폰될 때 UI 매니저에게 마커 생성 요청
		UIManager.Instance.CreatePlayerMarker(this.transform, isLocalPlayer);
	}

	private void OnEnable() {
		_playerCore.on_wingPoint_entered += PlayWarningText;
		_playerCore.on_wingPoint_entered += BlinkStartWingButton;
		_playerCore.on_redzone_entered += ActivateWingBtn;
		_playerCore.on_redzone_entered += BlinkStopWingButton;
		_playerCore.on_redzone_entered += StopWarningText;
		_playerCore.on_endpoint_landed += StopTimer;
		_playerCore.on_item_acquired += ActivateItemBtn;
		_playerCore.on_item_acquired += SetItemImgOnUI;
		_playerCore.on_item_button_clicked += UsedItemImgOnUI;
	}
	private void OnDisable() {
		_playerCore.on_wingPoint_entered -= BlinkStartWingButton;
		_playerCore.on_wingPoint_entered -= PlayWarningText;
		_playerCore.on_redzone_entered -= ActivateWingBtn;
		_playerCore.on_redzone_entered -= BlinkStopWingButton;
		_playerCore.on_redzone_entered -= StopWarningText;
		_playerCore.on_endpoint_landed -= StopTimer;
		_playerCore.on_item_acquired -= ActivateItemBtn;
		_playerCore.on_item_acquired -= SetItemImgOnUI;
		_playerCore.on_item_button_clicked -= UsedItemImgOnUI;
	}

	private void BindButton() {
		_wingButton = UIManager.Instance.BindWingButton();
		_itemButton = UIManager.Instance.BindItemButton();
		_itemImg = UIManager.Instance.BindItemImage();
	}

	private void BindBtnAction() {
		_wingButton.onClick.AddListener(() => _playerCore.on_wing_button_clicked());
		_wingButton.onClick.AddListener(DeActivateWingBtn);
		_itemButton.onClick.AddListener(() => _playerCore.on_item_button_clicked());
		_itemButton.onClick.AddListener(DeActivateItemBtn);
	}

	public void ActivateWingBtn() => _wingButton.interactable = true;
	public void DeActivateWingBtn() => _wingButton.interactable = false;
	public void ActivateItemBtn(IUseable _item) => _itemButton.interactable = true;
	public void DeActivateItemBtn() => _itemButton.interactable = false;
	public void SetItemImgOnUI(IUseable _item) => _itemImg.sprite = _item.Item_Image;
	public void UsedItemImgOnUI() => _itemImg.sprite = _noneImg;
	public void StopTimer() => UIManager.Instance.StopTimer();

	public void PlayWarningText() => UIManager.Instance.PlayIntroSequence();
	public void StopWarningText() => UIManager.Instance.PlayOutroSequence();	
	public void BlinkStartWingButton() => UIManager.Instance.StartBlinking();
	public void BlinkStopWingButton() => UIManager.Instance.StopBlinking();
}
