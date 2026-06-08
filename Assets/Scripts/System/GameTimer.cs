using UnityEngine;

// 완주 타이머. 씬 시작과 함께 시간을 잰다. 트로피에 닿으면 멈춘다.
public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance { get; private set; }

    public float Elapsed { get; private set; }
    public bool Running { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Elapsed = 0f;
        Running = true;
    }

    void Update()
    {
        if (Running) Elapsed += Time.unscaledDeltaTime;
    }

    public void StopTimer() => Running = false;

    // mm:ss.t 형식
    public static string Format(float t)
    {
        int m = Mathf.FloorToInt(t / 60f);
        float s = t - m * 60f;
        return string.Format("{0:00}:{1:00.0}", m, s);
    }
}
