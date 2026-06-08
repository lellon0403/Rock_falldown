using UnityEngine;

// 완주 트로피. 플레이어가 닿으면 타이머를 멈추고 완주 화면을 띄운다.
[RequireComponent(typeof(Collider))]
public class FinishTrophy : MonoBehaviour
{
    public FinishScreen finishScreen;
    bool done;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (done) return;
        if (other.GetComponentInParent<PlayerMove>() == null) return;   // 플레이어만
        done = true;

        float t = 0f;
        if (GameTimer.Instance != null)
        {
            t = GameTimer.Instance.Elapsed;
            GameTimer.Instance.StopTimer();
        }
        if (finishScreen != null) finishScreen.Show(t);
    }
}
