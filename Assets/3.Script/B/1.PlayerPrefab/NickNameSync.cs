using UnityEngine;
using TMPro;
using Mirror;

public class NickNameSync : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnNameChange))]
    public string player_nickname = "";

    [SerializeField] private TMP_Text _nicknamecard_tmp; // 캐릭터 머리 위 UI

    public void OnNameChange(string oldName, string newName)
    {
        // 1. 캐릭터 머리 위 닉네임 갱신
        if (_nicknamecard_tmp != null) _nicknamecard_tmp.text = newName;

        // 2. 로비 UI 갱신 (서버 개발자가 만든 LobbyTextUI 활용)
        if (LobbyTextUI.Instance != null)
        {
            AuthPlayer auth = GetComponent<AuthPlayer>();
            NetworkRoomPlayer roomPlayer = GetComponent<NetworkRoomPlayer>();

            if (auth != null && roomPlayer != null)
            {
                // 점수도 함께 표시하려면 ScoreSync를 가져옴
                int currentScore = TryGetComponent<ScoreSync>(out var s) ? s.player_score : 0;

                // roomPlayer.index 또는 auth.player_num을 사용하여 슬롯 위치 결정
                LobbyTextUI.Instance.UpdateUI(roomPlayer.index, newName, roomPlayer.readyToBegin);
                Debug.Log($"[Client] 로비 UI 갱신됨: {newName} (Score: {currentScore})");
            }
        }
    }
}