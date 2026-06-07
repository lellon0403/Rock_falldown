using UnityEngine;
using TMPro;

// 게이트의 문 한 짝. 본체 콜라이더는 감지용 트리거이고, 자식 barrier가 실제로 막는 벽이다.
// 정답 문: 플레이어가 닿으면 barrier를 끄고 통과시킨다.
// 오답 문: barrier가 그대로 남아 플레이어가 부딪혀 경사로 미끄러진다.
[RequireComponent(typeof(Collider))]
public class GateDoor : MonoBehaviour
{
    [Tooltip("막는 벽 (정답이면 통과 시 비활성화)")]
    public GameObject barrier;
    public TMP_Text answerLabel;
    [Tooltip("오답일 때 플레이어를 밀치는 세기")]
    public float wrongKnockbackForce = 15f;
    [Tooltip("밀리는 방향(경사 아래=뒤쪽)")]
    public Vector3 wrongDir = new Vector3(0f, -0.3f, -1f);

    MathGate gate;
    bool isCorrect;
    bool opened;

    public void Setup(MathGate owner, bool correct, int answer)
    {
        gate = owner;
        isCorrect = correct;
        opened = false;

        if (barrier != null) barrier.SetActive(true);
        if (answerLabel != null) answerLabel.text = answer.ToString();

        GetComponent<Collider>().isTrigger = true;   // 본체 콜라이더 = 감지 존
    }

    void OnTriggerEnter(Collider other)
    {
        if (gate == null) return;
        if (other.GetComponentInParent<PlayerMove>() == null) return;   // 플레이어만 반응

        if (isCorrect)
        {
            if (opened) return;
            opened = true;
            if (barrier != null) barrier.SetActive(false);   // 정답 → 문 열림
            gate.NotifyCorrect();
        }
        else
        {
            // 오답 → 물리 임펄스로 밀림 (기존 Rigidbody 느낌)
            var rb = other.attachedRigidbody;
            if (rb != null) rb.AddForce(wrongDir.normalized * wrongKnockbackForce, ForceMode.Impulse);
            var pm = other.GetComponentInParent<PlayerMove>();
            if (pm != null) pm.ApplyKnockback();

            gate.NotifyWrong();
        }
    }
}
