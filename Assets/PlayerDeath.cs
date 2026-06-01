using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour
{
    bool isDead = false;

    void Update()
    {
        if (!isDead && transform.position.y <= 0f)
        {
            isDead = true;
            Invoke("Restart", 1f);
        }
    }

    void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}