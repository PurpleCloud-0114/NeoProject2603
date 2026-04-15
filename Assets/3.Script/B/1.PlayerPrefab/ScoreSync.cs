using UnityEngine;
using Mirror;

public class ScoreSync : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnScoreChange))]
    public int player_score = 0;

    public void OnScoreChange(int oldVal, int newVal)
    {
        // 점수가 바뀌었을 때 실행할 로직 (예: UI 텍스트 업데이트 또는 애니메이션)
        if (newVal != oldVal)
        {
            AuthPlayer auth = GetComponent<AuthPlayer>();
            // 필요한 경우 PlayerListUIManager 등의 인스턴스를 통해 애니메이션 실행
            Debug.Log($"[Score] 점수 동기화: {oldVal} -> {newVal}");
        }
    }
}