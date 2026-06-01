using System.Collections;
using UnityEngine;

public class RockSpawner : MonoBehaviour
{
    public GameObject rockPrefab;
    public float interval = 1.5f;
    public float spawnRangeX = 10f;

    void Start()
    {
        InvokeRepeating("SpawnRock", 1f, interval);
    }

    void SpawnRock()
    {
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
        Instantiate(rockPrefab, spawnPos, Quaternion.identity);
    }
}