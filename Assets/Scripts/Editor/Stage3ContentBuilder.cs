#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// 메뉴: Tools > Rock Falldown > Setup Stage 3 Content (Fast Rocks)
// Stage_3 에 빨간 '빠른 바위' 프리팹 + 스포너(일반 + 빠른 혼합)를 붙인다.
public static class Stage3ContentBuilder
{
    [MenuItem("Tools/Rock Falldown/Setup Stage 3 Content (Fast Rocks)")]
    public static void Setup()
    {
        var s3 = GameObject.Find("Stage_3");
        if (s3 == null)
        {
            Debug.LogWarning("[Stage3] Stage_3가 없습니다. 먼저 'Create Next Stage (map)'로 생성하세요.");
            return;
        }

        var fast = CreateFastRockPrefab();
        var normal = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Rock.prefab");
        EnsureStage3Spawner(s3, normal, fast);

        AssetDatabase.SaveAssets();
        Debug.Log("[Stage3] 빠른 바위 콘텐츠 적용 완료.");
    }

    static GameObject CreateFastRockPrefab()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Rock_Fast";
        go.tag = "Rock";
        go.transform.localScale = Vector3.one * 3.5f;   // 작고 빠르게

        var rb = go.AddComponent<Rigidbody>();
        rb.mass = 1.5f;
        go.AddComponent<RockDestroyer>();
        go.AddComponent<Rock>().knockbackForce = 30f;
        go.AddComponent<FastRock>();
        go.GetComponent<MeshRenderer>().sharedMaterial = FastMat();

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/Rock_Fast.prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }

    // 빨간 경고색 바위 머티리얼 (바위 텍스처 + 빨강 틴트, 무광)
    static Material FastMat()
    {
        const string path = "Assets/Materials/Rock_Fast.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, path);
        }
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Rocks/rock.png");
        if (tex != null)
        {
            if (mat.HasProperty("_BaseMap"))
            {
                mat.SetTexture("_BaseMap", tex);
                mat.SetTextureScale("_BaseMap", new Vector2(2f, 2f));
            }
            mat.mainTexture = tex;
        }
        mat.color = new Color(0.9f, 0.25f, 0.2f);   // 빨강 경고색
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.05f);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static void EnsureStage3Spawner(GameObject s3, GameObject normal, GameObject fast)
    {
        var prefabs = (normal != null) ? new[] { normal, fast } : new[] { fast };

        // 이미 있으면 prefabs만 갱신
        foreach (var sp in Object.FindObjectsByType<RockSpawner>(FindObjectsSortMode.None))
        {
            if (sp.transform.IsChildOf(s3.transform))
            {
                sp.rockPrefabs = prefabs;
                EditorUtility.SetDirty(sp);
                return;
            }
        }

        // 원본 스포너(Stage_1) 복제해서 Stage_3에 배치
        var s1 = GameObject.Find("Stage_1");
        var src = s1 != null ? s1.GetComponentInChildren<RockSpawner>() : null;
        if (src == null) { Debug.LogWarning("[Stage3] 원본 스포너를 못 찾음 (Stage_1의 spawner 필요)"); return; }

        var dup = Object.Instantiate(src.gameObject);
        dup.name = "spawner_Stage3";
        dup.transform.SetParent(s3.transform, false);
        dup.transform.localPosition = src.transform.localPosition;
        dup.transform.localRotation = src.transform.localRotation;
        dup.transform.localScale = src.transform.localScale;
        dup.GetComponent<RockSpawner>().rockPrefabs = prefabs;
        Undo.RegisterCreatedObjectUndo(dup, "Create Stage3 Spawner");
    }
}
#endif
