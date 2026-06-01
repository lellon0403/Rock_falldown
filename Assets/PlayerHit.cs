using UnityEngine;

public class PlayerHit : MonoBehaviour
{
    public float knockbackForce = 5f;  // 날아가는 세기

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Rock"))
        {
            // 바위에서 캐릭터 방향으로 힘을 줌
            Vector3 dir = transform.position - collision.transform.position;
            dir.Normalize();
            rb.AddForce(dir * knockbackForce, ForceMode.Impulse);
               GetComponent<PlayerMove>().ApplyKnockback();
        }
    }
}