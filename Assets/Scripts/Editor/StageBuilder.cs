#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// 메뉴: Tools > Rock Falldown > Create Next Stage (map)
// Stage_1·Stage_2 간격(경사 방향)을 그대로 이어, 마지막 Stage 위에 다음 Stage 맵(평면)을 만든다.
// 맵 지오메트리만 복제(자식 스포너/게이트 등은 제거). 콘텐츠는 이후에 따로 붙인다.
public static class StageBuilder
{
    [MenuItem("Tools/Rock Falldown/Create Next Stage (map)")]
    public static void CreateNextStage()
    {
        var s1 = GameObject.Find("Stage_1");
        var s2 = GameObject.Find("Stage_2");
        if (s1 == null || s2 == null)
        {
            Debug.LogWarning("[StageBuilder] Stage_1, Stage_2가 있어야 합니다.");
            return;
        }

        // 가장 높은 번호의 Stage 찾기
        int n = 2;
        Transform last = s2.transform;
        for (int i = 3; i <= 30; i++)
        {
            var g = GameObject.Find("Stage_" + i);
            if (g == null) break;
            last = g.transform;
            n = i;
        }

        Vector3 offset = s2.transform.position - s1.transform.position;  // 한 스테이지 간격(경사 방향)

        var go = Object.Instantiate(last.gameObject);
        go.name = "Stage_" + (n + 1);
        go.transform.SetPositionAndRotation(last.position + offset, last.rotation);
        go.transform.localScale = last.localScale;

        // 맵만 남기고 자식(스포너/장애물 등) 제거
        for (int i = go.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(go.transform.GetChild(i).gameObject);

        Undo.RegisterCreatedObjectUndo(go, "Create Next Stage");
        Selection.activeGameObject = go;
        Debug.Log($"[StageBuilder] {go.name} 생성 (맵만). 위치 {go.transform.position}. " +
                  "테마를 바꾸려면 머티리얼을 교체하세요.");
    }
}
#endif
