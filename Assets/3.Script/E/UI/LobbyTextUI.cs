using UnityEngine;
using TMPro;

public class LobbyTextUI : MonoBehaviour
{
    [SerializeField] private Transform[] player = new Transform[10];
    [SerializeField] private TMP_Text[] playerName = new TMP_Text[10];
    [SerializeField] private TMP_Text[] playerReadyState = new TMP_Text[10];
    [SerializeField] public CountDownUI ui;
    public static LobbyTextUI Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }
        InitializeUIElements();
    }

    private void InitializeUIElements()
    {
        for (int i = 0; i < player.Length; i++)
        {
            if (player[i] == null)
            {
                Debug.LogError($"[LobbyTextUI] player[{i}]가 Inspector에 할당되지 않았습니다.");
                continue;
            }
            TMP_Text[] tmp = player[i].gameObject.GetComponentsInChildren<TMP_Text>();
            playerName[i] = tmp[0];
            playerReadyState[i] = tmp[1];
        }
    }
    public void ClearAllUI()
    {
        for (int i = 0; i < playerName.Length; i++)
        {
            if (playerName[i] != null) playerName[i].text = "";
            if (playerReadyState[i] != null) playerReadyState[i].text = "";
        }
    }

    public void UpdateUI(int playerNum, string newName, bool _isReady) 
    {
        if (playerNum < 0 || playerNum >= playerName.Length)
        {
            Debug.LogError($"[LobbyTextUI] UpdateUI: 유효하지 않은 playerNum={playerNum}");
            return;
        }
        if (playerName[playerNum] == null)
        {
            Debug.LogError($"[LobbyTextUI] UpdateUI: playerName[{playerNum}]이 null");
            return;
        }

        Debug.Log($"[LobbyTextUI] UpdateUI 실행: index={playerNum}, name={newName}, isReady={_isReady}");
        playerName[playerNum].text = newName;
        playerReadyState[playerNum].text = _isReady ? "Ready" : "Not Ready";
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}