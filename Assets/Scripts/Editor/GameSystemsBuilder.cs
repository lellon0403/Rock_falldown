#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

// 메뉴: Tools > Rock Falldown > Setup Game Systems
// IQ 매니저, 좌상단 IQ HUD(숫자 + 초상), StageManager, Stage 2 스포너를 구성한다.
// 캐릭터는 3D 유지하고, IQ 구간 이미지는 HUD 초상으로 표시한다.
public static class GameSystemsBuilder
{
    [MenuItem("Tools/Rock Falldown/Setup Game Systems")]
    public static void Setup()
    {
        EnsureIQManager();
        EnsureStageManager();
        var canvas = EnsureIQHud();
        EnsureStage2Spawner();
        RestorePlayer3D();
        EnsurePortrait(canvas);
        EnsurePlayerTint();
        EnsureGateBonus();
        ArrangeHud();

        Debug.Log("[GameSystemsBuilder] 셋업 완료. 초상 이미지는 생성 후 PortraitDisplay의 Tiers에 연결됩니다.");
    }

    static void EnsureIQManager()
    {
        var mgr = Object.FindFirstObjectByType<IQManager>();
        if (mgr == null)
        {
            var go = new GameObject("IQManager");
            mgr = go.AddComponent<IQManager>();
            Undo.RegisterCreatedObjectUndo(go, "Create IQManager");
        }
        // 기존 인스턴스의 startIQ가 0으로 굳어 있을 수 있으니 60으로 보정
        var so = new SerializedObject(mgr);
        var prop = so.FindProperty("startIQ");
        if (prop != null)
        {
            prop.intValue = 60;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(mgr);
        }
    }

    static void EnsureStageManager()
    {
        if (Object.FindFirstObjectByType<StageManager>() != null) return;
        var go = new GameObject("StageManager");
        go.AddComponent<StageManager>();
        Undo.RegisterCreatedObjectUndo(go, "Create StageManager");
    }

