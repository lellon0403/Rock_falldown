using UnityEngine;

public class GoalZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 1. 골인 지점에 닿은 오브젝트가 플레이어인지 확인
        PlayerMove player = other.GetComponent<PlayerMove>();

        if (player != null)
        {
            // 🟢 [순서 변경] 플레이어를 끄기 전에 '현재 점수'부터 확실하게 변수에 저장합니다.
            float finalScore = player.currentScore;

            // 2. 점수를 안전하게 확보한 뒤에 플레이어 조작 및 물리 정지
            player.enabled = false; // 키보드 입력 차단

            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero; // 물리 속도 0으로 정지
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true; // 자리에 딱 고정
            }

            // 3. 확보해둔 finalScore 변수로 최종 점수 출력!
            Debug.Log($"축하합니다! 골인지점에 도착하셨습니다!\n🏅 최종 클리어 점수: {finalScore}점");
        }
    }
}