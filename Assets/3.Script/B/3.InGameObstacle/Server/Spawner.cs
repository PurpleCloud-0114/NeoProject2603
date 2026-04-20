using UnityEngine;
using Mirror;

public class Spawner : NetworkBehaviour
{
    public static Spawner Instance = null;
    [SerializeField] private ObstacleSpawner _spawner;
    [SerializeField] private MapSpawner _map_spawner;

    private void Awake()
    {
        if (Instance = null) Instance = this;
    }

    private void Start()
    {
        StartNewGame();
    }

    [Server]
    public void StartNewGame()
    {
        Debug.Log("[Spawner] 맵 생성을 시작합니다.");

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