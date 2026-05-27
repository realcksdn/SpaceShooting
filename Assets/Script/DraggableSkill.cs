using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 스킬 아이콘에 붙이는 드래그 스크립트
/// Canvas 안의 Image 오브젝트에 추가하세요.
/// </summary>
public class DraggableSkill : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("이 아이콘의 스킬 타입")]
    public SkillType skillType;

    private Canvas        canvas;
    private CanvasGroup   canvasGroup;
    private RectTransform rect;
    private Transform     originalParent;
    private Vector2       originalPos;

    void Awake()
    {
        rect       = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData e)
    {
        // 구매한 스킬만 드래그 가능
        if (SkillManager.Instance != null && !SkillManager.Instance.HasSkill(skillType))
            return;

        originalParent = rect.parent;
        originalPos    = rect.anchoredPosition;

        // Canvas 루트로 이동 (다른 UI 위에 그려지도록)
        rect.SetParent(canvas.transform);
        rect.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false; // 드래그 중 레이캐스트 무시
    }

    public void OnDrag(PointerEventData e)
    {
        if (!canvasGroup.blocksRaycasts == false) return;
        rect.anchoredPosition += e.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData e)
    {
        canvasGroup.blocksRaycasts = true;

        // 슬롯에 드롭 안 됐으면 원래 위치로 복귀
        rect.SetParent(originalParent);
        rect.anchoredPosition = originalPos;
    }
}
