using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("발사 설정")]
    public GameObject projectilePrefab;
    public float fireInterval = 3f;

    [Header("이동 설정")]
    public float speed = 10f;
    public float stopDistance = 5f;   // 플레이어와 이 거리 이하면 정지

    [Header("타겟")]
    public Transform target;

    private float timer;

    void Start()
    {
        timer = fireInterval;

        if (target == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) target = p.transform;
        }
    }

    void Update()
    {
        // 타겟이 없거나 비활성화됐으면 Player 태그로만 재탐색
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            GameObject p = GameObject.FindWithTag("Player");
            target = p != null ? p.transform : null;
        }
        if (target == null) return;

        // 플레이어 방향으로 회전
        Vector3 dir = (target.position - transform.position).normalized;
        dir.y = 0f;
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            Quaternion.LookRotation(dir),
            200f * Time.deltaTime
        );

        // stopDistance보다 멀 때만 이동
        float dist = Vector3.Distance(transform.position, target.position);
        if (dist > stopDistance)
            transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // 발사
        if (projectilePrefab == null) return;
        timer += Time.deltaTime;
        if (timer >= fireInterval)
        {
            timer = 0f;
            Shoot();
        }
    }

    void Shoot()
    {
        
        Vector3 dir = (target.position - transform.position);
        dir.y = 0f;
        dir.Normalize();

        Instantiate(projectilePrefab, transform.position, Quaternion.LookRotation(dir));
    }
}
