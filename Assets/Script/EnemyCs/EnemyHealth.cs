using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("HP")]
    public int maxHp = 1;

    [Header("점수")]
    public int scoreValue = 100;

    [Header("드롭 아이템 (Inspector에서 직접 입력)")]
    [Range(0f, 1f)]
    public float lootDropChance = 0.5f;   // 0~1 확률 (1 = 항상 드롭)
    public string lootName      = "고철"; // 가방에 표시될 이름
    public int    lootSellValue = 10;     // 판매 코인
    public int    lootWeight    = 1;      // 무게 (0이면 표시 안됨)

    [Header("타격 이펙트")]
    public GameObject hitEffectPrefab;
    public float      hitEffectDuration = 0.3f;

    [Header("사망 이펙트")]
    public GameObject deathEffectPrefab;
    public float      deathEffectDuration = 1.5f;

    private int hp;

    void Start() => hp = maxHp;

    public void TakeDamage(int amount)
    {
        hp -= amount;
        SpawnHitEffect();
        if (hp <= 0) Die();
    }

    void SpawnHitEffect()
    {
        if (hitEffectPrefab == null) return;
        var fx = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        Destroy(fx, hitEffectDuration);
    }

    void Die()
    {
        GameManager.Instance?.AddScore(scoreValue);

        // 확률에 따라 가방에 직접 추가 (프리팹 불필요)
        if (!string.IsNullOrEmpty(lootName) && Random.value <= lootDropChance)
        {
            var entry = new LootEntry
            {
                lootName  = lootName,
                sellValue = lootSellValue,
                weight    = lootWeight
            };

            bool added = InventoryManager.Instance != null
                         && InventoryManager.Instance.AddLoot(entry);

            if (added)
            {
                BagUI.Instance?.Refresh(); // 가방이 열려있으면 즉시 갱신
                Debug.Log($"[드롭] {lootName} 가방에 추가됨");
            }
            else
            {
                Debug.Log("[드롭] 가방이 가득 차거나 InventoryManager 없음");
            }
        }

        Camera.main?.GetComponent<ShakeCamera>()?.SetUp(0.4f, 0.8f);
        SpawnDeathEffect();
        Destroy(gameObject);
    }

    void SpawnDeathEffect()
    {
        if (deathEffectPrefab == null) return;
        var fx = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        Destroy(fx, deathEffectDuration);
    }
}
