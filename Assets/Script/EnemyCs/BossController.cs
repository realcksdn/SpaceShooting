using System.Collections;
using UnityEngine;

/// <summary>
/// 보스 이동 + 3페이즈 공격패턴
/// BossHealth와 같은 오브젝트에 부착
/// </summary>
public class BossController : MonoBehaviour
{
    [Header("이동")]
    public float moveSpeed    = 4f;
    public float stopDistance = 5f;

    [Header("탄환 프리팹")]
    public GameObject bulletPrefab;         // 기존 (추적탄)
    public GameObject straightBulletPrefab; // 직선탄

    // ── 내부 상태 ──────────────────────────────────
    private BossHealth health;
    private Transform  target;
    private int        currentPhase = 0;
    private bool       isDropping   = true; // 낙하 연출 중에는 공격/이동 정지

    void Start()
    {
        health = GetComponent<BossHealth>();
        FindPlayer();

        // BossSpawnIndicator의 낙하 연출(0.2~0.4s) 이후 활성화
        StartCoroutine(ActivateAfterDrop());
    }

    IEnumerator ActivateAfterDrop()
    {
        yield return new WaitForSeconds(0.6f); // 낙하 완료 대기
        Camera.main?.GetComponent<ShakeCamera>()?.SetUp(0.4f, 0.8f);
        isDropping = false;
        currentPhase = 1;
        StartCoroutine(AttackRoutine());
    }

