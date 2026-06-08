using UnityEngine;
using TMPro;

// 화면에 경과 시간을 표시한다.
public class GameTimerDisplay : MonoBehaviour
{
    public TMP_Text label;

    void Update()
    {
        if (label != null && GameTimer.Instance != null)
            label.text = GameTimer.Format(GameTimer.Instance.Elapsed);
    }
}
