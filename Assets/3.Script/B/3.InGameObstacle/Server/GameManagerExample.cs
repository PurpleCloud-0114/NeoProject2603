using UnityEngine;
using Mirror;

public class GameManagerExample : NetworkBehaviour
{
    [SerializeField] private ObstacleSpawner _spawner;
    [SerializeField] private MapSpawner _map_spawner;

    [Server]
    public void StartNewGame()
    {
        _map_spawner.GenerateMap();
        _spawner.GenerateCylindricalMap();
        RpcNotifyGameStart();
    }

    [ClientRpc]
    private void RpcNotifyGameStart()
    {
        Debug.Log("GameStart!");
    }
}
