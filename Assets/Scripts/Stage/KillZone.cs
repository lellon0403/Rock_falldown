using UnityEngine;

// 구멍/낙사 구간. 플레이어가 닿으면 즉시 사망(처음부터 다시 시작).
[RequireComponent(typeof(Collider))]
public class KillZone : MonoBehaviour
{
    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        var pd = other.GetComponentInParent<PlayerDeath>();
        if (pd != null) pd.Die();
    }
}
