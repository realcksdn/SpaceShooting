using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shop씬 Canvas에 부착하면 스킬 상점 패널을 자동 생성합니다.
/// 씬 재생 시 패널이 만들어지고 기본적으로 숨겨집니다.
/// 기존 버튼 OnClick → 이 컴포넌트의 OpenPanel() 연결하세요.
/// </summary>
public class SkillShopPanelBuilder : MonoBehaviour
{
    GameObject  panel;
    SkillShopUI shopUI;

    void Awake() => Build();

    public void OpenPanel()  => panel?.SetActive(true);
    public void ClosePanel() => panel?.SetActive(false);

    // ══════════════════════════════════════════
    //  패널 전체 생성
    // ══════════════════════════════════════════
    void Build()
    {
        // 루트 패널
        panel = MakePanel(transform, new Vector2(960, 620));
        panel.SetActive(false);

        shopUI = panel.AddComponent<SkillShopUI>();

        // ── 헤더 ─────────────────────────────
        MakeText(panel.transform, "Title", "⚡ 스킬 상점",
            new Vector2(-40, 268), new Vector2(300, 46),
            25, FontStyle.Bold, Color.white);

        var coinText = MakeText(panel.transform, "CoinText", "0 C",
            new Vector2(310, 268), new Vector2(160, 42),
            20, FontStyle.Bold, Color.yellow, TextAnchor.MiddleRight);
        shopUI.coinText = coinText;

        // 닫기 버튼 (우상단)
        MakeButton(panel.transform, "CloseBtn", "✕",
            new Vector2(446, 268), new Vector2(42, 42),
            new Color(0.7f, 0.2f, 0.2f))
            .onClick.AddListener(ClosePanel);

        MakeDivider(panel.transform, new Vector2(0, 232));

        // ── 스킬 행 3개 ──────────────────────
        BuildRow("SpeedBoost", "스피드 부스트", "이동속도 +50%  /  4초",  "200 C",
            new Vector2(0, 138),
            shopUI.BuySpeedBoost, shopUI.AssignSlot1SpeedBoost, shopUI.AssignSlot2SpeedBoost);

        MakeDivider(panel.transform, new Vector2(0, 72));

        BuildRow("Shield", "무적 방어막", "피해 무효화  /  3초", "300 C",
            new Vector2(0, -10),
            shopUI.BuyShield, shopUI.AssignSlot1Shield, shopUI.AssignSlot2Shield);

        MakeDivider(panel.transform, new Vector2(0, -76));

        BuildRow("Reflect", "탄 반사", "주변 탄환 즉시 반사  /  즉발", "400 C",
            new Vector2(0, -158),
            shopUI.BuyReflect, shopUI.AssignSlot1Reflect, shopUI.AssignSlot2Reflect);

        // ── 슬롯 상태 표시 ────────────────────
        MakeDivider(panel.transform, new Vector2(0, -224));

        var slot1Text = MakeText(panel.transform, "Slot1Text", "슬롯1 (Q) : 없음",
            new Vector2(-150, -256), new Vector2(320, 32),
            16, FontStyle.Normal, new Color(0.6f, 0.9f, 1f));
        var slot2Text = MakeText(panel.transform, "Slot2Text", "슬롯2 (R) : 없음",
            new Vector2(180, -256), new Vector2(320, 32),
            16, FontStyle.Normal, new Color(0.8f, 0.6f, 1f));
        shopUI.slot1Text = slot1Text;
        shopUI.slot2Text = slot2Text;
    }

