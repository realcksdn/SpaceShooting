using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public EnemyData data;

    private int       hp;
    private float     fireTimer;
    private float     spiralAngle;
    private Transform target;

    // Zigzag용
    private float zigzagTimer;

    // Charge용
    private enum ChargeState { Approaching, Windup, Charging, Pausing }
    private ChargeState chargeState = ChargeState.Approaching;
    private Vector3     chargeDir;
    private float       chargeStateTimer;

    // Strafe용
    private float strafeAngle;
    private int   strafeSign = 1; // 선회 방향

    // ─────────────────────────────────────────────
    void Start()
    {
        hp        = data.maxHp;
        fireTimer = data.fireInterval;
        FindPlayer();
        ApplyVisuals();

        // Strafe 시작 각도 랜덤
        strafeAngle = Random.Range(0f, 360f);
        strafeSign  = Random.value > 0.5f ? 1 : -1;
    }

    void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
            FindPlayer();
        if (target == null) return;

        // 이동
        switch (data.movementType)
        {
            case MovementType.Chase:  MoveChase();  break;
            case MovementType.Zigzag: MoveZigzag(); break;
            case MovementType.Charge: MoveCharge(); break;
            case MovementType.Strafe: MoveStrafe(); break;
        }

        // 공격 (사거리 안에서만)
        if (data.projectilePrefab == null) return;
        float dist = Vector3.Distance(transform.position, target.position);
        if (dist > data.attackRange) return;

        fireTimer += Time.deltaTime;
        if (fireTimer >= data.fireInterval)
        {
            fireTimer = 0f;
            ExecuteAttack();
        }
    }

    // ═══════════════════════════════════════════
    // 이동 패턴
    // ═══════════════════════════════════════════

    // ① Chase: 일정 거리 유지 (너무 가까우면 후퇴)
    void MoveChase()
    {
        Vector3 dir  = ToPlayer();
        float   dist = Vector3.Distance(transform.position, target.position);
        FaceDir(dir);

        if (dist > data.stopDistance)
            transform.Translate(Vector3.forward * data.moveSpeed * Time.deltaTime);
        else if (dist < data.stopDistance * 0.8f && data.retreatSpeed > 0f)
            transform.Translate(-Vector3.forward * data.retreatSpeed * Time.deltaTime);
    }

    // ② Zigzag: 좌우로 흔들리며 거리 유지
    void MoveZigzag()
    {
        zigzagTimer += Time.deltaTime;
        float   dist    = Vector3.Distance(transform.position, target.position);
        Vector3 forward = ToPlayer();
        Vector3 right   = Vector3.Cross(Vector3.up, forward).normalized;
        float   side    = Mathf.Sin(zigzagTimer * data.zigzagFrequency) * data.zigzagAmplitude;
        Vector3 move    = (forward + right * side * Time.deltaTime).normalized;

        FaceDir(forward);
        if (dist > data.stopDistance)
            transform.position += move * data.moveSpeed * Time.deltaTime;
        else if (dist < data.stopDistance * 0.8f && data.retreatSpeed > 0f)
            transform.position -= forward * data.retreatSpeed * Time.deltaTime;
    }

    // ③ Charge: 예비동작 → 돌진 → 정지 반복
    void MoveCharge()
    {
        chargeStateTimer += Time.deltaTime;
        float dist = Vector3.Distance(transform.position, target.position);

        switch (chargeState)
        {
            case ChargeState.Approaching:
                // 사거리 안에 들어오면 예비동작 시작
                FaceDir(ToPlayer());
                if (dist > data.stopDistance * 2.5f)
                    transform.Translate(Vector3.forward * data.moveSpeed * Time.deltaTime);
                else
                    SetChargeState(ChargeState.Windup);
                break;

            case ChargeState.Windup:
                // 잠깐 멈추며 방향 고정 (플레이어가 이 방향 보며 긴장)
                FaceDir(ToPlayer());
                if (chargeStateTimer >= data.chargeWindup)
                {
                    chargeDir = ToPlayer();
                    SetChargeState(ChargeState.Charging);
                }
                break;

            case ChargeState.Charging:
                // 고정 방향으로 빠르게 돌진
                transform.position += chargeDir * data.chargeSpeed * Time.deltaTime;
                if (chargeStateTimer >= 0.4f) // 돌진 지속 시간
                    SetChargeState(ChargeState.Pausing);
                break;

            case ChargeState.Pausing:
                // 돌진 후 잠시 멈춤
                if (chargeStateTimer >= data.chargePausetime)
                    SetChargeState(ChargeState.Approaching);
                break;
        }
    }

    void SetChargeState(ChargeState next)
    {
        chargeState      = next;
        chargeStateTimer = 0f;
    }

    // ④ Strafe: 플레이어 주변을 원형으로 선회하며 거리 유지
    void MoveStrafe()
    {
        strafeAngle += strafeSign * data.strafeSpeed * Time.deltaTime;

        float dist      = Vector3.Distance(transform.position, target.position);
        float minRadius = Mathf.Max(data.strafeRadius, data.stopDistance);
        float targetRadius = dist > minRadius ? dist - data.moveSpeed * Time.deltaTime
                                              : minRadius;

        Vector3 offset   = new Vector3(Mathf.Sin(strafeAngle * Mathf.Deg2Rad), 0f,
                                       Mathf.Cos(strafeAngle * Mathf.Deg2Rad)) * targetRadius;
        Vector3 wantedPos = target.position + offset;
        transform.position = Vector3.MoveTowards(transform.position, wantedPos,
                                                  data.moveSpeed * Time.deltaTime);
        FaceDir(ToPlayer());
    }

    // ═══════════════════════════════════════════
    // 공격 패턴
    // ═══════════════════════════════════════════
    void ExecuteAttack()
    {
        switch (data.attackPattern)
        {
            case AttackPattern.Single: ShootSingle();                 break;
            case AttackPattern.Spread: ShootSpread();                 break;
            case AttackPattern.Circle: ShootCircle();                 break;
            case AttackPattern.Burst:  StartCoroutine(ShootBurst()); break;
            case AttackPattern.Spiral: ShootSpiral();                 break;
        }
    }

    void ShootSingle() => SpawnBullet(ToPlayer());

    void ShootSpread()
    {
        Vector3 center = ToPlayer();
        int     count  = Mathf.Max(1, data.bulletCount);
        float   half   = data.spreadAngle / 2f;
        float   step   = count == 1 ? 0f : data.spreadAngle / (count - 1);
        for (int i = 0; i < count; i++)
            SpawnBullet(Quaternion.Euler(0, -half + step * i, 0) * center);
    }

    void ShootCircle()
    {
        int   count = Mathf.Max(1, data.bulletCount);
        float step  = 360f / count;
        for (int i = 0; i < count; i++)
            SpawnBullet(Quaternion.Euler(0, step * i, 0) * Vector3.forward);
    }

    IEnumerator ShootBurst()
    {
        int count = Mathf.Max(1, data.bulletCount);
        for (int i = 0; i < count; i++)
        {
            if (target != null) SpawnBullet(ToPlayer());
            yield return new WaitForSeconds(data.burstInterval);
        }
    }

    void ShootSpiral()
    {
        SpawnBullet(Quaternion.Euler(0, spiralAngle, 0) * Vector3.forward);
        spiralAngle += data.spiralAngleStep;
    }

    // ═══════════════════════════════════════════
    // 외형 적용
    // ═══════════════════════════════════════════
    void ApplyVisuals()
    {
        // 크기: EnemyData.scaleMultiplier가 1이 아닐 때만 적용, 1이면 프리팹 스케일 유지
        if (!Mathf.Approximately(data.scaleMultiplier, 1f))
            transform.localScale = Vector3.one * data.scaleMultiplier;

        // 색상 (MeshRenderer 또는 SkinnedMeshRenderer)
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            // MaterialPropertyBlock으로 원본 머테리얼을 건드리지 않고 색 변경
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            rend.GetPropertyBlock(block);
            block.SetColor("_Color", data.bodyColor);
            rend.SetPropertyBlock(block);
        }
    }

    // ═══════════════════════════════════════════
    // 유틸
    // ═══════════════════════════════════════════
    void FindPlayer()
    {
        GameObject p = GameObject.FindWithTag("Player");
        target = p != null ? p.transform : null;
    }

    Vector3 ToPlayer()
    {
        if (target == null) return transform.forward;
        Vector3 d = target.position - transform.position;
        d.y = 0f;
        return d == Vector3.zero ? transform.forward : d.normalized;
    }

    void FaceDir(Vector3 dir)
    {
        if (dir == Vector3.zero) return;
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            Quaternion.LookRotation(dir),
            200f * Time.deltaTime);
    }

    void SpawnBullet(Vector3 dir)
    {
        if (dir == Vector3.zero) return;
        Instantiate(data.projectilePrefab, transform.position, Quaternion.LookRotation(dir));
    }

    // ═══════════════════════════════════════════
    // 피격 / 사망
    // ═══════════════════════════════════════════
    public void TakeDamage(int amount)
    {
        hp -= amount;
        SpawnHitEffect();
        if (hp <= 0) Die();
    }

    void SpawnHitEffect()
    {
        if (data == null || data.hitEffectPrefab == null) return;
        GameObject fx = Instantiate(data.hitEffectPrefab, transform.position, Quaternion.identity);
        Destroy(fx, data.hitEffectDuration);
    }

    void Die()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(data.scoreValue);
        if (StageManager.Instance != null)
            StageManager.Instance.OnEnemyKilled();

        Vector3 pos = transform.position;

        if (data.lootPrefab != null && Random.value <= data.lootDropChance)
            Instantiate(data.lootPrefab, pos, Quaternion.identity);

        if (data.itemPrefabs != null && data.itemPrefabs.Length > 0 && Random.value <= data.itemDropChance)
        {
            GameObject pick = data.itemPrefabs[Random.Range(0, data.itemPrefabs.Length)];
            if (pick != null) Instantiate(pick, pos, Quaternion.identity);
        }

        if (data.deathEffectPrefab != null)
        {
            var fx = Instantiate(data.deathEffectPrefab, pos, Quaternion.identity);
            Destroy(fx, data.deathEffectDuration);
        }

        Camera.main?.GetComponent<ShakeCamera>()?.SetUp(0.25f, 0.4f);
        Destroy(gameObject);
    }
}
