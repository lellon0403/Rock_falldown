#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// 메뉴: Tools > Rock Falldown > Setup Stage 4 Content (Phasing Rocks)
// Stage_4 에 '투명 바위' 프리팹 + 스포너(일반 + 투명 혼합)를 붙인다.
// 투명 바위는 물리는 일반 바위와 동일하고, 보였다/사라졌다를 반복한다.
public static class Stage4ContentBuilder
{
    [MenuItem("Tools/Rock Falldown/Setup Stage 4 Content (Phasing Rocks)")]
    public static void Setup()
    {
        var s4 = GameObject.Find("Stage_4");
        if (s4 == null)
        {
            Debug.LogWarning("[Stage4] Stage_4가 없습니다. 먼저 'Create Next Stage (map)'로 생성하세요.");
            return;
        }

        var phasing = CreatePhasingRockPrefab();
        EnsureStage4Spawner(s4, phasing);

        AssetDatabase.SaveAssets();
        Debug.Log("[Stage4] 투명 바위 콘텐츠 적용 완료.");
    }

    static GameObject CreatePhasingRockPrefab()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Rock_Phasing";
        go.tag = "Rock";
        go.transform.localScale = Vector3.one * 5f;   // 일반 바위와 동일 크기

        var rb = go.AddComponent<Rigidbody>();
        rb.mass = 50f;                                  // 일반 바위와 동일 질량
        go.AddComponent<RockDestroyer>();
        go.AddComponent<Rock>().knockbackForce = 5f;    // 일반 바위와 동일 넉백
        go.AddComponent<PhasingRock>();                 // 보임 2초 / 은신 3초 (기본값)
        go.GetComponent<MeshRenderer>().sharedMaterial = StarMat();

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/Rock_Phasing.prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }

    // 별이 빛나는 우주 바위 머티리얼 (어두운 바탕 + 별 텍스처 발광)
    static Material StarMat()
    {
        const string path = "Assets/Materials/Rock_Star.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, path);
        }
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Rocks/star.png");
        if (tex != null)
        {
            if (mat.HasProperty("_BaseMap"))
            {
                mat.SetTexture("_BaseMap", tex);
                mat.SetTextureScale("_BaseMap", new Vector2(2f, 2f));
            }
            mat.mainTexture = tex;
            // 별이 어둠 속에서 빛나도록 같은 텍스처를 발광으로
            mat.EnableKeyword("_EMISSION");
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            if (mat.HasProperty("_EmissionMap")) mat.SetTexture("_EmissionMap", tex);
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", new Color(0.7f, 0.75f, 1f) * 1.5f);
        }
        mat.color = new Color(0.05f, 0.05f, 0.12f);     // 거의 검은 우주 바탕
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.3f);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static void EnsureStage4Spawner(GameObject s4, GameObject normal, GameObject phasing)
    {
        var prefabs = (normal != null) ? new[] { normal, phasing } : new[] { phasing };

        // 이미 있으면 prefabs만 갱신
        foreach (var sp in Object.FindObjectsByType<RockSpawner>(FindObjectsSortMode.None))
        {
            if (sp.transform.IsChildOf(s4.transform))
            {
                sp.rockPrefabs = prefabs;
                EditorUtility.SetDirty(sp);
                return;
            }
        }

        // 원본 스포너(Stage_1) 복제해서 Stage_4에 배치
        var s1 = GameObject.Find("Stage_1");
        var src = s1 != null ? s1.GetComponentInChildren<RockSpawner>() : null;
        if (src == null) { Debug.LogWarning("[Stage4] 원본 스포너를 못 찾음 (Stage_1의 spawner 필요)"); return; }

        var dup = Object.Instantiate(src.gameObject);
        dup.name = "spawner_Stage4";
        dup.transform.SetParent(s4.transform, false);
        dup.transform.localPosition = src.transform.localPosition;
        dup.transform.localRotation = src.transform.localRotation;
        dup.transform.localScale = src.transform.localScale;
        dup.GetComponent<RockSpawner>().rockPrefabs = prefabs;
        Undo.RegisterCreatedObjectUndo(dup, "Create Stage4 Spawner");
    }
}
#endif
