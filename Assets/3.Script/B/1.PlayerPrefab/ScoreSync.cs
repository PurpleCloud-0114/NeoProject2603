using UnityEngine;
using Mirror;

public class ScoreSync : NetworkBehaviour
{
    [SyncVar] public string player_ID;
    [SyncVar(hook = nameof(OnScoreChange))] public int player_score = 0;
    [SyncVar(hook = nameof(OnRoundScoreChange))] public int round_total_score = 0;

    [Server] public void ServerAddRoundScore(int amount) { round_total_score += amount; }
    [Server] public void ServerResetRoundScore() { round_total_score = 0; }

    public void OnScoreChange(int old, int newVal) { if (newVal != old) TriggerSlotRefresh(false); }
    public void OnRoundScoreChange(int old, int newVal) { if (newVal != old) TriggerSlotRefresh(true); }

    private void TriggerSlotRefresh(bool isroundScore)
    {
        NetworkRoomPlayer roomPlayer = GetComponent<NetworkRoomPlayer>();

        // 인덱스를 기반으로 슬롯 애니메이션 실행
        if (roomPlayer != null && PlayerListUIManager.Instance != null)
        {
            PlayerListUIManager.Instance.PlaySlotAnimation(roomPlayer.index, isroundScore);
        }

        // 내 화면의 인게임 점수 팝업 갱신
        if (isLocalPlayer && InGameUIManager.Instance != null)
        {
            InGameUIManager.Instance.PlayScorePop(isroundScore ? round_total_score : player_score, isroundScore);
        }
    }
}