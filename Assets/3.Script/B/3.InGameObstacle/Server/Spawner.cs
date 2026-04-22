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
        ResetGame();
    }

    [Server]
    public void ResetGame()
    {
        Debug.Log("[Spawner] 게임 리셋");

        if (startRoutine != null)
        {
            StopCoroutine(startRoutine);
            startRoutine = null;
        }

        _mapSpawner.ReturnMapToPool();

        if (_obstacleSpawner != null)
            _obstacleSpawner.ReturnAllToPool();

        _mapSpawner.FullGenerate();

        RpcNotifyGameStart();
    }

    [ClientRpc]
    private void RpcNotifyGameStart()
    {
        Debug.Log("Game Start!");
    }

    [ContextMenu("Force Reset")]
    public void ForceReset()
    {
        if (NetworkServer.active)
            ResetGame();
    }
}