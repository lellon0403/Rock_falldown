using UnityEngine;
using System;

// 전역 IQ 값을 관리한다. 정답/이벤트로만 증가 (높이·시간 비례 없음).
public class IQManager : MonoBehaviour
{
    public static IQManager Instance { get; private set; }

    [SerializeField] int startIQ = 60;   // 기본 IQ 60, 게이트 정답마다 +30
    [SerializeField] int maxIQ = 150;    // 천재(최고 티어) 상한. 게이트가 많아도 이 값을 넘지 않음

    public int CurrentIQ { get; private set; }
    public event Action<int> OnChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        CurrentIQ = startIQ;
    }

    void Start()
    {
        OnChanged?.Invoke(CurrentIQ);   // 초기값 UI/진화에 반영
    }

    public void Add(int amount)
    {
        CurrentIQ = Mathf.Min(CurrentIQ + amount, maxIQ);
        OnChanged?.Invoke(CurrentIQ);
    }

    public void ResetIQ()
    {
        CurrentIQ = startIQ;
        OnChanged?.Invoke(CurrentIQ);
    }
}
