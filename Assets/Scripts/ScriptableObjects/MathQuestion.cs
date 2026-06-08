using UnityEngine;

[CreateAssetMenu(fileName = "MathQuestion", menuName = "RockFalldown/Math Question")]
public class MathQuestion : ScriptableObject
{
    [TextArea]
    public string questionText = "5 + 3 = ?";
    public int correctAnswer = 8;
    public int wrongAnswer = 7;

    [Tooltip("참이면 어느 문으로 가도 통과(둘 다 정답). 못 푸는 문제용 — 양쪽에 correctAnswer 표시")]
    public bool anyAnswerCorrect = false;
}
