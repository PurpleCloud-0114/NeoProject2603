using UnityEngine;
using Mirror;

public class ScoreSync : NetworkBehaviour
{
    [Header("Sync Variables")]
    [SyncVar] public string player_ID;

    [SyncVar(hook = nameof(OnScoreChange))]
    public int player_score = 0; // DB 누적 점수

    [SyncVar(hook = nameof(OnRoundScoreChange))]
    public int round_total_score = 0; // 현재 게임 세션 점수

    [Header("Local Display Variables (Inspector)")]
    [SerializeField] private int _player_score;
    [SerializeField] private int _player_roundscore;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        if (SQLManager.Instance.user_info != null)
        {
            string myName = SQLManager.Instance.user_info.user_name;
            CmdInitializePlayer(myName);
        }
    }

    [Command]
    private void CmdInitializePlayer(string name)
    {
        player_ID = name;

        if (SQLManager.Instance.GetScore(name))
        {
            player_score = SQLManager.Instance.user_info.user_score;
        }

        round_total_score = 0;
        Debug.Log($"[Server] Player {name} Init. DB Score: {player_score}");
    }

    [Server]
    public void AddRoundScore(int amount)
    {
        round_total_score += amount;
    }

    [Server]
    public void SaveAndResetScores()
    {
        int newTotal = player_score + round_total_score;

        if (SQLManager.Instance.SetScore(player_ID, newTotal))
        {
            player_score = newTotal;
            round_total_score = 0;
            Debug.Log($"[Server] {player_ID}'s score saved successfully.");
        }
    }

    public void OnScoreChange(int oldVal, int newVal) => _player_score = newVal;
    public void OnRoundScoreChange(int oldVal, int newVal) => _player_roundscore = newVal;
}