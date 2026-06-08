#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

// 메뉴: Tools > Rock Falldown
//   - Setup Stage 5 Content (Holes + Mixed Rocks) : 바닥 구멍(즉사) + 모든 바위 혼합 스포너
//   - Setup Stage 5 Finish (Trophy + Timer)       : 평지 결승로 + 완주 레일 + 트로피 + 타이머/완주 UI
public static class Stage5ContentBuilder
{
    const float MapWidth = 60f;

    // ───────────────────────── 구멍 + 혼합 스포너 ─────────────────────────
    [MenuItem("Tools/Rock Falldown/Setup Stage 5 Content (Holes + Mixed Rocks)")]
    public static void SetupContent()
    {
        var s5 = GameObject.Find("Stage_5");
        if (s5 == null) { Debug.LogWarning("[Stage5] Stage_5가 없습니다. 먼저 'Create Next Stage (map)'."); return; }
        if (!GetAxes(out Vector3 along, out Vector3 across, out Vector3 normal, out float halfLen)) return;

        BuildHoles(s5, along, across, normal, halfLen);
        EnsureMixedSpawner(s5);

        AssetDatabase.SaveAssets();
        Debug.Log("[Stage5] 구멍 + 혼합 바위 콘텐츠 적용 완료.");
    }

    static void BuildHoles(GameObject s5, Vector3 along, Vector3 across, Vector3 normal, float halfLen)
    {
        var old = s5.transform.Find("Holes_Stage5");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        var root = new GameObject("Holes_Stage5");
        root.transform.SetParent(s5.transform, true);

        var mat = MakeMat("Assets/Materials/Hole.mat", Color.black, 0.1f, 0f);

        // (경사 따라 비율, 가로 비율) — 결정적으로 흩뿌림
        float[,] spots = {
            {-0.6f,-0.55f},{-0.5f, 0.45f},{-0.3f,-0.8f},{-0.1f, 0.7f},
            { 0.1f,-0.3f },{ 0.3f, 0.65f},{ 0.45f,-0.6f},{ 0.6f, 0.25f},
        };
        const float holeDia = 6f;
        float halfW = MapWidth * 0.5f - holeDia * 0.5f - 1f;

        Quaternion rot = Quaternion.LookRotation(along, normal);

        for (int i = 0; i < spots.GetLength(0); i++)
        {
            Vector3 pos = s5.transform.position
                        + along * (spots[i, 0] * halfLen)
                        + across * (spots[i, 1] * halfW)
                        + normal * 0.1f;

            var hole = new GameObject("Hole_" + i);
            hole.transform.SetParent(root.transform, true);
            hole.transform.SetPositionAndRotation(pos, rot);

            var box = hole.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(holeDia, 3f, holeDia);   // 표면 위로 충분히 높게
            box.center = new Vector3(0f, 1f, 0f);
            hole.AddComponent<KillZone>();

            // 검은 원반(시각용) — 콜라이더 제거
            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = "Disc";
            Object.DestroyImmediate(disc.GetComponent<Collider>());
            disc.transform.SetParent(hole.transform, false);
            disc.transform.localPosition = new Vector3(0f, -0.08f, 0f);
            disc.transform.localScale = new Vector3(holeDia, 0.05f, holeDia);
            disc.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        Undo.RegisterCreatedObjectUndo(root, "Create Stage5 Holes");
    }

    static void EnsureMixedSpawner(GameObject s5)
    {
        string[] paths = {
            "Assets/Prefabs/Rock.prefab", "Assets/Prefabs/Rock_Fast.prefab",
            "Assets/Prefabs/Rock_Splitting.prefab", "Assets/Prefabs/Rock_Phasing.prefab",
        };
        var list = new System.Collections.Generic.List<GameObject>();
        foreach (var p in paths)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            if (go != null) list.Add(go);
        }
        var prefabs = list.ToArray();

        foreach (var sp in Object.FindObjectsByType<RockSpawner>(FindObjectsSortMode.None))
        {
            if (sp.transform.IsChildOf(s5.transform))
            {
                sp.rockPrefabs = prefabs;
                EditorUtility.SetDirty(sp);
                return;
            }
        }

        var s1 = GameObject.Find("Stage_1");
        var src = s1 != null ? s1.GetComponentInChildren<RockSpawner>() : null;
        if (src == null) { Debug.LogWarning("[Stage5] 원본 스포너를 못 찾음 (Stage_1)"); return; }

        var dup = Object.Instantiate(src.gameObject);
        dup.name = "spawner_Stage5";
        dup.transform.SetParent(s5.transform, false);
        dup.transform.localPosition = src.transform.localPosition;
        dup.transform.localRotation = src.transform.localRotation;
        dup.transform.localScale = src.transform.localScale;
        dup.GetComponent<RockSpawner>().rockPrefabs = prefabs;
        Undo.RegisterCreatedObjectUndo(dup, "Create Stage5 Spawner");
    }

