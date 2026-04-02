using UnityEngine;
using Mirror;

public class ScoreSync : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnScoreChange))]
    public int player_score = 0;

    [SerializeField] private int _player_score;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        int score = SQLManager.Instance.user_info.user_score;
        CommandSendScoreToServer(score);
    }

    public void SetScore(int score) // 오타 수정: SetScrore → SetScore
    {
        _player_score = score;
    }

    [Command]
    public void CommandSendScoreToServer(int score)
    {
        player_score = score;
        string userName = SQLManager.Instance.user_info.user_name;
        SQLManager.Instance.SetScore(userName, score);
    }

    public void OnScoreChange(int oldScore, int newScore)
    {
        SetScore(newScore);
    }
}