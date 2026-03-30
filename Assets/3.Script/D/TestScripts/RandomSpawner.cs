using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawner : MonoBehaviour {
    [Header("생성할 프리팹 설정")]
    [Tooltip("기본 큐브 프리팹을 넣으세요.")]
    public GameObject cubePrefab;
    [Tooltip("기본 스피어 프리팹을 넣으세요.")]
    public GameObject spherePrefab;

    [Header("생성 개수")]
    public int spawnCount = 10;

    [Header("위치 무작위 범위 (최소/최대)")]
    public Vector3 minPosition = new Vector3(-5f, 0f, -5f);
    public Vector3 maxPosition = new Vector3(5f, 5f, 5f);

    [Header("크기 무작위 범위 (최소/최대)")]
    public float minScale = 0.5f;
    public float maxScale = 2.0f;

    void Start() {
        // 게임 시작 시 spawnCount만큼 오브젝트 생성
        for (int i = 0; i < spawnCount; i++) {
            SpawnRandomObject();
        }
    }

    void SpawnRandomObject() {
        // 1. 큐브와 스피어 중 무작위로 하나 선택 (0 또는 1)
        GameObject prefabToSpawn = (Random.Range(0, 2) == 0) ? cubePrefab : spherePrefab;

        // 2. 설정된 범위 내에서 무작위 위치 계산
        float randomX = Random.Range(minPosition.x, maxPosition.x);
        float randomY = Random.Range(minPosition.y, maxPosition.y);
        float randomZ = Random.Range(minPosition.z, maxPosition.z);
        Vector3 randomPos = new Vector3(randomX, randomY, randomZ);

        // 3. 오브젝트 생성 (Instantiate)
        GameObject newObject = Instantiate(prefabToSpawn, randomPos, Quaternion.identity);

        // 4. 무작위 크기 적용 (XYZ 동일한 비율로 크기 조절)
        float randomScale = Random.Range(minScale, maxScale);
        newObject.transform.localScale = new Vector3(randomScale, randomScale, randomScale);

        // 5. 무작위 색상 적용
        Renderer objRenderer = newObject.GetComponent<Renderer>();
        if (objRenderer != null) {
            // Random.ColorHSV()를 사용하면 좀 더 선명하고 예쁜 무작위 색상이 나옵니다.
            objRenderer.material.color = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f);
        }
    }
}
