using UnityEngine;

// 투명 바위: 일정 주기로 모습이 보였다 사라졌다 반복한다. 은신 중에도 물리/충돌은 그대로라
// 위치를 '기억'해서 피해야 한다 (브레인 서바이벌 컨셉). 물리는 일반 바위와 동일.
public class PhasingRock : MonoBehaviour
{
    [Tooltip("보이는 시간(초)")]
    public float visibleTime = 2f;
    [Tooltip("완전히 사라지는 시간(초)")]
    public float hiddenTime = 3f;

    Renderer[] renderers;
    float timer;
    bool visible = true;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        // 바위마다 사이클 위상을 살짝 어긋나게 해서 전부 동시에 깜빡이지 않게 한다
        timer = Random.Range(0f, visibleTime + hiddenTime);
    }

    void Update()
    {
        timer += Time.deltaTime;
        float phase = visible ? visibleTime : hiddenTime;
        if (timer >= phase)
        {
            timer -= phase;
            visible = !visible;
            SetVisible(visible);
        }
    }

    void SetVisible(bool on)
    {
        foreach (var r in renderers)
            if (r != null) r.enabled = on;   // 콜라이더/물리는 건드리지 않음 — 안 보여도 위험
    }
}
