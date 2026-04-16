using UnityEngine;
using Mirror;

public class GameManagerExample : NetworkBehaviour
{
    [SerializeField] private ObstacleSpawner _spawner;
    [SerializeField] private MapSpawner _map_spawner;

    public override void OnStartServer() //나중에 신규 게임 시작할때나 끝났을때 미리 생성하던지 해야?
    {
        base.OnStartServer();

        // 서버가 기동되면 즉시 맵 생성을 시작함
        Debug.Log("서버가 시작되었습니다. 맵 생성을 트리거합니다.");
        StartNewGame();
    }

    [Server]
    public void StartNewGame()
    {
        _map_spawner.FullGenerate();
        _spawner.GenerateFloatingObstacles();
        RpcNotifyGameStart();
    }

    [ClientRpc]
    private void RpcNotifyGameStart()
    {
        Debug.Log("GameStart!");
    }
}
