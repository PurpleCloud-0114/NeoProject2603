using UnityEngine;
using Mirror;

public class GameManagerExample : NetworkBehaviour
{
    [SerializeField] private ObstacleSpawner _spawner;
    [SerializeField] private MapSpawner _map_spawner;

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
