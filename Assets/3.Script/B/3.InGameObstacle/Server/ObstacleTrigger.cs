using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 서버에서만 충돌 판정을 처리함 (권장)
        if (!Mirror.NetworkServer.active) return;

        if (other.CompareTag("Player"))
        {
            // 부모의 MapFloor를 찾아 마스크 업데이트 요청
            var floor = GetComponentInParent<MapFloor>();
            if (floor != null)
            {
                // 이 방해물의 인덱스를 찾아 서버에서 꺼버림
                floor.Server_DisableObstacle(gameObject);
            }
        }
    }
}
