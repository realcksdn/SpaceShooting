using UnityEngine;

public enum AttackPattern  { Single, Spread, Circle, Burst, Spiral }
public enum MovementType   { Chase, Zigzag, Charge, Strafe }

// 우클릭 → Create → Enemy → EnemyData 로 새 적 타입 생성
[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("기본 정보")]
    public string enemyName = "적";

    [Header("HP & 점수")]
    public int maxHp      = 1;
    public int scoreValue = 100;

    // ─────────────────────────────────────────────
    // 외형 (딱 보면 다르다)
    // ─────────────────────────────────────────────
    [Header("외형")]
    public Color  bodyColor       = Color.white; // 몸체 색상
    public float  scaleMultiplier = 1f;          // 크기 배율 (탱커 2f, 소형 0.6f 등)

    // ─────────────────────────────────────────────
    // 이동
    // ─────────────────────────────────────────────
    [Header("이동 타입")]
    public MovementType movementType = MovementType.Chase;

    [Header("이동 - 공통")]
    public float moveSpeed     = 4f;
    public float stopDistance  = 3f;  // 유지할 거리 (이 거리 안으로 들어오면 후퇴)
    public float retreatSpeed  = 3f;  // 후퇴 속도 (0이면 후퇴 안 함)

    [Header("이동 - Zigzag (좌우 흔들림)")]
    public float zigzagFrequency  = 2f;   // 흔들림 속도
    public float zigzagAmplitude  = 3f;   // 흔들림 폭

    [Header("이동 - Charge (돌진 후 멈춤)")]
    public float chargeSpeed      = 18f;  // 돌진 속도
    public float chargeWindup     = 1.2f; // 돌진 전 예비동작 시간
    public float chargePausetime  = 0.8f; // 돌진 후 정지 시간

    [Header("이동 - Strafe (선회)")]
    public float strafeRadius     = 5f;   // 선회 반경
    public float strafeSpeed      = 120f; // 초당 선회 각도

    // ─────────────────────────────────────────────
    // 공격
    // ─────────────────────────────────────────────
    [Header("공격 사거리")]
    public float attackRange = 10f;  // 이 거리 안에서만 공격 (≥ stopDistance 권장)

    [Header("발사 - 공통")]
    public GameObject projectilePrefab;
    public float fireInterval = 3f;

    [Header("공격 패턴")]
    public AttackPattern attackPattern = AttackPattern.Single;

    [Tooltip("Spread: 탄 수 / Circle: 탄 수 / Burst: 연사 수")]
    public int   bulletCount     = 3;

    [Tooltip("Spread 전체 퍼짐 각도 (예: 60 → 좌우 각 30도)")]
    public float spreadAngle     = 60f;

    [Tooltip("Burst 연사 간격 (초)")]
    public float burstInterval   = 0.1f;

    [Tooltip("Spiral 발사마다 회전하는 각도")]
    public float spiralAngleStep = 30f;

    // ─────────────────────────────────────────────
    // 드롭
    // ─────────────────────────────────────────────
    [Header("드롭")]
    public GameObject lootPrefab;
    [Range(0f, 1f)] public float lootDropChance = 0.3f;
    public GameObject[] itemPrefabs;
    [Range(0f, 1f)] public float itemDropChance = 0.3f;

    [Header("타격 이펙트")]
    public GameObject hitEffectPrefab;
    public float hitEffectDuration = 0.1f;

    [Header("사망 이펙트")]
    public GameObject deathEffectPrefab;
    public float deathEffectDuration = 1.5f;
}
