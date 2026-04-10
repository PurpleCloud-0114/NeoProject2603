using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class PlayerListUIManager : MonoBehaviour
{
    public static PlayerListUIManager Instance;

    [Header("UI Slots (Assign 10 Slots)")]
    [SerializeField] private PlayerSlotUI[] _slots;

    [Header("Settings")]
    [SerializeField] private float _updateInterval = 1f; // 갱신 주기 (1초)

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        InvokeRepeating(nameof(UpdateAllSlots), 0.5f, _updateInterval);
    }

    /// <summary>
    /// 모든 플레이어 정보를 찾아서 해당되는 슬롯(player_num 기반)에 배치합니다.
    /// </summary>
    public void UpdateAllSlots()
    {
        foreach (PlayerSlotUI slot in _slots) if (slot != null) slot.ClearSlot();

        foreach (var pair in AuthPlayer.AllPlayers)
        {
            int index = pair.Key; // player_num
            AuthPlayer auth = pair.Value;

            if (index >= 0 && index < _slots.Length && _slots[index] != null)
            {
                ScoreSync score = auth.GetComponent<ScoreSync>();
                NickNameSync nick = auth.GetComponent<NickNameSync>();
                if (score != null && nick != null)
                {
                    _slots[index].SetPlayer(auth, score, nick);
                }
            }
        }
    }

    /// <summary>
    /// 특정 인덱스의 슬롯을 즉시 갱신하고 애니메이션을 실행합니다.
    /// </summary>
    /// <param name="index">플레이어 번호 기반 인덱스 (player_num - 1)</param>
    /// <param name="isRoundScore">라운드 점수 변화인지 여부</param>
    public void PlaySlotAnimation(int index, bool isRoundScore)
    {
        // 인덱스 범위 체크 (10개 슬롯 기준)
        if (index >= 0 && index < _slots.Length)
        {
            if (_slots[index] != null)
            {
                // 1. 해당 슬롯의 텍스트들을 최신 데이터로 갱신
                _slots[index].RefreshUI();

                // 2. 해당 슬롯 오브젝트에 통통 튀는 애니메이션 실행
                _slots[index].PlayBounce(isRoundScore);
            }
        }
    }
}