using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VisionEffect : MonoBehaviour
{
    public static VisionEffect Instance { get; private set; }

    [Header("전체화면 검은 Image 연결")]
    public Image darkImage;

    [Header("암전 설정")]
    public float delay     = 1f;   // 데미지 후 암전까지 대기
    public float fadeIn    = 0.5f; // 어두워지는 시간
    public float holdTime  = 1f;   // 완전히 어두운 유지 시간
    public float fadeOut   = 0.5f; // 밝아지는 시간

    void Awake()
    {
        Instance = this;
        SetAlpha(0f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V)) TriggerVision();
    }

    public void TriggerVision()
    {
        StopAllCoroutines();
        StartCoroutine(DarkRoutine());
    }

    IEnumerator DarkRoutine()
    {
        yield return new WaitForSeconds(delay);

        // 점점 어두워짐
        float t = 0f;
        while (t < fadeIn)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(0f, 1f, t / fadeIn));
            yield return null;
        }
        SetAlpha(1f);

        // 유지
        yield return new WaitForSeconds(holdTime);

        // 점점 밝아짐
        t = 0f;
        while (t < fadeOut)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(1f, 0f, t / fadeOut));
            yield return null;
        }
        SetAlpha(0f);
    }

    void SetAlpha(float a)
    {
        if (darkImage == null) return;
        var c = darkImage.color;
        c.a = a;
        darkImage.color = c;
    }
}
