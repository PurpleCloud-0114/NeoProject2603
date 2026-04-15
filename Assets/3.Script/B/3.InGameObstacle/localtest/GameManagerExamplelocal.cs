using UnityEngine;

public class GameManagerExamplelocal : MonoBehaviour
{
    [SerializeField] private ObstacleSpawnerlocal _spawner;
    [SerializeField] private MapSpawnerlocal _map_spawner;

    private void Start()
    {
        // 로컬 테스트: 게임이 시작되자마자 맵 생성
        StartNewGame();
    }

    public void StartNewGame()
    {
        _map_spawner.FullGenerate();
        // _spawner.GenerateFloatingObstacles(); 는 MapSpawner에서 호출하므로 생략
        NotifyGameStart();
    }

    private void NotifyGameStart()
    {
        Debug.Log("GameStart!");
    }
}