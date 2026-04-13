using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

    private void Start()
    {
        LogTextViewing(string.Empty);
        _signup_button.onClick.AddListener(SignupEvent);
        _back_button.onClick.AddListener(BackToLogin);
    }

    private void LogTextViewing(string text) { _log_text.text = text; }

    public void SignupEvent()
    {
        if (_name_input.text.Equals(string.Empty) || _pw_input.text.Equals(string.Empty) ||
            _nickname_input.text.Equals(string.Empty) || _pw_confirm_input.text.Equals(string.Empty))
        {
            LogTextViewing("모든 항목을 입력하세요"); return;
        }

        if (!_pw_input.text.Equals(_pw_confirm_input.text))
        {
            LogTextViewing("Password 가 일치하지 않습니다");
            _pw_confirm_input.text = string.Empty;
            _pw_confirm_input.ActivateInputField(); return;
        }

        if (_pw_input.text.Length < 4)
        {
            LogTextViewing("Password 는 4자 이상이어야 합니다"); return;
        }

        // SQLManager 직접 호출 (오프라인씬)
        int result = SQLManager.Instance.Signup(
            _name_input.text, _pw_input.text, _nickname_input.text);

        switch (result)
        {
            case 0:
                ClearInputs();
                _login.SetActive(true);
                _logincontroller.LogTextViewing("회원가입이 완료되었습니다.");
                _signup_ob.SetActive(false);
                break;
            case 1: LogTextViewing("이미 사용 중인 ID 입니다"); break;
            case 2: LogTextViewing("이미 사용 중인 NickName 입니다"); break;
            default: LogTextViewing("회원가입에 실패했습니다. 다시 시도하세요"); break;
        }
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
    }
}