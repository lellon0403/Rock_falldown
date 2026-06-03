using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    public float speed = 5f;
    public float knockbackDuration = 0.5f;  // 입력 막을 시간

    Rigidbody rb;
    float knockbackTimer = 0f;

    [Header("점수 시스템")]
    public float currentScore = 0f; // 실시간 점수(높이)
    private float startYPosition;   // 처음 시작할 때의 Y값

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Start()
    {
        startYPosition = transform.position.y;
    }

    void Update()
    {
        float climbedHeight = transform.position.y - startYPosition;

        // 위로 올라갔다면 점수를 최신화 (미끄러질 때 점수 깎임 방지)
        if (climbedHeight > currentScore)
        {
            currentScore = Mathf.Round(climbedHeight * 10f) / 10f;
        }
    }

    void FixedUpdate()
    {
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            return;
        }

        var kb = Keyboard.current;
        if (kb == null) return;

        float x = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float z = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);

        Vector3 move = new Vector3(x, 0f, z).normalized * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }

    public void ApplyKnockback()
    {
        knockbackTimer = knockbackDuration;
    }
}