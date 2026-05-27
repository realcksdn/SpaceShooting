using UnityEngine;

/// <summary>
/// 별 프리팹에 추가하면 반짝임 효과.
/// SpriteRenderer 또는 MeshRenderer 둘 다 지원.
/// </summary>
public class StarTwinkle : MonoBehaviour
{
    [Header("반짝임 속도")]
    public float speedMin = 0.5f;
    public float speedMax = 2.0f;

    [Header("밝기 범위")]
    public float brightnessMin = 0.1f;
    public float brightnessMax = 1.0f;

    [Header("이동")]
    public float driftSpeed = 1f;   // 이동 속도
    public float wrapRange  = 100f; // StarField의 rangeX/Y와 맞출 것

    private float speed;
    private float offset;
    private float hue;         // 별마다 고정된 노란 계열 색조
    private Vector3 driftDir;
    private SpriteRenderer sr;
    private Renderer mr;

    void Start()
    {
        speed    = Random.Range(speedMin, speedMax);
        offset   = Random.Range(0f, Mathf.PI * 2f);
        hue      = Random.Range(0.08f, 0.18f); // 노란~주황 계열 Hue
        driftDir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;

        sr = GetComponent<SpriteRenderer>();
        mr = GetComponent<Renderer>();
    }

    void Update()
    {
        // 드리프트 이동 + 범위 벗어나면 반대쪽으로 랩
        transform.position += driftDir * driftSpeed * Time.deltaTime;
        Vector3 p = transform.position;
        if (p.x >  wrapRange) p.x = -wrapRange;
        if (p.x < -wrapRange) p.x =  wrapRange;
        if (p.z >  wrapRange) p.z = -wrapRange;
        if (p.z < -wrapRange) p.z =  wrapRange;
        transform.position = p;

        // 0~1 사이로 부드럽게 진동
        float t = (Mathf.Sin(Time.time * speed + offset) + 1f) / 2f;
        float saturation = Mathf.Lerp(brightnessMin, brightnessMax, t);

        Color baseColor = sr != null ? sr.color : (mr != null ? mr.material.color : Color.white);
        Color.RGBToHSV(baseColor, out float _, out float _, out float v);

        // 노란 계열 고정 Hue + 채도 변화
        Color newColor = Color.HSVToRGB(hue, saturation, v);
        newColor.a = baseColor.a;

        if (sr != null)
            sr.color = newColor;
        else if (mr != null)
            mr.material.color = newColor;
    }
}
