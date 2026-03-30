using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using MySql.Data;
using MySql.Data.MySqlClient;

public class JsonItem
{
    public string ip_json { get; private set; }
    public string tablename_json { get; private set; }
    public string id_json { get; private set; }
    public string pw_json { get; private set; }
    public string port_json { get; private set; }
    
    public JsonItem(string ip, string table, string id, string pw, string port)
    {
        ip_json = ip;
        tablename_json = table;
        id_json = id;
        pw_json = pw;
        port_json = port;
    }
}

public class UserInfo
{
    public string user_name { get; private set; }
    public string user_password { get; private set; }
    
    /// <summary>
    /// 추가 UserInfo 필요하면 여기서 추가
    /// </summary>
    //public string user_addmore { get; private set; }

    public UserInfo(string name, string password)//string _addmore
    {
        user_name = name;
        user_password = password;
        //user_addmore = _addmore;
    }
}

public class SQL_Manager : MonoBehaviour
{
    //private 변수는 _camelCase
    [SerializeField] private string _db_path = string.Empty;
    private MySqlConnection _connection;
    private MySqlDataReader _reader;

    //public 변수는 snake_case (또는 명시된 규칙 적용)
    public UserInfo user_info { get; private set; }
    //싱글톤은 Instance 고정
    public static SQL_Manager Instance = null;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _db_path = Application.dataPath + "/Database";
        string serverinfo = ServerSet(_db_path);//경로지정

        try
        {
            if (serverinfo.Equals(string.Empty))
            {
                Debug.Log("SQL Server json error");
                return;
            }
            _connection = new MySqlConnection(serverinfo);
            _connection.Open();
            Debug.Log("SQL Server connect!");
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    private string ServerSet(string path)
    {
        CreateFile(path);

        string jsonString = File.ReadAllText(path + "/config.json");
        JsonData itemData = JsonMapper.ToObject(jsonString);

        try
        {
            string serverInfo =
            $"Server={itemData[0]["ip_json"]};" +
            $"Database={itemData[0]["tablename_json"]};" +
            $"Uid={itemData[0]["id_json"]};" +
            $"Pwd={itemData[0]["pw_json"]};" +
            $"Port={itemData[0]["port_json"]};" +
            "Charset=utf8;";

            return serverInfo;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return string.Empty;
        }
    }

    private void CreateFile(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        path += "/config.json";
        if (!File.Exists(path))
        {
            List<JsonItem> item = new List<JsonItem>();
            item.Add(
                new JsonItem
                ("192.168.1.45", "programming", "root", "250930", "3306")); // DB설정
            JsonData data = JsonMapper.ToJson(item);
            File.WriteAllText(path, data.ToString());
                    
        }
    }

    private bool ConnectionCheck(MySqlConnection connection)
    {
        if (connection.State != System.Data.ConnectionState.Open)
        {
            connection.Open();
            if (connection.State != System.Data.ConnectionState.Open) return false;
        }
        return true;
    }

    public bool Login(string name, string password)
    {
        try
        {
            if (!ConnectionCheck(_connection)) return false;
            /* 
             SELECT User_Name,User_Password,User_PhoneNum
             FROM user_info 
             WHERE User_Name='박희수' AND User_Password='0204';
            //전화 번호등(_addmore) 추가 시 변경
            */

            string sqlCommand = string.Format
                (@"SELECT user_name,user_password
             FROM user_info 
             WHERE User_Name='{0}' AND User_Password='{1}';", name, password);

            MySqlCommand command = new MySqlCommand(sqlCommand, _connection);

            _reader = command.ExecuteReader();

            if (_reader.HasRows) //리더에 행이 있는가? -> 조회 데이터가 있는가?
            {
                while (_reader.Read())
                {
                    string readName = (_reader.IsDBNull(0)) ? string.Empty : _reader["user_name"].ToString();
                    string readPW = (_reader.IsDBNull(1)) ? string.Empty : _reader["user_password"].ToString();
                    //string addmore = (reader.IsDBNull(2)) ? string.Empty : reader["User_PhoneNum"].ToString();

                    if (!readName.Equals(string.Empty) || !readPW.Equals(string.Empty))
                    {//데이터를 정상적으로 가져옴
                        user_info = new UserInfo(readName, readPW);
                        if (!_reader.IsClosed) _reader.Close();
                        return true;
                    }
                    else
                    {
                        if (!_reader.IsClosed) _reader.Close();
                        return false;
                    }
                }
            }
            if (!_reader.IsClosed) _reader.Close();
            return false;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            if (!_reader.IsClosed) _reader.Close();
            return false;
        }
    }
}
