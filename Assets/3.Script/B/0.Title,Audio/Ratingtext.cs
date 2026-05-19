using UnityEngine;
using TMPro;

public class Ratingtext : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _scoreText;

    private void Start()
    {
        DisplayUserData();
    }

    public void DisplayUserData()
    {
        // 1. 싱글톤 및 로그인 정보 체크
        if (SQLManager.Instance == null || SQLManager.Instance.user_info == null)
        {
            Debug.LogWarning("[UserProfileDisplay] SQLManager가 없거나 로그인 정보가 없습니다.");
            if (_scoreText != null) _scoreText.text = "0";
            return;
        }

        string currentNickname = SQLManager.Instance.user_info.user_nickname;

        if (SQLManager.Instance.GetScore(currentNickname, out int dbScore))
        {
            if (_scoreText != null)
            {
                _scoreText.text = dbScore.ToString("N0");
            }
            Debug.Log($"[UI] {currentNickname}님의 DB 점수 로드 완료: {dbScore}점");
        }
        else
        {
            Debug.LogError($"[UI] DB로부터 {currentNickname}님의 점수를 가져오는 데 실패했습니다.");

            if (_scoreText != null)
            {
                _scoreText.text = "Error";
            }
        }
    }

}
