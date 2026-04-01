using UnityEngine;
using Mirror;

public class ScoreSync : NetworkBehaviour
{
    [SyncVar(hook = "OnScoreChange")]
    public int player_score = 0;

    [SerializeField] private int _player_score;
    public override void OnStartClient()
    {
        base.OnStartClient();
        int rate = SQLManager.Instance.user_info.user_score;
        CommandSendScoreToServer(rate);
    }
    public void SetScrore(int score)
    {
        _player_score = score;
    }
    [Command]
    public void CommandSendScoreToServer(int score)
    {
        player_score = score;
    }

    public void OnScoreChange(int oldscore, int newscore)
    {
        SetScrore(newscore);
    }
}
