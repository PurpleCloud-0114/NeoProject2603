// ※ 이 스크립트는 서버 빌드에만 포함되어야 합니다.
// 클라이언트(안드로이드) 빌드에는 포함하지 마세요.
// 서버 GameObject에만 부착하고, 클라이언트 씬에는 배치하지 않습니다.

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
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
        ip_json = ip; tablename_json = table;
        id_json = id; pw_json = pw; port_json = port;
    }
}

public class UserInfo
{
    public string user_name { get; private set; }
    public string user_nickname { get; private set; }
    public int user_score;
    public int player_num = -1;
    public int round_total_score = 0;

    public UserInfo(string name, string nickname, int score)
    {
        user_name = name;
        user_nickname = nickname;
        user_score = score;
    }
}

public class SQLManager : MonoBehaviour
{
    private string _db_path = string.Empty;
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
            if (serverinfo.Equals(string.Empty)) { Debug.LogError("SQL Server json error"); return; }
            _connection = new MySqlConnection(serverinfo);
            _connection.Open();
            Debug.Log("SQL Server connect!");
        }
        catch (Exception e) { Debug.LogError($"DB Connect Error: {e.Message}"); }
    }

    private string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }

    private string ServerSet(string path)
    {
        CreateFile(path);
        string jsonstring = File.ReadAllText(path + "/config.json");
        JsonData itemData = JsonMapper.ToObject(jsonstring);
        try
        {
            return $"Server={itemData[0]["ip_json"]};" +
                   $"Database={itemData[0]["tablename_json"]};" +
                   $"Uid={itemData[0]["id_json"]};" +
                   $"Pwd={itemData[0]["pw_json"]};" +
                   $"Port={itemData[0]["port_json"]};" +
                   "Charset=utf8;";
        }
        catch (Exception e) { Debug.LogError($"ServerSet Error: {e.Message}"); return string.Empty; }
    }

    private void CreateFile(string path)
    {
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        string filepath = path + "/config.json";
        if (!File.Exists(filepath))
        {
            List<ServerJsonItem> item = new List<ServerJsonItem>();
            item.Add(new ServerJsonItem("192.168.1.45", "neoproject", "root", "1234", "3306"));
            JsonData data = JsonMapper.ToJson(item);
            File.WriteAllText(filepath, data.ToString());
        }
    }

    private bool ConnectionCheck(MySqlConnection connection)
    {
        try
        {
            if (connection == null) return false;
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
                if (connection.State != System.Data.ConnectionState.Open) return false;
            }
            return true;
        }
        catch (Exception e) { Debug.LogError($"Connection Error: {e.Message}"); return false; }
    }

    public int Login(string name, string password, out string nickname, out int score)
    {
        nickname = ""; score = 0;
        try
        {
            if (!ConnectionCheck(_connection)) return 2;
            string hashedPw = HashPassword(password);
            string sql = "SELECT user_name, user_nickname, user_score FROM user_info WHERE user_name=@name AND user_password=@pw";
            using (MySqlCommand cmd = new MySqlCommand(sql, _connection))
            {
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@pw", hashedPw);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        nickname = reader["user_nickname"].ToString();
                        score = reader.IsDBNull(reader.GetOrdinal("user_score")) ? 0 : Convert.ToInt32(reader["user_score"]);
                        user_info = new UserInfo(name, nickname, score);
                        return 0; // 성공
                    }
                }
            }
            return 1; // 아이디/비번 불일치
        }
        catch (Exception e) { Debug.LogError($"Login Error: {e.Message}"); return 2; }
    }

    public void Logout() { user_info = null; }

    public int Signup(string name, string password, string nickname)
    {
        try
        {
            if (!ConnectionCheck(_connection)) return 3;
            if (SignupIDCheck(name)) return 1;
            if (SignupNicknameCheck(nickname)) return 2;

            string hashedPw = HashPassword(password);
            string sql = "INSERT INTO user_info (user_name, user_password, user_nickname, user_score) VALUES (@name, @pw, @nickname, @score)";
            using (MySqlCommand cmd = new MySqlCommand(sql, _connection))
            {
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@pw", hashedPw);
                cmd.Parameters.AddWithValue("@nickname", nickname);
                cmd.Parameters.AddWithValue("@score", 0);
                return cmd.ExecuteNonQuery() == 1 ? 0 : 3;
            }
        }
        catch (Exception e) { Debug.LogError($"Signup Error: {e.Message}"); return 3; }
    }

    public bool SignupIDCheck(string name)
    {
        try
        {
            if (!ConnectionCheck(_connection)) return false;
            string sql = "SELECT user_name FROM user_info WHERE user_name=@name";
            using (MySqlCommand cmd = new MySqlCommand(sql, _connection))
            {
                cmd.Parameters.AddWithValue("@name", name);
                using (MySqlDataReader reader = cmd.ExecuteReader()) { return reader.HasRows; }
            }
        }
        catch (Exception e) { Debug.LogError($"ID Check Error: {e.Message}"); return false; }
    }

    public bool SignupNicknameCheck(string nickname)
    {
        try
        {
            if (!ConnectionCheck(_connection)) return false;
            string sql = "SELECT user_nickname FROM user_info WHERE user_nickname=@nickname";
            using (MySqlCommand cmd = new MySqlCommand(sql, _connection))
            {
                cmd.Parameters.AddWithValue("@nickname", nickname);
                using (MySqlDataReader reader = cmd.ExecuteReader()) { return reader.HasRows; }
            }
        }
        catch (Exception e) { Debug.LogError($"Nickname Check Error: {e.Message}"); return false; }
    }

    public bool GetScore(string name, out int outscore)
    {
        outscore = 0;
        try
        {
            if (!ConnectionCheck(_connection)) return false;
            string sql = "SELECT user_score FROM user_info WHERE user_name=@name";
            using (MySqlCommand cmd = new MySqlCommand(sql, _connection))
            {
                cmd.Parameters.AddWithValue("@name", name);
                object result = cmd.ExecuteScalar();
                if (result != null) { outscore = Convert.ToInt32(result); return true; }
            }
            return false;
        }
        catch (Exception e) { Debug.LogError($"GetScore Error: {e.Message}"); return false; }
    }

    public bool SetScore(string name, int score)
    {
        try
        {
            if (!ConnectionCheck(_connection)) return false;
            string sql = "UPDATE user_info SET user_score=@score WHERE user_name=@name";
            using (MySqlCommand cmd = new MySqlCommand(sql, _connection))
            {
                cmd.Parameters.AddWithValue("@score", score);
                cmd.Parameters.AddWithValue("@name", name);
                int rows = cmd.ExecuteNonQuery();
                if (rows > 0 && user_info != null) user_info.user_score = score;
                return rows > 0;
            }
        }
        catch (Exception e) { Debug.LogError($"SetScore Error: {e.Message}"); return false; }
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