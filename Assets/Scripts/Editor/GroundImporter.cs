#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// 메뉴: Tools > Rock Falldown > Import & Assign Ground Textures
// Assets/Art/Ground 의 텍스처를 Repeat로 임포트하고 Stage 머티리얼의 BaseMap + 타일링에 적용.
public static class GroundImporter
{
    const string Dir = "Assets/Art/Ground";
    static readonly Vector2 Tiling = new Vector2(10f, 10f);   // 반복 횟수 (무늬가 지나가며 움직임 느낌)

    [MenuItem("Tools/Rock Falldown/Import & Assign Ground Textures")]
    public static void ImportAssign()
    {
        Apply($"{Dir}/grass.png", "Assets/Materials/Stage_Grass.mat");
        Apply($"{Dir}/ice.png", "Assets/Materials/Stage_Ice.mat");
        AssetDatabase.SaveAssets();
        Debug.Log("[GroundImporter] 바닥 텍스처 적용 완료. 타일이 너무 크/작으면 머티리얼의 Base Map Tiling 조절.");
    }

    static void Apply(string texPath, string matPath)
    {
        var ti = AssetImporter.GetAtPath(texPath) as TextureImporter;
        if (ti == null)
        {
            Debug.LogWarning($"[GroundImporter] 텍스처 미임포트(Unity 새로고침 필요?): {texPath}");
            return;
        }
        ti.wrapMode = TextureWrapMode.Repeat;
        ti.SaveAndReimport();

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (tex == null || mat == null)
        {
            Debug.LogWarning($"[GroundImporter] 로드 실패: tex={texPath}, mat={matPath}");
            return;
        }

        if (mat.HasProperty("_BaseMap"))
        {
            mat.SetTexture("_BaseMap", tex);
            mat.SetTextureScale("_BaseMap", Tiling);
        }
        mat.mainTexture = tex;
        mat.mainTextureScale = Tiling;
        EditorUtility.SetDirty(mat);
    }
}
#endif
