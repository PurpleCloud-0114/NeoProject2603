
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RoomPlayer : NetworkRoomPlayer
{
    public CountDownUI ui;
    public override void OnStartClient()
    {
        base.OnStartClient();
        // ¾À¿¡¼­ UI Ã£±â
    }

    [ClientRpc]
    public void RpcUpdateCountdown(int time)
    {
        ui?.SetTime(time);
    }

    [ClientRpc]
    public void RpcCancelCountdown()
    {
        ui?.Hide();
    }
}
