using UnityEngine;
using Mirror;

public class AuthPlayer : NetworkBehaviour
{
    [SyncVar] public string player_ID = "";
    [SyncVar] public bool is_authenticated = false;
    [SyncVar] public int player_num = -1;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        // 클라이언트: 로그인 시 저장된 내 ID를 서버에 보내서 초기화 요청
        if (SQLManager.Instance?.user_info != null)
        {
            CmdInitialize(SQLManager.Instance.user_info.user_name);
        }
    }

    [Command]
    public void CmdInitialize(string name)
    {
        player_ID = name;
        is_authenticated = true;

        // [핵심] 서버가 DB에서 해당 ID의 실제 닉네임과 점수를 가져옴
        if (SQLManager.Instance.GetNickname(name, out string dbNickname))
        {
            if (TryGetComponent<NickNameSync>(out var nickSync))
                nickSync.player_nickname = dbNickname; // SyncVar에 할당 -> 자동 동기화
        }

        if (SQLManager.Instance.GetScore(name, out int dbScore))
        {
            if (TryGetComponent<ScoreSync>(out var scoreSync))
                scoreSync.player_score = dbScore; // SyncVar에 할당 -> 자동 동기화
        }

        Debug.Log($"[Server] {name} 데이터 동기화 완료: {dbNickname}, {dbScore}");
    }
}