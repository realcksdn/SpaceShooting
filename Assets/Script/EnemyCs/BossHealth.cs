using UnityEngine;
using UnityEngine.UI;

public class BossHealth : MonoBehaviour
{
    [Header("HP")]
    public int maxHp = 50;

    [Header("보상")]
    public int scoreValue = 1000;
    public int coinReward = 500;

    [Header("타격 이펙트")]
    public GameObject hitEffectPrefab;
    public float hitEffectDuration = 0.1f;

    [Header("사망 이펙트")]
    public GameObject deathEffectPrefab;
    public float      deathEffectDuration = 2.5f;

    private int   hp;
    private Image hpBar;

    /// <summary>BossController에서 페이즈 계산에 사용</summary>
    public float HpPercent => (float)hp / maxHp;

    void Start()
    {
        hp = maxHp;

        // "BossHpBar" 태그로 Image 탐색 (비활성 포함)
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (!go.scene.isLoaded) continue;
            if (go.CompareTag("BossHpBar"))
            {
                hpBar = go.GetComponent<Image>();
                go.SetActive(true); // 꺼져있어도 켜줌
                break;
            }
        }

        UpdateHpBar();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            TakeDamage(1);
    }

    public void TakeDamage(int amount)
    {
        hp = Mathf.Max(0, hp - amount);
        Camera.main?.GetComponent<ShakeCamera>()?.SetUp(0.8f, 0.3f);
        UpdateHpBar();
        SpawnHitEffect();
        if (hp <= 0) Die();
    }

    void SpawnHitEffect()
    {
        if (hitEffectPrefab == null) return;
        GameObject fx = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        Destroy(fx, hitEffectDuration);
    }

    void UpdateHpBar()
    {
        if (hpBar != null) hpBar.fillAmount = HpPercent;
    }

    [Header("클리어")]
    public GameObject portalPrefab; // 포탈 프리팹

    void Die()
    {
        Camera.main?.GetComponent<ShakeCamera>()?.SetUp(0.8f, 1.5f);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
            GameManager.Instance.AddCoins(coinReward);
        }

        // 모든 총알 제거
        foreach (var bullet in FindObjectsByType<Bullet>(FindObjectsSortMode.None))
            Destroy(bullet.gameObject);

        // 포탈 스폰 (보스 위치 중앙)
        if (portalPrefab != null)
            Instantiate(portalPrefab, transform.position, Quaternion.identity);

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
