using UnityEngine;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [Header("보스 스폰 조건")]
    public int killsToSpawnBoss = 30;
    public GameObject bossIndicatorPrefab; // BossSpawnIndicator 스크립트가 붙은 빨간 원 프리팹

    [Header("보스 스폰 위치")]
    public float bossSpawnDistance = 12f;

    [Header("킬 텍스트 UI")]
    public Text killText;

    private int killCount = 0;
    private bool bossSpawned = false;

    // 보스 출현 메시지 표시용
    private float bossAlertTimer = 0f;
    private const float bossAlertDuration = 3f;

    void Awake()
    {
        Instance = this;
    }

    // EnemyController.Die() 에서 호출
    public void OnEnemyKilled()
    {
        if (bossSpawned) return;

        killCount++;

        if (killText != null)
            killText.text = $"{killCount} / {killsToSpawnBoss}";

        if (killCount >= killsToSpawnBoss)
            SpawnBoss();
    }

    void SpawnBoss()
    {
        if (bossIndicatorPrefab == null) return;

        bossSpawned = true;
        bossAlertTimer = bossAlertDuration;

        // 일반 적 스폰 중단
        EnemySpawner spawner = FindAnyObjectByType<EnemySpawner>();
        if (spawner != null) spawner.enabled = false;

        // 플레이어 앞에 인디케이터(빨간 원) 스폰 → BossSpawnIndicator가 보스를 소환
        GameObject player = GameObject.FindWithTag("Player");
        Vector3 spawnPos = player != null
            ? player.transform.position + player.transform.forward * bossSpawnDistance
            : Vector3.zero;
        spawnPos.y = 0f;

        Instantiate(bossIndicatorPrefab, spawnPos, Quaternion.identity);
        Debug.Log("[스테이지] 보스 인디케이터 출현!");
    }

    void Update()
    {
        if (bossAlertTimer > 0f)
            bossAlertTimer -= Time.deltaTime;

        // ── 치트: F1 키 → 킬 수 최대로 올려 보스 즉시 소환
        if (Input.GetKeyDown(KeyCode.F8) && !bossSpawned)
        {
            killCount = killsToSpawnBoss;
            SpawnBoss();
            Debug.Log("[치트] 보스 즉시 소환");
        }
    }

    void OnGUI()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 15;
        style.normal.textColor = Color.white;

        // 보스 출현 경고 메시지
        if (bossAlertTimer > 0f)
        {
            GUIStyle alertStyle = new GUIStyle(GUI.skin.label);
            alertStyle.fontSize = 36;
            alertStyle.fontStyle = FontStyle.Bold;
            alertStyle.normal.textColor = Color.red;
            alertStyle.alignment = TextAnchor.MiddleCenter;

            GUI.Label(
                new Rect(0, Screen.height / 2f - 40, Screen.width, 80),
                "⚠ 보스 출현 ⚠",
                alertStyle
            );
        }
    }
}
