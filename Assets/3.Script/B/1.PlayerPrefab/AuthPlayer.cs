using UnityEngine;
using Mirror;
using System;
using System.Collections.Generic;

public class AuthPlayer : NetworkBehaviour
{
    public static AuthPlayer LocalInstance { get; private set; }
    public static Dictionary<int, AuthPlayer> AllPlayers = new Dictionary<int, AuthPlayer>();

    public Action<bool, int> OnScoreSaveResult;

    [SyncVar] public string player_ID = "";
    [SyncVar] public bool is_authenticated = false;

    [SyncVar(hook = nameof(OnPlayerNumChange))]
    public int player_num = -1;

    private static List<int> _usedNums = new List<int>();

    // ── 생명주기 ──
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        LocalInstance = this;

        // 로그인 씬에서 저장해둔 user_info로 서버에 초기화 요청
        if (SQLManager.Instance?.user_info != null)
        {
            CmdInitialize(SQLManager.Instance.user_info.user_name);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        RegisterPlayer(player_num);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        if (isLocalPlayer) LocalInstance = null;
        UnregisterPlayer(player_num);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        if (player_num != -1) ReleasePlayerNum(player_num);
    }

    // ── player_num 관리 ──
    [Server]
    private int AssignPlayerNum()
    {
        int num = 1;
        while (_usedNums.Contains(num)) num++;
        _usedNums.Add(num);
        return num;
    }

    [Server]
    private void ReleasePlayerNum(int num) { _usedNums.Remove(num); }

    public void OnPlayerNumChange(int oldVal, int newVal)
    {
        UnregisterPlayer(oldVal);
        RegisterPlayer(newVal);
        if (isLocalPlayer) Debug.Log($"[AuthPlayer] 내 번호: {newVal}");
    }

    private void RegisterPlayer(int num)
    {
        if (num >= 0 && !AllPlayers.ContainsKey(num)) AllPlayers.Add(num, this);
    }

    private void UnregisterPlayer(int num)
    {
        if (num >= 0 && AllPlayers.ContainsKey(num)) AllPlayers.Remove(num);
    }

    // ── 초기화 (OnStartLocalPlayer에서 호출) ──
    // 로그인은 이미 완료된 상태 — 서버에 player_num, SyncVar만 세팅
    [Command]
    private void CmdInitialize(string name)
    {
        player_ID = name;
        is_authenticated = true;
        player_num = AssignPlayerNum();

        // ScoreSync, NickNameSync에 로그인 데이터 반영
        if (TryGetComponent<ScoreSync>(out var scoreSync))
        {
            SQLManager.Instance.GetScore(name, out int score);
            scoreSync.player_ID = name;
            scoreSync.player_score = score;
        }
        if (TryGetComponent<NickNameSync>(out var nickSync))
        {
            // SQLManager.user_info는 서버에도 있으므로 바로 참조
            if (SQLManager.Instance?.user_info != null)
                nickSync.player_nickname = SQLManager.Instance.user_info.user_nickname;
        }

        Debug.Log($"[Server] {name} 초기화 완료. player_num: {player_num}");
    }

    // ── 외부 데이터 조회 ──
    public static bool TryGetPlayerData(int index, out string nickname, out int roundscore, out int totalScore)
    {
        nickname = ""; roundscore = 0; totalScore = 0;
        if (AllPlayers.TryGetValue(index, out AuthPlayer player))
        {
            if (player.TryGetComponent<NickNameSync>(out var n)) nickname = n.player_nickname;
            if (player.TryGetComponent<ScoreSync>(out var s))
            {
                roundscore = s.round_total_score;
                totalScore = s.player_score;
            }
            return true;
        }
        return false;
    }

    // ── 점수 저장 (온라인씬, Player Prefab에서 호출) ──
    [Command]
    public void CmdRequestSaveScore()
    {
        if (!is_authenticated || SQLManager.Instance == null) return;
        if (!TryGetComponent<ScoreSync>(out var scoreSync)) return;

        SQLManager.Instance.GetScore(player_ID, out int currentScore);
        int newTotal = currentScore + scoreSync.round_total_score;

        if (SQLManager.Instance.SetScore(player_ID, newTotal))
        {
            scoreSync.ServerResetRoundScore();
            scoreSync.player_score = newTotal;
            TargetRpcScoreSaveResult(connectionToClient, true, newTotal);
            Debug.Log($"[Server] {player_ID} 점수 저장: {newTotal}");
        }
        else
        {
            TargetRpcScoreSaveResult(connectionToClient, false, currentScore);
        }
    }

    [TargetRpc]
    private void TargetRpcScoreSaveResult(NetworkConnectionToClient target, bool success, int newTotal)
    {
        OnScoreSaveResult?.Invoke(success, newTotal);
    }
}