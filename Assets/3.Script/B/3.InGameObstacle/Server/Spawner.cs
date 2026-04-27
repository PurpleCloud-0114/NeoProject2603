using UnityEngine;
using Mirror;
using System.Collections;

public class Spawner : NetworkBehaviour
{
    public static Spawner Instance;

    [SerializeField] private MapSpawner _mapSpawner;
    [SerializeField] private ObstacleSpawner _obstacleSpawner;

    private Coroutine startRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public override void OnStartServer()
    {
        startRoutine = StartCoroutine(StartRoutine());
    }

    private IEnumerator StartRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        StartNewGame();
    }

    [Server]
    public void StartNewGame()
    {
        Debug.Log("[Spawner] 새 게임 시작");

        if (startRoutine != null)
        {
            StopCoroutine(startRoutine);
            startRoutine = null;
        }

        _mapSpawner.ReturnMapToPool();
        _obstacleSpawner.ReturnAllToPool();

        _mapSpawner.FullGenerate();

        RpcNotifyGameStart();
    }

    [ClientRpc]
    private void RpcNotifyGameStart()
    {
        Debug.Log("Game Start!");
    }

    [ContextMenu("Force Generate")]
    public void ForceGenerate()
    {
        if (NetworkServer.active)
            StartNewGame();
    }
}