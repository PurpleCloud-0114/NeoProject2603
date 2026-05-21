using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using MySql.Data.MySqlClient;
using Mirror;

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
public class PlayerScore
{
    //public Dictionary<NetworkIdentity, int> player_score_management = new Dictionary<NetworkIdentity, int>();
    //public Dictionary<NetworkIdentity, int> GetPlayerScoreList()
    //{
    //    return player_score_management;
    //}
    //public int GetPlayerScore(NetworkIdentity player)
    //{
    //    if (!player_score_management.ContainsKey(player))
    //    {
    //        Debug.LogWarning($"[PlayerScore] 미등록 플레이어 조회: {player.name}");
    //        return 0;
    //    }
    //    return player_score_management[player];
    //}
    //public void SetPlayerScoreList(Dictionary<NetworkIdentity, int> inputList)
    //{
    //    player_score_management = inputList;
    //}
    //public void SetPlayerScore(NetworkIdentity player, int score)
    //{
    //    player_score_management[player] = score;
    //}
    //public void AddPlayerScore(NetworkIdentity player, int amount)
    //{
    //    if (!player_score_management.ContainsKey(player))
    //    {
    //        Debug.LogWarning($"[PlayerScore] AddScore - 미등록 플레이어: {player.name}");
    //        player_score_management[player] = 0;
    //    }
    //    player_score_management[player] += amount;
    //}
    //public void InitPlayerScore(NetworkIdentity player)
    //{
    //    if (!player_score_management.ContainsKey(player))
    //        player_score_management.Add(player, 0);
    //}
    public Dictionary<string, int> player_score_management = new Dictionary<string, int>();

    public int GetPlayerScore(string playerName) {
        if (!player_score_management.ContainsKey(playerName)) return 0;
        return player_score_management[playerName];
    }

    public void AddPlayerScore(string playerName, int amount) {
        // 미등록 상태면 0점으로 자동 등록
        if (!player_score_management.ContainsKey(playerName)) {
            player_score_management[playerName] = 0;
        }
        player_score_management[playerName] += amount;
    }

    public void InitPlayerScore(string playerName) {
        if (!player_score_management.ContainsKey(playerName))
            player_score_management.Add(playerName, 0);
    }
}

public class SQLManager : MonoBehaviour
{
    private string _db_path = string.Empty;
    private MySqlConnection _connection;

    public UserInfo user_info { get; private set; }
    public static SQLManager Instance = null;
    public PlayerScore player_score = new PlayerScore();
    [Header("Network Settings")]
    [SerializeField] private bool _is_it_Client = false;
    [Tooltip("와이파이 연결후 ipconfig, 해당 IPv4 주소 입력")]
    [SerializeField] private string _serverIP = "192.168.1.135";
    private string _dbName = "neoproject";
    private string _dbPort = "3306";

