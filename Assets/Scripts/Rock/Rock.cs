using UnityEngine;

// 바위별 밀치는 세기. PlayerHit가 이 값을 읽어 적용한다. (없으면 PlayerHit 기본값)
public class Rock : MonoBehaviour
{
    [Tooltip("플레이어를 밀치는 세기 (낮을수록 살짝)")]
    public float knockbackForce = 5f;
}
