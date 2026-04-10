
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CreateRoomUI : MonoBehaviour
{
    public void CreateRoom()
    {
        NetworkManager networkManager = NetworkManager.singleton;
        networkManager.networkAddress = "192.168.45.104";
        networkManager.networkAddress = "0.0.0.0";
        Debug.Log("Server Address: " + NetworkManager.singleton.networkAddress);
        networkManager.StartServer();
        Debug.Log("Server Started");
    }
}
