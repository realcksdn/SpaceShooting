using System.Collections;
using UnityEngine;

// 이 스크립트를 빨간 원 프리팹에 부착
// bossPrefab 에 실제 보스 프리팹 연결
public class BossSpawnIndicator : MonoBehaviour
{
    [Header("보스")]
    public GameObject bossPrefab;

    [Header("연출")]
    public float warningDuration = 3f;   // 빨간 원 표시 시간
    public float dropHeight      = 20f;  // 위에서 낙하 시작 높이 (탑뷰 기준 하늘 위)
    public float dropDuration    = 0.2f; // 낙하 소요 시간 (짧을수록 강렬)

    private float timer;

    void Start()
    {
        timer = warningDuration;
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        // ── 빨간 원 펄스 ──────────────────────────
        StartCoroutine(Pulse());

        // ── 3초 대기 ──────────────────────────────
        yield return new WaitForSeconds(warningDuration);

        // ── 보스 낙하 스폰 ────────────────────────
        if (bossPrefab != null)
        {
            Vector3 groundPos = new Vector3(transform.position.x, 0f, transform.position.z);
            Vector3 startPos  = groundPos + Vector3.up * dropHeight;

            GameObject boss = Instantiate(bossPrefab, startPos, Quaternion.identity);
            StartCoroutine(DropBoss(boss.transform, groundPos));
        }

        // 인디케이터 제거 (보스 낙하 코루틴은 boss 오브젝트에서 계속 실행)
        Destroy(gameObject);
    }

    // 원 크기 펄스 (점점 빠르게)
    IEnumerator Pulse()
    {
        Vector3 baseScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < warningDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / warningDuration;        // 0 → 1
            float freq     = Mathf.Lerp(3f, 10f, progress);   // 점점 빠르게
            float scale    = 1f + 0.2f * Mathf.Abs(Mathf.Sin(elapsed * Mathf.PI * freq));
            transform.localScale = baseScale * scale;
            yield return null;
        }
    }

    // 위에서 지면으로 내려찍기 (ease-in = 가속)
    IEnumerator DropBoss(Transform boss, Vector3 targetPos)
    {
        Vector3 startPos = boss.position;
        float elapsed    = 0f;

        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dropDuration;
            boss.position = Vector3.Lerp(startPos, targetPos, t * t); // t² = 가속
            yield return null;
        }
        boss.position = targetPos;
    }

    void Update()
    {
        timer -= Time.deltaTime;
    }

    // 카운트다운 UI
    void OnGUI()
    {
        if (timer <= 0f) return;
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize    = 32;
        style.fontStyle   = FontStyle.Bold;
        style.alignment   = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.red;

        GUI.Label(
            new Rect(0, Screen.height / 2f - 90, Screen.width, 60),
            $"⚠  보스 출현까지  {Mathf.CeilToInt(timer)}  ⚠",
            style
        );
    }
}
