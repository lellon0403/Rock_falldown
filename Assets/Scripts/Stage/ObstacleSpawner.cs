using UnityEngine;

// 스테이지 표면 위에 장애물을 무작위로 흩뿌린다 (시작 시 1회). 매 판 배치가 달라진다.
public class ObstacleSpawner : MonoBehaviour
{
    [Tooltip("비우면 큐브를 자동 생성")]
    public GameObject obstaclePrefab;
    [Tooltip("이 Renderer의 표면 위에 배치 (보통 해당 Stage의 Plane)")]
    public Renderer stageArea;
    public int count = 8;
    [Tooltip("가장자리에서 띄울 여유")]
    public float edgeMargin = 5f;
    [Tooltip("기둥 굵기(XZ) 범위")]
    public Vector2 scaleRange = new Vector2(2f, 3.5f);
    [Tooltip("기둥 높이(Y) 범위 — 너무 크면 문제를 가리고 돌이 정체됨")]
    public Vector2 heightScale = new Vector2(2f, 3.5f);
    [Tooltip("표면 아래로 파묻을 깊이 (경사에 박힌 느낌)")]
    public float sink = 1.5f;

    void Start()
    {
        if (stageArea == null) { Debug.LogWarning("[ObstacleSpawner] stageArea가 비어 있습니다.", this); return; }

        Transform holder = new GameObject("Obstacles").transform;   // 스케일 왜곡 방지용 루트 컨테이너
        Bounds b = stageArea.bounds;

        int placed = 0, attempts = 0;
        while (placed < count && attempts < count * 6)
        {
            attempts++;
            if (TryPlace(b, holder)) placed++;
        }
    }

    bool TryPlace(Bounds b, Transform holder)
    {
        float x = Random.Range(b.min.x + edgeMargin, b.max.x - edgeMargin);
        float z = Random.Range(b.min.z + edgeMargin, b.max.z - edgeMargin);
        Vector3 from = new Vector3(x, b.max.y + 30f, z);

        if (!Physics.Raycast(from, Vector3.down, out RaycastHit hit, 300f)) return false;
        if (hit.transform != stageArea.transform) return false;   // 이 스테이지 표면에만 배치

        GameObject obs = obstaclePrefab != null
            ? Instantiate(obstaclePrefab)
            : GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        float r = Random.Range(scaleRange.x, scaleRange.y);
        float h = Random.Range(heightScale.x, heightScale.y);
        obs.transform.localScale = new Vector3(r, h, r);
        obs.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        // 높이 절반 - sink 만큼 띄워 표면 아래로 파묻히게 배치 (프리팹 종류 무관)
        var rend = obs.GetComponentInChildren<Renderer>();
        float half = rend != null ? rend.bounds.extents.y : h;
        obs.transform.position = hit.point + Vector3.up * (half - sink);
        obs.transform.SetParent(holder, true);

        if (obs.GetComponent<Obstacle>() == null) obs.AddComponent<Obstacle>();
        return true;
    }
}
