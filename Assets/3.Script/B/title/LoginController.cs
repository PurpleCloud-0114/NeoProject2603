using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using UnityEngine.SceneManagement;

public class LoginController : MonoBehaviour
{
    [SerializeField] private TMP_InputField _name_input;
    [SerializeField] private TMP_InputField _pw_input;
    [SerializeField] private TMP_Text _log_text;
    [SerializeField] private Button _login_button;
    [SerializeField] private Button _signup_button;
    [SerializeField] private SignupController _signupController;
    [SerializeField] private GameObject _title_ob;

    private void Start()
    {
        LogTextViewing(string.Empty);
        _login_button.onClick.AddListener(LoginEvent);
        _signup_button.onClick.AddListener(OpenSignUpPage);
    }

    public void LogTextViewing(string text) { _log_text.text = text; }

    public void LoginEvent()
    {
        if (_name_input.text.Equals(string.Empty) || _pw_input.text.Equals(string.Empty))
        {
            LogTextViewing("이름이나 비밀번호를 입력하세요");
            return;
        }

        if (SQLManager.Instance.Login(_name_input.text, _pw_input.text))
        {
            PlayerPrefs.SetString("PlayerNickname", _name_input.text);
            PlayerPrefs.Save();

            GameObject manager = NetworkManager.singleton.gameObject;
            if (manager.TryGetComponent(out ServerChecker checker))
            {
                checker.Start_Client();
                SceneManager.LoadScene("1.B_Prototypeloginscene"); //추후 제거
            }

            if (_title_ob != null)
                _title_ob.SetActive(true);

            gameObject.SetActive(false);
            //여기까지 제거
        }
        else
        {
            LogTextViewing("아이디 또는 비밀번호를 확인하세요");
        }
    }

    public void OpenSignUpPage()
    {
        if (_signupController != null)
            _signupController.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        ClearInputs();
    }
    private void ClearInputs()
    {
        if (_name_input != null) _name_input.text = string.Empty;
        if (_pw_input != null) _pw_input.text = string.Empty;
        LogTextViewing(string.Empty);
    }
}