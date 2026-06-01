using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    public float speed = 5f;
    public float knockbackDuration = 0.5f;  // 입력 막을 시간

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
            return;  // 날아가는 중엔 입력 무시
        }

        var kb = Keyboard.current;
        if (kb == null) return;

        float x = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float z = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);

        Vector3 move = new Vector3(x, 0f, z).normalized * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }
}