using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class TitleController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject _loginPanel; 
    [SerializeField] private GameObject _optionPanel;

    [Header("Buttons")]
    [SerializeField] private Button _start_button;
    [SerializeField] private Button _option_button;
    [SerializeField] private Button _logout_button;

    [Header("Greeting UI")]
    [SerializeField] private TextMeshProUGUI _welcome_text;

    private void Start()
    {
        _start_button.onClick.AddListener(StartGameEvent);
        _option_button.onClick.AddListener(OpenOptionPage);
        _logout_button.onClick.AddListener(LogoutEvent);
    }

    private void OnEnable()
    {
        if (_welcome_text != null)
        {
            string playerName = PlayerPrefs.GetString("PlayerNickname");

            _welcome_text.text = $"{playerName} 님\n환영합니다!";
        }
    }

    public void StartGameEvent()
    {
        Debug.Log("스타트 게임 추후 작업예정");
    }

    public void OpenOptionPage()
    {
        if (SQLManager.Instance != null)
        {
            SQLManager.Instance.Logout();
        }

        if (NetworkClient.active)
        {
            NetworkManager.singleton.StopClient();
        }

        if (_optionPanel != null)
            _optionPanel.SetActive(true);

        gameObject.SetActive(false);
    }

    public void LogoutEvent()
    {
        // 필요하다면 SQLManager에서 세션 정보를 지우는 로직 추가 가능

        if (_loginPanel != null)
            _loginPanel.SetActive(true);

        if (NetworkClient.active)
        {
            NetworkManager.singleton.StopClient();
            Debug.Log("Network Client Stopped.");
        }

        gameObject.SetActive(false);
    }
}