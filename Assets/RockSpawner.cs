using System.Collections;
using UnityEngine;

public class RockSpawner : MonoBehaviour
{
    [Header("바위 프리팹 목록")]
    [Header("⚠️ 0: 기본 큰돌 | 1: 그냥 작은돌(속도기본) | 2: 작고 빠른돌(4배 핵가속)")]
    public GameObject[] rockPrefabs;

    public float interval = 1.5f;

    void Start()
    {
        InvokeRepeating("SpawnRock", 1f, interval);
    }

    void SpawnRock()
    {
        if (rockPrefabs == null || rockPrefabs.Length == 0) return;

        // 1. 구역 나누는 로직 (그대로 유지)
        float randomX;
        int zone = Random.Range(0, 3);

        if (zone == 0) randomX = Random.Range(-13f, -3f);      // 왼쪽
        else if (zone == 1) randomX = Random.Range(-3f, 3f);   // 가운데
        else randomX = Random.Range(3f, 13f);                  // 오른쪽

        Vector3 spawnPos = new Vector3(
            transform.position.x + randomX,
            transform.position.y,
            transform.position.z
        );

        // 2. 랜덤으로 돌 종류 선택
        int randomRockIndex = Random.Range(0, rockPrefabs.Length);
        GameObject selectedRock = rockPrefabs[randomRockIndex];

        // 3. 돌 소환
        GameObject spawnedRock = Instantiate(selectedRock, spawnPos, Quaternion.identity);

        // 4. 🔥 3번째 돌(Element 2 = 작고 빠른 돌) 속도 4배 초강력 부스터 적용!
        if (randomRockIndex == 2)
        {
            Rigidbody rb = spawnedRock.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // 속도가 너무 빨라 맵을 뚫는 걸 방지하기 위해 물리 감지 모드를 가장 정밀하게 변경
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                // 가속도가 미쳐 날뛰도록 무게 세팅
                rb.mass = 200f;

                // 🚀 [4배 가속] Y축 -140f, Z축 -100f로 문자 그대로 레일건처럼 쏴버립니다.
                Vector3 heavyVelocity = new Vector3(Random.Range(-5f, 5f), -140f, -100f);
                rb.linearVelocity = heavyVelocity;

                // 뒤틀리며 날아가도록 회전력도 4배 추가
                rb.angularVelocity = new Vector3(Random.Range(-40f, 40f), 0f, Random.Range(-40f, 40f));
            }
        }
    }
}