    // ───────────────────────── 결승로 + 트로피 + 타이머 ─────────────────────────
    [MenuItem("Tools/Rock Falldown/Setup Stage 5 Finish (Trophy + Timer)")]
    public static void SetupFinish()
    {
        var s5 = GameObject.Find("Stage_5");
        if (s5 == null) { Debug.LogWarning("[Stage5] Stage_5가 없습니다."); return; }
        if (!GetAxes(out Vector3 along, out Vector3 across, out _, out float halfLen)) return;

        Vector3 topEnd = s5.transform.position + along * halfLen;
        Vector3 flatDir = new Vector3(along.x, 0f, along.z).normalized;

        var old = GameObject.Find("FinishArea");
        if (old != null) Object.DestroyImmediate(old);

        var area = new GameObject("FinishArea");
        Undo.RegisterCreatedObjectUndo(area, "Create FinishArea");

        const float pathLen = 50f;
        Quaternion flatRot = Quaternion.LookRotation(flatDir, Vector3.up);

        // 평지 결승로 (수평)
        var path = GameObject.CreatePrimitive(PrimitiveType.Plane);
        path.name = "FinishPath";
        path.transform.SetParent(area.transform, true);
        Vector3 pathCenter = topEnd + flatDir * (pathLen * 0.5f);
        pathCenter.y = topEnd.y;
        path.transform.SetPositionAndRotation(pathCenter, flatRot);
        path.transform.localScale = new Vector3(MapWidth / 10f, 1f, pathLen / 10f);
        var pathMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Stage_Space.mat");
        if (pathMat != null) path.GetComponent<MeshRenderer>().sharedMaterial = pathMat;

        // 완주 레일 (가로로 깔린 결승선) — 시작 지점
        Vector3 lineCenter = topEnd + flatDir * 3f; lineCenter.y = topEnd.y + 0.2f;
        var redMat = MakeMat("Assets/Materials/Finish_Red.mat", new Color(0.9f, 0.1f, 0.1f), 0.2f, 0f);
        var bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bar.name = "FinishRail";
        bar.transform.SetParent(area.transform, true);
        bar.transform.SetPositionAndRotation(lineCenter, flatRot);
        bar.transform.localScale = new Vector3(MapWidth, 0.4f, 0.6f);
        bar.GetComponent<MeshRenderer>().sharedMaterial = redMat;
        for (int s = -1; s <= 1; s += 2)
        {
            var post = GameObject.CreatePrimitive(PrimitiveType.Cube);
            post.name = "Post";
            post.transform.SetParent(area.transform, true);
            post.transform.SetPositionAndRotation(
                lineCenter + across * (s * MapWidth * 0.5f) + Vector3.up * 1.2f, flatRot);
            post.transform.localScale = new Vector3(0.6f, 3f, 0.6f);
            post.GetComponent<MeshRenderer>().sharedMaterial = redMat;
        }

        // 트로피 (결승로 끝)
        Vector3 trophyPos = topEnd + flatDir * (pathLen - 5f); trophyPos.y = topEnd.y;
        var trophy = BuildTrophy(area.transform, trophyPos, flatRot);

        // 타이머 + 완주 UI 연결
        EnsureTimer();
        var finish = EnsureFinishUI();
        var ft = trophy.AddComponent<FinishTrophy>();
        ft.finishScreen = finish;

        Debug.Log("[Stage5] 결승로 + 트로피 + 타이머/완주 UI 적용 완료.");
    }

    static GameObject BuildTrophy(Transform parent, Vector3 pos, Quaternion rot)
    {
        var gold = MakeMat("Assets/Materials/Trophy_Gold.mat", new Color(1f, 0.84f, 0.1f), 0.85f, 1f);

        var root = new GameObject("Trophy");
        root.transform.SetParent(parent, true);
        root.transform.SetPositionAndRotation(pos, rot);

        var baseCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseCube.name = "Base"; baseCube.transform.SetParent(root.transform, false);
        baseCube.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        baseCube.transform.localScale = new Vector3(2.4f, 0.8f, 2.4f);
        StripCollider(baseCube, gold);

        var stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stem.name = "Stem"; stem.transform.SetParent(root.transform, false);
        stem.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        stem.transform.localScale = new Vector3(0.4f, 0.8f, 0.4f);
        StripCollider(stem, gold);

        var cup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cup.name = "Cup"; cup.transform.SetParent(root.transform, false);
        cup.transform.localPosition = new Vector3(0f, 2.8f, 0f);
        cup.transform.localScale = new Vector3(2.2f, 2.2f, 2.2f);
        StripCollider(cup, gold);

        // 닿음 판정용 트리거
        var box = root.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.center = new Vector3(0f, 2f, 0f);
        box.size = new Vector3(4f, 4f, 4f);

        return root;
    }

