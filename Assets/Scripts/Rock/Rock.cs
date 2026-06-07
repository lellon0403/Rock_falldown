using UnityEngine;

// 바위별 밀치기 힘. PlayerHit가 이 값을 읽어 적용한다. (없으면 PlayerHit 기본값 사용)
public class Rock : MonoBehaviour
{
    [Tooltip("플레이어를 밀치는 힘. 낮을수록 '뿅' 대신 '질질 끌리는' 느낌")]
    public float knockbackForce = 5f;
}
