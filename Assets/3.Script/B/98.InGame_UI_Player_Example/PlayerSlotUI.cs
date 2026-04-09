using UnityEngine;
using TMPro;
using System.Collections; // 코루틴 사용을 위해 추가

public class PlayerSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text _nickname_text;
    [SerializeField] private TMP_Text _score_text;
    [SerializeField] private TMP_Text _roundscore_text;
    [SerializeField] private TMP_Text _playernum_text;

    private AuthPlayer _auth;
    private ScoreSync _scoreSync;
    private NickNameSync _nickSync;

    public void SetPlayer(AuthPlayer auth, ScoreSync score, NickNameSync nick)
    {
        _auth = auth;
        _scoreSync = score;
        _nickSync = nick;

        gameObject.SetActive(true);
        RefreshUI();
    }

    public void ClearSlot()
    {
        _auth = null;
        _scoreSync = null;
        _nickSync = null;
        if (gameObject.activeSelf) gameObject.SetActive(false);
    }

    public void RefreshUI()
    {
        if (_auth == null || _scoreSync == null || _nickSync == null) return;

        if (_nickname_text != null)
            _nickname_text.text = _nickSync.player_nickname;

        if (_score_text != null)
            _score_text.text = $"{_scoreSync.player_score}";

        if (_roundscore_text != null)
            _roundscore_text.text = $"{_scoreSync.round_total_score}";

        if (_playernum_text != null)
            _playernum_text.text = $"P{_auth.player_num + 1}";
    }

    // 숫자 애니메이션
    public void PlayBounce(bool isRoundScore)
    {
        if (!gameObject.activeInHierarchy) return;
        StopAllCoroutines();
        float targetScale = isRoundScore ? 1.1f : 1.1f;
        StartCoroutine(BounceRoutine(targetScale));
    }

    private IEnumerator BounceRoutine(float targetScale)
    {
        transform.localScale = Vector3.one * targetScale;
        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.one * targetScale, Vector3.one, elapsed / duration);
            yield return null;
        }
        transform.localScale = Vector3.one;
    }
}