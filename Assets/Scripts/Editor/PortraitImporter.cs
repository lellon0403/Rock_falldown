#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

// 메뉴: Tools > Rock Falldown > Import & Assign Portraits
// Assets/Art/Portraits 의 PNG들을 Sprite로 임포트하고 PortraitDisplay.tiers에 연결한다.
public static class PortraitImporter
{
    const string Dir = "Assets/Art/Portraits";
    static readonly string[] TierFiles = { "Tier0_Caveman", "Tier1_Student", "Tier2_Professor", "Tier3_Genius" };

    [MenuItem("Tools/Rock Falldown/Import & Assign Portraits")]
    public static void ImportAssign()
    {
        // 1) PNG → Sprite 임포트 설정
        foreach (var name in TierFiles)
        {
            SetSprite($"{Dir}/{name}_a.png");
            SetSprite($"{Dir}/{name}_b.png");
        }
        AssetDatabase.Refresh();

        // 2) PortraitDisplay에 연결
        var pd = Object.FindFirstObjectByType<PortraitDisplay>();
        if (pd == null)
        {
            Debug.LogWarning("[PortraitImporter] PortraitDisplay가 없습니다. 먼저 Setup Game Systems를 실행하세요.");
            return;
        }
        if (pd.tiers == null || pd.tiers.Length < TierFiles.Length)
        {
            Debug.LogWarning("[PortraitImporter] PortraitDisplay.tiers 개수가 부족합니다.");
            return;
        }

        for (int i = 0; i < TierFiles.Length; i++)
        {
            var a = AssetDatabase.LoadAssetAtPath<Sprite>($"{Dir}/{TierFiles[i]}_a.png");
            var b = AssetDatabase.LoadAssetAtPath<Sprite>($"{Dir}/{TierFiles[i]}_b.png");
            if (a == null || b == null)
            {
                Debug.LogWarning($"[PortraitImporter] 스프라이트 로드 실패: {TierFiles[i]} (Unity가 PNG를 임포트했는지 확인)");
                continue;
            }
            pd.tiers[i].frames = new Sprite[] { a, b };
        }

        if (pd.image != null) pd.image.color = Color.white;   // placeholder 반투명 → 불투명
        EditorUtility.SetDirty(pd);
        Debug.Log("[PortraitImporter] 초상 임포트 + 연결 완료. 씬 저장 후 Play로 확인하세요.");
    }

    static void SetSprite(string path)
    {
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null)
        {
            Debug.LogWarning($"[PortraitImporter] 임포터 없음(아직 미임포트?): {path} — Unity 창을 한 번 클릭해 새로고침 후 다시 실행하세요.");
            return;
        }
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.SaveAndReimport();
    }
}
#endif
