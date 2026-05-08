using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using UnityEngine.SceneManagement;

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

    [SerializeField] private string _logoutscene = "ClientTitle";

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
        //룸변경
    }

    public void OpenOptionPage()
    {
        if (_optionPanel != null)
            _optionPanel.SetActive(true);

        gameObject.SetActive(false);
    }

    public void LogoutEvent()
    {
        if (_loginPanel != null)
            _loginPanel.SetActive(true);

        gameObject.SetActive(false);

        SceneManager.LoadScene(_logoutscene);
    }
}