using UnityEngine;

// 빠른 바위: 경사면에 착지하는 순간 큰 속도를 받아 고속으로 굴러온다 (반응시간 짧음). 기획서 4-1.
[RequireComponent(typeof(Rigidbody))]
public class FastRock : MonoBehaviour
{
    [Tooltip("발사 방향(경사 아래쪽). 올라가는 방향이 +Z라 기본 -Z")]
    public Vector3 launchDir = new Vector3(0f, -0.2f, -1f);
    [Tooltip("발사 속도 (클수록 빠름)")]
    public float launchSpeed = 25f;

    bool launched;

    void OnCollisionEnter(Collision collision)
    {
        if (launched) return;
        if (collision.gameObject.CompareTag("Rock")) return;  // 다른 바위와의 충돌은 무시

        launched = true;
        GetComponent<Rigidbody>().linearVelocity = launchDir.normalized * launchSpeed;
    }
}
