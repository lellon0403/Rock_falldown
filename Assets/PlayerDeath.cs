using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour
{
    bool isDead = false;

    void Update()
    {
        if (transform.position.y <= 0f) Die();   // 맵 아래로 추락
    }

    // 구멍(KillZone) 등 외부에서도 즉시 사망시킬 수 있다.
    public void Die()
    {
        if (isDead) return;
        isDead = true;
        Invoke(nameof(Restart), 1f);
    }

    void Restart()
    {
        Time.timeScale = 1f;   // 완주 화면 등에서 멈춰 있었을 수 있으니 복구
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
