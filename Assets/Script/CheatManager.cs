using UnityEngine;
using UnityEngine.SceneManagement;

public class CheatManager : MonoBehaviour
{
    [Header("치트 설정")]
    public int coinCheatAmount = 10000;

    [Header("스테이지 씬 이름")]
    public string stage1Scene = "Stage1";
    public string stage2Scene = "Stage2";
    public string stage3Scene = "Stage3";

    // 상태
    private bool isInvincible = false;
    private bool isDebugMode  = false;

    private PlayerHealth playerHealth;

    // 디버그 표시용
    private float fps;
    private float fpsTimer;

    void Awake()
    {
        // 싱글톤 — 씬 이동해도 유지
        var existing = FindObjectsByType<CheatManager>(FindObjectsSortMode.None);
        foreach (var other in existing)
        {
            if (other != this) { Destroy(gameObject); return; }
        }
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // F1: 디버그 모드
        if (Input.GetKeyDown(KeyCode.F1))
            ToggleDebug();

        // F2: 무적 토글
        if (Input.GetKeyDown(KeyCode.F2))
            ToggleInvincible();

        // F3: 적 전체 즉사
        if (Input.GetKeyDown(KeyCode.F3))
            KillAllEnemies();

        // F4: 코인 추가
        if (Input.GetKeyDown(KeyCode.F4))
            AddCoins();

        // F5~F7: 스테이지 이동
        if (Input.GetKeyDown(KeyCode.F5)) LoadStage(stage1Scene);
        if (Input.GetKeyDown(KeyCode.F6)) LoadStage(stage2Scene);
        if (Input.GetKeyDown(KeyCode.F7)) LoadStage(stage3Scene);

        // F9: 현재 스테이지 클리어 (보스 즉사)
        if (Input.GetKeyDown(KeyCode.F9)) ClearStage();

        // FPS 계산
        if (isDebugMode)
        {
            fpsTimer += Time.deltaTime;
            if (fpsTimer >= 0.5f)
            {
                fps = 1f / Time.unscaledDeltaTime;
                fpsTimer = 0f;
            }
        }
    }

    // ── F1: 디버그 모드 ────────────────────────────
    void ToggleDebug()
    {
        isDebugMode = !isDebugMode;
        Debug.Log($"[치트] 디버그 모드 {(isDebugMode ? "ON" : "OFF")}");
    }

    // ── F2: 무적 ──────────────────────────────────
    void ToggleInvincible()
    {
        if (playerHealth == null) playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth == null) return;

        isInvincible = !isInvincible;
        playerHealth.SetInvincible(isInvincible);
        Debug.Log($"[치트] 무적 {(isInvincible ? "ON" : "OFF")}");
    }

    // ── F3: 적 전체 즉사 ──────────────────────────
    void KillAllEnemies()
    {
        // EnemyController 방식
        foreach (var e in FindObjectsByType<EnemyController>(FindObjectsSortMode.None))
            e.TakeDamage(99999);

        // 구형 EnemyHealth 방식도 지원
        foreach (var e in FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None))
            e.TakeDamage(99999);

        Debug.Log("[치트] 적 전체 즉사");
    }

    // ── F4: 코인 추가 ─────────────────────────────
    void AddCoins()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.AddCoins(coinCheatAmount);
        Debug.Log($"[치트] 코인 +{coinCheatAmount}");
    }

    // ── F8: 스테이지 클리어 ───────────────────────
    void ClearStage()
    {
        // 보스 즉사 → BossHealth.Die()가 포탈 스폰
        foreach (var b in FindObjectsByType<BossHealth>(FindObjectsSortMode.None))
            b.TakeDamage(99999);

        // 보스 없으면 적 전체 즉사
        KillAllEnemies();
        Debug.Log("[치트] 스테이지 클리어");
    }

    // ── F5~F7: 스테이지 이동 ──────────────────────
    void LoadStage(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
        Debug.Log($"[치트] 스테이지 이동 → {sceneName}");
    }

    // ── 디버그 UI ─────────────────────────────────
    void OnGUI()
    {
        if (!isDebugMode) return;
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 13;
        style.normal.textColor = Color.green;

        int enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        PlayerHealth ph = playerHealth;

        GUILayout.BeginArea(new Rect(10, 10, 260, 280));
        GUI.Box(new Rect(0, 0, 260, 260), "");
        GUILayout.Label($"[DEBUG MODE]", style);
        GUILayout.Label($"FPS       : {fps:F0}", style);
        GUILayout.Label($"적 수     : {enemyCount}", style);
        GUILayout.Label($"무적      : {(isInvincible ? "ON" : "OFF")}", style);
        GUILayout.Label($"코인      : {(GameManager.Instance != null ? GameManager.Instance.coins : 0)}", style);
        GUILayout.Label($"점수      : {(GameManager.Instance != null ? GameManager.Instance.score : 0)}", style);
        GUILayout.Space(6);
        GUILayout.Label("─── 치트 키 ───", style);
        GUILayout.Label("F1 디버그  F2 무적", style);
        GUILayout.Label("F3 적즉사  F4 +코인", style);
        GUILayout.Label("F5 S1  F6 S2  F7 S3", style);
        GUILayout.Label("F9 스테이지클리어", style);
        GUILayout.EndArea();
    }
}
