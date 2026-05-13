using UnityEngine;
using Mirror;

public class PlayerItemController : NetworkBehaviour
{
    private PlayerCore _playerCore;
    public IUseable current_item = null;

    private void Awake()
    {
        TryGetComponent(out _playerCore);
    }

    private void OnEnable()
    {
        _playerCore.on_item_acquired += GetItem;
        _playerCore.on_item_button_clicked += UseItem;
    }
    private void OnDisable()
    {
        _playerCore.on_item_acquired -= GetItem;
        _playerCore.on_item_button_clicked -= UseItem;
    }

    private void GetItem(IUseable newItem)
    {
        current_item = newItem;
    }

    public void UseItem()
    {
        if (current_item != null)
        {
            // 1. 소리를 먼저 로컬에서 재생 (본인 화면에서 즉각 반응)
            PlayLocalItemSFX(current_item.Type);

            // 2. 서버에 아이템 사용 로직만 요청
            CmdUseItem(current_item.Type);
        }
        current_item = null;
    }

    [Command]
    private void CmdUseItem(ItemType itemType)
    {
        IUseable itemToUse = ItemManager.Instance.GetItemUseable(itemType);
        if (itemToUse != null)
        {
            itemToUse.Use(gameObject);
            // 여기서 Rpc 호출을 삭제했으므로 다른 사람에게는 소리가 나지 않습니다.
        }
    }

    // -------------- LOCAL SOUND (NEW) ---------------------

    private void PlayLocalItemSFX(ItemType type)
    {
        string sfxName = GetItemActivationSFX(type);
        if (!string.IsNullOrEmpty(sfxName))
        {
            // AudioManager의 인스턴스를 통해 2D/UI 사운드 방식으로 재생
            AudioManager.Instance.PlaySFX(sfxName);
        }
    }

    private string GetItemActivationSFX(ItemType type)
    {
        return type switch
        {
            ItemType.WeightAcceleration => "ItemWeightActivate",
            ItemType.Shockwave => "ItemShockwaveActivate",
            ItemType.Magnetic => "ItemMagneticActivate",
            ItemType.Spiderweb => "Jump",
            _ => ""
        };
    }
}