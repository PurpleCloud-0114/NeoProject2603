using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using kcp2k;
using LitJson;
using System.IO;

public enum NetworkType
{
    Empty = 0,
    Server,
    Client
}

// JSON 직렬화 키와 프로퍼티명 통일
public class LicenseItem
{
    public string license { get; private set; }
    public string server_ip { get; private set; }
    public string port { get; private set; }

    public LicenseItem(string license, string ip, string port)
    {
        this.license = license;
        server_ip = ip;
        this.port = port;
    }
}

public class ServerChecker : MonoBehaviour
{
    public NetworkType networkType;

    private NetworkManager _manager;
    private KcpTransport _transport;

    public string server_ip { get; private set; }
    public string server_port { get; private set; }

    private string _path = string.Empty;

    private void Awake()
    {
        // dataPath 대신 persistentDataPath 사용 (모바일 권한 문제 해결)
        _path = Application.persistentDataPath + "/License";

        if (!Directory.Exists(_path)) Directory.CreateDirectory(_path);

        string fullFilePath = _path + "/License.json";
        if (!File.Exists(fullFilePath)) CreateDefaultData(_path);

        _path = fullFilePath;
        _manager = NetworkManager.singleton;

        if (_manager.transport == null)
        {
            Debug.LogError("NetworkManager에 Transport가 할당되지 않았습니다!");
        }
        _transport = (KcpTransport)_manager.transport;
    }

    private void CreateDefaultData(string path)
    {
        // LitJson은 프로퍼티명 그대로 직렬화하므로 소문자 키로 통일
        List<LicenseItem> items = new List<LicenseItem>();
        items.Add(new LicenseItem("Client", "127.0.0.1", "7777"));
        JsonData data = JsonMapper.ToJson(items);
        File.WriteAllText(path + "/License.json", data.ToString());
    }

    private NetworkType ReadLicenseType(string path)
    {
        try
        {
            string jsonString = File.ReadAllText(path);
            JsonData itemData = JsonMapper.ToObject(jsonString);

            // 소문자 키로 읽기 (LicenseItem 프로퍼티명과 일치)
            string typeStr = itemData[0]["license"].ToString();
            server_ip = itemData[0]["server_ip"].ToString();
            server_port = itemData[0]["port"].ToString();

            _manager.networkAddress = server_ip;
            _transport.port = ushort.Parse(server_port);

            return (NetworkType)Enum.Parse(typeof(NetworkType), typeStr);
        }
        catch (Exception e)
        {
            Debug.LogError($"License Read Error: {e.Message}");
            return NetworkType.Empty;
        }
    }

    private void Start()
    {
        networkType = ReadLicenseType(_path);

        if (networkType == NetworkType.Server) StartServer();
    }

    private void StartServer()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Debug.LogWarning("WebGL cannot run as server.");
            return;
        }

        _manager.StartServer();
        Debug.Log($"{_manager.networkAddress}: Server Started");

        NetworkServer.OnConnectedEvent += (conn) => Debug.Log($"Client connected: {conn.address}");
        NetworkServer.OnDisconnectedEvent += (conn) => Debug.Log($"Client disconnected: {conn.address}");
    }

    public void Start_Client()
    {
        _manager.StartClient();
        Debug.Log($"{_manager.networkAddress}: Client Started");
    }

    private void OnApplicationQuit()
    {
        if (NetworkClient.isConnected) _manager.StopClient();
        if (NetworkServer.active) _manager.StopServer();
    }
}