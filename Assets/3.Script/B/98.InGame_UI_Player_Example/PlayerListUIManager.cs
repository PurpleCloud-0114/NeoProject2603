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
        // 1. 먼저 모든 슬롯을 비활성화/초기화합니다.
        foreach (PlayerSlotUI slot in _slots)
        {
            if (slot != null) slot.ClearSlot();
        }

        // 2. 현재 씬에 존재하는 모든 AuthPlayer 컴포넌트를 찾습니다.
        AuthPlayer[] players = FindObjectsByType<AuthPlayer>(FindObjectsSortMode.None);

        if (players == null || players.Length == 0) return;

        // 3. 각 플레이어의 player_num에 맞춰 슬롯에 데이터를 채웁니다.
        foreach (AuthPlayer auth in players)
        {
            // player_num이 할당되지 않았거나 범위를 벗어나면 스킵
            if (auth.player_num <= 0 || auth.player_num > _slots.Length) continue;

            // player_num은 1부터 시작하므로 인덱스는 -1을 해줍니다.
            int index = auth.player_num - 1;

            if (_slots[index] != null)
            {
                ScoreSync score = auth.GetComponent<ScoreSync>();
                NickNameSync nick = auth.GetComponent<NickNameSync>();

                if (score != null && nick != null)
                {
                    // 해당 슬롯에 데이터 전달 및 활성화
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