using UnityEngine;
using Mirror;

public class GameManagerExample : MonoBehaviour
{
    [SerializeField] private ObstacleSpawner _spawner;

    [Server]
    public void StartNewGame()
    {
        _spawner.GenerateCylindricalMap();
        RpcNotifyGameStart();
    }

    [ClientRpc]
    private void RpcNotifyGameStart()
    {
        Debug.Log("GameStart!");
    }
}
