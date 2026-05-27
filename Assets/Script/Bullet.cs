using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("기본 설정")]
    public float speed = 10f;
    public float lifetime = 30f;
    public float rotateSpeed = 60f;   // 호밍 꺾임 강도
    public float collisionDelay = 1f; // 생성 직후 충돌 무시 시간
    public Transform target;

    [Header("모드")]
    public bool isStraight = false;   // true = 직진, false = 호밍(추적)
    [HideInInspector] public bool isBossBullet = false; // 보스가 쏜 탄 여부

    [Header("피격 이펙트")]
    public GameObject hitEffectPrefab;
    public float hitEffectDuration = 0.1f;

    bool canHit = false;      // collisionDelay 전까지 충돌 무시
    bool isReflected = false; // 반사된 탄 여부

    // ────────────────────────────────────────
    void Start()
    {
        // 타겟 미지정 시 플레이어 자동 탐색
        if (target == null)
            target = GameObject.FindWithTag("Player")?.transform;

        Destroy(gameObject, lifetime);
        Invoke(nameof(EnableCollision), collisionDelay);
    }

    void EnableCollision() => canHit = true;

    // ────────────────────────────────────────
    void Update()
    {
        // 호밍 모드 : 매 프레임 타겟 방향으로 조금씩 꺾음
        if (!isStraight && target != null)
        {
            Vector3 dir = (target.position - transform.position).With(y: 0f);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, Quaternion.LookRotation(dir),
                rotateSpeed * Time.deltaTime);
        }

        // 항상 앞 방향으로 직진
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    // ────────────────────────────────────────
    // PlayerReflect에서 호출 — 타겟을 적으로 바꾸고 즉시 그 방향으로 회전
    public void Reflect(Transform newTarget)
    {
        target = newTarget;
        isReflected = true;
        canHit = true;

        if (newTarget != null)
        {
            Vector3 dir = (newTarget.position - transform.position).With(y: 0f);
            if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    // ────────────────────────────────────────
    void OnTriggerEnter(Collider other)  => HandleHit(other);
    void OnCollisionEnter(Collision col) => HandleHit(col.collider);

    void HandleHit(Collider other)
    {
        if (!canHit) return;
        if (other.GetComponentInParent<Item>()     != null) return; // 아이템 통과
        if (other.GetComponentInParent<LootItem>() != null) return; // 루트 통과

        // 일반 적
        var enemy = other.GetComponentInParent<EnemyController>();
        if (enemy  != null) { enemy.TakeDamage(1);  Hit(); return; }

        // 단순 적 (EnemyShooter 계열)
        var legacy = other.GetComponentInParent<EnemyHealth>();
        if (legacy != null) { legacy.TakeDamage(1); Hit(); return; }

        // 보스 — 보스 본인 탄은 반사됐을 때만 피해
        var boss = other.GetComponentInParent<BossHealth>();
        if (boss   != null)
        {
            if (!isBossBullet || isReflected) boss.TakeDamage(1);
            Hit(); return;
        }

        // 플레이어 — 반사탄은 플레이어에게 피해 없음
        if (!isReflected)
        {
            var player = other.GetComponentInParent<PlayerHealth>();
            if (player != null) { player.TakeDamage(1); Hit(); }
        }
    }

    void Hit()
    {
        if (hitEffectPrefab != null)
            Destroy(Instantiate(hitEffectPrefab, transform.position, Quaternion.identity), hitEffectDuration);
        Destroy(gameObject);
    }
}

// Vector3 확장 — With(y:0f) 처럼 한 축만 바꿀 때 사용
public static class Vector3Ext
{
    public static Vector3 With(this Vector3 v, float? x = null, float? y = null, float? z = null)
        => new Vector3(x ?? v.x, y ?? v.y, z ?? v.z);
}
 