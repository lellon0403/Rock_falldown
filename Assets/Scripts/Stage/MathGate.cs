using UnityEngine;
using UnityEngine.Events;
using TMPro;

// 수학 퍼즐 게이트. 문제를 표시하고, 정답/오답 문을 무작위로 좌우에 배치한다.
public class MathGate : MonoBehaviour
{
    [Header("문제")]
    public MathQuestion question;
    public TMP_Text questionLabel;

    [Header("문 (2개)")]
    public GateDoor leftDoor;
    public GateDoor rightDoor;

    [Header("보상 / 연출 (선택)")]
    public int iqBonus = 30;   // 스테이지 클리어당 +30 (60→90→120→150)
    public UnityEvent onCorrect;   // 정답 통과 시 — IQ/VFX/사운드 인스펙터 연결용
    public UnityEvent onWrong;     // 오답 충돌 시

    bool solved;

    void Start()
    {
        if (question == null)
        {
            Debug.LogWarning("[MathGate] question(문제 데이터)이 비어 있습니다.", this);
            return;
        }

        if (questionLabel != null)
        {
            questionLabel.text = question.questionText;
            questionLabel.color = new Color(1f, 0.84f, 0.1f);   // 노란색 = 문제
            questionLabel.fontStyle = FontStyles.Bold;
            questionLabel.outlineColor = Color.black;
            questionLabel.outlineWidth = 0.2f;
        }

        // 정답을 좌/우 무작위 배치
        bool correctOnLeft = Random.value < 0.5f;

        if (leftDoor != null)
            leftDoor.Setup(this, correctOnLeft,
                correctOnLeft ? question.correctAnswer : question.wrongAnswer);

        if (rightDoor != null)
            rightDoor.Setup(this, !correctOnLeft,
                correctOnLeft ? question.wrongAnswer : question.correctAnswer);
    }

    public void NotifyCorrect()
    {
        if (solved) return;
        solved = true;
        if (IQManager.Instance != null) IQManager.Instance.Add(iqBonus);
        Debug.Log($"[MathGate] 정답! +{iqBonus} IQ");
        onCorrect?.Invoke();
    }

    public void NotifyWrong()
    {
        Debug.Log("[MathGate] 오답! 벽에 막혀 미끄러짐");
        onWrong?.Invoke();
    }
}
