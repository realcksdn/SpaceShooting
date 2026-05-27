using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDash : MonoBehaviour
{
    [Header("대쉬 설정")]
    public float dashSpeed    = 60f;
    public float dashDuration = 0.15f;
    public float cooldown     = 2f;

    [Header("쿨타임 이미지")]
    public Image cooldownImage;

    [Header("몸통박치기 (보스 전용)")]
    public int slamDamage = 2;
    public GameObject slamEffectPrefab;
    public float slamEffectDuration = 0.3f;

    private bool isDashing   = false;
    private bool hitBossDash = false; // 대쉬 1회당 1번만 타격
    private float timer;

    private PlayerMovement movement;
    private PlayerHealth   health;
    private Rigidbody      rb;
    private TrailRenderer  trail;
    private GhostTrail     ghost;

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
        health   = GetComponent<PlayerHealth>();
        rb       = GetComponent<Rigidbody>();
        trail    = GetComponent<TrailRenderer>();
        ghost    = GetComponent<GhostTrail>();
        timer    = cooldown;
        if (trail != null) trail.emitting = false;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        timer += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.E) && !isDashing && timer >= cooldown)
        {
            timer = 0f;
            StartCoroutine(DashRoutine());
        }

        if (cooldownImage != null)
            cooldownImage.fillAmount = Mathf.Clamp01(timer / cooldown);
    }

    // 대쉬 중 보스 충돌 → 몸통박치기 (kinematic 상태에서도 OnTriggerEnter 정상 작동)
    void OnTriggerEnter(Collider other)
    {
        if (!isDashing || hitBossDash) return;
        var boss = other.GetComponentInParent<BossHealth>();
        if (boss == null) return;

        hitBossDash = true;
        boss.TakeDamage(slamDamage);
        if (slamEffectPrefab != null)
            Destroy(Instantiate(slamEffectPrefab, transform.position, Quaternion.identity), slamEffectDuration);
    }

    IEnumerator DashRoutine()
    {
        isDashing    = true;
        hitBossDash  = false;

        Vector3 dashDir = transform.forward;
        dashDir.y = 0f;
        if (dashDir == Vector3.zero) dashDir = Vector3.forward;

        if (movement != null) movement.enabled = false;
        if (health   != null) health.SetInvincible(true);
        if (trail    != null) trail.emitting = true;
        ghost?.StartTrail();

        // Kinematic으로 전환 → 적 콜라이더에 막히지 않음
        bool prevKinematic = false;
        if (rb != null) { prevKinematic = rb.isKinematic; rb.isKinematic = true; }

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            transform.Translate(dashDir * dashSpeed * Time.deltaTime, Space.World);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (rb       != null) rb.isKinematic = prevKinematic;
        if (movement != null) movement.enabled = true;
        if (health   != null) health.SetInvincible(false);
        if (trail    != null) trail.emitting = false;
        ghost?.StopTrail();

        isDashing = false;
    }
}
