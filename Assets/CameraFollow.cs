using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;        // 캐릭터 드래그 앤 드롭
    public Vector3 offset = new Vector3(0f, 5f, -7f);  // 뒤+위에서 바라봄

    void LateUpdate()
    {
        transform.position = target.position + offset;
        transform.rotation = Quaternion.Euler(15f, 0f, 0f);
    }
}