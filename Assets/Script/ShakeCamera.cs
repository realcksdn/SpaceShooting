using System.Collections;
using UnityEngine;
//원희형 스크립트
public class ShakeCamera : MonoBehaviour
{
    private Vector3 originalPosition;  // 초기 카메라 위치 저장
    public float shakePower;          // 흔들림 세기

    private void Start()
    {
        // 시작 시 원위치 저장
        originalPosition = transform.position;
    }

    public void SetUp(float time, float power)
    {
        shakePower = power;
        originalPosition = transform.position;
        StopCoroutine(nameof(Shake));
        StartCoroutine(Shake(time));
    }

    private IEnumerator Shake(float time)
    {
        float elapsed = 0f;

        // 흔들림 효과
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            Vector3 randomOffset = Random.insideUnitSphere * shakePower;
            transform.position = new Vector3(
                originalPosition.x + randomOffset.x,
                originalPosition.y,
                originalPosition.z + randomOffset.z
            );
            yield return null;
        }

        float returnDuration = 0.1f;
        float t = 0f;
        Vector3 startPos = transform.position;
        while (t < returnDuration)
        {
            t += Time.deltaTime;
            float lerpFactor = t / returnDuration;
            transform.position = Vector3.Lerp(startPos, originalPosition, lerpFactor);
            yield return null;
        }

        transform.position = originalPosition;
    }
}
