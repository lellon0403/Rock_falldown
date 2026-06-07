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
        go.AddComponent<Rock>().knockbackForce = 1.5f;   // 조각: 아주 약하게 (여러 개라)
        go.GetComponent<MeshRenderer>().sharedMaterial = SnowMat();   // 흰 눈덩이

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/Rock_Fragment.prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject CreateSplittingRockPrefab(GameObject fragment)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Rock_Splitting";
        go.tag = "Rock";
        go.transform.localScale = Vector3.one * 9f;   // 거대 눈덩이

        var rb = go.AddComponent<Rigidbody>();
        rb.mass = 2f;
        go.AddComponent<RockDestroyer>();
        go.AddComponent<Rock>().knockbackForce = 2.5f;   // 본체: 약하게

        var sr = go.AddComponent<SplittingRock>();
        sr.fragmentPrefab = fragment;
        go.GetComponent<MeshRenderer>().sharedMaterial = SnowMat();   // 흰 눈덩이

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

    // 거대 눈덩이용 흰 눈 머티리얼 (snow.png 있으면 텍스처도 입힘)
    static Material SnowMat()
    {
        const string path = "Assets/Materials/Rock_Snow.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = Color.white };
            AssetDatabase.CreateAsset(mat, path);
        }
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Rocks/snow.png");
        if (tex != null)
        {
            if (mat.HasProperty("_BaseMap"))
            {
                mat.SetTexture("_BaseMap", tex);
                mat.SetTextureScale("_BaseMap", new Vector2(2f, 2f));
            }
            mat.mainTexture = tex;
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.1f);   // 눈은 살짝만
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);
            EditorUtility.SetDirty(mat);
        }
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
        sp.count = 6;                              // 겹침/정체 완화
        sp.heightScale = new Vector2(2f, 3.5f);    // 기둥 낮게 (문제 가림 방지)
        sp.sink = 1.5f;
        EditorUtility.SetDirty(sp);
    }

    // Stage_2 스포너가 일반(강한) 돌 + 분열 바위를 섞어 스폰하게 설정
    static void AssignSplittingRockToStage2(GameObject s2, GameObject splitting)
    {
        var normal = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Rock.prefab");

        bool assigned = false;
        foreach (var sp in Object.FindObjectsByType<RockSpawner>(FindObjectsSortMode.None))
        {
            if (sp.transform.IsChildOf(s2.transform))
            {
                sp.rockPrefabs = (normal != null)
                    ? new[] { normal, splitting }   // 강한 일반 돌 + 분열 바위 혼합
                    : new[] { splitting };
                EditorUtility.SetDirty(sp);
                assigned = true;
            }
        }
        if (!assigned)
            Debug.LogWarning("[Stage2ContentBuilder] Stage_2 자식 스포너를 못 찾음. 수동으로 Rock Prefabs에 Rock + Rock_Splitting을 넣으세요.");
    }
}
#endif
