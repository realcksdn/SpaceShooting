using UnityEngine;
using UnityEngine.UI;
public class PlayerReflect : MonoBehaviour
{
    [Header("반사 설정")]
    public float reflectRadius = 7f; // 반사 가능 범위
    public float cooldown = 1f;      // 쿨타임 (초)
    public Image cooldownImage;
    private float timer = 0f;

    void Update()
    {
        if (GameManager.Instance.isGameOver) return;

        timer += Time.deltaTime;

        // Space 키로 반사 발동
        if (Input.GetKeyDown(KeyCode.Space) && timer >= cooldown)
        {
            timer = 0f;
            ReflectBullets();
        }

        if (cooldownImage != null)
            cooldownImage.fillAmount = timer / cooldown; // 쿨타임 전용아ㅣㅁ
    }

    /// <summary>스킬로 외부에서 직접 호출</summary>
    public void ReflectAll() => ReflectBullets();

    void ReflectBullets()
    {
        // 가장 가까운 적 찾기 (반사 타겟)
        Transform nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null) return;

        // 반사 범위 안의 탄환 모두 반사
        Collider[] cols = Physics.OverlapSphere(transform.position, reflectRadius);
        foreach (Collider col in cols)
        {
            // GetComponentInParent: Collider가 자식에 있어도 부모까지 탐색
            Bullet bullet = col.GetComponentInParent<Bullet>();
            if (bullet != null)
                bullet.Reflect(nearestEnemy);
        }
    }

    Transform FindNearestEnemy()
    {
        // "Enemy" 태그를 가진 오브젝트 중 가장 가까운 것
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject e in enemies)
        {
            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = e.transform;
            }
        }

        return nearest;
    }

    // 씬 뷰에서 반사 범위 시각화 (개발용)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, reflectRadius);
    }
}
