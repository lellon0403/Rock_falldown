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

        GenerateHoledFloor(s5);
        EnsureKillSlab(s5);
        EnsureMixedSpawner(s5);

        AssetDatabase.SaveAssets();
        Debug.Log("[Stage5] 실제 구멍 + 낙사 판정 + 혼합 바위 콘텐츠 적용 완료.");
    }

    // (경사 따라 비율, 가로 비율, 월드 반지름) — 구멍 중심을 평면 로컬에 결정적으로 배치
    static readonly float[,] HoleSpots = {
        { 0.0f, 0.0f, 9.0f },   // 가운데 큰 구멍 (피해 가기 어려움)
        {-0.6f,-0.55f, 3f},{-0.4f, 0.6f, 3f},
        { 0.45f,-0.65f, 3f},{ 0.6f, 0.4f, 3f},
    };

    // 평면 메시를 격자로 다시 만들면서 구멍 위치의 칸을 빼버린다 → 진짜로 뚫린 바닥.
    // MeshFilter + MeshCollider 둘 다 교체해서 보이는 구멍이자 빠지는 구멍이 된다.
    static void GenerateHoledFloor(GameObject s5)
    {
        // 예전 디스크 방식(Holes_Stage5)이 남아 있으면 제거 — 진짜 뚫린 메시와 겹침 방지.
        var legacy = s5.transform.Find("Holes_Stage5");
        if (legacy != null) Object.DestroyImmediate(legacy.gameObject);

        var mf = s5.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
        {
            Debug.LogWarning("[Stage5] Stage_5에 평면 MeshFilter가 없습니다."); return;
        }

        Bounds b = mf.sharedMesh.bounds;                 // 로컬 평면 범위(보통 10×10)
        float minX = b.center.x - b.size.x * 0.5f, maxX = b.center.x + b.size.x * 0.5f;
        float minZ = b.center.z - b.size.z * 0.5f, maxZ = b.center.z + b.size.z * 0.5f;
        float y = b.center.y;
        float halfX = b.size.x * 0.5f, halfZ = b.size.z * 0.5f;

        // 월드에서 둥근 구멍이 되도록 로컬 거리에 스케일 반영
        Vector3 ls = s5.transform.lossyScale;
        float sx = Mathf.Abs(ls.x) < 1e-4f ? 1f : ls.x;
        float sz = Mathf.Abs(ls.z) < 1e-4f ? 1f : ls.z;

        // 구멍 중심(로컬): along→Z, across→X 로 매핑, 반지름도 함께
        int hn = HoleSpots.GetLength(0);
        var hx = new float[hn]; var hz = new float[hn]; var hr = new float[hn];
        for (int i = 0; i < hn; i++)
        {
            hx[i] = HoleSpots[i, 1] * halfX * 0.9f;
            hz[i] = HoleSpots[i, 0] * halfZ * 0.9f;
            hr[i] = HoleSpots[i, 2];
        }

        // 격자 해상도 (칸 크기 ~ 로컬 0.2)
        int cx = Mathf.Clamp(Mathf.CeilToInt(b.size.x / 0.2f), 8, 200);
        int cz = Mathf.Clamp(Mathf.CeilToInt(b.size.z / 0.2f), 8, 200);

        var verts = new System.Collections.Generic.List<Vector3>();
        var uvs = new System.Collections.Generic.List<Vector2>();
        var tris = new System.Collections.Generic.List<int>();

        for (int iz = 0; iz < cz; iz++)
        {
            for (int ix = 0; ix < cx; ix++)
            {
                float x0 = Mathf.Lerp(minX, maxX, ix / (float)cx);
                float x1 = Mathf.Lerp(minX, maxX, (ix + 1) / (float)cx);
                float z0 = Mathf.Lerp(minZ, maxZ, iz / (float)cz);
                float z1 = Mathf.Lerp(minZ, maxZ, (iz + 1) / (float)cz);

                float mxp = (x0 + x1) * 0.5f, mzp = (z0 + z1) * 0.5f;
                if (InAnyHole(mxp, mzp, hx, hz, hr, sx, sz)) continue;   // 구멍 칸은 건너뜀

                int baseIdx = verts.Count;
                AddVert(verts, uvs, x0, y, z0, minX, maxX, minZ, maxZ);
                AddVert(verts, uvs, x1, y, z0, minX, maxX, minZ, maxZ);
                AddVert(verts, uvs, x1, y, z1, minX, maxX, minZ, maxZ);
                AddVert(verts, uvs, x0, y, z1, minX, maxX, minZ, maxZ);
                // 윗면이 +Y(로컬 up)를 보도록 (기본 Plane과 동일)
                tris.Add(baseIdx); tris.Add(baseIdx + 2); tris.Add(baseIdx + 1);
                tris.Add(baseIdx); tris.Add(baseIdx + 3); tris.Add(baseIdx + 2);
            }
        }

        var mesh = new Mesh { name = "Stage5_Floor" };
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        const string meshPath = "Assets/Meshes/Stage5_Floor.mesh";
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(
            System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), meshPath)));
        var existing = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        if (existing != null) { EditorUtility.CopySerialized(mesh, existing); mesh = existing; }
        else AssetDatabase.CreateAsset(mesh, meshPath);

        mf.sharedMesh = mesh;
        var mc = s5.GetComponent<MeshCollider>();
        if (mc == null) mc = s5.AddComponent<MeshCollider>();
        mc.sharedMesh = mesh;
        EditorUtility.SetDirty(mf);
        EditorUtility.SetDirty(mc);
    }

    static void AddVert(System.Collections.Generic.List<Vector3> v,
                        System.Collections.Generic.List<Vector2> uv,
                        float x, float y, float z,
                        float minX, float maxX, float minZ, float maxZ)
    {
        v.Add(new Vector3(x, y, z));
        uv.Add(new Vector2(Mathf.InverseLerp(minX, maxX, x), Mathf.InverseLerp(minZ, maxZ, z)));
    }

    static bool InAnyHole(float x, float z, float[] hx, float[] hz, float[] hr, float sx, float sz)
    {
        for (int i = 0; i < hx.Length; i++)
        {
            float wdx = (x - hx[i]) * sx;
            float wdz = (z - hz[i]) * sz;
            if (wdx * wdx + wdz * wdz < hr[i] * hr[i]) return true;
        }
        return false;
    }

    // 표면 아래 평행하게 깔린 낙사 트리거. 구멍으로 빠지면 곧장 즉사(처음부터).
    static void EnsureKillSlab(GameObject s5)
    {
        var old = s5.transform.Find("KillSlab_Stage5");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        var slab = new GameObject("KillSlab_Stage5");
        slab.transform.SetParent(s5.transform, false);
        slab.transform.localPosition = new Vector3(0f, -6f, 0f);   // 표면 아래
        slab.transform.localRotation = Quaternion.identity;

        var box = slab.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(14f, 8f, 14f);   // 평면(로컬 ~10)보다 넉넉히
        slab.AddComponent<KillZone>();

        Undo.RegisterCreatedObjectUndo(slab, "Create Stage5 KillSlab");
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
        var pathMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Stage_Finish.mat")
                   ?? AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Stage_Space.mat");
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
