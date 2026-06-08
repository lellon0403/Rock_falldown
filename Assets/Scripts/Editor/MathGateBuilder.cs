#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;

// 메뉴: Tools > Rock Falldown > Create Math Gate ...
// 벽 + 좌/우 문 + 문제/정답 텍스트 + 판정 스크립트가 연결된 게이트를 씬에 생성한다.
public static class MathGateBuilder
{
    const float MapWidth = 60f;   // 맵 가로 폭 (RockSpawner와 동일 기준)
    const float Height = 8f;
    const float Thickness = 1f;

    [MenuItem("Tools/Rock Falldown/Create Math Gate at End of Stage 1")]
    public static void CreateGateStage1()
    {
        var s1 = GameObject.Find("Stage_1");
        var s2 = GameObject.Find("Stage_2");
        if (s1 == null || s2 == null) { Fallback("Assets/ScriptableObjects/Q_Stage2_01.asset"); return; }

        // Stage_1 → Stage_2 경계(=Stage_1 끝)
        Vector3 boundary = Vector3.Lerp(s1.transform.position, s2.transform.position, 0.5f);
        Quaternion rot = SlopeRotation(s1.transform.position, s2.transform.position);
        BuildGate(boundary + Vector3.up * (Height * 0.5f), rot, "Assets/ScriptableObjects/Q_Stage2_01.asset");
    }

    [MenuItem("Tools/Rock Falldown/Create Math Gate at End of Stage 2")]
    public static void CreateGateStage2()
    {
        var s1 = GameObject.Find("Stage_1");
        var s2 = GameObject.Find("Stage_2");
        if (s1 == null || s2 == null) { Fallback("Assets/ScriptableObjects/Q_Stage2_02.asset"); return; }

        // Stage_2 끝 = Stage_2 중심에서 경사 위로 반 스테이지 길이만큼
        Vector3 slopeDir = (s2.transform.position - s1.transform.position).normalized;
        float halfLen = Vector3.Distance(s1.transform.position, s2.transform.position) * 0.5f;
        Vector3 topEnd = s2.transform.position + slopeDir * halfLen;
        Quaternion rot = SlopeRotation(s1.transform.position, s2.transform.position);
        BuildGate(topEnd + Vector3.up * (Height * 0.5f), rot, "Assets/ScriptableObjects/Q_Stage2_02.asset");
    }

    [MenuItem("Tools/Rock Falldown/Create Math Gate at End of Stage 3")]
    public static void CreateGateStage3()
    {
        var s1 = GameObject.Find("Stage_1");
        var s2 = GameObject.Find("Stage_2");
        var s3 = GameObject.Find("Stage_3");
        if (s1 == null || s2 == null || s3 == null) { Fallback("Assets/ScriptableObjects/Q_Stage3_01.asset"); return; }

        // Stage_3 끝 = Stage_3 중심에서 경사 위로 반 스테이지 길이만큼
        Vector3 slopeDir = (s2.transform.position - s1.transform.position).normalized;
        float halfLen = Vector3.Distance(s1.transform.position, s2.transform.position) * 0.5f;
        Vector3 topEnd = s3.transform.position + slopeDir * halfLen;
        Quaternion rot = SlopeRotation(s1.transform.position, s2.transform.position);
        BuildGate(topEnd + Vector3.up * (Height * 0.5f), rot, "Assets/ScriptableObjects/Q_Stage3_01.asset");
    }

    static void Fallback(string qPath)
    {
        BuildGate(new Vector3(0f, Height * 0.5f, 40f), Quaternion.identity, qPath);
        Debug.LogWarning("[MathGateBuilder] Stage_1/Stage_2를 못 찾아 기본 위치에 생성했습니다. 직접 옮기세요.");
    }

    static Quaternion SlopeRotation(Vector3 from, Vector3 to)
    {
        Vector3 slope = (to - from).normalized;
        float pitch = Mathf.Atan2(slope.y, new Vector2(slope.x, slope.z).magnitude) * Mathf.Rad2Deg;
        return Quaternion.Euler(-pitch, 0f, 0f);
    }

    static void BuildGate(Vector3 pos, Quaternion rot, string questionPath)
    {
        float doorW = MapWidth / 2f;

        var root = new GameObject("MathGate");
        var gate = root.AddComponent<MathGate>();
        root.transform.SetPositionAndRotation(pos, rot);

        gate.leftDoor = BuildDoor(root.transform, "Door_Left", -doorW * 0.5f, doorW);
        gate.rightDoor = BuildDoor(root.transform, "Door_Right", doorW * 0.5f, doorW);

        float front = -(Thickness * 0.5f + 0.4f);

        gate.questionLabel = CreateText(root.transform, "QuestionLabel",
            new Vector3(0f, 0f, front), 50f, MapWidth * 0.9f, "Q");
        gate.leftDoor.answerLabel = CreateText(gate.leftDoor.transform, "AnswerLabel",
            new Vector3(0f, 0f, front), 24f, doorW * 0.8f, "?");
        gate.rightDoor.answerLabel = CreateText(gate.rightDoor.transform, "AnswerLabel",
            new Vector3(0f, 0f, front), 24f, doorW * 0.8f, "?");

        var question = AssetDatabase.LoadAssetAtPath<MathQuestion>(questionPath);
        if (question != null) gate.question = question;
        else Debug.LogWarning($"[MathGateBuilder] 문제 에셋 없음: {questionPath} (나중에 MathGate.question에 연결)");

        Undo.RegisterCreatedObjectUndo(root, "Create Math Gate");
        Selection.activeGameObject = root;
        Debug.Log("[MathGateBuilder] MathGate 생성 완료. 위치/기울기가 안 맞으면 미세조정하세요.");
    }

    static GateDoor BuildDoor(Transform parent, string name, float xOffset, float doorW)
    {
        var door = new GameObject(name);
        door.transform.SetParent(parent, false);
        door.transform.localPosition = new Vector3(xOffset, 0f, 0f);

        var trigger = door.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(doorW, Height, 4f);
        trigger.center = new Vector3(0f, 0f, -3f);

        var gd = door.AddComponent<GateDoor>();

        var barrier = GameObject.CreatePrimitive(PrimitiveType.Cube);
        barrier.name = "Barrier";
        barrier.transform.SetParent(door.transform, false);
        barrier.transform.localPosition = Vector3.zero;
        barrier.transform.localScale = new Vector3(doorW, Height, Thickness);
        gd.barrier = barrier;

        return gd;
    }

    static TMP_Text CreateText(Transform parent, string name, Vector3 localPos,
                               float fontSize, float areaWidth, string text)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.identity;

        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.black;
        tmp.rectTransform.sizeDelta = new Vector2(areaWidth, Height);
        return tmp;
    }
}
#endif
