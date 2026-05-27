using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("적 프리팹 목록")]
    public GameObject[] enemyPrefabs;

    [Header("스폰 설정")]
    public float spawnInterval = 3f;
    public int   maxEnemies   = 10;

    [Header("스폰 포인트 (씬에 오브젝트로 배치)")]
    [Tooltip("스폰 위치로 사용할 오브젝트들을 여기에 드래그")]
    public Transform[] spawnPoints;

    private float timer;

    void Update()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;
        if (spawnPoints  == null || spawnPoints.Length  == 0) return;
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        if (GameObject.FindGameObjectsWithTag("Enemy").Length >= maxEnemies) return;

        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Transform  point  = spawnPoints[Random.Range(0, spawnPoints.Length)];
        if (prefab == null || point == null) return;

        Instantiate(prefab, point.position, Quaternion.identity);
        Camera.main?.GetComponent<ShakeCamera>()?.SetUp(0.25f, 0.1f);
    }

    // 스폰 포인트 위치 시각화
    void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;
        Gizmos.color = Color.cyan;
        foreach (var p in spawnPoints)
        {
            if (p == null) continue;
            Gizmos.DrawWireSphere(p.position, 0.5f);
            Gizmos.DrawLine(p.position, p.position + Vector3.up * 2f);
        }
    }
}
