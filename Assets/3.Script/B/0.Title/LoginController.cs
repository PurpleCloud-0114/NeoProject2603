using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using System.Collections;

public class LoginController : MonoBehaviour
{
    [SerializeField] private TMP_InputField _name_input;
    [SerializeField] private TMP_InputField _pw_input;
    [SerializeField] private TMP_Text _log_text;
    [SerializeField] private Button _login_button;
    [SerializeField] private Button _signup_button;
    [SerializeField] private SignupController _signupController;
    [SerializeField] private GameObject _title_ob;

    private bool _isWaiting = false;

    private void Start()
    {
        LogTextViewing(string.Empty);
        _login_button.onClick.AddListener(LoginEvent);
        _signup_button.onClick.AddListener(OpenSignUpPage);
    }

    public void LogTextViewing(string text) { _log_text.text = text; }

    public void LoginEvent()
    {
        if (_isWaiting) return;

        if (_name_input.text.Equals(string.Empty) || _pw_input.text.Equals(string.Empty))
        {
            LogTextViewing("ID 와 Password 를 확인하세요");
            return;
        }

        GameObject manager = NetworkManager.singleton.gameObject;
        if (!manager.TryGetComponent(out ServerChecker checker)) return;

        _isWaiting = true;
        _login_button.interactable = false;
        LogTextViewing("서버에 연결 중...");

        checker.Start_Client();

        StartCoroutine(WaitAndSendLogin(_name_input.text, _pw_input.text));
    }

    private IEnumerator WaitAndSendLogin(string name, string pw)
    {
        float timeout = 5f;
        while (AuthPlayer.LocalInstance == null && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (AuthPlayer.LocalInstance == null)
        {
            LogTextViewing("서버에 연결할 수 없습니다");
            _isWaiting = false;
            _login_button.interactable = true;
            yield break;
        }

        AuthPlayer.LocalInstance.OnLoginResult = (success, nickname, score, message) =>
        {
            _isWaiting = false;
            _login_button.interactable = true;

            if (success)
            {
                PlayerPrefs.SetString("PlayerNickname", nickname);
                PlayerPrefs.Save();

                if (_title_ob != null) _title_ob.SetActive(true);
                gameObject.SetActive(false);
            }
            else
            {
                LogTextViewing(message);
                if (NetworkClient.active) NetworkManager.singleton.StopClient();
            }
        };

        AuthPlayer.LocalInstance.CmdRequestLogin(name, pw);
    }

    public void OpenSignUpPage()
    {
        if (!NetworkClient.active)
        {
            GameObject manager = NetworkManager.singleton.gameObject;
            if (manager.TryGetComponent(out ServerChecker checker))
                checker.Start_Client();
        }

        if (_signupController != null)
            _signupController.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

    private void OnDisable() { ClearInputs(); }

    private void ClearInputs()
    {
        if (_name_input != null) _name_input.text = string.Empty;
        if (_pw_input != null) _pw_input.text = string.Empty;
        _isWaiting = false;
        _login_button.interactable = true;
        LogTextViewing(string.Empty);
    }
}