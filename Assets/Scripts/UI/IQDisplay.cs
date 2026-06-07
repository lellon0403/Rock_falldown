using UnityEngine;
using TMPro;

// 화면 좌상단 IQ 숫자 표시. IQManager 변화에 맞춰 갱신된다.
public class IQDisplay : MonoBehaviour
{
    public TMP_Text label;
    public string format = "IQ {0}";

    void Start()
    {
        if (IQManager.Instance != null)
        {
            IQManager.Instance.OnChanged += Refresh;
            Refresh(IQManager.Instance.CurrentIQ);
        }
    }

    void OnDestroy()
    {
        if (IQManager.Instance != null) IQManager.Instance.OnChanged -= Refresh;
    }

    void Refresh(int iq)
    {
        if (label != null) label.text = string.Format(format, iq);
    }
}
