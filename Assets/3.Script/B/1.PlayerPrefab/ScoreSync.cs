using UnityEngine;
using Mirror;

public class ScoreSync : NetworkBehaviour
{
    [SyncVar] public string player_ID;
    [SyncVar(hook = nameof(OnScoreChange))] public int player_score = 0;
    [SyncVar(hook = nameof(OnRoundScoreChange))] public int round_total_score = 0;

    [Server] public void ServerAddRoundScore(int amount) { round_total_score += amount; }
    [Server] public void ServerResetRoundScore() { round_total_score = 0; }

    public void OnScoreChange(int old, int newVal) { if (newVal > old) TriggerSlotRefresh(false); }
    public void OnRoundScoreChange(int old, int newVal) { if (newVal > old) TriggerSlotRefresh(true); }

    private void TriggerSlotRefresh(bool isroundScore)
    {
        AuthPlayer auth = GetComponent<AuthPlayer>();
        if (auth != null && PlayerListUIManager.Instance != null)
        {
            PlayerListUIManager.Instance.PlaySlotAnimation(auth.player_num, isroundScore);
        }
    }
}