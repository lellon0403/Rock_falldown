using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    public float speed = 5f;
    public float knockbackDuration = 0.5f;  // 입력 막을 시간

    Rigidbody rb;
    float knockbackTimer = 0f;

    // 🟢 [위치 수정] 변수들을 클래스 안쪽으로 안전하게 이동시켰습니다!
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
        // 게임 시작할 때의 Y축 높이를 기준점으로 저장
        startYPosition = transform.position.y;
    }

    void Update()
    {
        // 현재 높이에서 시작 높이를 빼서 '얼마나 올라갔는지'를 점수로 환산
        float climbedHeight = transform.position.y - startYPosition;

        // 만약 위로 올라갔다면 점수를 최신화 (뒤로 미끄러질 때 점수가 깎이지 않게 방지)
        if (climbedHeight > currentScore)
        {
            // 소수점 첫째 짜리까지만 깔끔하게 점수로 저장 (예: 15.4점)
            currentScore = Mathf.Round(climbedHeight * 10f) / 10f;
        }
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

    public void ApplyKnockback()
    {
        knockbackTimer = knockbackDuration;
    }
}