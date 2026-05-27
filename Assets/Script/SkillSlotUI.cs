using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 스킬 슬롯에 붙이는 드롭 스크립트
/// 슬롯 패널(Image)에 추가하세요.
/// </summary>
public class SkillSlotUI : MonoBehaviour, IDropHandler
{
    [Header("슬롯 번호 (1 또는 2)")]
    public int slotIndex = 1;

    [Header("슬롯 안에 표시할 이미지 (선택)")]
    public Image slotIcon;

    [Header("슬롯 이름 텍스트 (선택)")]
    public Text slotText;

    void Start() => Refresh();

    public void OnDrop(PointerEventData e)
    {
        var draggable = e.pointerDrag?.GetComponent<DraggableSkill>();
        if (draggable == null) return;

        var sm = SkillManager.Instance;
        if (sm == null) return;

        // 구매 안 한 스킬은 장착 불가
        if (!sm.HasSkill(draggable.skillType))
        {
            Debug.Log("[슬롯] 먼저 구매하세요!");
            return;
        }

        sm.AssignSlot(slotIndex, draggable.skillType);

        // 슬롯 아이콘 복사
        if (slotIcon != null)
            slotIcon.sprite = draggable.GetComponent<Image>()?.sprite;

        Refresh();
        Debug.Log($"[슬롯{slotIndex}] {draggable.skillType} 장착!");
    }

    public void Refresh()
    {
        var sm = SkillManager.Instance;
        if (sm == null || slotText == null) return;

        SkillType type = slotIndex == 1 ? sm.slot1 : sm.slot2;
        string key     = slotIndex == 1 ? "Q" : "R";

        if (type == SkillType.None)
            slotText.text = $"슬롯{slotIndex} ({key})\n비어있음";
        else
            slotText.text = $"슬롯{slotIndex} ({key})\n{sm.GetData(type)?.skillName}";
    }
}
