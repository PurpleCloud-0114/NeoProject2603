using UnityEngine;
using Mirror;

public class ScoreSync : NetworkBehaviour
{
    [Header("Sync Variables")]
    [SyncVar] public string player_ID;

    [SyncVar(hook = nameof(OnScoreChange))]
    public int player_score = 0;

    [SyncVar(hook = nameof(OnRoundScoreChange))]
    public int round_total_score = 0;

    [Header("Local Display (Inspector 확인용)")]
    [SerializeField] private int _player_score;
    [SerializeField] private int _player_roundscore;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        // AuthPlayer의 점수 저장 결과를 ScoreSync에 반영
        if (AuthPlayer.LocalInstance != null)
        {
            AuthPlayer.LocalInstance.OnScoreSaveResult = (success, newTotal) =>
            {
                if (success) Debug.Log($"[ScoreSync] 점수 저장 완료: {newTotal}");
                else Debug.LogWarning("[ScoreSync] 점수 저장 실패");
            };
        }
    }

    // 라운드 점수 추가 (클라이언트에서 호출)
    public void AddRoundScore(int amount)
    {
        if (!isLocalPlayer) return;
        CmdAddRoundScore(amount);
    }

    [Command]
    private void CmdAddRoundScore(int amount)
    {
        round_total_score += amount;
    }

    // 라운드 종료 시 저장 (클라이언트에서 호출)
    public void SaveAndResetScores()
    {
        if (!isLocalPlayer) return;
        if (AuthPlayer.LocalInstance == null) { Debug.LogWarning("[ScoreSync] AuthPlayer 없음"); return; }

        AuthPlayer.LocalInstance.CmdSaveAndResetScores(round_total_score);
        CmdResetRoundScore();
    }

    [Command]
    private void CmdResetRoundScore()
    {
        round_total_score = 0;
    }

    public void OnScoreChange(int oldVal, int newVal) => _player_score = newVal;
    public void OnRoundScoreChange(int oldVal, int newVal) => _player_roundscore = newVal;
}