    void Update()
    {
        if (isDropping) return;
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        if (target == null || !target.gameObject.activeInHierarchy)
            FindPlayer();
        if (target == null) return;

        // ── 이동 ──────────────────────────────────
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;

        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(dir),
                150f * Time.deltaTime
            );
        }

        if (Vector3.Distance(transform.position, target.position) > stopDistance)
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        // ── 페이즈 갱신 (스테이지별) ──────────────
        if (health != null)
        {
            float pct = health.HpPercent;
            switch (StageIndex)
            {
                case 0: // Stage1: 페이즈 1만
                    currentPhase = 1;
                    break;
                case 1: // Stage2: 페이즈 1~2
                    currentPhase = pct > 0.5f ? 1 : 2;
                    break;
                default: // Stage3: 페이즈 1~3
                    currentPhase = pct > 0.6f ? 1 : pct > 0.3f ? 2 : 3;
                    break;
            }
        }
    }

    void FindPlayer()
    {
        var p = GameObject.FindWithTag("Player");
        target = p != null ? p.transform : null;
    }

    // ══════════════════════════════════════════════
    //  공격 루틴
    // ══════════════════════════════════════════════
    IEnumerator AttackRoutine()
    {
        int idx = 0;
        while (true)
        {
            if (GameManager.Instance != null && GameManager.Instance.isGameOver) yield break;
            if (target == null) { yield return null; continue; }

            float cooldown;
            switch (currentPhase)
            {
                case 1:
                    yield return StartCoroutine(Phase1Pattern(idx % 2));
                    cooldown = 1.8f;
                    break;
                case 2:
                    yield return StartCoroutine(Phase2Pattern(idx % 4));
                    cooldown = 1.3f;
                    break;
                default: // 3
                    yield return StartCoroutine(Phase3Pattern(idx % 4));
                    cooldown = 0.8f;
                    break;
            }
            idx++;
            yield return new WaitForSeconds(cooldown);
        }
    }

    int StageIndex => GameManager.Instance != null ? GameManager.Instance.currentStage : 0;

    // ── Phase 1 (HP 60~100%) ──────────────────────
    IEnumerator Phase1Pattern(int idx)
    {
        switch (StageIndex)
        {
            case 0: // Stage1 - 쉬움
                switch (idx % 2)
                {
                    case 0: yield return StartCoroutine(CircleShot(6));        break;
                    case 1: yield return StartCoroutine(AimedBurst(2, 0.25f)); break;
                }
                break;
            case 1: // Stage2 - 보통
                switch (idx % 3)
                {
                    case 0: yield return StartCoroutine(CircleShot(8));        break;
                    case 1: yield return StartCoroutine(AimedBurst(3, 0.2f));  break;
                    case 2: yield return StartCoroutine(FanShot(5, 50f));      break;
                }
                break;
            default: // Stage3 - 어려움
                switch (idx % 4)
                {
                    case 0: yield return StartCoroutine(CircleShot(10));       break;
                    case 1: yield return StartCoroutine(AimedBurst(4, 0.15f)); break;
                    case 2: yield return StartCoroutine(FanShot(6, 60f));      break;
                    case 3: yield return StartCoroutine(RingWave(2, 8));       break;
                }
                break;
        }
    }

    // ── Phase 2 (HP 30~60%) ──────────────────────
    IEnumerator Phase2Pattern(int idx)
    {
        switch (StageIndex)
        {
            case 0: // Stage1
                switch (idx % 3)
                {
                    case 0: yield return StartCoroutine(CircleShot(8));        break;
                    case 1: yield return StartCoroutine(AimedBurst(3, 0.18f)); break;
                    case 2: yield return StartCoroutine(FanShot(5, 60f));      break;
                }
                break;
            case 1: // Stage2
                switch (idx % 4)
                {
                    case 0: yield return StartCoroutine(CircleShot(12));       break;
                    case 1: yield return StartCoroutine(SpiralDual(14, 22f));  break;
                    case 2: yield return StartCoroutine(AimedBurst(4, 0.14f)); break;
                    case 3: yield return StartCoroutine(CrossShot());          break;
                }
                break;
            default: // Stage3
                switch (idx % 6)
                {
                    case 0: yield return StartCoroutine(CircleShot(14));        break;
                    case 1: yield return StartCoroutine(SpiralDual(18, 20f));   break;
                    case 2: yield return StartCoroutine(AimedBurst(5, 0.11f));  break;
                    case 3: yield return StartCoroutine(CrossShot());           break;
                    case 4: yield return StartCoroutine(RotatingCross(3, 15f)); break;
                    case 5:
                        StartCoroutine(CircleShot(8));
                        yield return StartCoroutine(FanShot(7, 80f));
                        break;
                }
                break;
        }
    }

    // ── Phase 3 Enrage (HP 0~30%) ─────────────────
    IEnumerator Phase3Pattern(int idx)
    {
        switch (StageIndex)
        {
            case 0: // Stage1
                switch (idx % 3)
                {
                    case 0: yield return StartCoroutine(CircleShot(10));      break;
                    case 1: yield return StartCoroutine(SpiralDual(16, 22f)); break;
                    case 2: yield return StartCoroutine(SweepShot(7, 60f));   break;
                }
                break;
            case 1: // Stage2
                switch (idx % 4)
                {
                    case 0:
                        StartCoroutine(CircleShot(12));
                        yield return StartCoroutine(AimedBurst(3, 0.1f));
                        break;
                    case 1: yield return StartCoroutine(SpiralDual(22, 16f)); break;
                    case 2: yield return StartCoroutine(SweepShot(8, 65f));   break;
                    case 3: yield return StartCoroutine(AllAround(14));       break;
                }
                break;
            default: // Stage3
                switch (idx % 6)
                {
                    case 0:
                        StartCoroutine(CircleShot(16));
                        yield return StartCoroutine(AimedBurst(4, 0.07f));
                        break;
                    case 1: yield return StartCoroutine(SpiralDual(28, 13f)); break;
                    case 2: yield return StartCoroutine(SweepShot(9, 70f));   break;
                    case 3:
                        StartCoroutine(StarBurst());
                        yield return StartCoroutine(CircleShot(8));
                        break;
                    case 4: yield return StartCoroutine(AllAround(20));       break;
                    case 5:
                        StartCoroutine(SniperBurst(3));
                        yield return StartCoroutine(RingWave(3, 12));
                        break;
                }
                break;
        }
    }

    // ══════════════════════════════════════════════
    //  패턴 구현
    // ══════════════════════════════════════════════

    /// <summary>360도 균등 원형 발사</summary>
    IEnumerator CircleShot(int count)
    {
        Camera.main?.GetComponent<ShakeCamera>()?.SetUp(0.4f, 0.8f);
        float step = 360f / count;
        for (int i = 0; i < count; i++)
        {
            Vector3 dir = Quaternion.Euler(0, step * i, 0) * Vector3.forward;
            SpawnBullet(dir);
        }
        yield break;
    }

    /// <summary>플레이어 방향으로 count발 연사</summary>
    IEnumerator AimedBurst(int count, float interval)
    {
        Camera.main?.GetComponent<ShakeCamera>()?.SetUp(0.4f, 0.8f);
        for (int i = 0; i < count; i++)
        {
            if (target != null) SpawnBullet(AimDir());
            yield return new WaitForSeconds(interval);
        }
    }

    /// <summary>시계/반시계 양방향 나선</summary>
    IEnumerator SpiralDual(int count, float angleStep)
    {
        Camera.main?.GetComponent<ShakeCamera>()?.SetUp(0.4f, 0.8f);
        float angle = 0f;
        for (int i = 0; i < count; i++)
        {
            SpawnBullet(Quaternion.Euler(0,  angle, 0) * Vector3.forward);
            SpawnBullet(Quaternion.Euler(0, -angle, 0) * Vector3.forward);
            angle += angleStep;
            yield return new WaitForSeconds(0.055f);
        }
    }

    /// <summary>플레이어 방향 중심으로 부채꼴 스윕</summary>
    IEnumerator SweepShot(int count, float totalAngle)
    {
        Camera.main?.GetComponent<ShakeCamera>()?.SetUp(0.4f, 0.8f);
        Vector3 center = AimDir();
        float half = totalAngle / 2f;
        float step = count <= 1 ? 0f : totalAngle / (count - 1);
        for (int i = 0; i < count; i++)
        {
            float a = -half + step * i;
            SpawnBullet(Quaternion.Euler(0, a, 0) * center);
            yield return new WaitForSeconds(0.04f);
        }
    }

    /// <summary>직선탄 십자(+) 4방향 동시 발사</summary>
    IEnumerator CrossShot()
    {
        Camera.main?.GetComponent<ShakeCamera>()?.SetUp(0.4f, 0.8f);
        float[] angles = { 0f, 90f, 180f, 270f };
        foreach (float a in angles)
        {
            Vector3 dir = Quaternion.Euler(0, a, 0) * Vector3.forward;
            SpawnBullet(dir, true);
        }
        // 잠깐 후 45도 회전 십자(×) 추가 발사
        yield return new WaitForSeconds(0.3f);
        float[] angles2 = { 45f, 135f, 225f, 315f };
        foreach (float a in angles2)
        {
            Vector3 dir = Quaternion.Euler(0, a, 0) * Vector3.forward;
            SpawnBullet(dir, true);
        }
    }

    /// <summary>직선탄 8방향 별 모양 발사</summary>
    IEnumerator StarBurst()
    {
        Camera.main?.GetComponent<ShakeCamera>()?.SetUp(0.4f, 0.8f);
        int count = 8;
        float step = 360f / count;
        for (int i = 0; i < count; i++)
        {
            Vector3 dir = Quaternion.Euler(0, step * i, 0) * Vector3.forward;
            SpawnBullet(dir, true);
        }
        yield break;
    }

    /// <summary>플레이어 방향 중심 부채꼴 즉시 발사</summary>
    IEnumerator FanShot(int count, float totalAngle)
    {
        Camera.main?.GetComponent<ShakeCamera>()?.SetUp(0.4f, 0.8f);
        Vector3 center = AimDir();
        float half = totalAngle / 2f;
        float step = count <= 1 ? 0f : totalAngle / (count - 1);
        for (int i = 0; i < count; i++)
        {
            float a = -half + step * i;
            SpawnBullet(Quaternion.Euler(0, a, 0) * center);
        }
        yield break;
    }

    /// <summary>원형 탄막을 waveCount번 연속으로 발사</summary>
    IEnumerator RingWave(int waveCount, int bulletsPerWave)
    {
        Camera.main?.GetComponent<ShakeCamera>()?.SetUp(0.4f, 0.8f);
        for (int w = 0; w < waveCount; w++)
        {
            yield return StartCoroutine(CircleShot(bulletsPerWave));
            yield return new WaitForSeconds(0.4f);
        }
    }

    /// <summary>직선탄 십자가 angleStep씩 회전하며 count번 발사</summary>
    IEnumerator RotatingCross(int count, float angleStep)
    {
        Camera.main?.GetComponent<ShakeCamera>()?.SetUp(0.4f, 0.8f);
        float baseAngle = 0f;
        for (int i = 0; i < count; i++)
        {
            float[] offsets = { 0f, 90f, 180f, 270f };
            foreach (float offset in offsets)
            {
                Vector3 dir = Quaternion.Euler(0, baseAngle + offset, 0) * Vector3.forward;
                SpawnBullet(dir, true);
            }
            baseAngle += angleStep;
            yield return new WaitForSeconds(0.25f);
        }
    }

    /// <summary>랜덤 방향으로 count발 동시 폭발</summary>
    IEnumerator AllAround(int count)
    {
        Camera.main?.GetComponent<ShakeCamera>()?.SetUp(0.4f, 0.8f);
        for (int i = 0; i < count; i++)
        {
            float angle = Random.Range(0f, 360f);
            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            // 홀수는 추적탄, 짝수는 직선탄으로 섞음
            if (i % 2 == 0) SpawnBullet(dir);
            else             SpawnBullet(dir, true);
        }
        yield break;
    }

    /// <summary>플레이어 정확히 조준 → count발 빠르게 연사 후 원형 폭발</summary>
    IEnumerator SniperBurst(int count)
    {
        Camera.main?.GetComponent<ShakeCamera>()?.SetUp(0.4f, 0.8f);
        for (int i = 0; i < count; i++)
        {
            if (target != null) SpawnBullet(AimDir());
            yield return new WaitForSeconds(0.08f);
        }
        yield return new WaitForSeconds(0.15f);
        yield return StartCoroutine(CircleShot(10));
    }

    // ── 공통 유틸 ─────────────────────────────────
    Vector3 AimDir()
    {
        if (target == null) return transform.forward;
        Vector3 d = target.position - transform.position;
        d.y = 0f;
        return d == Vector3.zero ? transform.forward : d.normalized;
    }

    // straight=true 이면 직선탄 프리팹 사용
    void SpawnBullet(Vector3 dir, bool straight = false)
    {
        Camera.main?.GetComponent<ShakeCamera>()?.SetUp(0.4f, 0.8f);
        if (dir == Vector3.zero) return;
        GameObject prefab = (straight && straightBulletPrefab != null) ? straightBulletPrefab : bulletPrefab;
        if (prefab == null) return;

        Vector3 pos = transform.position + dir.normalized * 1.5f;
        pos.y = transform.position.y;
        var obj = Instantiate(prefab, pos, Quaternion.LookRotation(dir));

        var b = obj.GetComponent<Bullet>();
        if (b != null) { b.isStraight = straight; b.isBossBullet = true; }

        var bc = GetComponent<Collider>();
        var oc = obj.GetComponent<Collider>();
        if (bc != null && oc != null) Physics.IgnoreCollision(bc, oc);
    }

    // ── 페이즈 표시 (HUD) ─────────────────────────
    void OnGUI()
    {
        if (isDropping || currentPhase == 0) return;
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 14;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = currentPhase == 3 ? Color.red
                               : currentPhase == 2 ? Color.yellow
                               : Color.white;

        string label = currentPhase == 3 ? "PHASE 3  [ENRAGE]"
                     : currentPhase == 2 ? "PHASE 2"
                     : "PHASE 1";

        GUI.Label(new Rect(Screen.width / 2f - 60, 50, 120, 24), label, style);
    }
}
