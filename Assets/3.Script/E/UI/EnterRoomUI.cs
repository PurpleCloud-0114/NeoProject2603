
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class EnterRoomUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _inputField;
    public void EnterRoom()
    {
        _inputField.text = "EnterRoom";
        NetworkManager networkManager = NetworkManager.singleton;
        //lan
        //networkManager.networkAddress = "192.168.45.93";
        //wifi
        networkManager.networkAddress = "192.168.45.197";
        _inputField.text = "Server Address: " + NetworkManager.singleton.networkAddress;
        networkManager.StartClient();
    }
}
