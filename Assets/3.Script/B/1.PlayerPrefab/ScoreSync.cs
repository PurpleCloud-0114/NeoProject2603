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

        if (AuthPlayer.LocalInstance != null)
        {
            AuthPlayer.LocalInstance.OnScoreSaveResult = (success, newTotal) =>
            {
                if (success) Debug.Log($"[ScoreSync] 점수 저장 완료: {newTotal}");
                else Debug.LogWarning("[ScoreSync] 점수 저장 실패");
            };
        }
    }

    // Player 점수 (서버 전용)
    [Server]
    public void ServerAddRoundScore(int amount)
    {
        round_total_score += amount;
    }

    [Server]
    public void ServerResetRoundScore()
    {
        round_total_score = 0;
    }

    // 라운드 종료 시 저장 요청
    public void RequestSaveScore()
    {
        if (!isLocalPlayer) return;
        if (AuthPlayer.LocalInstance == null) { Debug.LogWarning("[ScoreSync] AuthPlayer 없음"); return; }

        AuthPlayer.LocalInstance.CmdRequestSaveScore();
    }

    public void OnScoreChange(int oldVal, int newVal)
    {
        _player_score = newVal;
        if (newVal > oldVal) TriggerSlotRefresh(false);
    }

    // 라운드 점수 변경 시 호출 (상대방 점수도 여기서 감지됨)
    public void OnRoundScoreChange(int oldVal, int newVal)
    {
        _player_roundscore = newVal;
        if (newVal > oldVal) TriggerSlotRefresh(true);
    }

    private void TriggerSlotRefresh(bool isRoundScore)
    {
        AuthPlayer auth = GetComponent<AuthPlayer>();
        if (auth != null && PlayerListUIManager.Instance != null)
        {
            // player_num - 1 인덱스로 해당 슬롯 갱신 및 애니메이션 명령
            PlayerListUIManager.Instance.PlaySlotAnimation(auth.player_num - 1, isRoundScore);
        }
    }
}