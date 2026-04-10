using UnityEngine;
using Mirror;

public class RoomPlayer : NetworkRoomPlayer
{
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        GameObject btn = GameObject.Find("ReadyButton");
        if (btn != null)
        {
            btn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnClickReady);
        }
    }
    public void OnClickReady()
    {
        if (!isLocalPlayer) return;
        CmdChangeReadyState(!readyToBegin);
    }
}