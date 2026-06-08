using UnityEngine;

// 바위 자동 제거: 일정 시간이 지나거나 맵 아래로 떨어지면 파괴 (씬에 쌓이는 것 방지).
public class RockDestroyer : MonoBehaviour
{
    [Tooltip("스폰 후 이 시간(초)이 지나면 자동 제거")]
    public float lifetime = 8f;
    [Tooltip("이 Y 높이 아래로 내려가면 즉시 제거")]
    public float killBelowY = 0f;

    float age;

    void Update()
    {
        age += Time.deltaTime;
        if (age >= lifetime || transform.position.y <= killBelowY)
            Destroy(gameObject);
    }
}