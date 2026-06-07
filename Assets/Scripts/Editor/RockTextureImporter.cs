#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// 메뉴: Tools > Rock Falldown > Apply Rock Texture
// Assets/Art/Rocks/rock.png 를 일반 돌(회색 바위)과 분열바위/조각(갈색 바위)에 입힌다.
// 돌은 구(Sphere) 물리라 굴러갈 때 텍스처 회전이 보여 '굴러오는' 느낌이 난다.
public static class RockTextureImporter
{
    const string TexPath = "Assets/Art/Rocks/rock.png";
    static readonly Vector2 Tiling = new Vector2(2f, 2f);

    [MenuItem("Tools/Rock Falldown/Apply Rock Texture")]
    public static void Apply()
    {
        var ti = AssetImporter.GetAtPath(TexPath) as TextureImporter;
        if (ti == null)
        {
            Debug.LogWarning($"[RockTextureImporter] 텍스처 미임포트: {TexPath} — Unity 새로고침 후 재실행");
            return;
        }
        ti.wrapMode = TextureWrapMode.Repeat;
        ti.SaveAndReimport();
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath);

        // 1) 일반 돌: 회색 바위 머티리얼 → Rock.prefab
        var boulder = GetOrCreateMat("Assets/Materials/Rock_Boulder.mat", Color.white);
        SetTex(boulder, tex);
        AssignToPrefab("Assets/Prefabs/Rock.prefab", boulder);

        // 2) 분열바위/조각: 눈 텍스처 → Rock_Snow (거대 눈덩이)
        const string snowPath = "Assets/Art/Rocks/snow.png";
        var snowTi = AssetImporter.GetAtPath(snowPath) as TextureImporter;
        if (snowTi != null)
        {
            snowTi.wrapMode = TextureWrapMode.Repeat;
            snowTi.SaveAndReimport();
            var snow = AssetDatabase.LoadAssetAtPath<Texture2D>(snowPath);
            var snowMat = GetOrCreateMat("Assets/Materials/Rock_Snow.mat", Color.white);
            SetTex(snowMat, snow);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[RockTextureImporter] 바위 텍스처 적용 완료. 돌이 굴러갈 때 회전이 보입니다.");
    }

    static Material GetOrCreateMat(string path, Color color)
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color };
            AssetDatabase.CreateAsset(m, path);
        }
        return m;
    }

    static void SetTex(Material m, Texture2D tex)
    {
        if (m.HasProperty("_BaseMap"))
        {
            m.SetTexture("_BaseMap", tex);
            m.SetTextureScale("_BaseMap", Tiling);
        }
        m.mainTexture = tex;
        m.mainTextureScale = Tiling;
        // 거친 바위/눈 느낌: 광택 제거
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.05f);
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0f);
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0.05f);
        EditorUtility.SetDirty(m);
    }

    static void AssignToPrefab(string prefabPath, Material mat)
    {
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (go == null) { Debug.LogWarning($"[RockTextureImporter] 프리팹 없음: {prefabPath}"); return; }
        var mr = go.GetComponentInChildren<MeshRenderer>();
        if (mr != null)
        {
            mr.sharedMaterial = mat;
            PrefabUtility.SavePrefabAsset(go);
        }
    }
}
#endif
