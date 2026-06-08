using UnityEngine;

public class RockSpawner : MonoBehaviour
{
    [Header("Spawn")]
    public GameObject rockPrefab;
    [Tooltip("여러 종류를 섞어 스폰하려면 채우기 (비우면 rockPrefab 사용)")]
    public GameObject[] rockPrefabs;
    public float interval = 1.5f;
    public float startDelay = 1f;
    [Tooltip("한 번에 동시에 떨굴 바위 개수 (서로 다른 레인에 배치)")]
    public int rocksPerSpawn = 2;
    [Tooltip("StageManager 없이 단독으로 즉시 스폰 시작할지")]
    public bool autoStart = true;

    [Header("Lane Setup")]
    [Tooltip("경사로의 가로 폭(월드 X). Map의 Scale.z × 10 값 (현재 6 × 10 = 60)")]
    public float mapWidth = 60f;
    [Tooltip("벽 안쪽 여유. 양쪽에서 이만큼씩 빼고 레인을 계산")]
    public float wallMargin = 0f;
    [Tooltip("0이면 rockPrefab에서 자동 측정. 직접 지정하려면 바위 지름 입력")]
    public float rockSize = 0f;

    int laneCount;
    float laneStep;
    float firstLaneCenter;
    int[] lanes;
    bool spawning;

    float killBelowY;
    bool killYComputed;

    void Awake()
    {
        ComputeLanes();
        ComputeKillY();
    }

    // 이 스포너가 속한 스테이지의 '아래 경계 Y'를 계산.
    // 바위가 이 높이 밑으로 굴러내려가면(=스테이지를 벗어나면) 제거된다.
    void ComputeKillY()
    {
        var s1 = GameObject.Find("Stage_1");
        var s2 = GameObject.Find("Stage_2");
        Transform stage = transform.parent;   // 스포너의 소속 스테이지
        if (s1 == null || s2 == null || stage == null) return;

        float dropPerStage = Mathf.Abs(s2.transform.position.y - s1.transform.position.y);
        killBelowY = stage.position.y - dropPerStage * 0.5f;   // 스테이지 아래 경계
        killYComputed = true;
    }

    void Start()
    {
        if (autoStart) StartSpawning();
    }

    // StageManager(또는 외부)에서 호출. 현재 스테이지의 스포너만 작동시키기 위함.
    public void StartSpawning()
    {
        if (spawning) return;
        spawning = true;
        CancelInvoke(nameof(SpawnRock));
        InvokeRepeating(nameof(SpawnRock), startDelay, interval);
    }

    public void StopSpawning()
    {
        spawning = false;
        CancelInvoke(nameof(SpawnRock));
    }

    void ComputeLanes()
    {
        if (rockSize <= 0f) rockSize = MeasureRockSize();
        if (rockSize <= 0f) rockSize = 1f;   // 측정 실패 시 안전장치

        float usableWidth = mapWidth - wallMargin * 2f;
        laneCount = Mathf.Max(1, Mathf.FloorToInt(usableWidth / rockSize));

        laneStep = usableWidth / laneCount;
        firstLaneCenter = -usableWidth * 0.5f + laneStep * 0.5f;

        lanes = new int[laneCount];
        for (int i = 0; i < laneCount; i++) lanes[i] = i;
    }

    void SpawnRock()
    {
        int count = Mathf.Clamp(rocksPerSpawn, 1, laneCount);

        // 부분 Fisher-Yates로 서로 다른 레인 count개 선택
        for (int i = 0; i < count; i++)
        {
            int j = Random.Range(i, laneCount);
            (lanes[i], lanes[j]) = (lanes[j], lanes[i]);
            SpawnAtLane(lanes[i]);
        }
    }

    void SpawnAtLane(int lane)
    {
        float x = firstLaneCenter + laneStep * lane;
        Vector3 spawnPos = transform.position + new Vector3(x, 0f, 0f);

        GameObject prefab = (rockPrefabs != null && rockPrefabs.Length > 0)
            ? rockPrefabs[Random.Range(0, rockPrefabs.Length)]
            : rockPrefab;
        if (prefab == null) return;

        var rock = Instantiate(prefab, spawnPos, Quaternion.identity);
        if (killYComputed)
        {
            var rd = rock.GetComponent<RockDestroyer>();
            if (rd != null) rd.killBelowY = killBelowY;
        }
    }

    // 프리팹의 메시/콜라이더에서 바위의 가로 지름을 잰다
    float MeasureRockSize()
    {
        if (rockPrefab == null) return 0f;

        var mf = rockPrefab.GetComponentInChildren<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
            return mf.sharedMesh.bounds.size.x * rockPrefab.transform.lossyScale.x;

        var sphere = rockPrefab.GetComponentInChildren<SphereCollider>();
        if (sphere != null)
            return sphere.radius * 2f * rockPrefab.transform.lossyScale.x;

        var col = rockPrefab.GetComponentInChildren<Collider>();
        if (col != null) return col.bounds.size.x;

        return 0f;
    }
}
