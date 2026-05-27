using UnityEngine;

/// <summary>
/// 날아오는 장애물 본체.
/// ObstacleSpawner가 방향을 설정해줌.
/// </summary>
public class FlyingObstacle : MonoBehaviour
{
    [Header("이동")]
    public float speed       = 15f;
    public float lifetime    = 6f;   // 이 시간 지나면 자동 제거

    [Header("데미지")]
    public int   damage      = 1;
    public bool  destroyOnHit = false; // 플레이어에 맞으면 사라질지

    private Vector3 direction;

    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
        // 날아가는 방향으로 회전
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var ph = other.GetComponentInParent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(damage);

            if (destroyOnHit) Destroy(gameObject);
        }
    }
}
