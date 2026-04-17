using UnityEngine;
using Mirror;

public class GameManagerExample : NetworkBehaviour
{
    [SerializeField] private ObstacleSpawner _spawner;
    [SerializeField] private MapSpawner _map_spawner;

    // 기존의 OnStartServer() 자동 실행 로직 제거

    [Server]
    public void StartNewGame()
    {
        Debug.Log("[GameManager] 게임 씬 진입 완료! 맵 생성을 시작합니다.");

        _map_spawner.FullGenerate();
        _spawner.GenerateFloatingObstacles();

        RpcNotifyGameStart();
    }

    [ClientRpc]
    private void RpcNotifyGameStart()
    {
        Debug.Log("GameStart!");
    }

    [ContextMenu("Force Generate Map Now")]
    public void ForceGenerate()
    {
        if (NetworkServer.active) StartNewGame();
    }
}