using UnityEngine;

public class RockSpawner1 : MonoBehaviour
{
    public GameObject[] rockPrefabs; // 배열로 변경
    public Transform[] spawnPoints;
    public float spawnInterval = 2f;
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnRock();
            timer = 0;
        }
    }

    void SpawnRock()
    {
        int randomRock = Random.Range(0, rockPrefabs.Length);
        int randomPoint = Random.Range(0, spawnPoints.Length);
        Instantiate(rockPrefabs[randomRock], spawnPoints[randomPoint].position, Quaternion.identity);
    }
}