    // ══════════════════════════════════════════
    //  스킬 한 행 생성
    // ══════════════════════════════════════════
    void BuildRow(string id, string skillName, string desc, string price, Vector2 pos,
        UnityEngine.Events.UnityAction onBuy,
        UnityEngine.Events.UnityAction onSlot1,
        UnityEngine.Events.UnityAction onSlot2)
    {
        var row = MakeRect(panel.transform, id + "Row", pos, new Vector2(880, 72));

        // 스킬 이름
        MakeText(row.transform, "Name", skillName,
            new Vector2(-272, 11), new Vector2(220, 28),
            17, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);

        // 설명
        MakeText(row.transform, "Desc", desc,
            new Vector2(-272, -13), new Vector2(240, 22),
            12, FontStyle.Normal, new Color(0.7f, 0.7f, 0.7f), TextAnchor.MiddleLeft);

        // 가격
        MakeText(row.transform, "Price", price,
            new Vector2(-76, 0), new Vector2(110, 34),
            16, FontStyle.Bold, Color.yellow, TextAnchor.MiddleCenter);

        // 구매 버튼
        MakeButton(row.transform, "BuyBtn", "구매",
            new Vector2(60, 0), new Vector2(96, 48),
            new Color(0.15f, 0.55f, 0.15f))
            .onClick.AddListener(() => { onBuy(); shopUI.Refresh(); });

        // Q슬롯 버튼
        MakeButton(row.transform, "Slot1Btn", "Q슬롯",
            new Vector2(170, 0), new Vector2(96, 48),
            new Color(0.15f, 0.35f, 0.7f))
            .onClick.AddListener(() => { onSlot1(); shopUI.Refresh(); });

        // R슬롯 버튼
        MakeButton(row.transform, "Slot2Btn", "R슬롯",
            new Vector2(280, 0), new Vector2(96, 48),
            new Color(0.45f, 0.15f, 0.7f))
            .onClick.AddListener(() => { onSlot2(); shopUI.Refresh(); });
    }

    // ══════════════════════════════════════════
    //  UI 헬퍼
    // ══════════════════════════════════════════

    GameObject MakePanel(Transform parent, Vector2 size)
    {
        var go  = MakeRect(parent, "SkillShopPanel", Vector2.zero, size);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.06f, 0.06f, 0.14f, 0.97f);

        var outline = go.AddComponent<Outline>();
        outline.effectColor    = new Color(0.3f, 0.6f, 1f, 0.7f);
        outline.effectDistance = new Vector2(2, -2);
        return go;
    }

    Button MakeButton(Transform parent, string name, string label,
        Vector2 pos, Vector2 size, Color bg)
    {
        var go  = MakeRect(parent, name, pos, size);
        var img = go.AddComponent<Image>();
        img.color = bg;

        var btn = go.AddComponent<Button>();
        var cb  = btn.colors;
        cb.normalColor      = bg;
        cb.highlightedColor = new Color(bg.r + 0.15f, bg.g + 0.15f, bg.b + 0.15f);
        cb.pressedColor     = new Color(bg.r - 0.1f,  bg.g - 0.1f,  bg.b - 0.1f);
        cb.selectedColor    = bg;
        btn.colors = cb;

        var txtGo = MakeRect(go.transform, "Label", Vector2.zero, size);
        var txt   = txtGo.AddComponent<Text>();
        txt.text      = label;
        txt.fontSize  = 13;
        txt.fontStyle = FontStyle.Bold;
        txt.color     = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        return btn;
    }

    Text MakeText(Transform parent, string name, string content,
        Vector2 pos, Vector2 size, int fontSize,
        FontStyle style, Color color,
        TextAnchor anchor = TextAnchor.MiddleLeft)
    {
        var go = MakeRect(parent, name, pos, size);
        var t  = go.AddComponent<Text>();
        t.text      = content;
        t.fontSize  = fontSize;
        t.fontStyle = style;
        t.color     = color;
        t.alignment = anchor;
        t.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return t;
    }

    void MakeDivider(Transform parent, Vector2 pos)
    {
        var go  = MakeRect(parent, "Divider", pos, new Vector2(860, 1));
        var img = go.AddComponent<Image>();
        img.color = new Color(0.3f, 0.5f, 0.9f, 0.35f);
    }

    GameObject MakeRect(Transform parent, string name, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;
        return go;
    }
}
