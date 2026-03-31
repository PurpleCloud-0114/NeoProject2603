using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using kcp2k;
using LitJson;
using System.IO;


public enum Type
{
    enum_Empty = 0, //라이센스용
    enum_Server,
    enum_Client
}

public class Item
{
    public string license { get; private set; }
    public string server_ip { get; private set; }
    public string port { get; private set; }

    public Item(string L_index, string _ip, string _port)
    {
        license = L_index;
        server_ip = _ip;
        port = _port;
    }
}

public class ServerChecker : MonoBehaviour
{
    [SerializeField] public Type type;

    private NetworkManager _manager;
    private KcpTransport _transport;

    public string server_ip { get; private set; } //캡슐화
    public string server_port { get; private set; }

    private string _path = string.Empty;

    private void Awake()
    {
        if (_path.Equals(string.Empty))
        {
            _path = Application.dataPath + "/License";
        }

        if (!Directory.Exists(_path))
        {
            Directory.CreateDirectory(_path);
        }

        if (!File.Exists(_path + "/License.json"))
        {
            DefalutData(_path);
        }
        _path = _path + "/License.json";
        _manager = NetworkManager.singleton;
        _transport = (KcpTransport)_manager.transport;
    }

    private void DefalutData(string path)
    {
        List<Item> item = new List<Item>();
        item.Add(new Item("0", "127.0.0.1", "7777"));

        JsonData data = JsonMapper.ToJson(item);
        File.WriteAllText(path + "/License.json", data.ToString());
    }

    private Type LicenseType(string path)
    {
        Type type = Type.enum_Empty;
        /*
         public string License { get; private set; }
         public string ServerIP { get; private set; }
         public string Port { get; private set; }
         */
        try
        {
            string stringjson = File.ReadAllText(path);
            JsonData itemdata = JsonMapper.ToObject(stringjson);
            string stringtype = itemdata[0]["License"].ToString();
            string stringserverip = itemdata[0]["ServerIP"].ToString();
            string stringport = itemdata[0]["Port"].ToString();

            server_ip = stringserverip;
            server_port = stringport;
            type = (Type)Enum.Parse(typeof(Type), stringtype);

            _manager.networkAddress = server_ip;
            _transport.port = ushort.Parse(server_port);//부호가 없음

            return type;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return Type.enum_Empty;
        }

    }

    private void Start()
    {
        type = LicenseType(_path);

        //type 별로 각자 행동을 넣을 것...

        if (type.Equals(Type.enum_Server)) { Start_Server(); }
        //else if(type.Equals(Type.Client)){ Start_Client(); };
    }

    private void Start_Server()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Debug.Log("cannot webGL Server");
        }
        else
        {
            _manager.StartServer();
            Debug.Log($"{_manager.networkAddress}: Start Server");

            NetworkServer.OnConnectedEvent += (NetworkConnectionToClient) =>
            {
                Debug.Log($"new client connect : {NetworkConnectionToClient.address}");
            };
            NetworkServer.OnDisconnectedEvent += (NetworkConnectionToClient) =>
            {
                Debug.Log($"client connect : {NetworkConnectionToClient.address}");
            };
        }
    }

    public void Start_Client()
    {
        _manager.StartClient();
        Debug.Log($"{_manager.networkAddress}: Start Client");
    }

    private void OnApplicationQuit()
    {//프로그램이 꺼졌을때
        if (NetworkClient.isConnected)//클라이언트 입장
        {
            _manager.StopClient();
        }
        if (NetworkServer.active)
        {
            _manager.StopServer();
        }
    }
}
