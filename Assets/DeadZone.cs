using UnityEngine;

public class DeadZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 1. 떨어진 오브젝트가 플레이어인지 확인
        PlayerMove player = other.GetComponent<PlayerMove>();

        if (player != null)
        {
            // 2. 플레이어의 움직임과 물리 연산을 완전히 정지시켜 버림 (게임 종료 효과)
            player.enabled = false; // 키보드 입력 스크립트 끄기

            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero; // 떨어지던 속도 0으로 멈추기
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true; // 더 이상 중력이나 물리 안 받게 고정
            }

            // 3. 콘솔창에 최종 점수 표시!
            Debug.Log($"❌❌ GAME OVER ❌❌\n추락했습니다! 최종 점수: {player.currentScore}점");
        }
    }
}