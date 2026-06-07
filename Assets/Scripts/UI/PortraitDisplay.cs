using UnityEngine;
using UnityEngine.UI;

// 좌상단 HUD 초상. IQ 구간(원시인/학생/교수/천재)에 따라 이미지를 바꾸고,
// 같은 구간 안에서는 2장(frames)을 번갈아 보여 '움직이는' 느낌을 준다.
public class PortraitDisplay : MonoBehaviour
{
    [System.Serializable]
    public class Tier
    {
        public string name;
        public int minIQ;
        public Sprite[] frames;   // 2장 권장 (움직이는 부분만 다르게)
    }

    public Image image;
    [Tooltip("minIQ 오름차순")]
    public Tier[] tiers;
    [Tooltip("프레임 전환 간격(초)")]
    public float frameInterval = 0.4f;

    int curTier = -1;
    int frameIdx;
    float timer;

    void Start()
    {
        if (IQManager.Instance != null)
        {
            IQManager.Instance.OnChanged += OnIQChanged;
            OnIQChanged(IQManager.Instance.CurrentIQ);
        }
    }

    void OnDestroy()
    {
        if (IQManager.Instance != null) IQManager.Instance.OnChanged -= OnIQChanged;
    }

    void OnIQChanged(int iq)
    {
        if (tiers == null || tiers.Length == 0) return;

        int t = 0;
        for (int i = 0; i < tiers.Length; i++)
            if (iq >= tiers[i].minIQ) t = i;

        if (t == curTier) return;
        curTier = t;
        frameIdx = 0;
        timer = 0f;
        ShowFrame();
    }

    void Update()
    {
        if (curTier < 0) return;
        var frames = tiers[curTier].frames;
        if (frames == null || frames.Length < 2) return;

        timer += Time.deltaTime;
        if (timer >= frameInterval)
        {
            timer = 0f;
            frameIdx = (frameIdx + 1) % frames.Length;
            ShowFrame();
        }
    }

    void ShowFrame()
    {
        if (image == null || curTier < 0) return;
        var frames = tiers[curTier].frames;
        if (frames != null && frames.Length > 0 && frames[frameIdx] != null)
            image.sprite = frames[frameIdx];
    }
}
