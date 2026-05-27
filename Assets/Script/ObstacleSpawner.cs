using UnityEngine;

/// <summary>
/// 맵 외곽에서 장애물을 날려보내는 스포너.
/// 빈 오브젝트에 붙이고 spawnPoints 배열에
/// 맵 외곽 위치 오브젝트들을 드래그해서 연결.
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    [Header("장애물 프리팹")]
    public GameObject[] obstaclePrefabs; // 여러 종류 랜덤 스폰 가능

    [Header("스폰 포인트 (맵 외곽 오브젝트들)")]
    public Transform[] spawnPoints;      // 외곽 위치 지정

    [Header("타겟")]
    public Transform target;             // 보통 플레이어 (비워두면 자동 탐색)

    [Header("스폰 간격")]
    public float spawnInterval  = 2f;    // 몇 초마다 스폰
    public float intervalMin    = 0.5f;  // 랜덤 간격 최솟값
    public float intervalMax    = 3f;    // 랜덤 간격 최댓값
    public bool  useRandomInterval = true;

    [Header("방향 흔들림")]
    [Range(0f, 45f)]
    public float spreadAngle = 15f;      // 플레이어 방향에서 ±각도 랜덤

    private float timer;

    void Start()
    {
        timer = spawnInterval;

        if (target == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null) target = player.transform;
        }
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Spawn();
            timer = useRandomInterval
                ? Random.Range(intervalMin, intervalMax)
                : spawnInterval;
        }
    }

    void Spawn()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;
        if (spawnPoints     == null || spawnPoints.Length     == 0) return;

        // 랜덤 스폰 포인트
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // 랜덤 프리팹
        GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        if (prefab == null) return;

        GameObject obj = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        var obstacle = obj.GetComponent<FlyingObstacle>();
        if (obstacle == null) return;

        // 방향 계산: 타겟(플레이어) 쪽 + 랜덤 흔들림
        Vector3 dir;
        if (target != null)
        {
            dir = (target.position - spawnPoint.position).normalized;

            // spreadAngle 만큼 랜덤으로 방향 틀기
            float yaw   = Random.Range(-spreadAngle, spreadAngle);
            float pitch = Random.Range(-spreadAngle * 0.3f, spreadAngle * 0.3f);
            dir = Quaternion.Euler(pitch, yaw, 0) * dir;
        }
        else
        {
            // 타겟 없으면 스폰 포인트의 forward 방향
            dir = spawnPoint.forward;
        }

        obstacle.SetDirection(dir);
    }

    // 에디터에서 스폰 포인트 위치 표시
    void OnDrawGizmos()
    {
        if (spawnPoints == null) return;
        Gizmos.color = Color.red;
        foreach (var sp in spawnPoints)
        {
            if (sp == null) continue;
            Gizmos.DrawWireSphere(sp.position, 0.5f);
            Gizmos.DrawLine(sp.position, sp.position + sp.forward * 2f);
        }
    }
}
