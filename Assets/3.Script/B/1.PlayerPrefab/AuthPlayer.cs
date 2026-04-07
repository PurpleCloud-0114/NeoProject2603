using UnityEngine;
using Mirror;
using System;
using System.Collections.Generic;

public class AuthPlayer : NetworkBehaviour
{
    public static AuthPlayer LocalInstance { get; private set; }

    public Action<bool, string, int, string> OnLoginResult;
    public Action<bool, string> OnSignupResult;
    public Action<bool, int> OnScoreSaveResult;

    [SyncVar] public string player_ID = "";
    [SyncVar] public bool is_authenticated = false;

    [SyncVar(hook = nameof(OnPlayerNumChange))]
    public int player_num = -1;

    private static List<int> _used_playernums = new List<int>();

    [Server]
    private int AssignPlayerNum() //번호부여. 방에 들어왔을때 호출해야됌
    {
        int num = 1;
        while (_used_playernums.Contains(num)) num++;
        _used_playernums.Add(num);
        return num;
    }

    [Server]
    private void ReleasePlayerNum(int num)
    {
        _used_playernums.Remove(num);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        LocalInstance = this;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        if (isLocalPlayer) LocalInstance = null;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        if (player_num != -1) ReleasePlayerNum(player_num);
    }

    [Command]
    public void CmdLeaveRoom()
    {
        if (player_num != -1)
        {
            ReleasePlayerNum(player_num);
            player_num = -1;
        }
    }

    public void OnPlayerNumChange(int oldVal, int newVal)
    {
        if (isLocalPlayer)
            Debug.Log($"[AuthPlayer] 내 플레이어 번호: {newVal}");
    }

    // 로그인
    [Command]
    public void CmdRequestLogin(string name, string password)
    {
        if (SQLManager.Instance == null)
        {
            TargetRpcLoginResult(connectionToClient, false, "", 0, "서버 오류");
            return;
        }

        int result = SQLManager.Instance.Login(name, password, out string nickname, out int score);

        if (result == 0) // 로그인 성공
        {
            player_ID = name;
            is_authenticated = true;

            // [핵심 변경점] 서버가 직접 NickNameSync와 ScoreSync에 값을 세팅합니다.
            if (TryGetComponent<NickNameSync>(out var nickSync))
            {
                nickSync.player_nickname = nickname;
            }
            if (TryGetComponent<ScoreSync>(out var scoreSync))
            {
                scoreSync.player_ID = name;
                scoreSync.player_score = score;
            }

            TargetRpcLoginResult(connectionToClient, true, nickname, score, "");
        }
        else
        {
            string msg = result == 1 ? "아이디 또는 비밀번호를 확인하세요" : "서버 오류가 발생했습니다";
            TargetRpcLoginResult(connectionToClient, false, "", 0, msg);
            StartCoroutine(DelayDisconnect(connectionToClient));
        }
    }

    private System.Collections.IEnumerator DelayDisconnect(NetworkConnectionToClient conn)
    {
        yield return new WaitForSeconds(0.2f);
        conn.Disconnect();
    }

    [TargetRpc]
    private void TargetRpcLoginResult(NetworkConnectionToClient target, bool success, string nickname, int score, string message)
    {
        OnLoginResult?.Invoke(success, nickname, score, message);
    }

    // 회원가입
    [Command]
    public void CmdRequestSignup(string name, string password, string nickname)
    {
        if (SQLManager.Instance == null)
        {
            TargetRpcSignupResult(connectionToClient, false, "서버 오류");
            return;
        }

        int result = SQLManager.Instance.Signup(name, password, nickname);
        switch (result)
        {
            case 0: TargetRpcSignupResult(connectionToClient, true, "회원가입이 완료되었습니다."); break;
            case 1: TargetRpcSignupResult(connectionToClient, false, "이미 사용 중인 아이디입니다"); break;
            case 2: TargetRpcSignupResult(connectionToClient, false, "이미 사용 중인 닉네임입니다"); break;
            default: TargetRpcSignupResult(connectionToClient, false, "회원가입에 실패했습니다. 다시 시도하세요"); break;
        }
    }

    [TargetRpc]
    private void TargetRpcSignupResult(NetworkConnectionToClient target, bool success, string message)
    {
        OnSignupResult?.Invoke(success, message);
    }

    // 점수 저장
    [Command]
    public void CmdRequestSaveScore()
    {
        if (!is_authenticated || SQLManager.Instance == null) return;

        if (TryGetComponent<ScoreSync>(out var scoreSync))
        {
            int scoreToSave = scoreSync.round_total_score;

            SQLManager.Instance.GetScore(player_ID, out int currentScore);
            int newTotal = currentScore + scoreToSave;

            if (SQLManager.Instance.SetScore(player_ID, newTotal))
            {
                // DB 저장이 성공했을 때만 점수를 리셋
                scoreSync.ServerResetRoundScore();
                scoreSync.player_score = newTotal; // 누적 점수도 갱신

                TargetRpcScoreSaveResult(connectionToClient, true, newTotal);
                Debug.Log($"[Server] {player_ID} score saved: {newTotal}");
            }
            else
            {
                TargetRpcScoreSaveResult(connectionToClient, false, currentScore);
            }
        }
    }

    [TargetRpc]
    private void TargetRpcScoreSaveResult(NetworkConnectionToClient target, bool success, int newTotal)
    {
        OnScoreSaveResult?.Invoke(success, newTotal);
    }
}