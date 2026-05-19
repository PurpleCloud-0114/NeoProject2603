using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerDataSync : NetworkBehaviour
{
    [SyncVar]
    [SerializeField] private string _syncNickname;
    [SyncVar]
    [SerializeField] private int _syncScore;

    //이거 가져다 쓰면 됍니다.
    public string SyncNickname => _syncNickname;
    public int SyncScore => _syncScore;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        string myNickname = PlayerPrefs.GetString("PlayerNickname", "Unknown");
        int myScore = PlayerPrefs.GetInt("PlayerScore", 0);

        Debug.Log($"[LocalPlayer] PlayerPrefs 로드 완료 - 이름: {myNickname}, 점수: {myScore} 를 서버로 전송합니다.");

        CmdSendPlayerDataToServer(myNickname, myScore);
    }

    [Command]
    private void CmdSendPlayerDataToServer(string nickname, int score)
    {

        Debug.Log($"[Server] 클라이언트(NetId: {netId})로부터 데이터 수신 - 이름: {nickname}, 점수: {score}");
        _syncNickname = nickname;
        _syncScore = score;
        Debug.Log($"[Server] SyncVar 업데이트 완료 (NetId: {netId}) - _syncNickname: {_syncNickname}, _syncScore: {_syncScore}");
    }
}
