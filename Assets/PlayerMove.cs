using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    public float speed = 5f;
    [Tooltip("넉백 지속 시간 (이 동안 입력 잠금, 끝에 속도 0)")]
    public float knockbackDuration = 0.5f;
    [Tooltip("넉백 중 감속 세기 (클수록 빨리 느려짐)")]
    public float knockbackDamping = 5f;

    Rigidbody rb;
    float knockbackTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    public void ApplyKnockback()
    {
        knockbackTimer = knockbackDuration;
    }

    void FixedUpdate()
    {
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            if (knockbackTimer <= 0f)
                rb.linearVelocity = Vector3.zero;   // 지속시간 끝 → 완전 정지
            else
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero,
                                                 knockbackDamping * Time.fixedDeltaTime);  // 빠르게 감속
            return;  // 밀리는 중엔 입력 잠금
        }

        var kb = Keyboard.current;
        if (kb == null) return;

        float x = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float z = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);

        Vector3 move = new Vector3(x, 0f, z).normalized * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }
}
