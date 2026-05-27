using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ShopPanel의 탭 전환을 담당합니다.
/// ShopPanelCreator가 자동으로 설정합니다.
/// </summary>
public class ShopTabController : MonoBehaviour
{
    [Header("탭 콘텐츠 패널")]
    public GameObject partsContent;
    public GameObject itemsContent;
    public GameObject bagContent;

    [Header("탭 버튼")]
    public Button partsTab;
    public Button itemsTab;
    public Button bagTab;

    static readonly Color activeColor   = new Color(0.2f, 0.45f, 0.85f);
    static readonly Color inactiveColor = new Color(0.12f, 0.12f, 0.22f);

    void Start() => ShowTab(0);

    public void ShowParts() => ShowTab(0);
    public void ShowItems() => ShowTab(1);
    public void ShowBag()   => ShowTab(2);

    void ShowTab(int index)
    {
        if (partsContent != null) partsContent.SetActive(index == 0);
        if (itemsContent != null) itemsContent.SetActive(index == 1);
        if (bagContent   != null) bagContent.SetActive(index == 2);

        SetTabActive(partsTab, index == 0);
        SetTabActive(itemsTab, index == 1);
        SetTabActive(bagTab,   index == 2);
    }

    void SetTabActive(Button btn, bool active)
    {
        if (btn == null) return;
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = active ? activeColor : inactiveColor;
    }
}
