using UnityEngine;
using System.Collections;

// 빠른 바위: 스폰 후 launchDelay 초 뒤 경사 아래로 큰 속도를 받아 고속으로 굴러온다 (반응시간 짧음). 기획서 4-1.
[RequireComponent(typeof(Rigidbody))]
public class FastRock : MonoBehaviour
{
    [Tooltip("발사 방향(경사 아래쪽). 올라가는 방향이 +Z라 기본 -Z")]
    public Vector3 launchDir = new Vector3(0f, -0.2f, -1f);
    [Tooltip("발사 속도 (클수록 빠름)")]
    public float launchSpeed = 25f;
    [Tooltip("스폰 후 속도를 주기까지 대기 시간(초). 경사면에 착지한 뒤 발사되도록 조정")]
    public float launchDelay = 0.5f;

    void Start()
    {
        StartCoroutine(LaunchAfterDelay());
    }

    IEnumerator LaunchAfterDelay()
    {
        yield return new WaitForSeconds(launchDelay);
        GetComponent<Rigidbody>().linearVelocity = launchDir.normalized * launchSpeed;
    }
}