    [Header("Debug Option")]
    [Tooltip("체크하면 기존 config.json을 무시하고 위 인스펙터 설정값으로 덮어씁니다.")]
    [SerializeField] private bool _overwriteConfig = true;

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
            if (string.IsNullOrEmpty(serverinfo)) { Debug.LogError("SQL Server info string is empty"); return; }
            _connection = new MySqlConnection(serverinfo);
            _connection.Open();
            Debug.Log($"<color=green>SQL Server connect success! (IP: {_serverIP})</color>");
        }
        catch (Exception e)
        {
            Debug.LogError($"DB Connect Error: {e.Message}");
            Debug.LogError("Tip: PC 방화벽 3306 포트가 열려있는지, IP가 맞는지 확인하세요.");
        }
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
        string filepath = Path.Combine(path, "config.json");
        string jsonstring = File.ReadAllText(filepath);
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
        string filepath = Path.Combine(path, "config.json");

        // 파일이 없거나, 덮어쓰기 옵션이 켜져 있을 때 새로 생성
        if (!File.Exists(filepath) || _overwriteConfig)
        {
            List<ServerJsonItem> item = new List<ServerJsonItem>();

            // 인스펙터 설정값 사용
            string userId = _is_it_Client ? "game_client" : "game_server";
            item.Add(new ServerJsonItem(_serverIP, _dbName, userId, "1234", _dbPort));

            JsonData data = JsonMapper.ToJson(item);
            File.WriteAllText(filepath, data.ToString());
            Debug.Log($"[SQLManager] Config file updated. Target IP: {_serverIP}");
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

    // ── 로그인 ──
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
                        return 0;
                    }
                }
            }
            return 1;
        }
        catch (Exception e) { Debug.LogError($"Login Error: {e.Message}"); return 2; }
    }

    public void Logout() { user_info = null; }

    // ── 회원가입 ──
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
            string sql = "SELECT COUNT(*) FROM user_info WHERE user_name=@name";
            using (MySqlCommand cmd = new MySqlCommand(sql, _connection))
            {
                cmd.Parameters.AddWithValue("@name", name);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
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

    // ── 점수 관리 ──
    public bool GetScore(string nickname, out int outscore)
    {
        outscore = 0;
        try
        {
            if (!ConnectionCheck(_connection)) return false;

            string sql = "SELECT user_score FROM user_info WHERE user_nickname=@nickname";
            using (MySqlCommand cmd = new MySqlCommand(sql, _connection))
            {
                cmd.Parameters.AddWithValue("@nickname", nickname);
                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    outscore = Convert.ToInt32(result);
                    return true;
                }
            }
            return false;
        }
        catch (Exception e) { Debug.LogError($"GetScore Error: {e.Message}"); return false; }
    }

    public bool SetScore(string nickname, int score)
    {
        if (_is_it_Client)
        {
            Debug.LogWarning("[SQLManager] 클라이언트에서 SetScore 호출 차단");
            return false;
        }

        try
        {
            if (!ConnectionCheck(_connection)) return false;

            string sql = "UPDATE user_info SET user_score=@score WHERE user_nickname=@nickname";
            using (MySqlCommand cmd = new MySqlCommand(sql, _connection))
            {
                cmd.Parameters.AddWithValue("@score", score);
                cmd.Parameters.AddWithValue("@nickname", nickname);

                int rows = cmd.ExecuteNonQuery();

                if (rows > 0 && user_info != null && user_info.user_nickname == nickname)
                {
                    user_info.user_score = score;
                }
                return rows > 0;
            }
        }
        catch (Exception e) { Debug.LogError($"SetScore Error: {e.Message}"); return false; }
    }

    public bool GetNickname(string name, out string outNickname)
    {
        outNickname = "";
        try
        {
            if (!ConnectionCheck(_connection)) return false;
            string sql = "SELECT user_nickname FROM user_info WHERE user_name=@name";
            using (MySqlCommand cmd = new MySqlCommand(sql, _connection))
            {
                cmd.Parameters.AddWithValue("@name", name);
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    outNickname = result.ToString();
                    return true;
                }
            }
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SQLManager] GetNickname Error: {e.Message}");
            return false;
        }
    }

    public bool AddScore(string nickname, int amount)
    {
        if (_is_it_Client)
        {
            Debug.LogWarning("[SQLManager] 클라이언트에서 AddScore 호출 차단");
            return false;
        }

        try
        {
            if (!ConnectionCheck(_connection)) return false;

            string sql = "UPDATE user_info SET user_score = user_score + @amount WHERE user_nickname = @nickname";

            using (MySqlCommand cmd = new MySqlCommand(sql, _connection))
            {
                cmd.Parameters.AddWithValue("@amount", amount);
                cmd.Parameters.AddWithValue("@nickname", nickname);

                int rows = cmd.ExecuteNonQuery();

                if (rows > 0 && user_info != null && user_info.user_nickname == nickname)
                {
                    user_info.user_score += amount;
                    Debug.Log($"[SQLManager] DB 점수 가산 완료: {nickname} (+{amount}) -> 현재: {user_info.user_score}");
                }

                return rows > 0;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"AddScore Error: {e.Message}");
            return false;
        }
    }

    // ── 라운드 점수 관리 (서버 전용) ──

    public bool AddRoundScore(string nickname, int amount)
    {
        if (_is_it_Client)
        {
            Debug.LogWarning("[SQLManager] 클라이언트에서 AddRoundScore 호출 차단");
            return false;
        }

        try
        {
            if (!ConnectionCheck(_connection)) return false;

            string sql = "UPDATE user_info SET user_round_score = user_round_score + @amount WHERE user_nickname = @nickname";
            using (MySqlCommand cmd = new MySqlCommand(sql, _connection))
            {
                cmd.Parameters.AddWithValue("@amount", amount);
                cmd.Parameters.AddWithValue("@nickname", nickname);

                int rows = cmd.ExecuteNonQuery();

                if (rows > 0 && user_info != null && user_info.user_nickname == nickname)
                {
                    user_info.round_total_score += amount;
                    Debug.Log($"[SQLManager] DB/메모리 라운드 점수 가산 완료: {nickname} (+{amount}) -> 현재: {user_info.round_total_score}");
                }
                return rows > 0;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"AddRoundScore Error: {e.Message}");
            return false;
        }
    }

    public bool ResetRoundScore(string nickname)
    {
        if (_is_it_Client)
        {
            Debug.LogWarning("[SQLManager] 클라이언트에서 ResetRoundScore 호출 차단");
            return false;
        }

        try
        {
            if (!ConnectionCheck(_connection)) return false;

            string sql = "UPDATE user_info SET user_round_score = 0 WHERE user_nickname = @nickname";
            using (MySqlCommand cmd = new MySqlCommand(sql, _connection))
            {
                cmd.Parameters.AddWithValue("@nickname", nickname);

                int rows = cmd.ExecuteNonQuery();

                if (rows > 0 && user_info != null && user_info.user_nickname == nickname)
                {
                    user_info.round_total_score = 0;
                    Debug.Log($"[SQLManager] DB/메모리 {nickname}의 라운드 점수가 리셋되었습니다. (0점)");
                }
                return rows > 0;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"ResetRoundScore Error: {e.Message}");
            return false;
        }
    }

    private void OnApplicationQuit()
    {
        try
        {
            if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
            {
                _connection.Close();
                _connection.Dispose();
            }
        }
        catch { }
    }
}