    static void StripCollider(GameObject go, Material mat)
    {
        var col = go.GetComponent<Collider>();
        if (col != null) Object.DestroyImmediate(col);
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    static void EnsureTimer()
    {
        if (Object.FindFirstObjectByType<GameTimer>() != null) return;
        var go = new GameObject("GameTimer");
        go.AddComponent<GameTimer>();
        Undo.RegisterCreatedObjectUndo(go, "Create GameTimer");
    }

    static FinishScreen EnsureFinishUI()
    {
        var existing = Object.FindFirstObjectByType<FinishScreen>();
        if (existing != null) return existing;

        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            var cgo = new GameObject("HUD_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = cgo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var sc = cgo.GetComponent<CanvasScaler>();
            sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1920, 1080);
        }

        // 진행 중 경과 시간 (우상단)
        if (Object.FindFirstObjectByType<GameTimerDisplay>() == null)
        {
            var tGO = new GameObject("Timer_Text", typeof(TextMeshProUGUI));
            tGO.transform.SetParent(canvas.transform, false);
            var trt = tGO.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(1f, 1f); trt.anchorMax = new Vector2(1f, 1f);
            trt.pivot = new Vector2(1f, 1f);
            trt.anchoredPosition = new Vector2(-24f, -24f);
            trt.sizeDelta = new Vector2(300f, 80f);
            var ttmp = tGO.GetComponent<TextMeshProUGUI>();
            ttmp.text = "00:00.0"; ttmp.fontSize = 44f; ttmp.color = Color.white;
            ttmp.alignment = TextAlignmentOptions.Right; ttmp.fontStyle = FontStyles.Bold;
            tGO.AddComponent<GameTimerDisplay>().label = ttmp;
        }

        // 완주 패널 (전체 화면, 처음엔 숨김)
        var panel = new GameObject("FinishPanel", typeof(Image));
        panel.transform.SetParent(canvas.transform, false);
        var prt = panel.GetComponent<RectTransform>();
        prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
        prt.offsetMin = Vector2.zero; prt.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);

        var resGO = new GameObject("Result_Text", typeof(TextMeshProUGUI));
        resGO.transform.SetParent(panel.transform, false);
        var rrt = resGO.GetComponent<RectTransform>();
        rrt.anchorMin = new Vector2(0.5f, 0.5f); rrt.anchorMax = new Vector2(0.5f, 0.5f);
        rrt.pivot = new Vector2(0.5f, 0.5f);
        rrt.anchoredPosition = Vector2.zero; rrt.sizeDelta = new Vector2(1200f, 400f);
        var rtmp = resGO.GetComponent<TextMeshProUGUI>();
        rtmp.text = "완주!"; rtmp.fontSize = 96f; rtmp.color = Color.white;
        rtmp.alignment = TextAlignmentOptions.Center; rtmp.fontStyle = FontStyles.Bold;

        var fs = canvas.gameObject.AddComponent<FinishScreen>();
        fs.panel = panel; fs.resultLabel = rtmp;
        panel.SetActive(false);

        EditorUtility.SetDirty(canvas);
        return fs;
    }

    // ───────────────────────── 공통 ─────────────────────────
    static bool GetAxes(out Vector3 along, out Vector3 across, out Vector3 normal, out float halfLen)
    {
        along = across = normal = Vector3.zero; halfLen = 0f;
        var s1 = GameObject.Find("Stage_1");
        var s2 = GameObject.Find("Stage_2");
        if (s1 == null || s2 == null) { Debug.LogWarning("[Stage5] Stage_1/Stage_2 필요"); return false; }

        along = (s2.transform.position - s1.transform.position).normalized;   // 경사 위 방향
        across = Vector3.Cross(Vector3.up, along).normalized;                 // 가로(수평)
        normal = Vector3.Cross(along, across).normalized;                     // 표면 법선
        halfLen = Vector3.Distance(s1.transform.position, s2.transform.position) * 0.5f;
        return true;
    }

    static Material MakeMat(string path, Color color, float smoothness, float metallic)
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
        EditorUtility.SetDirty(mat);
        return mat;
    }
}
#endif
