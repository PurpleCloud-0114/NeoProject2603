using UnityEngine;
using Mirror;

public class PlayerListUIManager : MonoBehaviour
{
    public static PlayerListUIManager Instance;

    [Header("UI Slots (Assign 10 Slots)")]
    [SerializeField] private PlayerSlotUI[] _slots;

    [Header("Settings")]
    [SerializeField] private float _updateInterval = 1f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        InvokeRepeating(nameof(UpdateAllSlots), 0.5f, _updateInterval);
    }

    public void UpdateAllSlots()
    {
        // 1. 모든 슬롯 초기화
        foreach (PlayerSlotUI slot in _slots) if (slot != null) slot.ClearSlot();

        // 2. RoomManager에 등록된 모든 roomSlots를 순회
        // RoomManager가 싱글톤이므로 singleton으로 접근 가능합니다.
        var roomManager = NetworkManager.singleton as NetworkRoomManager;
        if (roomManager == null) return;

        foreach (var roomPlayer in roomManager.roomSlots)
        {
            if (roomPlayer == null) continue;

            // 3. roomPlayer의 index를 슬롯 번호로 사용
            int realIndex = roomPlayer.index;

            if (realIndex >= 0 && realIndex < _slots.Length && _slots[realIndex] != null)
            {
                // 4. 같은 오브젝트에 붙어있는 컴포넌트들 가져오기
                AuthPlayer auth = roomPlayer.GetComponent<AuthPlayer>();
                ScoreSync score = roomPlayer.GetComponent<ScoreSync>();
                NickNameSync nick = roomPlayer.GetComponent<NickNameSync>();

                if (auth != null && score != null && nick != null)
                {
                    _slots[realIndex].SetPlayer(auth, score, nick);
                }
            }
        }
    }

    public void PlaySlotAnimation(int index, bool isRoundScore)
    {
        if (index >= 0 && index < _slots.Length)
        {
            if (_slots[index] != null && _slots[index].gameObject.activeSelf)
            {
                _slots[index].RefreshUI();
                _slots[index].PlayBounce(isRoundScore);
            }
        }
    }
}