using UnityEngine;
using Mirror;
using System;
using System.Collections.Generic;

public class AuthPlayer : NetworkBehaviour
{
    public static AuthPlayer LocalInstance { get; private set; }

    // 인덱스 기반 플레이어 캐싱 딕셔너리
    public static Dictionary<int, AuthPlayer> AllPlayers = new Dictionary<int, AuthPlayer>();

    public Action<bool, string, int, string> OnLoginResult;
    public Action<bool, string> OnSignupResult;
    public Action<bool, int> OnScoreSaveResult;

    [SyncVar] public string player_ID = "";
    [SyncVar] public bool is_authenticated = false;

    // 서버(RoomPlayer)에서 할당받은 인덱스
    [SyncVar(hook = nameof(OnPlayerNumChange))]
    public int player_num = -1;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        LocalInstance = this;
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

    public void OnPlayerNumChange(int oldVal, int newVal)
    {
        UnregisterPlayer(oldVal);
        RegisterPlayer(newVal);
        if (isLocalPlayer) Debug.Log($"[AuthPlayer] 내 번호 할당됨: {newVal}");
    }

    private void RegisterPlayer(int num)
    {
        if (num >= 0 && !AllPlayers.ContainsKey(num)) AllPlayers.Add(num, this);
    }

    private void UnregisterPlayer(int num)
    {
        if (num >= 0 && AllPlayers.ContainsKey(num)) AllPlayers.Remove(num);
    }

    // 외부 참조용 데이터 추출기, 인덱스만 줘서 매개변수 빼서 쓰면 됩니다
    public static bool TryGetPlayerData(int index, out string nickname, out int roundScore, out int totalScore)
    {
        nickname = ""; roundScore = 0; totalScore = 0;
        if (AllPlayers.TryGetValue(index, out AuthPlayer player))
        {
            if (player.TryGetComponent<NickNameSync>(out var n)) nickname = n.player_nickname;
            if (player.TryGetComponent<ScoreSync>(out var s))
            {
                roundScore = s.round_total_score;
                totalScore = s.player_score;
            }
            return true;
        }
        return false;
    }
    /* 예시
     void DisplayPlayerInfo(int targetIndex)
    {
        if (AuthPlayer.TryGetPlayerData(targetIndex, out string name, out int rScore, out int tScore))
        {
            // 여기서 name, rScore, tScore는 이미 해당 프리펩에서 뽑아온 최신 값입니다.
            Debug.Log($"{targetIndex}번 플레이어 이름: {name}, 라운드 점수: {rScore}");
        }
        else
        {
            Debug.Log("해당 인덱스의 플레이어를 찾을 수 없습니다.");
        }
    }
     */

    // 로그인 요청
    [Command]
    public void CmdRequestLogin(string name, string password)
    {
        if (SQLManager.Instance == null)
        {
            TargetRpcLoginResult(connectionToClient, false, "", 0, "서버 DB 연결 오류");
            return;
        }

        string nickname;
        int totalScore;
        int result = SQLManager.Instance.Login(name, password, out nickname, out totalScore);

        if (result == 0) // 로그인 성공
        {
            player_ID = name;
            is_authenticated = true;

            // 서버에서 SyncVar 값들을 세팅하면 클라이언트들로 자동 동기화됨
            if (TryGetComponent<NickNameSync>(out var nickSync))
            {
                nickSync.player_nickname = nickname;
            }
            if (TryGetComponent<ScoreSync>(out var scoreSync))
            {
                scoreSync.player_ID = name;
                scoreSync.player_score = totalScore;
            }

            TargetRpcLoginResult(connectionToClient, true, nickname, totalScore, "로그인 성공");
        }
        else
        {
            string msg = result == 1 ? "아이디 또는 비밀번호가 틀렸습니다" : "서버 오류";
            TargetRpcLoginResult(connectionToClient, false, "", 0, msg);
        }
    }

    [TargetRpc]
    private void TargetRpcLoginResult(NetworkConnectionToClient target, bool success, string nickname, int score, string message)
    {
        OnLoginResult?.Invoke(success, nickname, score, message);
    }

    // 회원가입 및 점수 저장
    [Command]
    public void CmdRequestSignup(string name, string password, string nickname)
    {
        if (SQLManager.Instance == null) { TargetRpcSignupResult(connectionToClient, false, "서버 연결 오류"); return; }
        int result = SQLManager.Instance.Signup(name, password, nickname);
        string msg = result switch { 0 => "회원가입 완료", 1 => "중복된 아이디", 2 => "중복된 닉네임", _ => "가입 실패" };
        TargetRpcSignupResult(connectionToClient, result == 0, msg);
    }

    [TargetRpc]
    private void TargetRpcSignupResult(NetworkConnectionToClient target, bool success, string message)
    { OnSignupResult?.Invoke(success, message); }

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
                scoreSync.ServerResetRoundScore();
                scoreSync.player_score = newTotal;
                TargetRpcScoreSaveResult(connectionToClient, true, newTotal);
            }
        }
    }

    [TargetRpc]
    private void TargetRpcScoreSaveResult(NetworkConnectionToClient target, bool success, int newTotal)
    { OnScoreSaveResult?.Invoke(success, newTotal); }
}