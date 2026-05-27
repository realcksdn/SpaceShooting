using UnityEngine;

/// <summary>
/// 오브젝트가 앞으로 계속 나아갑니다.
/// </summary>
public class MoveForward : MonoBehaviour
{
    public float speed = 5f;

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}
