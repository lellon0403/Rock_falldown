#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// 메뉴: Tools > Rock Falldown > Import & Assign Ground Textures
// Assets/Art/Ground 텍스처를 Repeat로 임포트하고, 각 스테이지 머티리얼+표면에 할당.
// Stage_1 잔디 / Stage_2 얼음 / Stage_3 고원
public static class GroundImporter
{
    const string Dir = "Assets/Art/Ground";
    static readonly Vector2 Tiling = new Vector2(10f, 10f);

    [MenuItem("Tools/Rock Falldown/Import & Assign Ground Textures")]
    public static void ImportAssign()
    {
        Apply($"{Dir}/grass.png", "Assets/Materials/Stage_Grass.mat", "Stage_1");
        Apply($"{Dir}/ice.png", "Assets/Materials/Stage_Ice.mat", "Stage_2");
        Apply($"{Dir}/plateau.png", "Assets/Materials/Stage_Plateau.mat", "Stage_3");
        Apply($"{Dir}/space.png", "Assets/Materials/Stage_Space.mat", "Stage_4");
        Apply($"{Dir}/heaven.png", "Assets/Materials/Stage_Heaven.mat", "Stage_5");   // 천국(천상계)
        Apply($"{Dir}/finish.png", "Assets/Materials/Stage_Finish.mat", "");   // 결승로 바닥 (FinishPath가 사용)
        AssetDatabase.SaveAssets();
        Debug.Log("[GroundImporter] 바닥 텍스처 적용 완료 (1 잔디 / 2 얼음 / 3 고원 / 4 우주 / 5 천국 + 결승선).");
    }

    static void Apply(string texPath, string matPath, string stageName)
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
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.1f);   // 바닥은 무광에 가깝게
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);
            AssetDatabase.CreateAsset(mat, matPath);
        }
        if (mat.HasProperty("_BaseMap"))
        {
            mat.SetTexture("_BaseMap", tex);
            mat.SetTextureScale("_BaseMap", Tiling);
        }
        mat.mainTexture = tex;
        mat.mainTextureScale = Tiling;
        EditorUtility.SetDirty(mat);

        // 스테이지 표면에 머티리얼 할당
        if (!string.IsNullOrEmpty(stageName))
        {
            var stage = GameObject.Find(stageName);
            var mr = stage != null ? stage.GetComponent<MeshRenderer>() : null;
            if (mr != null) { mr.sharedMaterial = mat; EditorUtility.SetDirty(mr); }
            else if (stage == null)
                Debug.Log($"[GroundImporter] {stageName} 없음 — 머티리얼만 준비됨(나중에 생성 시 할당).");
        }
    }
}
#endif
