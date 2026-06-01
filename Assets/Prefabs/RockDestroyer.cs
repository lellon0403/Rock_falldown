using UnityEngine;

public class RockDestroyer : MonoBehaviour
{
  
    void Update()
    {
        if (transform.position.y <= 0f)
        {
            Destroy(gameObject);
        }
    }
}