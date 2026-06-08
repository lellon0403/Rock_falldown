#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;
using TMPro;

// 메뉴: Tools > Rock Falldown > Setup Korean Font (Fallback)
// Windows 맑은 고딕(malgun.ttf)을 프로젝트로 가져와 동적 TMP 폰트로 만들고,
// 기본 폰트(LiberationSans SDF)의 폴백으로 등록한다 → 게임 내 모든 한글이 렌더링됨.
// (결승 "완주!/기록" 등 □□ 깨짐 해결)
public static class KoreanFontSetup
{
    const string SrcFont = @"C:\Windows\Fonts\malgun.ttf";
    const string FontDir = "Assets/Fonts";
    const string FontTtf = "Assets/Fonts/malgun.ttf";
    const string FontTmp = "Assets/Fonts/Malgun SDF.asset";
    const string BaseFont = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

    [MenuItem("Tools/Rock Falldown/Setup Korean Font (Fallback)")]
    public static void Setup()
    {
        if (!Directory.Exists(FontDir)) Directory.CreateDirectory(FontDir);

        // 1) 시스템 폰트 복사
        if (!File.Exists(FontTtf))
        {
            if (!File.Exists(SrcFont))
            {
                Debug.LogError($"[KoreanFont] 시스템 폰트 없음: {SrcFont}");
                return;
            }
            File.Copy(SrcFont, FontTtf, true);
            AssetDatabase.ImportAsset(FontTtf, ImportAssetOptions.ForceUpdate);
            Debug.Log("[KoreanFont] malgun.ttf 임포트 완료");
        }

        var srcUnityFont = AssetDatabase.LoadAssetAtPath<Font>(FontTtf);
        if (srcUnityFont == null)
        {
            Debug.LogError("[KoreanFont] malgun.ttf 로드 실패 (Unity 새로고침 후 재시도)");
            return;
        }

        // 2) 동적 TMP 폰트 에셋 생성 (런타임에 글리프 생성 → 한글 전체 커버)
        var krFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontTmp);
        if (krFont == null)
        {
            krFont = TMP_FontAsset.CreateFontAsset(srcUnityFont);
            krFont.name = "Malgun SDF";
            AssetDatabase.CreateAsset(krFont, FontTmp);
            // 아틀라스 텍스처/머티리얼을 에셋 하위로 저장
            if (krFont.atlasTexture != null)
            {
                krFont.atlasTexture.name = "Malgun SDF Atlas";
                AssetDatabase.AddObjectToAsset(krFont.atlasTexture, krFont);
            }
            if (krFont.material != null)
            {
                krFont.material.name = "Malgun SDF Material";
                AssetDatabase.AddObjectToAsset(krFont.material, krFont);
            }
            EditorUtility.SetDirty(krFont);
            Debug.Log("[KoreanFont] 동적 TMP 폰트 생성: " + FontTmp);
        }

        // 3) 기본 폰트의 폴백으로 등록 → 모든 한글 텍스트 자동 렌더링
        var baseFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BaseFont);
        if (baseFont != null)
        {
            if (baseFont.fallbackFontAssetTable == null)
                baseFont.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();
            if (!baseFont.fallbackFontAssetTable.Contains(krFont))
            {
                baseFont.fallbackFontAssetTable.Add(krFont);
                EditorUtility.SetDirty(baseFont);
                Debug.Log("[KoreanFont] LiberationSans SDF 폴백에 한글 폰트 추가");
            }
            else Debug.Log("[KoreanFont] 이미 폴백에 등록됨");
        }
        else Debug.LogWarning("[KoreanFont] 기본 폰트 못 찾음: " + BaseFont);

        // 4) TMP 전역 설정에도 폴백 추가 (안전망)
        if (TMP_Settings.fallbackFontAssets == null)
        {
            // 일부 버전은 읽기전용 — 실패해도 무시
        }
        else if (!TMP_Settings.fallbackFontAssets.Contains(krFont))
        {
            TMP_Settings.fallbackFontAssets.Add(krFont);
            var settings = Resources.Load<TMP_Settings>("TMP Settings");
            if (settings != null) EditorUtility.SetDirty(settings);
            Debug.Log("[KoreanFont] TMP Settings 전역 폴백에 추가");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[KoreanFont] 완료 — Play 다시 시작하면 한글이 정상 표시됩니다.");
    }
}
#endif
