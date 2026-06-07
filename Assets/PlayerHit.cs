using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerMove))]
public class PlayerHit : MonoBehaviour
{
    [Tooltip("기본 밀치는 세기 (Rock 컴포넌트 없을 때). 클수록 더 멀리 밀림")]
    public float knockbackForce = 40f;

    Rigidbody rb;
    PlayerMove move;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        move = GetComponent<PlayerMove>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Rock")) return;

        // 바위별 세기 (Rock 컴포넌트 있으면 그 값)
        float force = knockbackForce;
        var rock = collision.gameObject.GetComponent<Rock>();
        if (rock != null) force = rock.knockbackForce;

        // 바위 → 플레이어 방향으로 물리 임펄스 (기존 Rigidbody 느낌)
        Vector3 dir = (transform.position - collision.transform.position).normalized;
        rb.AddForce(dir * force, ForceMode.Impulse);
        move.ApplyKnockback();
    }
}
