using UnityEngine;

// 분열 바위: 장애물에 부딪히거나 일정 시간이 지나면 작은 조각들로 쪼개진다. (기획서 4-1)
[RequireComponent(typeof(Rigidbody))]
public class SplittingRock : MonoBehaviour
{
    public GameObject fragmentPrefab;
    public int fragmentCount = 3;
    [Tooltip("조각 크기 = 본체 크기 × 이 값")]
    public float fragmentRelativeScale = 0.45f;
    public float scatterForce = 3f;
    [Tooltip("장애물을 못 만나도 이 시간 후 자동 분열")]
    public float autoSplitAfter = 4f;

    bool done;
    float bornTime;

    void Start() => bornTime = Time.time;

    void Update()
    {
        if (!done && Time.time - bornTime >= autoSplitAfter) Split();
    }

    void OnCollisionEnter(Collision c)
    {
        if (done) return;
        if (c.gameObject.GetComponent<Obstacle>() != null) Split();   // 장애물에 부딪히면 분열
    }

    void Split()
    {
        done = true;

        if (fragmentPrefab != null)
        {
            float childScale = transform.localScale.x * fragmentRelativeScale;
            for (int i = 0; i < fragmentCount; i++)
            {
                float ang = (360f / fragmentCount) * i + Random.Range(-25f, 25f);
                Vector3 dir = Quaternion.Euler(0f, ang, 0f) * Vector3.forward;
                Vector3 pos = transform.position + dir * 0.6f + Vector3.up * 0.2f;

                var frag = Instantiate(fragmentPrefab, pos, Random.rotation);
                frag.transform.localScale = Vector3.one * childScale;

                // 본체의 스테이지 경계를 파편에도 물려줌 (스테이지 벗어나면 제거)
                var myRd = GetComponent<RockDestroyer>();
                var fragRd = frag.GetComponent<RockDestroyer>();
                if (myRd != null && fragRd != null) fragRd.killBelowY = myRd.killBelowY;

                var rb = frag.GetComponent<Rigidbody>();
                if (rb != null) rb.AddForce(dir * scatterForce + Vector3.up, ForceMode.Impulse);
            }
        }

        Destroy(gameObject);
    }
}
