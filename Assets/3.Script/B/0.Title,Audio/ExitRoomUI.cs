using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class ExitRoomUI : MonoBehaviour
{
    public void ExitRoom()
    {
        NetworkManager networkManager = NetworkManager.singleton;

        if (networkManager == null) return;

        if (NetworkClient.isConnected)
        {
            networkManager.StopClient();
        }
    }
}