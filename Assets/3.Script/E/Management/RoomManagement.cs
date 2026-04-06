
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class RoomManagement : NetworkRoomManager
{
    public float start_delay = 3f;
    Coroutine _startCoroutine;
    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        Debug.Log("클라이언트 접속 시도");
    }
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        Debug.Log("클라이언트 연결 끊김");

        if (conn.identity != null)
        {
            var roomPlayer = conn.identity.GetComponent<NetworkRoomPlayer>();

            if (roomPlayer != null)
            {
                roomSlots.Remove(roomPlayer);
                Destroy(roomPlayer.gameObject);
            }
        }
        base.OnServerDisconnect(conn);
    }
    public override void OnRoomServerPlayersReady()
    {
        Debug.Log("모두 Ready → 카운트다운 시작");

        if (_startCoroutine == null)
            _startCoroutine = StartCoroutine(StartGameCountdown_co());
    }
    public override void OnRoomServerPlayersNotReady()
    {
        Debug.Log("누군가 Ready 취소 → 카운트다운 중단");

        if (_startCoroutine != null)
        {
            StopCoroutine(_startCoroutine);
            _startCoroutine = null;
        }
    }
    bool AllPlayersReady()
    {
        foreach (var player in roomSlots)
        {
            if (player == null) continue;

            if (player.connectionToClient == null)
                continue; // 끊긴 유저 무시

            if (!player.readyToBegin)
                return false;
        }

        return true;
    }
    IEnumerator StartGameCountdown_co()
    {
        float timer = start_delay;

        while (timer > 0f)
        {
            // 중간에 조건 깨졌는지 체크
            if (!AllPlayersReady())
            {
                Debug.Log("Ready 깨짐 → 카운트다운 취소");
                _startCoroutine = null;
                yield break;
            }

            Debug.Log($"게임 시작까지: {timer:F1}초");
            yield return new WaitForSeconds(1f);
            timer -= 1f;
        }

        Debug.Log("게임 시작!");
        ServerChangeScene(GameplayScene);
        _startCoroutine = null;
    }
    void UpdateAllPlayersCountdown(int time)
    {
        foreach (var player in roomSlots)
        {
            if (player == null) continue;
            if (player.connectionToClient == null) continue;

            //var p = player as CustomRoomPlayer;
            //p?.RpcUpdateCountdown(time);
        }
    }

    void CancelAllPlayersCountdown()
    {
        foreach (var player in roomSlots)
        {
            if (player == null) continue;
            if (player.connectionToClient == null) continue;

            //var p = player as CustomRoomPlayer;
            //p?.RpcCancelCountdown();
        }
    }
}
