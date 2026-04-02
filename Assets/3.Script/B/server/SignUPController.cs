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
    [SerializeField] private TMP_Text _log_text;
    [SerializeField] private Button _signup_button;

    private void Start()
    {
        LogTextViewing(string.Empty);
        _signup_button.onClick.AddListener(SignupEvent);
    }

    private void LogTextViewing(string text) { _log_text.text = text; }

    public void SignupEvent()
    {
        if (_name_input.text.Equals(string.Empty) || _pw_input.text.Equals(string.Empty) || _nickname_input.text.Equals(string.Empty))
        {
            LogTextViewing("이름, 닉네임, 비밀번호를 모두 입력하세요");
            return;
        }

        if (SQLManager.Instance.SignupIDCheck(_name_input.text))
        {
            LogTextViewing("이미 사용 중인 아이디입니다");
            return;
        }

        if (SQLManager.Instance.SignupNicknameCheck(_nickname_input.text))
        {
            LogTextViewing("이미 사용 중인 닉네임입니다");
            return;
        }

        if (SQLManager.Instance.Signup(_name_input.text, _pw_input.text, _nickname_input.text))
        {
            _name_input.text = string.Empty;
            _pw_input.text = string.Empty;
            _nickname_input.text = string.Empty;

            _login.SetActive(true);
            _logincontroller.LogTextViewing("회원가입이 완료되었습니다."); // 하나만
            _signup_ob.SetActive(false);
        }
        else
        {
            LogTextViewing("회원가입에 실패했습니다. 다시 시도해주세요");
        }
    }
}