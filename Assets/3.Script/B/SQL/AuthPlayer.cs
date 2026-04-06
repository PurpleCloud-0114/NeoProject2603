// Player Prefab에 NickNameSync, ScoreSync와 함께 부착합니다.
// 로그인/회원가입/점수저장 요청을 서버로 중계하는 핵심 스크립트입니다.

using UnityEngine;
using Mirror;

public class AuthPlayer : NetworkBehaviour
{
    // 클라이언트에서 콜백을 등록해서 결과를 받습니다
    public static AuthPlayer LocalInstance { get; private set; }

    // 로그인 결과 콜백: (성공여부, 닉네임, 점수, 메시지)
    public System.Action<bool, string, int, string> OnLoginResult;

    // 회원가입 결과 콜백: (성공여부, 메시지)
    public System.Action<bool, string> OnSignupResult;

    // 점수 저장 결과 콜백: (성공여부, 새 총점)
    public System.Action<bool, int> OnScoreSaveResult;

    // 서버에서 관리하는 이 플레이어의 인증 정보
    [SyncVar] public string player_ID = "";
    [SyncVar] public bool is_authenticated = false;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        LocalInstance = this;
        Debug.Log("[AuthPlayer] LocalPlayer ready.");
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        if (isLocalPlayer) LocalInstance = null;
    }

    // ────────────────────────────────────────
    // 로그인
    // ────────────────────────────────────────
    [Command]
    public void CmdRequestLogin(string name, string password)
    {
        if (SQLManager.Instance == null) { TargetRpcLoginResult(connectionToClient, false, "", 0, "서버 오류"); return; }

        int result = SQLManager.Instance.Login(name, password, out string nickname, out int score);

        if (result == 0)
        {
            player_ID = name;
            is_authenticated = true;
            TargetRpcLoginResult(connectionToClient, true, nickname, score, "");
        }
        else
        {
            string msg = result == 1 ? "아이디 또는 비밀번호를 확인하세요" : "서버 오류가 발생했습니다";
            TargetRpcLoginResult(connectionToClient, false, "", 0, msg);
            // 인증 실패 시 연결 끊기
            StartCoroutine(DelayDisconnect(connectionToClient));
        }
    }

    private System.Collections.IEnumerator DelayDisconnect(NetworkConnectionToClient conn)
    {
        yield return new WaitForSeconds(0.2f); // Rpc 전달 후 끊기
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
        if (SQLManager.Instance == null) { TargetRpcSignupResult(connectionToClient, false, "서버 오류"); return; }

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