using UnityEngine;
using Mirror;

public class MapOptimization : NetworkBehaviour
{
    [Tooltip("플레이어보다 이 값만큼 위에 있으면 내 화면에서 비활성화")]
    [SerializeField] private float _disableOffset = 20f;

    private Transform _localPlayerTr;

    void Update()
    {
        // 로컬 플레이어 트랜스폼 캐싱
        if (_localPlayerTr == null)
        {
            if (NetworkClient.localPlayer != null)
                _localPlayerTr = NetworkClient.localPlayer.transform;
            return;
        }

        // 장애물 높이가 (플레이어 높이 + 오프셋)보다 커지면 (즉, 플레이어가 한참 아래로 가면)
        if (transform.position.y > _localPlayerTr.position.y + _disableOffset)
        {
            // 서버에서 Destroy 하는 것이 아니라, 내 화면에서만 끄는 것 (최적화)
            gameObject.SetActive(false);
        }
    }
}