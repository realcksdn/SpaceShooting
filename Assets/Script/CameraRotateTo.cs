using UnityEngine;

public class CameraRotateTo : MonoBehaviour
{
    public float speed = 1f;

    void Update()
    {
        var e = transform.eulerAngles;
        transform.eulerAngles = new Vector3(
            Mathf.LerpAngle(e.x, 0f, speed * Time.deltaTime), e.y, e.z);
    }
}
