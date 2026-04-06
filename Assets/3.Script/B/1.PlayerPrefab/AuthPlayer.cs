using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class AuthPlayer : NetworkBehaviour
{
    public static AuthPlayer LocalInstance { get; private set; }

    public System.Action<bool, string, int, string> OnLoginResult;
    public System.Action<bool, string> OnSignupResult;
    public System.Action<bool, int> OnScoreSaveResult;

    [SyncVar] public string player_ID = "";
    [SyncVar] public bool is_authenticated = false;

    // 접속 순서 번호 (1번부터 시작)
    [SyncVar(hook = nameof(OnPlayerNumChange))]
    public int player_num = -1;

    // ── 서버에서만 사용하는 번호 관리 ──
    private static List<int> _usedNums = new List<int>(); // 사용 중인 번호들

    [Server]
    private int AssignPlayerNum()
    {
        int num = 1;
        while (_usedNums.Contains(num)) num++;
        _usedNums.Add(num);
        return num;
    }

    [Server]
    private void ReleasePlayerNum(int num)
    {
        _usedNums.Remove(num);
    }

    // ────────────────────────────────────────
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

    // 서버에서 플레이어가 나갈 때 번호 반납
    //public override void OnStopServer()
    //{
    //    base.OnStopServer();
    //    if (player_num != -1) ReleasePlayerNum(player_num);
    //}

    [Command]
    public void CmdLeaveRoom()
    {
        if (player_num != -1)
        {
            ReleasePlayerNum(player_num);
            player_num = -1;
        }
        // 클라이언트를 로그인 씬으로 보내야될거임
    }

    public void OnPlayerNumChange(int oldVal, int newVal)
    {
        // UI 갱신은 InGameUIManager에서 처리
        if (isLocalPlayer)
            Debug.Log($"[AuthPlayer] 내 플레이어 번호: {newVal}");
    }

    // ────────────────────────────────────────
    // 로그인
    // ────────────────────────────────────────
    [Command]
    public void CmdRequestLogin(string name, string password)
    {
        if (SQLManager.Instance == null)
        {
            TargetRpcLoginResult(connectionToClient, false, "", 0, "서버 오류");
            return;
        }

        int result = SQLManager.Instance.Login(name, password, out string nickname, out int score);

        if (result == 0)
        {
            player_ID = name;
            is_authenticated = true;
            //player_num = AssignPlayerNum(); // 번호 부여
            TargetRpcLoginResult(connectionToClient, true, nickname, score, "");
        }
        else
        {
            string msg = result == 1 ? "아이디 또는 비밀번호를 확인하세요" : "서버 오류가 발생했습니다";
            TargetRpcLoginResult(connectionToClient, false, "", 0, msg);
            StartCoroutine(DelayDisconnect(connectionToClient));
        }
    }

    /* 
    //RoomScene등에서 시작될 때 호출하여 번호 부여
    public override void OnStartServer()
    {
        base.OnStartServer();

        // 만약 로그인된 사용자라면 룸 입장 시점에 번호 부여
        if (is_authenticated && player_num == -1)
        {
            player_num = AssignPlayerNum();
            Debug.Log($"[Server] Player {player_ID} joined room. Assigned Num: {player_num}");
        }
    }
    */

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

    // ────────────────────────────────────────
    // 회원가입
    // ────────────────────────────────────────
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

    // ────────────────────────────────────────
    // 점수 저장
    // ────────────────────────────────────────
    [Command]
    public void CmdSaveAndResetScores(int round_score)
    {
        if (!is_authenticated || SQLManager.Instance == null) return;

        SQLManager.Instance.GetScore(player_ID, out int currentScore);
        int newTotal = currentScore + round_score;

        if (SQLManager.Instance.SetScore(player_ID, newTotal))
        {
            TargetRpcScoreSaveResult(connectionToClient, true, newTotal);
            Debug.Log($"[Server] {player_ID} score saved: {newTotal}");
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