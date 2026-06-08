using UnityEngine;

// 게임 시작 시 자동으로 배경음악을 루프 재생한다.
// 씬에 오브젝트를 추가할 필요 없이 RuntimeInitializeOnLoadMethod로 스스로 생성되고,
// 씬이 바뀌어도 DontDestroyOnLoad로 음악이 끊기지 않는다.
public class BGMPlayer : MonoBehaviour
{
    // Resources/Audio/ 안의 파일 이름(확장자 제외).
    const string ClipPath = "Audio/bgm_unwelcome_school";

    [Range(0f, 1f)]
    public float volume = 0.25f;

    static BGMPlayer instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoCreate()
    {
        if (instance != null) return;

        var go = new GameObject("BGMPlayer");
        instance = go.AddComponent<BGMPlayer>();
        DontDestroyOnLoad(go);
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        var clip = Resources.Load<AudioClip>(ClipPath);
        if (clip == null)
        {
            Debug.LogWarning($"[BGMPlayer] 음악을 찾을 수 없음: Resources/{ClipPath}");
            return;
        }

        var src = gameObject.AddComponent<AudioSource>();
        src.clip = clip;
        src.loop = true;
        src.volume = volume;
        src.playOnAwake = false;
        src.Play();
    }
}
