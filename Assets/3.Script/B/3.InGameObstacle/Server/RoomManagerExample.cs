using UnityEngine;
using Mirror;

public class MyRoomManager : NetworkRoomManager
{
    // 서버가 씬 전환(대기실 -> 게임 씬)을 완전히 끝냈을 때 호출됩니다.
    public override void OnRoomServerSceneChanged(string sceneName)
    {
        base.OnRoomServerSceneChanged(sceneName);

        // 현재 넘어온 씬이 GameplayScene인지 확인합니다.
        if (sceneName == GameplayScene)
        {
            // 씬에 배치된 GameManagerExample을 찾습니다.
            GameManagerExample gm = FindAnyObjectByType<GameManagerExample>();

            if (gm != null)
            {
                gm.StartNewGame();
            }
            else
            {
                Debug.LogError("[MyRoomManager] 씬에서 GameManagerExample을 찾을 수 없습니다!");
            }
        }
    }
}