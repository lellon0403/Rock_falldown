using UnityEngine;

// 바위 자동 제거: 소속 스테이지의 아래 경계(killBelowY)보다 내려가면 파괴.
// 값은 RockSpawner가 스폰 시 스테이지 경계로 설정한다. 장애물에 걸려 경계 위에 머무르는 바위는 유지된다.
public class RockDestroyer : MonoBehaviour
{
    [Tooltip("이 Y 높이 아래로 내려가면 제거 (스포너가 스테이지 아래 경계로 설정)")]
    public float killBelowY = 0f;

    void Update()
    {
        if (transform.position.y <= killBelowY)
            Destroy(gameObject);
    }
}