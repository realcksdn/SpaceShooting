using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menu 씬 Canvas 루트 오브젝트에 추가.
/// 각 패널·버튼을 Inspector에서 연결하세요.
/// </summary>
public class MenuUI : MonoBehaviour
{
    public Animator StartAnimator;
    public Animator CamAnimator;
    [Header("── 패널 ──────────────────────────")]
    public GameObject mainPanel;
    public GameObject stageSelectPanel;
    public GameObject settingsPanel;
    public GameObject shopPanel;
    public GameObject skillShopPanel;
    public GameObject rankingPanel;

    [Header("── 메인 패널 텍스트 ───────────────")]
    public Text coinText;   // 보유 코인 표시용 (없으면 비워도 됨)

    [Header("── 스테이지 버튼 (비활성 처리용) ───")]
    public Button btnStage1;
    public Button btnStage2;
    public Button btnStage3;

    // ──────────────────────────────────────────
    void Start()
    {
        ShowPanel(mainPanel);
        RefreshCoin();
        RefreshStageButtons();
    }

    // ══════════════════════════════════════════
    // 메인 패널
    // ══════════════════════════════════════════
    public void OnClickStart()
    {
        CamAnimator.SetTrigger("Move");
        

        RefreshStageButtons();
        ShowPanel(stageSelectPanel);
    }

    public void OnClickShop()
    {
        ShowPanel(shopPanel);
        CamAnimator.SetTrigger("ShopMove");
    }

    public void OnClickSettings()
    {
        CamAnimator.SetTrigger("SettMove");
        ShowPanel(settingsPanel);
    }

    public void OnExit()
    {
        Application.Quit();
    }
    // ══════════════════════════════════════════
    // 스테이지 선택  ← 버튼 OnClick에 각각 연결
    // ══════════════════════════════════════════
    public void OnClickStage1() => StartStage(0);
    public void OnClickStage2() => StartStage(1);
    public void OnClickStage3() => StartStage(2);

    void StartStage(int index)
    {
        var gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogError("GameManager.Instance 가 null 입니다. Menu 씬에 GameManager 오브젝트가 있는지 확인하세요.");
            return;
        }
        if (index >= gm.stageSceneNames.Length)
        {
            Debug.LogError($"stageSceneNames 배열에 인덱스 {index} 가 없습니다.");
            return;
        }

        StartAnimator.SetTrigger("StartAnim");
        gm.selectedStage = index;
        gm.StartSelectedStage();
    }

    // ══════════════════════════════════════════
    // 뒤로가기 (공용)
    // ══════════════════════════════════════════
    public void OnClickBack()
    {
        // 스킬 상점이 열려있으면 → 상점으로 복귀
        if (skillShopPanel != null && skillShopPanel.activeSelf)
        {
            skillShopPanel.GetComponent<SkillShopUI>()?.Close();
            return;
        }

        // 그 외 → 메인으로
        CamAnimator.SetTrigger("SettBack");
        ShowPanel(mainPanel);
        RefreshCoin();
    }

    // ══════════════════════════════════════════
    // 랭킹
    // ══════════════════════════════════════════
    public void OnClickRanking()
    {
        ShowPanel(rankingPanel);
        rankingPanel?.GetComponent<RankingViewer>()?.Refresh();
    }

    // ══════════════════════════════════════════
    // 저장 / 초기화
    // ══════════════════════════════════════════

    /// <summary>저장 버튼 OnClick에 연결</summary>
    public void OnClickSave()
    {
        GameManager.Instance?.SaveGame();
    }

    /// <summary>초기화 버튼 OnClick에 연결</summary>
    public void OnClickReset()
    {
        GameManager.Instance?.ResetGame();
        RefreshCoin();
        RefreshStageButtons();
    }

    // ══════════════════════════════════════════
    // 설정 패널
    // ══════════════════════════════════════════
    public void OnBgmChanged(float value)
    {
        AudioListener.volume = value;
    }

    public void OnSfxChanged(float value)
    {
        // SFX AudioMixer 파라미터 연결 시 여기에 작성
    }

    // ══════════════════════════════════════════
    // 내부 유틸
    // ══════════════════════════════════════════
    void ShowPanel(GameObject target)
    {
        if (mainPanel        != null) mainPanel.SetActive(mainPanel        == target);
        if (stageSelectPanel != null) stageSelectPanel.SetActive(stageSelectPanel == target);
        if (settingsPanel    != null) settingsPanel.SetActive(settingsPanel == target);
        if (shopPanel        != null) shopPanel.SetActive(shopPanel        == target);
        if (skillShopPanel   != null) skillShopPanel.SetActive(false); // 항상 닫힘 (SkillShopUI.Close로 관리)
        if (rankingPanel     != null) rankingPanel.SetActive(rankingPanel  == target);
        
    }

    void RefreshCoin()
    {
        if (coinText == null) return;
        int c = GameManager.Instance != null ? GameManager.Instance.coins : 0;
        coinText.text = c + " C";
    }

    void RefreshStageButtons()
    {
        int unlocked = GameManager.Instance != null ? GameManager.Instance.unlockedStages : 1;

        SetStageButton(btnStage1, 0, unlocked);
        SetStageButton(btnStage2, 1, unlocked);
        SetStageButton(btnStage3, 2, unlocked);
    }

    void SetStageButton(Button btn, int index, int unlocked)
    {
        if (btn == null) return;
        bool isOpen = index < unlocked;
        btn.interactable = isOpen;

        // 버튼 안 Text 자동 갱신
        var label = btn.GetComponentInChildren<Text>();
        if (label != null)
            label.text = isOpen ? $"STAGE {index + 1}" : $"STAGE {index + 1}  🔒";
    }
}
