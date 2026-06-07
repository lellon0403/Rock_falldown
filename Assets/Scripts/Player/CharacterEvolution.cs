using UnityEngine;

// IQ 구간에 따라 캐릭터 비주얼 프리팹을 교체한다. (원시인 → 학생 → 교수 → 천재)
// 물리/이동은 루트에 그대로 두고, visualAnchor 아래의 비주얼만 갈아끼운다.
public class CharacterEvolution : MonoBehaviour
{
    [System.Serializable]
    public class EvolutionTier
    {
        public string name;
        public int minIQ;
        public GameObject prefab;
    }

    [Tooltip("minIQ 오름차순으로 정렬해 둘 것")]
    public EvolutionTier[] tiers;
    [Tooltip("비주얼이 붙는 위치 (비우면 이 오브젝트 기준)")]
    public Transform visualAnchor;

    int currentTier = -1;
    GameObject currentVisual;

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

        if (t == currentTier) return;
        currentTier = t;

        if (currentVisual != null) Destroy(currentVisual);

        var prefab = tiers[t].prefab;
        if (prefab == null) return;

        Transform anchor = visualAnchor != null ? visualAnchor : transform;
        currentVisual = Instantiate(prefab, anchor.position, anchor.rotation, anchor);
        currentVisual.transform.localPosition = Vector3.zero;
        currentVisual.transform.localRotation = Quaternion.identity;
    }
}
