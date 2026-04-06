using UnityEngine;
using TMPro;
using Mirror;

public class InGameUIManager : MonoBehaviour
{
    [Header("내 정보 UI")]
    [SerializeField] private TMP_Text _nickname_text;
    [SerializeField] private TMP_Text _score_text;
    [SerializeField] private TMP_Text _roundscore_text;
    [SerializeField] private TMP_Text _playernum_text;

    private ScoreSync _myScoreSync;
    private NickNameSync _myNickSync;

    private void Start()
    {
        // 로컬 플레이어 컴포넌트 찾기
        StartCoroutine(WaitForLocalPlayer());
    }

    private System.Collections.IEnumerator WaitForLocalPlayer()
    {
        // 로컬 플레이어 스폰 대기
        while (NetworkClient.localPlayer == null)
            yield return null;

        _myScoreSync = NetworkClient.localPlayer.GetComponent<ScoreSync>();
        _myNickSync = NetworkClient.localPlayer.GetComponent<NickNameSync>();

        // SyncVar 변경 시 UI 갱신을 위해 hook 대신 Update에서 폴링
        InvokeRepeating(nameof(RefreshUI), 0f, 0.5f); // 0.5초마다 갱신
    }

    private void RefreshUI()
    {
        if (_myScoreSync == null || _myNickSync == null) return;

        _nickname_text.text = _myNickSync.player_nickname;
        _score_text.text = $"총점: {_myScoreSync.player_score}";
        _roundscore_text.text = $"라운드: {_myScoreSync.round_total_score}";
        _playernum_text.text = $"P{_myScoreSync.player_ID}";
    }
}