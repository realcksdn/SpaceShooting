using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    [Header("떠다니는 효과")]
    public float floatAmplitude = 0.3f;   // 위아래 움직임 폭
    public float floatSpeed = 1.5f;        // 위아래 속도

    [Header("회전 효과")]
    public bool enableRotation = true;
    public float rotationSpeed = 30f;      // 초당 회전 각도

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // 위아래 부유
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);

       
    }
}