    static GameObject EnsureIQHud()
    {
        var existing = Object.FindFirstObjectByType<IQDisplay>();
        if (existing != null)
        {
            var c = existing.GetComponentInParent<Canvas>();
            if (c != null) return c.gameObject;
        }

        var canvasGO = new GameObject("HUD_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        var textGO = new GameObject("IQ_Text", typeof(TextMeshProUGUI));
        textGO.transform.SetParent(canvasGO.transform, false);
        var rt = textGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(200f, -24f);   // 초상 옆에 표시
        rt.sizeDelta = new Vector2(400f, 90f);

        var tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = "IQ 0";
        tmp.fontSize = 48f;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.fontStyle = FontStyles.Bold;

        canvasGO.AddComponent<IQDisplay>().label = tmp;

        Undo.RegisterCreatedObjectUndo(canvasGO, "Create IQ HUD");
        return canvasGO;
    }

    static void EnsureStage2Spawner()
    {
        var spawners = Object.FindObjectsByType<RockSpawner>(FindObjectsSortMode.None);
        if (spawners.Length >= 2 || spawners.Length == 0) return;

        var src = spawners[0];
        var dup = Object.Instantiate(src.gameObject);
        dup.name = "spawner_Stage2";

        var s2 = GameObject.Find("Stage_2");
        if (s2 != null)
        {
            dup.transform.SetParent(s2.transform, false);
            dup.transform.localPosition = src.transform.localPosition;
            dup.transform.localRotation = src.transform.localRotation;
            dup.transform.localScale = src.transform.localScale;
        }
        Undo.RegisterCreatedObjectUndo(dup, "Create Stage2 Spawner");
    }

    // 이전 셋업이 3D 메시를 끄고 캡슐 교체를 넣었다면 원복 (이제 3D 유지)
    static void RestorePlayer3D()
    {
        var pm = Object.FindFirstObjectByType<PlayerMove>();
        if (pm == null) return;

        var rend = pm.GetComponent<MeshRenderer>();
        if (rend != null) rend.enabled = true;

        var evo = pm.GetComponent<CharacterEvolution>();
        if (evo != null) Object.DestroyImmediate(evo);

        var visual = pm.transform.Find("Visual");
        if (visual != null) Object.DestroyImmediate(visual.gameObject);
    }

    // 모든 수학 게이트의 IQ 보너스를 +30으로 (스테이지 클리어당 60→90→120→150)
    static void EnsureGateBonus()
    {
        foreach (var gate in Object.FindObjectsByType<MathGate>(FindObjectsSortMode.None))
        {
            gate.iqBonus = 30;
            EditorUtility.SetDirty(gate);
        }
    }

    // 초상과 IQ 숫자가 겹치지 않게 배치 (재실행해도 항상 정리)
    static void ArrangeHud()
    {
        var pd = Object.FindFirstObjectByType<PortraitDisplay>();
        if (pd != null && pd.image != null)
        {
            var rt = pd.image.rectTransform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(24f, -24f);
            rt.sizeDelta = new Vector2(150f, 150f);
        }

        var iq = Object.FindFirstObjectByType<IQDisplay>();
        if (iq != null && iq.label != null)
        {
            var rt = iq.label.rectTransform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(190f, -54f);   // 초상 오른쪽
            rt.sizeDelta = new Vector2(500f, 100f);
            iq.label.fontSize = 56f;
            iq.label.alignment = TextAlignmentOptions.Left;
            EditorUtility.SetDirty(iq.label);
        }
    }

    // IQ 구간별 플레이어 3D 색 (각 초상 대표 색과 유사)
    static void EnsurePlayerTint()
    {
        var pm = Object.FindFirstObjectByType<PlayerMove>();
        if (pm == null) return;

        var pt = pm.GetComponent<PlayerTint>();
        if (pt == null) pt = Undo.AddComponent<PlayerTint>(pm.gameObject);
        pt.targetRenderer = pm.GetComponent<MeshRenderer>();
        pt.tiers = new PlayerTint.Tier[]
        {
            new PlayerTint.Tier { name = "원시인", minIQ = 60,  color = new Color(0.572f, 0.397f, 0.236f) },
            new PlayerTint.Tier { name = "학생",   minIQ = 90,  color = new Color(0.478f, 0.419f, 0.298f) },
            new PlayerTint.Tier { name = "교수",   minIQ = 120, color = new Color(0.411f, 0.361f, 0.331f) },
            new PlayerTint.Tier { name = "천재",   minIQ = 150, color = new Color(0.725f, 0.573f, 0.366f) },
        };
        EditorUtility.SetDirty(pt);

        // 돌 밀치기(임펄스) — 적당값. 인스펙터에서 조절 (기존 컴포넌트에도 적용)
        var ph = pm.GetComponent<PlayerHit>();
        if (ph != null) { ph.knockbackForce = 40f; EditorUtility.SetDirty(ph); }
        pm.knockbackDuration = 0.5f;
        pm.knockbackDamping = 5f;
        EditorUtility.SetDirty(pm);
    }

    static void EnsurePortrait(GameObject canvas)
    {
        if (canvas == null) return;

        int[] mins = { 60, 90, 120, 150 };
        var existingPd = Object.FindFirstObjectByType<PortraitDisplay>();
        if (existingPd != null)
        {
            // 이미 있으면 frames(초상 이미지)는 보존하고 임계값만 갱신
            if (existingPd.tiers != null)
                for (int i = 0; i < existingPd.tiers.Length && i < mins.Length; i++)
                    existingPd.tiers[i].minIQ = mins[i];
            EditorUtility.SetDirty(existingPd);
            return;
        }

        var imgGO = new GameObject("Portrait", typeof(Image));
        imgGO.transform.SetParent(canvas.transform, false);
        var rt = imgGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(24f, -24f);
        rt.sizeDelta = new Vector2(160f, 160f);

        var img = imgGO.GetComponent<Image>();
        img.preserveAspect = true;
        img.color = new Color(1f, 1f, 1f, 0.25f);   // 이미지 연결 전엔 반투명 placeholder

        var pd = imgGO.AddComponent<PortraitDisplay>();
        pd.image = img;
        pd.tiers = new PortraitDisplay.Tier[]
        {
            new PortraitDisplay.Tier { name = "원시인", minIQ = 60,  frames = new Sprite[0] },
            new PortraitDisplay.Tier { name = "학생",   minIQ = 90,  frames = new Sprite[0] },
            new PortraitDisplay.Tier { name = "교수",   minIQ = 120, frames = new Sprite[0] },
            new PortraitDisplay.Tier { name = "천재",   minIQ = 150, frames = new Sprite[0] },
        };

        Undo.RegisterCreatedObjectUndo(imgGO, "Create Portrait");
    }
}
#endif
