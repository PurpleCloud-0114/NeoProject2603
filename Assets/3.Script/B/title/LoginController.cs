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

    private bool _isWaiting = false; // 서버 응답 대기 중 중복 클릭 방지

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
            LogTextViewing("이름이나 비밀번호를 입력하세요");
            return;
        }

        // 1. 서버에 먼저 접속
        GameObject manager = NetworkManager.singleton.gameObject;
        if (!manager.TryGetComponent(out ServerChecker checker)) return;

        _isWaiting = true;
        _login_button.interactable = false;
        LogTextViewing("서버에 연결 중...");

        checker.Start_Client();

        // 2. AuthPlayer가 생성되면 로그인 Command 전송
        // NetworkManager의 OnClientConnect 시점에서 호출하기 위해 대기
        StartCoroutine(WaitAndSendLogin(_name_input.text, _pw_input.text));
    }

    private System.Collections.IEnumerator WaitAndSendLogin(string name, string pw)
    {
        // AuthPlayer.LocalInstance가 생길 때까지 대기 (최대 5초)
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

        // 3. 로그인 결과 콜백 등록
        AuthPlayer.LocalInstance.OnLoginResult = (success, nickname, score, message) =>
        {
            _isWaiting = false;
            _login_button.interactable = true;

            if (success)
            {
                // PlayerPrefs에는 nickname 저장 (TitleController에서 사용)
                PlayerPrefs.SetString("PlayerNickname", nickname);
                PlayerPrefs.Save();

                if (_title_ob != null) _title_ob.SetActive(true);
                gameObject.SetActive(false);
            }
            else
            {
                LogTextViewing(message);
                // 실패 시 서버가 끊어주지만 클라이언트 측도 정리
                if (NetworkClient.active) NetworkManager.singleton.StopClient();
            }
        };

        // 4. 로그인 요청
        AuthPlayer.LocalInstance.CmdRequestLogin(name, pw);
    }

    public void OpenSignUpPage()
    {
        // 회원가입은 서버 접속 후 가능 — 먼저 접속 후 패널 전환
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