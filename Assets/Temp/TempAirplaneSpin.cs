using UnityEngine;

public class TempAirplaneSpin : MonoBehaviour
{
    public float spinSpeed = 120f;   // 도/초 (X축 배럴롤)
    public float yawSpeed  =  60f;   // 도/초 (Y축 수평 회전)
    public float bobHeight =  1.5f;  // 위아래 진폭
    public float bobSpeed  =   1f;   // 위아래 주기

    private Vector3 _originPos;

    void Start()
    {
        _originPos = transform.position;
    }

    void Update()
    {
        transform.Rotate(spinSpeed * Time.deltaTime, yawSpeed * Time.deltaTime, 0f, Space.Self);
        float y = _originPos.y + Mathf.Sin(Time.time * bobSpeed * Mathf.PI * 2f) * bobHeight;
        transform.position = new Vector3(_originPos.x, y, _originPos.z);
    }
}
