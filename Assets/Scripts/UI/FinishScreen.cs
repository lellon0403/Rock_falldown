using UnityEngine;
using TMPro;

// 완주 화면. 트로피에 닿으면 기록(시간)과 함께 패널을 띄운다.
public class FinishScreen : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text resultLabel;
    public string format = "완주!\n기록 {0}";

    void Awake()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void Show(float elapsed)
    {
        if (panel != null) panel.SetActive(true);
        if (resultLabel != null)
            resultLabel.text = string.Format(format, GameTimer.Format(elapsed));
        Time.timeScale = 0f;   // 게임 정지
    }
}
