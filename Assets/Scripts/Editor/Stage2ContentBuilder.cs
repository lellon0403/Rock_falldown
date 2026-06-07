#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// 메뉴: Tools > Rock Falldown > Setup Stage 2 Content
// 장애물 프리팹 + 랜덤 장애물 스포너 + 분열 바위/조각 프리팹을 만들고 Stage 2에 연결한다.
public static class Stage2ContentBuilder
{
    [MenuItem("Tools/Rock Falldown/Setup Stage 2 Content (Obstacles + Splitting Rock)")]
    public static void Setup()
    {
        var fragment = CreateFragmentPrefab();
        var splitting = CreateSplittingRockPrefab(fragment);
        var obstacle = CreateObstaclePrefab();

        var s2 = GameObject.Find("Stage_2");
        if (s2 == null)
        {
            Debug.LogWarning("[Stage2ContentBuilder] Stage_2를 찾지 못했습니다. 장애물 스포너/분열바위 연결을 건너뜀.");
            return;
        }

        AddObstacleSpawner(s2, obstacle);
        AssignSplittingRockToStage2(s2, splitting);

        AssetDatabase.SaveAssets();
        Debug.Log("[Stage2ContentBuilder] 완료: 장애물 랜덤 스포너 + 분열 바위(약한 밀치기)를 Stage 2에 적용했습니다.");
    }

    static GameObject CreateFragmentPrefab()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Rock_Fragment";
        go.tag = "Rock";
        go.transform.localScale = Vector3.one * 2f;

        var rb = go.AddComponent<Rigidbody>();
        rb.mass = 0.5f;
        go.AddComponent<RockDestroyer>();
        go.AddComponent<Rock>().knockbackForce = 1.5f;   // 조각: 아주 약하게 (질질 끌림)
        go.GetComponent<MeshRenderer>().sharedMaterial = BrownMat();   // 갈색

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/Rock_Fragment.prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject CreateSplittingRockPrefab(GameObject fragment)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Rock_Splitting";
        go.tag = "Rock";
        go.transform.localScale = Vector3.one * 5f;

        var rb = go.AddComponent<Rigidbody>();
        rb.mass = 2f;
        go.AddComponent<RockDestroyer>();
        go.AddComponent<Rock>().knockbackForce = 2.5f;   // 본체도 약하게

        var sr = go.AddComponent<SplittingRock>();
        sr.fragmentPrefab = fragment;
        go.GetComponent<MeshRenderer>().sharedMaterial = BrownMat();   // 갈색

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/Rock_Splitting.prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject CreateObstaclePrefab()
    {
        // 원기둥 기둥 (높이는 ObstacleSpawner가 배치 시 키움)
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "Obstacle";
        go.transform.localScale = new Vector3(3f, 5f, 3f);
        go.AddComponent<Obstacle>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/Obstacle.prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }

    static Material BrownMat()
    {
        const string path = "Assets/Materials/Rock_Brown.mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"))
        {
            color = new Color(0.45f, 0.30f, 0.16f)   // 갈색
        };
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    static void AddObstacleSpawner(GameObject s2, GameObject obstaclePrefab)
    {
        var rend = s2.GetComponent<Renderer>();

        ObstacleSpawner sp = null;
        foreach (var e in Object.FindObjectsByType<ObstacleSpawner>(FindObjectsSortMode.None))
            if (e.stageArea == rend) { sp = e; break; }

        if (sp == null)
        {
            var go = new GameObject("ObstacleSpawner_Stage2");
            go.transform.SetParent(s2.transform, false);
            sp = go.AddComponent<ObstacleSpawner>();
            Undo.RegisterCreatedObjectUndo(go, "Create ObstacleSpawner");
        }
        sp.obstaclePrefab = obstaclePrefab;
        sp.stageArea = rend;
        sp.count = 8;
        EditorUtility.SetDirty(sp);
    }

    // Stage_2의 자식 스포너(= Stage 2 스포너)의 Rock Prefab을 분열 바위로 교체
    static void AssignSplittingRockToStage2(GameObject s2, GameObject splitting)
    {
        bool assigned = false;
        foreach (var sp in Object.FindObjectsByType<RockSpawner>(FindObjectsSortMode.None))
        {
            if (sp.transform.IsChildOf(s2.transform))
            {
                sp.rockPrefab = splitting;
                EditorUtility.SetDirty(sp);
                assigned = true;
            }
        }
        if (!assigned)
            Debug.LogWarning("[Stage2ContentBuilder] Stage_2 자식 스포너를 못 찾음. Stage 2 스포너의 Rock Prefab을 수동으로 Rock_Splitting으로 바꾸세요.");
    }
}
#endif
