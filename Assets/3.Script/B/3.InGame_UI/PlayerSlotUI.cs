using UnityEngine;
using TMPro;

public class PlayerSlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _nickname_text;
    [SerializeField] private TMP_Text _score_text;
    [SerializeField] private TMP_Text _roundscore_text;
    [SerializeField] private TMP_Text _playernum_text;

    private ScoreSync _scoreSync;
    private NickNameSync _nickSync;

    // 플레이어 스폰 시 외부에서 호출
    public void SetPlayer(ScoreSync score, NickNameSync nick)
    {
        _scoreSync = score;
        _nickSync = nick;
        gameObject.SetActive(true);
        RefreshUI();
    }

    public void ClearSlot()
    {
        _scoreSync = null;
        _nickSync = null;
        gameObject.SetActive(false);
    }

    // ScoreSync의 hook에서 호출하거나 매니저에서 호출
    public void RefreshUI()
    {
        if (_scoreSync == null || _nickSync == null) return;
        _nickname_text.text = _nickSync.player_nickname;
        _score_text.text = $"{_scoreSync.player_score}";
        _roundscore_text.text = $"{_scoreSync.round_total_score}";
        _playernum_text.text = $"P{_scoreSync.player_ID}";
    }
}