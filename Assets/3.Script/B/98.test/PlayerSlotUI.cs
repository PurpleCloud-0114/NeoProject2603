using UnityEngine;
using TMPro;
using System.Collections;
using Mirror; // <--- 1. 이 줄이 없으면 NetworkRoomPlayer에서 빨간 줄이 뜹니다.

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

        // 2. _auth가 할당된 후 해당 오브젝트에서 NetworkRoomPlayer를 가져옵니다.
        NetworkRoomPlayer roomPlayer = _auth.GetComponent<NetworkRoomPlayer>();

        if (_nickname_text != null) _nickname_text.text = _nickSync.player_nickname;
        if (_score_text != null) _score_text.text = $"{_scoreSync.player_score}";
        if (_roundscore_text != null) _roundscore_text.text = $"{_scoreSync.round_total_score}";

        // index가 0이면 P1, 1이면 P2로 표시
        if (_playernum_text != null && roomPlayer != null)
        {
            _playernum_text.text = $"P{roomPlayer.index + 1}";
        }
    }

    // 숫자 애니메이션
    public void PlayBounce(bool isRoundScore)
    {
        if (!gameObject.activeInHierarchy) return;
        StopAllCoroutines();
        float targetScale = 1.1f;
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