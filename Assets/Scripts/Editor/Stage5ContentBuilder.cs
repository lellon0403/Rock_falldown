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

    // ───────────────────────── 완전 새로 만들기 ─────────────────────────
    // 기존 Stage_5가 누적 편집으로 망가졌을 때, Stage_4(깨끗한 표준 Plane)를 복제해
    // 같은 위치·스케일로 Stage_5를 처음부터 다시 만든다 + 천국 머티리얼 + 구멍 + 바위.
    [MenuItem("Tools/Rock Falldown/Rebuild Stage 5 (Fresh)")]
    public static void RebuildStage5()
    {
        var s1 = GameObject.Find("Stage_1");
        var s2 = GameObject.Find("Stage_2");
        var s4 = GameObject.Find("Stage_4");
        if (s1 == null || s2 == null || s4 == null)
        {
            Debug.LogWarning("[Stage5] Stage_1/Stage_2/Stage_4가 필요합니다."); return;
        }

        // 1) 기존 Stage_5 + 깎인 메시 에셋 제거
        var old = GameObject.Find("Stage_5");
        if (old != null) Object.DestroyImmediate(old);
        AssetDatabase.DeleteAsset("Assets/Meshes/Stage5_Floor.mesh");

        // 2) Stage_4 복제 → 깨끗한 표준 Plane으로 Stage_5 생성 (같은 간격으로 위에 적층)
        Vector3 offset = s2.transform.position - s1.transform.position;
        var go = Object.Instantiate(s4);
        go.name = "Stage_5";
        go.transform.SetPositionAndRotation(s4.transform.position + offset, s4.transform.rotation);
        go.transform.localScale = s4.transform.localScale;
        for (int i = go.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(go.transform.GetChild(i).gameObject);   // 스포너 등 자식 제거

        // 3) 천국 머티리얼 (없으면 Import & Assign Ground Textures 먼저)
        var heaven = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Stage_Heaven.mat");
        var mr = go.GetComponent<MeshRenderer>();
        if (heaven != null && mr != null) mr.sharedMaterial = heaven;
        else Debug.LogWarning("[Stage5] Stage_Heaven.mat 없음 — 'Import & Assign Ground Textures' 먼저 실행하세요.");

        Undo.RegisterCreatedObjectUndo(go, "Rebuild Stage 5");

        // 4) 구멍 + 바위
        GenerateHoledFloor(go);
        EnsureMixedSpawner(go);

        AssetDatabase.SaveAssets();
        Selection.activeGameObject = go;
        Debug.Log("[Stage5] Stage_5 새로 생성 완료 (천국 바닥 + 구멍 + 기본/빠른 바위).");
    }

    // ───────────────────────── 구멍 + 혼합 스포너 ─────────────────────────
    [MenuItem("Tools/Rock Falldown/Setup Stage 5 Content (Holes + Mixed Rocks)")]
    public static void SetupContent()
    {
        var s5 = GameObject.Find("Stage_5");
        if (s5 == null) { Debug.LogWarning("[Stage5] Stage_5가 없습니다. 먼저 'Create Next Stage (map)'."); return; }

        // 예전 낙사 트리거(KillSlab)는 더 이상 쓰지 않음 — 구멍에 빠지면 그냥 떨어져 y<=0에서 재시작.
        var oldSlab = s5.transform.Find("KillSlab_Stage5");
        if (oldSlab != null) Object.DestroyImmediate(oldSlab.gameObject);

        GenerateHoledFloor(s5);
        EnsureMixedSpawner(s5);

        AssetDatabase.SaveAssets();
        Debug.Log("[Stage5] 구멍 뚫기 + 빠른 바위 콘텐츠 적용 완료 (낙사=떨어지면 재시작).");
    }

    // 정규화 좌표(평면 -1~1) 기준: { x(-1~1), y(-1~1), 반지름(half-extent 비율) }
    // 스케일/회전에 의존하지 않아 어떤 평면이든 동일 비율로 구멍이 뚫린다.
    // 작은 구멍(~0.13)과 그 1.5배(~0.20)까지, 겹치지 않게 불규칙 배치.
    static readonly float[,] HoleSpots = {
        {-0.50f,-0.72f, 0.16f},
        { 0.35f,-0.60f, 0.12f},
        {-0.05f,-0.30f, 0.20f},   // 가장 큰 구멍 (작은 것의 약 1.5배)
        { 0.60f,-0.15f, 0.13f},
        {-0.62f,-0.05f, 0.11f},
        { 0.15f, 0.25f, 0.15f},
        {-0.40f, 0.50f, 0.13f},
        { 0.55f, 0.55f, 0.17f},
        {-0.05f, 0.75f, 0.12f},
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

        // 이미 깎인 메시(Stage5_Floor)를 다시 읽으면 bounds 중심이 이동해 구멍이 밀린다.
        // 항상 기본 Plane(중심 0, 10×10) 기준으로 고정해 가운데 구멍이 진짜 가운데에 오게 한다.
        Bounds b;
        if (mf.sharedMesh.name.StartsWith("Stage5_Floor"))
        {
            var tmp = GameObject.CreatePrimitive(PrimitiveType.Plane);
            b = tmp.GetComponent<MeshFilter>().sharedMesh.bounds;
            Object.DestroyImmediate(tmp);
        }
        else b = mf.sharedMesh.bounds;                   // 로컬 평면 범위(보통 10×10)
        float minX = b.center.x - b.size.x * 0.5f, maxX = b.center.x + b.size.x * 0.5f;
        float minZ = b.center.z - b.size.z * 0.5f, maxZ = b.center.z + b.size.z * 0.5f;
        float y = b.center.y;
        float cX = b.center.x, cZ = b.center.z;
        float halfX = b.size.x * 0.5f, halfZ = b.size.z * 0.5f;

        // 격자 해상도 (칸 크기 ~ 로컬 0.2)
        int cx = Mathf.Clamp(Mathf.CeilToInt(b.size.x / 0.2f), 8, 200);
        int cz = Mathf.Clamp(Mathf.CeilToInt(b.size.z / 0.2f), 8, 200);

        var verts = new System.Collections.Generic.List<Vector3>();
        var uvs = new System.Collections.Generic.List<Vector2>();
        var tris = new System.Collections.Generic.List<int>();

        int kept = 0, removed = 0;
        for (int iz = 0; iz < cz; iz++)
        {
            for (int ix = 0; ix < cx; ix++)
            {
                float x0 = Mathf.Lerp(minX, maxX, ix / (float)cx);
                float x1 = Mathf.Lerp(minX, maxX, (ix + 1) / (float)cx);
                float z0 = Mathf.Lerp(minZ, maxZ, iz / (float)cz);
                float z1 = Mathf.Lerp(minZ, maxZ, (iz + 1) / (float)cz);

                float mxp = (x0 + x1) * 0.5f, mzp = (z0 + z1) * 0.5f;
                // 정규화 좌표 (-1~1): across=u, along=v
                float u = (mxp - cX) / halfX;
                float v = (mzp - cZ) / halfZ;
                if (InAnyHole(u, v)) { removed++; continue; }   // 구멍 칸은 건너뜀
                kept++;

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

        Debug.Log($"[Stage5] 구멍 메시 생성: bounds(size {b.size.x:F1}x{b.size.z:F1}, center {b.center}), " +
                  $"격자 {cx}x{cz}, 남긴칸 {kept} / 뚫은칸 {removed} (구멍 {HoleSpots.GetLength(0)}개)");

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

    // 정규화 좌표(-1~1)에서 어떤 구멍 원 안이면 true.
    static bool InAnyHole(float u, float v)
    {
        int n = HoleSpots.GetLength(0);
        for (int i = 0; i < n; i++)
        {
            float du = u - HoleSpots[i, 0];   // across
            float dv = v - HoleSpots[i, 1];   // along
            float r = HoleSpots[i, 2];
            if (du * du + dv * dv < r * r) return true;
        }
        return false;
    }

    static void EnsureMixedSpawner(GameObject s5)
    {
        string[] paths = {
            "Assets/Prefabs/Rock_Fast.prefab",   // 5스테이지는 빠른 바위만
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
