using UnityEngine;

public class PlayerHit : MonoBehaviour
{
    [Tooltip("기본 밀치기 세기 (Rock 컴포넌트 없을 때)")]
    public float knockbackForce = 5f;
    [Tooltip("밀치는 방향(경사 아래쪽). 올라가는 방향이 +Z라 기본 -Z")]
    public Vector3 downhillDir = new Vector3(0f, -0.3f, -1f);

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Rock")) return;

        // 바위별 밀치기 힘 (Rock 컴포넌트 있으면 그 값, 없으면 기본값)
        float force = knockbackForce;
        var rock = collision.gameObject.GetComponent<Rock>();
        if (rock != null) force = rock.knockbackForce;

        // 임펄스 누적 대신 '경사 아래로 살짝' 속도를 직접 지정.
        // PlayerMove가 knockbackDuration 후 위치를 다시 잡아 멈추므로 잔류 힘이 남지 않는다.
        rb.linearVelocity = downhillDir.normalized * force;
        GetComponent<PlayerMove>().ApplyKnockback();
    }
}
