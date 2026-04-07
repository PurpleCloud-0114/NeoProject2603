using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror;

public class SignupController : MonoBehaviour
{
    [SerializeField] private GameObject _login;
    [SerializeField] private LoginController _logincontroller;
    [SerializeField] private GameObject _signup_ob;
    [SerializeField] private TMP_InputField _name_input;
    [SerializeField] private TMP_InputField _nickname_input;
    [SerializeField] private TMP_InputField _pw_input;
    [SerializeField] private TMP_InputField _pw_confirm_input;
    [SerializeField] private TMP_Text _log_text;
    [SerializeField] private Button _signup_button;
    [SerializeField] private Button _back_button;

    private bool _isWaiting = false;

    private void Start()
    {
        LogTextViewing(string.Empty);
        _signup_button.onClick.AddListener(SignupEvent);
        _back_button.onClick.AddListener(BackToLogin);
    }

    private void LogTextViewing(string text) { _log_text.text = text; }

    public void SignupEvent()
    {
        if (_isWaiting) return;

        if (_name_input.text.Equals(string.Empty) || _pw_input.text.Equals(string.Empty) ||
            _nickname_input.text.Equals(string.Empty) || _pw_confirm_input.text.Equals(string.Empty))
        {
            LogTextViewing("모든 항목을 입력하세요"); return;
        }

        if (!_pw_input.text.Equals(_pw_confirm_input.text))
        {
            LogTextViewing("비밀번호가 일치하지 않습니다");
            _pw_confirm_input.text = string.Empty;
            _pw_confirm_input.ActivateInputField(); return;
        }

        if (_pw_input.text.Length < 4)
        {
            LogTextViewing("비밀번호는 4자 이상이어야 합니다"); return;
        }

        if (AuthPlayer.LocalInstance == null)
        {
            LogTextViewing("서버에 연결되어 있지 않습니다"); return;
        }

        _isWaiting = true;
        _signup_button.interactable = false;
        LogTextViewing("처리 중...");

        AuthPlayer.LocalInstance.OnSignupResult = (success, message) =>
        {
            _isWaiting = false;
            _signup_button.interactable = true;

            if (success)
            {
                ClearInputs();
                _login.SetActive(true);
                _logincontroller.LogTextViewing(message);
                _signup_ob.SetActive(false);
            }
            else
            {
                LogTextViewing(message);
            }
        };

        AuthPlayer.LocalInstance.CmdRequestSignup(_name_input.text, _pw_input.text, _nickname_input.text);
    }

    public void BackToLogin()
    {
        ClearInputs();
        LogTextViewing(string.Empty);
        _login.SetActive(true);
        _signup_ob.SetActive(false);
        _logincontroller.LogTextViewing(string.Empty);
    }

    private void ClearInputs()
    {
        _name_input.text = string.Empty;
        _pw_input.text = string.Empty;
        _pw_confirm_input.text = string.Empty;
        _nickname_input.text = string.Empty;
        _isWaiting = false;
        _signup_button.interactable = true;
    }
}