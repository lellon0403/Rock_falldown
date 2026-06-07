using UnityEngine;

// 플레이어의 현재 위치(진행 Z)에 따라 각 스테이지의 스포너를 켜고 끈다.
// 위로 올라가면 다음 스테이지 스포너만, 아래로 내려오면 이전 스테이지 스포너만 작동.
// (스테이지 2에서 맞고 1로 내려오면 Stage 1 스포너가 다시 켜짐)
public class StageManager : MonoBehaviour
{
    Transform player;
    RockSpawner[] spawners;     // 진행 Z 오름차순
    float[] boundaries;         // MathGate들의 Z (오름차순) = 스테이지 경계
    int current = -1;

    void Awake()
    {
        // Awake는 모든 Start보다 먼저 실행 → 스포너가 스스로 시작하기 전에 막는다.
        spawners = FindObjectsByType<RockSpawner>(FindObjectsSortMode.None);
        System.Array.Sort(spawners, (a, b) => a.transform.position.z.CompareTo(b.transform.position.z));
        foreach (var s in spawners)
        {
            s.autoStart = false;     // StageManager가 시작/정지를 전담
            s.StopSpawning();
        }
    }

    void Start()
    {
        var pm = FindFirstObjectByType<PlayerMove>();
        if (pm != null) player = pm.transform;

        var gates = FindObjectsByType<MathGate>(FindObjectsSortMode.None);
        boundaries = new float[gates.Length];
        for (int i = 0; i < gates.Length; i++) boundaries[i] = gates[i].transform.position.z;
        System.Array.Sort(boundaries);

        foreach (var s in spawners) s.StopSpawning();
        Apply(true);
    }

    void Update() => Apply(false);

    void Apply(bool force)
    {
        if (player == null || spawners == null || spawners.Length == 0) return;

        // 플레이어보다 아래쪽 경계 개수 = 현재 스테이지 인덱스
        int idx = 0;
        for (int i = 0; i < boundaries.Length; i++)
            if (player.position.z >= boundaries[i]) idx = i + 1;
        idx = Mathf.Clamp(idx, 0, spawners.Length - 1);

        if (idx == current && !force) return;
        current = idx;

        for (int i = 0; i < spawners.Length; i++)
        {
            if (i == idx) spawners[i].StartSpawning();
            else spawners[i].StopSpawning();
        }
    }
}
