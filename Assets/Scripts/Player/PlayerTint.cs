using UnityEngine;

// IQ 구간에 따라 플레이어(3D) 메시 색을 바꾼다. 색은 각 진화 초상의 대표 색과 유사.
public class PlayerTint : MonoBehaviour
{
    [System.Serializable]
    public class Tier
    {
        public string name;
        public int minIQ;
        public Color color = Color.white;
    }

    public Renderer targetRenderer;
    [Tooltip("minIQ 오름차순")]
    public Tier[] tiers;

    Material mat;
    int curTier = -1;

    void Start()
    {
        if (targetRenderer == null) targetRenderer = GetComponentInChildren<Renderer>();
        if (targetRenderer != null) mat = targetRenderer.material;   // 인스턴스화(공유 머티리얼 오염 방지)

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
        if (tiers == null || tiers.Length == 0 || mat == null) return;

        int t = 0;
        for (int i = 0; i < tiers.Length; i++) if (iq >= tiers[i].minIQ) t = i;
        if (t == curTier) return;
        curTier = t;

        Color c = tiers[t].color;
        mat.color = c;                                           // URP Lit는 _BaseColor가 MainColor
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
    }
}
