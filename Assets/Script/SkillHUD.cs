using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스테이지 씬 HUD - 장착된 스킬과 쿨타임 표시
/// 스테이지 씬 Canvas에 추가하고 슬롯 UI 연결
/// </summary>
public class SkillHUD : MonoBehaviour
{
    [Header("슬롯1 (Q키)")]
    public Image slot1Icon;        // 스킬 아이콘 이미지
    public Image slot1Cooldown;    // 쿨타임 Fill 이미지 (fillAmount로 표시)
    public Text  slot1Label;       // 스킬 이름 텍스트

    [Header("슬롯2 (R키)")]
    public Image slot2Icon;
    public Image slot2Cooldown;
    public Text  slot2Label;

    [Header("슬롯 아이콘 스프라이트")]
    public Sprite speedSprite;
    public Sprite shieldSprite;
    public Sprite reflectSprite;

    void Start() => RefreshIcons();

    void Update()
    {
        var sm = SkillManager.Instance;
        if (sm == null) return;

        // 쿨타임 fillAmount 갱신
        if (slot1Cooldown != null)
            slot1Cooldown.fillAmount = 1f - sm.Slot1Fill;

        if (slot2Cooldown != null)
            slot2Cooldown.fillAmount = 1f - sm.Slot2Fill;
    }

    void RefreshIcons()
    {
        var sm = SkillManager.Instance;
        if (sm == null) return;

        SetSlot(slot1Icon, slot1Label, sm.slot1);
        SetSlot(slot2Icon, slot2Label, sm.slot2);
    }

    void SetSlot(Image icon, Text label, SkillType type)
    {
        if (icon != null)
        {
            icon.sprite = GetSprite(type);
            icon.enabled = type != SkillType.None;
        }

        if (label != null)
        {
            var sm = SkillManager.Instance;
            label.text = type == SkillType.None ? "없음"
                : sm?.GetData(type)?.skillName ?? "";
        }
    }

    Sprite GetSprite(SkillType type)
    {
        switch (type)
        {
            case SkillType.SpeedBoost: return speedSprite;
            case SkillType.Shield:     return shieldSprite;
            case SkillType.Reflect:    return reflectSprite;
            default: return null;
        }
    }
}
