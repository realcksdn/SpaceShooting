using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public Animator HitUI;
    public Animator stage3Camera;
    [Header("HP")]
    public int maxHp = 5;
    public Image hpBar;

    public ParticleSystem Particle;

    [Header("타격 이펙트")]
    public GameObject hitEffectPrefab;
    public float hitEffectDuration = 0.1f;

    private int hp;
    private int  invincibleStack = 0;          // 여러 소스가 동시에 무적 줘도 안 꺼짐
    private bool isInvincible => invincibleStack > 0;
    private bool hasDefenseBoost = false;

    void Start()
    {
        hp = maxHp;
        UpdateHpBar();
        Particle = GetComponentInChildren<ParticleSystem>();
    }

    public void TakeDamage(int amount)
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;
        if (isInvincible) return;

        int damage = hasDefenseBoost ? Mathf.Max(1, amount / 2) : amount;
        hp -= damage;
        UpdateHpBar();
        SpawnHitEffect();
        if (Particle != null) Particle.Play();
        if (HitUI != null) HitUI.SetTrigger("Hit");
        VisionEffect.Instance?.TriggerVision();
        Camera.main?.GetComponent<ShakeCamera>()?.SetUp(0.1f, 3f);
        if (hp <= 0) Die();
    }

    public void Heal(int amount)
    {
        hp = Mathf.Min(hp + amount, maxHp);
        UpdateHpBar();
    }

    // 외부에서 무적 on/off (대쉬, 치트 등) — 스택 방식으로 서로 안 꺼짐
    public void SetInvincible(bool value)
    {
        invincibleStack += value ? 1 : -1;
        if (invincibleStack < 0) invincibleStack = 0;
    }

    // 무적 적용
    public void ApplyInvincibility(float duration)
    {
        StartCoroutine(InvincibilityRoutine(duration));
    }

    IEnumerator InvincibilityRoutine(float duration)
    {
        invincibleStack++;
        yield return new WaitForSeconds(duration);
        invincibleStack = Mathf.Max(0, invincibleStack - 1);
    }

    // 방어력 증가 적용 (피해 절반)
    public void ApplyDefenseBoost(float duration)
    {
        StartCoroutine(DefenseBoostRoutine(duration));
    }

    IEnumerator DefenseBoostRoutine(float duration)
    {
        hasDefenseBoost = true;
        yield return new WaitForSeconds(duration);
        hasDefenseBoost = false;
    }

    void UpdateHpBar()
    {
        if (hpBar != null)
            hpBar.fillAmount = (float)hp / maxHp;
    }

    void SpawnHitEffect()
    {
        if (hitEffectPrefab == null) return;
        GameObject fx = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        Destroy(fx, hitEffectDuration);
    }

    void Die()
    {
        GameManager.Instance.GameOver();
        gameObject.SetActive(false);
    }
}
