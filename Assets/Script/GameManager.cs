using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    Animator anim;

    [Header("점수")]
    public int score = 0;
    public Text scoreText;

    [Header("코인")]
    public int coins = 0;
    public Text coinText;

    [Header("스테이지")]
    public string menuSceneName    = "Shop";                              // 메뉴 씬 이름
    public string rankingSceneName = "RankingScene";                     // 랭킹 씬 이름
    public string[] stageSceneNames = { "Stage1", "Stage2", "Stage3" }; // 순서대로 등록
    public int currentStage   = 0; // 현재 플레이 중인 스테이지 인덱스
    public int selectedStage  = 0; // 메뉴에서 선택한 스테이지 인덱스
    public int unlockedStages = 1; // 해금된 스테이지 수 (처음엔 1개)

    [Header("게임오버")]
    public bool isGameOver = false;
    public GameObject gameOverPanel;
    public Text finalScoreText;
    public Text finalCoinText;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadGame(); // 게임 시작 시 자동 불러오기
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // ══════════════════════════════════════════
    //  저장 / 불러오기 / 초기화
    // ══════════════════════════════════════════

    public void SaveGame()
    {
        PlayerPrefs.SetInt("Coins",          coins);
        PlayerPrefs.SetInt("UnlockedStages", unlockedStages);
        PlayerPrefs.SetInt("Score",          score);
        PlayerPrefs.Save();
    }

    public void LoadGame()
    {
        coins          = PlayerPrefs.GetInt("Coins",          0);
        unlockedStages = PlayerPrefs.GetInt("UnlockedStages", 1);
        score          = PlayerPrefs.GetInt("Score",          0);
    }

    public void ResetGame()
    {
        // 랭킹 데이터는 유지하고 게임 데이터만 삭제
        PlayerPrefs.DeleteKey("Coins");
        PlayerPrefs.DeleteKey("UnlockedStages");
        PlayerPrefs.DeleteKey("Score");
        PlayerPrefs.Save();
        coins          = 0;
        unlockedStages = 1;
        score          = 0;
        currentStage   = 0;
        selectedStage  = 0;
        InventoryManager.Instance?.ResetAll();
        RefreshUI();
    }

    // ── 일회성 데이터 초기화 (필요할 때만 주석 해제 후 실행) ──
    // void ClearAllData()
    // {
    //     PlayerPrefs.DeleteAll();
    //     PlayerPrefs.Save();
    //     coins          = 0;
    //     unlockedStages = 1;
    //     score          = 0;
    //     currentStage   = 0;
    //     selectedStage  = 0;
    //     InventoryManager.Instance?.ResetAll();
    //     Debug.Log("전체 데이터 초기화 완료");
    // }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 비활성 오브젝트 포함해서 태그로 찾기
    GameObject FindWithTagIncludeInactive(string tag)
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            if (go.CompareTag(tag) && go.scene.isLoaded) return go;
        return null;
    }

    // 씬 로드 시 새 씬의 UI에 현재 값 반영
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isGameOver = false;

        // 새 씬의 UI 오브젝트를 다시 찾아서 연결 (비활성 포함)
        scoreText     = FindWithTagIncludeInactive("ScoreText")    ?.GetComponent<Text>();
        coinText      = FindWithTagIncludeInactive("CoinText")     ?.GetComponent<Text>();
        gameOverPanel = FindWithTagIncludeInactive("GameOverPanel");
        finalScoreText= FindWithTagIncludeInactive("FinalScoreText")?.GetComponent<Text>();
        finalCoinText = FindWithTagIncludeInactive("FinalCoinText") ?.GetComponent<Text>();

        RefreshUI();

        // 씬 진입 시 페이드인. SceneChangeRoutine이 이미 처리 중이면 isFading=true라 자동 무시됨
        FadeManager.Instance?.FadeIn();
    }

    void RefreshUI()
    {
        if (scoreText != null) scoreText.text = "점수: " + score;
        if (coinText  != null) coinText.text  = "코인: " + coins;
    }

    // 점수 추가 → 적 처치 시
    public void AddScore(int amount)
    {
        if (isGameOver) return;
        score += amount;
        if (scoreText != null)
            scoreText.text = "점수: " + score;
    }

    // 코인 추가 → 상점 판매 시
    public void AddCoins(int amount)
    {
        if (isGameOver) return;
        coins += amount;
        if (coinText != null)
            coinText.text = "코인: " + coins;
    }

    public bool SpendCoins(int amount)
    {
        if (coins < amount) return false;
        coins -= amount;
        if (coinText != null)
            coinText.text = "코인: " + coins;
        return true;
    }

    // 카운트다운 표시용
    private int    countdownValue = 0;
    private bool   showCountdown  = false;

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (FadeManager.Instance != null)
            FadeManager.Instance.UnblockRaycasts();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        if (finalScoreText != null)
            finalScoreText.text = "최종 점수: " + score;
        if (finalCoinText != null)
            finalCoinText.text = "최종 코인: " + coins;

        // timeScale 유지 (1f) → 코루틴이 카운트다운 진행
        Time.timeScale = 1f;
        StartCoroutine(CountdownToMenu());
    }

    IEnumerator CountdownToMenu()
    {
        showCountdown = true;

        for (int i = 3; i >= 1; i--)
        {
            countdownValue = i;
            yield return new WaitForSeconds(1f);
        }

        showCountdown = false;
        currentStage  = 0;
        selectedStage = 0;
        FadeAndLoad(menuSceneName);
    }

    void OnGUI()
    {
        if (!showCountdown) return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize  = 80;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.white;

        // 화면 정중앙
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), countdownValue.ToString(), style);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        currentStage  = 0;
        selectedStage = 0;
        FadeAndLoad(menuSceneName);
    }

    /// <summary>보스 처치 후 호출 → 다음 스테이지 해금 + 메뉴로 복귀</summary>
    public void UnlockNextStage()
    {
        int next = currentStage + 2; // 인덱스+1 → 스테이지 번호+1
        if (next > unlockedStages)
            unlockedStages = Mathf.Min(next, stageSceneNames.Length);
    }

    /// <summary>보스 처치 후 메뉴로 이동</summary>
    public void GoToMenu()
    {
        Time.timeScale = 1f;
        FadeAndLoad(menuSceneName);
    }

    /// <summary>메뉴에서 스테이지 선택 후 시작</summary>
    public void StartSelectedStage()
    {
        currentStage = selectedStage;
        Time.timeScale = 1f;
        
        StartCoroutine(DelayedLoad(stageSceneNames[selectedStage], 2f));
    }

    IEnumerator DelayedLoad(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        FadeAndLoad(sceneName);
    }

    /// <summary>FadeManager가 있으면 페이드, 없으면 즉시 이동</summary>
    private void FadeAndLoad(string sceneName)
    {
        if (FadeManager.Instance != null)
            FadeManager.Instance.CallScene(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }
}
