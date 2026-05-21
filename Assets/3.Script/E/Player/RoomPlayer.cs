using UnityEngine;
using Mirror;

public class RoomPlayer : NetworkRoomPlayer
{
    [SyncVar]
    public string NicknameSync = "";
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        string saved_nickname = PlayerPrefs.GetString("PlayerNickname", "NoName");
        CmdSetNickname(saved_nickname);
        GameObject btn = GameObject.Find("ReadyButton");
        if (btn != null)
        {
            btn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnClickReady);
        }
    }
    [Command]
    private void CmdSetNickname(string nickname)
    {
        NicknameSync = nickname;
        if(NetworkManager.singleton is RoomManagement rm)
        {
            rm.RefreshLobbyUI();
        }
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        CmdRequestUIRefresh();
    }
    [Command]
    void CmdRequestUIRefresh()
    {
        if (NetworkManager.singleton is RoomManagement rm)
        {
            rm.RefreshLobbyUI();
        }
    }

    public void OnClickReady()
    {
        if (!isLocalPlayer) return;
        CmdChangeReadyState(!readyToBegin);
    }
    [ClientRpc]
    public void RpcUpdateUI(int slot_index, string player_name, bool _isReady)
    {
        if (LobbyTextUI.Instance == null)
        {
            Debug.LogWarning("[RoomPlayer] RpcUpdateUI: LobbyTextUI.Instance°¡ null");
            return; 
        }
        Debug.Log($"[RoomPlayer] RpcUpdateUI ½ÇÇà: index={slot_index}, name={player_name}, isReady={_isReady}");
        LobbyTextUI.Instance.UpdateUI(slot_index, player_name, _isReady);
    }
    [ClientRpc]
    public void RpcSetCountdown(int time)
    {
        LobbyTextUI.Instance?.ui?.SetTime(time);
    }
    [ClientRpc]
    public void RpcCancelCountdown()
    {
        LobbyTextUI.Instance?.ui?.Hide();
    }
}