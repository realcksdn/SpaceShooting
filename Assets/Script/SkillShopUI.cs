using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shop씬 스킬 상점 UI
/// Inspector에서 버튼/텍스트 연결 후
/// 버튼 OnClick에 각 메서드 연결하세요.
/// </summary>
public class SkillShopUI : MonoBehaviour
{
    [Header("우주선 업그레이드 연출")]
    public Animator shipAnimator;
    public string   upgradeTrigger = "Upgrade";

    [Header("이속 스킬 구매 연출")]
    public FishAndChips fishAndChips;


    [Header("코인 표시")]
    public Text coinText;

    [Header("슬롯 표시 텍스트")]
    public Text slot1Text;
    public Text slot2Text;

    [Header("연동")]
    public GameObject mainShopPanel; // 열릴 때 숨길 메인 상점 패널

    void OnEnable()
    {
        Refresh();
        // 이미 SpeedBoost를 구매한 경우 파츠 연출 복원
        if (SkillManager.Instance != null && SkillManager.Instance.HasSkill(SkillType.SpeedBoost))
            fishAndChips?.SetPartImmediate();
    }

    public void Open()
    {
        if (mainShopPanel != null) mainShopPanel.SetActive(false);
        gameObject.SetActive(true);
    }

    // ── 스킬 구매 버튼 OnClick에 연결 ──────────
    public void BuySpeedBoost() => TryBuy(SkillType.SpeedBoost);
    public void BuyShield()     => TryBuy(SkillType.Shield);
    public void BuyReflect()    => TryBuy(SkillType.Reflect);

    void TryBuy(SkillType type)
    {
        var sm = SkillManager.Instance;
        if (sm == null) return;

        if (sm.HasSkill(type))
        {
            Debug.Log("[스킬상점] 이미 구매한 스킬입니다.");
            return;
        }

        bool ok = sm.BuySkill(type);
        if (ok)
        {
            if (shipAnimator != null)
                shipAnimator.SetTrigger(upgradeTrigger);

            if (type == SkillType.SpeedBoost)
                fishAndChips?.PlayPart();


            Refresh();
        }
        else
        {
            Debug.Log("[스킬상점] 코인 부족!");
        }
    }

    // ── 슬롯 장착 버튼 OnClick에 연결 ──────────
    // 슬롯1 장착
    public void AssignSlot1SpeedBoost() => Assign(1, SkillType.SpeedBoost);
    public void AssignSlot1Shield()     => Assign(1, SkillType.Shield);
    public void AssignSlot1Reflect()    => Assign(1, SkillType.Reflect);

    public void AssignSlot2SpeedBoost() => Assign(2, SkillType.SpeedBoost);
    public void AssignSlot2Shield()     => Assign(2, SkillType.Shield);
    public void AssignSlot2Reflect()    => Assign(2, SkillType.Reflect);

    void Assign(int slot, SkillType type)
    {
        SkillManager.Instance?.AssignSlot(slot, type);
        Refresh();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        if (mainShopPanel != null) mainShopPanel.SetActive(true);
    }

    // ── UI 갱신 ─────────────────────────────────
    public void Refresh()
    {
        var sm = SkillManager.Instance;
        var gm = GameManager.Instance;

        if (coinText != null && gm != null)
            coinText.text = $"{gm.coins} C";

        if (sm == null) return;

        string SlotName(SkillType t) => t == SkillType.None ? "없음"
            : sm.GetData(t)?.skillName ?? t.ToString();

        if (slot1Text != null)
            slot1Text.text = $"슬롯1 (Q): {SlotName(sm.slot1)}";
        if (slot2Text != null)
            slot2Text.text = $"슬롯2 (R): {SlotName(sm.slot2)}";
    }
}
