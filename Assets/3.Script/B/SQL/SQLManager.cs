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
    public string user_nickname { get; private set; }
    public int user_score;

    public UserInfo(string name, string nickname, int score)
    {
        user_name = name;
        user_nickname = nickname;
        user_score = score;
    }
}

public class SQLManager : MonoBehaviour
{
    [SerializeField] private string _db_path = string.Empty;
    private MySqlConnection _connection;

    public UserInfo user_info { get; private set; }
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
        string serverinfo = ServerSet(_db_path);

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
        string filePath = path + "/config.json";
        if (!File.Exists(filePath))
        {
            List<ServerJsonItem> item = new List<ServerJsonItem>();
            item.Add(new ServerJsonItem("192.168.1.45", "programming", "root", "250930", "3306"));
            JsonData data = JsonMapper.ToJson(item);
            File.WriteAllText(filePath, data.ToString());
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

            string sqlCommand = "SELECT user_name, user_nickname, user_score FROM user_info WHERE user_name=@name AND user_password=@pw";

            using (MySqlCommand command = new MySqlCommand(sqlCommand, _connection))
            {
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@pw", password);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string readName = reader["user_name"].ToString();
                        string readNickname = reader["user_nickname"].ToString();
                        int readScore = reader.IsDBNull(reader.GetOrdinal("user_score")) ? 0 : Convert.ToInt32(reader["user_score"]);

                        user_info = new UserInfo(readName, readNickname, readScore);
                        return true;
                    }
                }
            }
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"Login Error: {e.Message}");
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
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    return reader.HasRows;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"ID Check Error: {e.Message}");
            return false;
        }
    }

    public bool SignupNicknameCheck(string nickname)
    {
        try
        {
            if (!ConnectionCheck(_connection)) return false;

            string sqlCommand = "SELECT user_nickname FROM user_info WHERE user_nickname=@nickname";
            using (MySqlCommand command = new MySqlCommand(sqlCommand, _connection))
            {
                command.Parameters.AddWithValue("@nickname", nickname);
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    return reader.HasRows;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Nickname Check Error: {e.Message}");
            return false;
        }
    }

    public bool Signup(string name, string password, string nickname)
    {
        try
        {
            if (!ConnectionCheck(_connection)) return false;

            string sqlCommand = "INSERT INTO user_info (user_name, user_password, user_nickname, user_score) VALUES (@name, @pw, @nickname, @score)";
            using (MySqlCommand command = new MySqlCommand(sqlCommand, _connection))
            {
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@pw", password);
                command.Parameters.AddWithValue("@nickname", nickname);
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

    public string GetNickname(string name)
    {
        try
        {
            if (!ConnectionCheck(_connection)) return "";

            string sqlCommand = "SELECT user_nickname FROM user_info WHERE user_name=@name";
            using (MySqlCommand command = new MySqlCommand(sqlCommand, _connection))
            {
                command.Parameters.AddWithValue("@name", name);
                object result = command.ExecuteScalar();
                return result != null ? result.ToString() : "";
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"GetNickname Error: {e.Message}");
            return "";
        }
    }

    public bool GetScore(string name)
    {
        try
        {
            if (!ConnectionCheck(_connection)) return false;

            string sqlcommand = "SELECT user_name, user_nickname, user_score FROM user_info WHERE user_name=@name";
            using (MySqlCommand command = new MySqlCommand(sqlcommand, _connection))
            {
                command.Parameters.AddWithValue("@name", name);
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string nametemp = reader["user_name"].ToString();
                        string nicktemp = reader["user_nickname"].ToString();
                        int score = reader.IsDBNull(reader.GetOrdinal("user_score")) ? 0 : Convert.ToInt32(reader["user_score"]);

                        user_info = new UserInfo(nametemp, nicktemp, score);
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

    private void OnApplicationQuit()
    {
        if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
        {
            _connection.Close();
            Debug.Log("SQL Connection Closed.");
        }
    }
}