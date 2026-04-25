using System.Collections;
using UnityEngine;
using Mirror;

public class RoomManagement : NetworkRoomManager
{
    public float start_delay = 3f;
    Coroutine _startCoroutine;
    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
        Debug.Log("[RoomManagement] 클라이언트 접속");
    }

    public override void OnRoomServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnRoomServerAddPlayer(conn);
        Debug.Log("[RoomManagement] OnRoomServerAddPlayer 호출됨");
        RefreshLobbyUI(); 
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        Debug.Log("[RoomManagement] 클라이언트 연결 끊김");
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
        RefreshLobbyUI();
    }

    public override void ReadyStatusChanged()
    {
        base.ReadyStatusChanged(); 
        Debug.Log("[RoomManagement] Ready 상태 변경됨");
        RefreshLobbyUI(); 
    }

    public void RefreshLobbyUI()
    {
        if (LobbyTextUI.Instance == null)
        {
            Debug.LogWarning("[RoomManagement] RefreshLobbyUI: LobbyTextUI.Instance가 null");
            return;
        }

        LobbyTextUI.Instance.ClearAllUI();

        int slot_index = 0;
        foreach (NetworkRoomPlayer slot in roomSlots)
        {
            RoomPlayer rp = slot as RoomPlayer;
            if (rp == null) continue;

            string player_name = $"WoWPlayer {slot_index + 1}";
            bool _isReady = rp.readyToBegin;
            Debug.Log($"[RoomManagement] UI 갱신 slot={slot_index}, name={player_name }, isReady={_isReady}");

            LobbyTextUI.Instance.UpdateUI(slot_index, player_name, _isReady);
            rp.RpcUpdateUI(slot_index, player_name, _isReady);
            slot_index++;
        }
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
        CancelAllPlayersCountdown();
    }

    bool AllPlayersReady()
    {
        foreach (NetworkRoomPlayer player in roomSlots)
        {
            if (player == null) continue;
            if (player.connectionToClient == null) continue;
            if (!player.readyToBegin) return false;
        }
        return true;
    }

    IEnumerator StartGameCountdown_co()
    {
        float timer = start_delay;
        while (timer > 0f)
        {
            if (!AllPlayersReady())
            {
                Debug.Log("Ready 깨짐 → 카운트다운 취소");
                _startCoroutine = null;
                CancelAllPlayersCountdown();
                yield break;
            }
            UpdateAllPlayersCountdown(Mathf.CeilToInt(timer));
            yield return new WaitForSeconds(1f);
            timer -= 1f;
        }
        Debug.Log("게임 시작!");
        UpdateAllPlayersCountdown(0);
        ServerChangeScene(GameplayScene);
        _startCoroutine = null;
    }

    void UpdateAllPlayersCountdown(int time)
    {
        foreach (var player in roomSlots)
        {
            if (player == null) continue;
            if (player.connectionToClient == null) continue;
            RoomPlayer rp = player as RoomPlayer;
            if (rp == null) continue;
            rp.RpcSetCountdown(time);
        }
    }

    void CancelAllPlayersCountdown()
    {
        foreach (var player in roomSlots)
        {
            if (player == null) continue;
            if (player.connectionToClient == null) continue;
            RoomPlayer rp = player as RoomPlayer;
            if (rp == null) continue;
            rp.RpcCancelCountdown();
        }
    }
}