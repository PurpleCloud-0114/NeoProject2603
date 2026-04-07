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

        _myScoreSync = NetworkClient.localPlayer.GetComponent<ScoreSync>();
        _myNickSync = NetworkClient.localPlayer.GetComponent<NickNameSync>();

        // 초기 UI 설정 (한 번만 실행)
        InitialSetup();
    }

    private void InitialSetup()
    {
        if (_myScoreSync == null || _myNickSync == null) return;

        if (_nickname_text != null) _nickname_text.text = _myNickSync.player_nickname;
        if (_score_text != null) _score_text.text = $"총점: {_myScoreSync.player_score}";
        if (_roundscore_text != null) _roundscore_text.text = $"라운드: {_myScoreSync.round_total_score}";

        // player_ID 대신 player_num을 사용하는 것이 UI상 더 깔끔할 수 있습니다.
        if (_playernum_text != null) _playernum_text.text = $"P{_myScoreSync.player_ID}";
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