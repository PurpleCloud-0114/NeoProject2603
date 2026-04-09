using UnityEngine;
using TMPro;
using Mirror;
using System.Collections;

public class InGameUIManager : MonoBehaviour
{
    public static InGameUIManager Instance;

    [Header("내 정보 UI")]
    [SerializeField] private TMP_Text _nickname_text;
    [SerializeField] private TMP_Text _score_text;
    [SerializeField] private TMP_Text _roundscore_text;
    [SerializeField] private TMP_Text _playernum_text;

    private ScoreSync _myScoreSync;
    private NickNameSync _myNickSync;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(WaitForLocalPlayer());
    }

    private IEnumerator WaitForLocalPlayer()
    {
        while (NetworkClient.localPlayer == null)
            yield return null;

        AuthPlayer myAuth = NetworkClient.localPlayer.GetComponent<AuthPlayer>();
        float timer = 0f;
        while (myAuth != null && myAuth.player_num == -1 && timer < 2f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        _myScoreSync = NetworkClient.localPlayer.GetComponent<ScoreSync>();
        _myNickSync = NetworkClient.localPlayer.GetComponent<NickNameSync>();

        // 초기 UI 설정 (한 번만 실행)
        InitialSetup();
    }

    private void InitialSetup()
    {
        if (NetworkClient.localPlayer == null) { return; }
        
        if (_myScoreSync == null || _myNickSync == null)
        {
            Debug.LogError("[InGameUIManager] ScoreSync 또는 NickNameSync 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        if (_nickname_text != null)
            _nickname_text.text = _myNickSync.player_nickname;

        if (_score_text != null)
            _score_text.text = $"총점: {_myScoreSync.player_score}";

        if (_roundscore_text != null)
            _roundscore_text.text = $"라운드: {_myScoreSync.round_total_score}";

        // AuthPlayer 및 player_num 체크
        AuthPlayer myAuth = NetworkClient.localPlayer.GetComponent<AuthPlayer>();
        if (_playernum_text != null)
        {
            if (myAuth != null && myAuth.player_num != -1)
            {
                _playernum_text.text = $"P{myAuth.player_num + 1}";
            }
            else
            {
                _playernum_text.text = "준비 중...";
            }
        }
    }

    /// <summary>
    /// ScoreSync의 Hook에서 호출될 애니메이션 재생 함수
    /// </summary>
    public void PlayScorePop(int newValue, bool isRoundScore)
    {
        if (isRoundScore)
        {
            if (_roundscore_text != null)
            {
                _roundscore_text.text = $"라운드: {newValue}";
                StartCoroutine(BounceRoutine(_roundscore_text.transform));
            }
        }
        else
        {
            if (_score_text != null)
            {
                _score_text.text = $"총점: {newValue}";
                StartCoroutine(BounceRoutine(_score_text.transform));
            }
        }
    }

    private IEnumerator BounceRoutine(Transform target)
    {
        // 간단한 팝 애니메이션
        target.localScale = Vector3.one * 1.2f;
        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            target.localScale = Vector3.Lerp(Vector3.one * 1.2f, Vector3.one, elapsed / duration);
            yield return null;
        }
        target.localScale = Vector3.one;
    }
}