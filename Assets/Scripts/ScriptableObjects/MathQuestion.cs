using UnityEngine;

[CreateAssetMenu(fileName = "MathQuestion", menuName = "RockFalldown/Math Question")]
public class MathQuestion : ScriptableObject
{
    [TextArea]
    public string questionText = "5 + 3 = ?";
    public int correctAnswer = 8;
    public int wrongAnswer = 7;
}
