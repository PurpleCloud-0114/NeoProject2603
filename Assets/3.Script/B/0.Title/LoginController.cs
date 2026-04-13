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

        _isWaiting = true;
        _login_button.interactable = false;
        LogTextViewing("로그인 중...");

        // SQLManager 직접 호출 (오프라인씬)
        int result = SQLManager.Instance.Login(
            _name_input.text, _pw_input.text,
            out string nickname, out int score);

        if (result == 0)
        {
            // PlayerPrefs에 닉네임 저장 (TitleController 환영 메시지용)
            PlayerPrefs.SetString("PlayerNickname", nickname);
            PlayerPrefs.Save();

            // Mirror 서버 접속 (Player Prefab 스폰 → AuthPlayer.CmdInitialize 자동 호출)
            GameObject manager = NetworkManager.singleton.gameObject;
            if (manager.TryGetComponent(out ServerChecker checker))
                checker.Start_Client();

            if (_title_ob != null) _title_ob.SetActive(true);
            gameObject.SetActive(false);
        }
        else
        {
            string msg = result == 1 ? "ID 또는 Password 가 틀렸습니다" : "서버 오류가 발생했습니다";
            LogTextViewing(msg);
            _isWaiting = false;
            _login_button.interactable = true;
        }
    }

    public void OpenSignUpPage()
    {
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