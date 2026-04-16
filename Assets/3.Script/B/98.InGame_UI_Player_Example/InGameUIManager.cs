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
        // 로컬 플레이어 오브젝트가 생성될 때까지 대기
        while (NetworkClient.localPlayer == null)
            yield return null;

        // [수정] NetworkRoomPlayer의 index가 -1이 아닐 때까지 대기 (Mirror가 인덱스를 할당할 시간 필요)
        NetworkRoomPlayer myRoom = NetworkClient.localPlayer.GetComponent<NetworkRoomPlayer>();
        float timer = 0f;
        while (myRoom != null && myRoom.index == -1 && timer < 2f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        _myScoreSync = NetworkClient.localPlayer.GetComponent<ScoreSync>();
        _myNickSync = NetworkClient.localPlayer.GetComponent<NickNameSync>();

        InitialSetup();
    }

    private void InitialSetup()
    {
        if (NetworkClient.localPlayer == null) return;

        if (_myScoreSync == null || _myNickSync == null)
        {
            Debug.LogError("[InGameUIManager] ScoreSync 또는 NickNameSync를 찾을 수 없습니다.");
            return;
        }

        if (_nickname_text != null) _nickname_text.text = _myNickSync.player_nickname;
        if (_score_text != null) _score_text.text = $"총점: {_myScoreSync.player_score}";
        if (_roundscore_text != null) _roundscore_text.text = $"라운드: {_myScoreSync.round_total_score}";

        // [수정] NetworkRoomPlayer.index를 사용하여 P1, P2 표시
        NetworkRoomPlayer myRoom = NetworkClient.localPlayer.GetComponent<NetworkRoomPlayer>();
        if (_playernum_text != null)
        {
            if (myRoom != null && myRoom.index != -1)
            {
                _playernum_text.text = $"P{myRoom.index + 1}";
            }
            else
            {
                _playernum_text.text = "준비 중...";
            }
        }
    }

    public void PlayScorePop(int newValue, bool isRoundScore)
    {
        if (isRoundScore)
        {
            if (_roundscore_text != null)
            {
                _roundscore_text.text = $"라운드스코어: {newValue}";
                StartCoroutine(BounceRoutine(_roundscore_text.transform));
            }
        }
        else
        {
            if (_score_text != null)
            {
                _score_text.text = $"레이팅: {newValue}";
                StartCoroutine(BounceRoutine(_score_text.transform));
            }
        }
    }

    private IEnumerator BounceRoutine(Transform target)
    {
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