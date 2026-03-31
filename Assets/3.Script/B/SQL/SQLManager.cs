using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using MySql.Data;
using MySql.Data.MySqlClient;

public class ServerJsonItem
{
    public string ip_json { get; private set; }
    public string tablename_json { get; private set; }
    public string id_json { get; private set; }
    public string pw_json { get; private set; }
    public string port_json { get; private set; }
    
    public ServerJsonItem(string ip, string table, string id, string pw, string port)
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
    public int user_score;
    //public string user_password { get; private set; }
    
    /// <summary>
    /// 추가 UserInfo 필요하면 여기서 추가
    /// </summary>
    //public string user_addmore { get; private set; }

    public UserInfo(string name, int score)//string _addmore
    {
        user_name = name;
        user_score = score;
        //user_password = password;
        //user_addmore = _addmore;
    }
}

public class SQLManager : MonoBehaviour
{
    //private 변수는 _camelCase
    [SerializeField] private string _db_path = string.Empty;
    private MySqlConnection _connection;
    private MySqlDataReader _reader;

    //public 변수는 snake_case (또는 명시된 규칙 적용)
    public UserInfo user_info { get; private set; }
    //싱글톤은 Instance 고정
    public static SQLManager Instance = null;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _db_path = Path.Combine(Application.persistentDataPath, "Database");
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
            List<ServerJsonItem> item = new List<ServerJsonItem>();
            item.Add(
                new ServerJsonItem
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

            string sqlCommand = "SELECT user_name, user_password, user_score FROM user_info WHERE user_name=@name AND user_password=@pw";

            using (MySqlCommand command = new MySqlCommand(sqlCommand, _connection))
            {
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@pw", password);

                using (_reader = command.ExecuteReader())
                {
                    if (_reader.Read())
                    {
                        string readName = _reader["user_name"].ToString();
                        // GetOrdinal을 쓰면 인덱스 번호를 직접 계산 안 해도 되어 안전
                        int readScore = _reader.IsDBNull(_reader.GetOrdinal("user_score")) ? 0 : Convert.ToInt32(_reader["user_score"]);

                        user_info = new UserInfo(readName, readScore);
                        return true;
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


    public bool SignupIDCheck(string name)
    {
        try
        {
            if (!ConnectionCheck(_connection)) return false;

            string sqlCommand = "SELECT user_name FROM user_info WHERE user_name=@name";
            using (MySqlCommand command = new MySqlCommand(sqlCommand, _connection))
            {
                command.Parameters.AddWithValue("@name", name);
                using (var reader = command.ExecuteReader())
                {
                    bool hasRows = reader.HasRows;
                    return hasRows; // 데이터가 있으면 이미 존재하는 아이디(true)
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"ID Check Error: {e.Message}");
            return false;
        }
    }
    public bool Signup(string name,string password)
    {
        try
        {
            if (!ConnectionCheck(_connection)) return false;

            string sqlCommand = "INSERT INTO user_info (user_name, user_password, user_score) VALUES (@name, @pw, @score)";
            using (MySqlCommand command = new MySqlCommand(sqlCommand, _connection))
            {
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@pw", password);
                command.Parameters.AddWithValue("@score", 0);

                return command.ExecuteNonQuery() == 1;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Signup Error: {e.Message}");
            return false;
        }
    }
    public bool GetScore(string name)
    {
        try
        {
            if (!ConnectionCheck(_connection)) return false;

            string sqlcommand = "SELECT user_name, user_score FROM user_info WHERE user_name=@name";
            using (MySqlCommand command = new MySqlCommand(sqlcommand, _connection))
            {
                command.Parameters.AddWithValue("@name", name);
                // 내부 로컬 변수 reader 사용 (using으로 자동 Close)
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string nametemp = reader["user_name"].ToString();
                        int score = reader.IsDBNull(reader.GetOrdinal("user_score")) ? 0 : Convert.ToInt32(reader["user_score"]);

                        user_info = new UserInfo(nametemp, score);
                        return true;
                    }
                }
            }
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"GetScore Error: {e.Message}");
            return false;
        }
    }
    public bool SetScore(string name, int score)
    {
        try
        {
            if (!ConnectionCheck(_connection)) return false;

            string sqlcommand = "UPDATE user_info SET user_score=@score WHERE user_name=@name";
            using (MySqlCommand command = new MySqlCommand(sqlcommand, _connection))
            {
                command.Parameters.AddWithValue("@score", score);
                command.Parameters.AddWithValue("@name", name);

                int affectedrows = command.ExecuteNonQuery();

                // Null 체크 추가 (안정성 강화)
                if (user_info != null) user_info.user_score = score;

                return affectedrows >= 0;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"SetScore Error: {e.Message}");
            return false;
        }
    }
}
