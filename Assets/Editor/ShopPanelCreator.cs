using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;

/// <summary>
/// Unity 메뉴 → Tools → 상점 패널 생성
/// Shop씬 Canvas에 ShopPanel을 자동 생성합니다.
/// </summary>
public static class ShopPanelCreator
{
    // ── 색상 상수 ─────────────────────────────────
    static readonly Color BgColor      = new Color(0.06f, 0.06f, 0.14f, 0.97f);
    static readonly Color DividerColor = new Color(0.3f, 0.5f, 0.9f, 0.35f);
    static readonly Color BuyColor     = new Color(0.15f, 0.55f, 0.15f);
    static readonly Color SellColor    = new Color(0.6f, 0.35f, 0.1f);
    static readonly Color CloseColor   = new Color(0.7f, 0.2f, 0.2f);
    static readonly Color TabActive    = new Color(0.2f, 0.45f, 0.85f);
    static readonly Color TabInactive  = new Color(0.12f, 0.12f, 0.22f);

    static Font font;

    [MenuItem("Tools/상점 패널 생성")]
    static void Create()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("오류", "씬에 Canvas가 없습니다.", "확인");
            return;
        }

        var old = canvas.transform.Find("ShopPanel");
        if (old != null) Undo.DestroyObjectImmediate(old.gameObject);

        // ── 루트 패널 ─────────────────────────────
        var panelGO = MakePanelRoot(canvas.transform, "ShopPanel", new Vector2(960, 640));
        panelGO.SetActive(false);

        var shopUI  = panelGO.AddComponent<ShopUI>();
        var tabCtrl = panelGO.AddComponent<ShopTabController>();

        // ── 헤더 ─────────────────────────────────
        MakeText(panelGO.transform, "Title", "🛒 상점",
            new Vector2(-40, 288), new Vector2(260, 46),
            25, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);

        shopUI.coinText = MakeText(panelGO.transform, "CoinText", "0 C",
            new Vector2(310, 288), new Vector2(160, 42),
            20, FontStyle.Bold, Color.yellow, TextAnchor.MiddleRight);

        var closeBtn = MakeButton(panelGO.transform, "CloseBtn", "✕",
            new Vector2(446, 288), new Vector2(42, 42), CloseColor);
        UnityEventTools.AddPersistentListener(closeBtn.onClick, shopUI.Close);

        MakeDivider(panelGO.transform, new Vector2(0, 252));

        // ── 탭 바 ─────────────────────────────────
        var tabBar = MakeRect(panelGO.transform, "TabBar", new Vector2(0, 220), new Vector2(880, 46));

        var partsTab = MakeButton(tabBar.transform, "PartsTab", "⚙  파츠",
            new Vector2(-270, 0), new Vector2(270, 44), TabActive);
        var itemsTab = MakeButton(tabBar.transform, "ItemsTab", "💊  아이템 · 폭탄",
            new Vector2(30, 0), new Vector2(270, 44), TabInactive);
        var bagTab   = MakeButton(tabBar.transform, "BagTab", "🎒  가방",
            new Vector2(330, 0), new Vector2(270, 44), TabInactive);

        MakeDivider(panelGO.transform, new Vector2(0, 195));

        // ── 콘텐츠 공통 영역 (세 패널이 같은 위치에 겹침) ──
        const float contentY = -60f;
        const float contentH = 370f;

        // ── ① 파츠 콘텐츠 ────────────────────────
        var partsContent = MakeRect(panelGO.transform, "PartsContent",
            new Vector2(0, contentY), new Vector2(900, contentH));

        MakeShopRow(partsContent.transform, "MoveSpeed",
            "이동속도 증가", "이동속도 +20%  /  영구", "100 C",
            new Vector2(0, 138), shopUI.Buy_MoveSpeedUp, shopUI.Refresh);

        MakeShopRow(partsContent.transform, "MaxHp",
            "최대 HP 증가", "최대 HP +1  /  영구", "150 C",
            new Vector2(0, 66), shopUI.Buy_MaxHpUp, shopUI.Refresh);

        MakeShopRow(partsContent.transform, "ReflectRadius",
            "반사 범위 증가", "반사 범위 +0.5  /  영구", "120 C",
            new Vector2(0, -6), shopUI.Buy_ReflectRadiusUp, shopUI.Refresh);

        MakeShopRow(partsContent.transform, "ReflectCooldown",
            "반사 쿨타임 감소", "반사 쿨타임 -0.5초  /  영구", "120 C",
            new Vector2(0, -78), shopUI.Buy_ReflectCooldownDown, shopUI.Refresh);

        MakeShopRow(partsContent.transform, "DashDamage",
            "돌진 데미지 증가", "대쉬 피해 +1  /  영구", "100 C",
            new Vector2(0, -150), shopUI.Buy_DashDamageUp, shopUI.Refresh);

        // ── ② 아이템·폭탄 콘텐츠 ─────────────────
        var itemsContent = MakeRect(panelGO.transform, "ItemsContent",
            new Vector2(0, contentY), new Vector2(900, contentH));

        MakeShopRow(itemsContent.transform, "HealPotion",
            "회복 포션", "HP +3 즉시 회복  /  소모품", "50 C",
            new Vector2(0, 110), shopUI.Buy_HealPotion, shopUI.Refresh);

        MakeShopRow(itemsContent.transform, "InvincPotion",
            "무적 포션", "5초 동안 피해 무효화  /  소모품", "80 C",
            new Vector2(0, 38), shopUI.Buy_InvincibilityPotion, shopUI.Refresh);

        MakeShopRow(itemsContent.transform, "Bomb",
            "폭탄", "화면 내 모든 탄환 제거  /  Q키 사용", "60 C",
            new Vector2(0, -34), shopUI.Buy_Bomb, shopUI.Refresh);

        MakeText(itemsContent.transform, "HintText",
            "💡  소모품은 인벤토리에 저장되며 숫자키(1~5)로 사용합니다.",
            new Vector2(0, -130), new Vector2(820, 28),
            12, FontStyle.Normal, new Color(0.6f, 0.6f, 0.6f), TextAnchor.MiddleCenter);

        // ── ③ 가방 콘텐츠 ─────────────────────────
        var bagContent = MakeRect(panelGO.transform, "BagContent",
            new Vector2(0, contentY), new Vector2(900, contentH));

        MakeText(bagContent.transform, "BagTitle", "보유 부산물",
            new Vector2(-360, 160), new Vector2(200, 30),
            16, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);

        shopUI.bagListText = MakeText(bagContent.transform, "BagListText",
            "(가방이 비어있습니다)",
            new Vector2(0, 60), new Vector2(820, 200),
            14, FontStyle.Normal, new Color(0.85f, 0.85f, 0.85f), TextAnchor.UpperLeft);

        MakeDivider(bagContent.transform, new Vector2(0, -70));

        var sellAllBtn = MakeButton(bagContent.transform, "SellAllBtn", "전체 판매",
            new Vector2(0, -110), new Vector2(200, 50), SellColor);
        UnityEventTools.AddPersistentListener(sellAllBtn.onClick, shopUI.SellAll);
        UnityEventTools.AddPersistentListener(sellAllBtn.onClick, shopUI.Refresh);

        MakeText(bagContent.transform, "SellHint", "판매 시 코인으로 전환됩니다.",
            new Vector2(0, -155), new Vector2(400, 26),
            12, FontStyle.Normal, new Color(0.6f, 0.6f, 0.6f), TextAnchor.MiddleCenter);

        // ── 탭 컨트롤러 연결 ─────────────────────
        tabCtrl.partsContent = partsContent;
        tabCtrl.itemsContent = itemsContent;
        tabCtrl.bagContent   = bagContent;
        tabCtrl.partsTab     = partsTab;
        tabCtrl.itemsTab     = itemsTab;
        tabCtrl.bagTab       = bagTab;

        UnityEventTools.AddPersistentListener(partsTab.onClick, tabCtrl.ShowParts);
        UnityEventTools.AddPersistentListener(itemsTab.onClick, tabCtrl.ShowItems);
        UnityEventTools.AddPersistentListener(bagTab.onClick,   tabCtrl.ShowBag);

        // ── 스킬 상점 열기 버튼 ──────────────────
        // 파츠 탭 하단에 스킬 상점으로 이동하는 버튼 추가
        var skillOpenBtn = MakeButton(partsContent.transform, "SkillShopOpenBtn", "⚡ 스킬 상점",
            new Vector2(0, -220), new Vector2(220, 46), new Color(0.25f, 0.25f, 0.55f));
        skillOpenBtn.name = "SkillShopOpenBtn";

        // SkillShopPanel이 씬에 있으면 자동 연결
        var skillShopPanel = canvas.transform.Find("SkillShopPanel");
        if (skillShopPanel != null)
        {
            var skillShopUI = skillShopPanel.GetComponent<SkillShopUI>();
            if (skillShopUI != null)
            {
                skillShopUI.mainShopPanel = panelGO;
                UnityEventTools.AddPersistentListener(skillOpenBtn.onClick, skillShopUI.Open);
            }
        }

        // ── MenuUI 자동 연결 ─────────────────────
        var menuUI = Object.FindFirstObjectByType<MenuUI>();
        if (menuUI != null)
            menuUI.shopPanel = panelGO;

        // ── 완료 ─────────────────────────────────
        Selection.activeGameObject = panelGO;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[상점] ShopPanel 생성 완료!");
    }

    // ══════════════════════════════════════════
    //  구매 행 생성 (이름 + 설명 + 가격 + 구매버튼)
    // ══════════════════════════════════════════
    static void MakeShopRow(Transform parent, string id,
        string itemName, string desc, string price, Vector2 pos,
        UnityEngine.Events.UnityAction onBuy,
        UnityEngine.Events.UnityAction onRefresh)
    {
        var row = MakeRect(parent, id + "Row", pos, new Vector2(860, 60));

        MakeText(row.transform, "Name", itemName,
            new Vector2(-272, 10), new Vector2(240, 26),
            16, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);

        MakeText(row.transform, "Desc", desc,
            new Vector2(-272, -12), new Vector2(280, 20),
            11, FontStyle.Normal, new Color(0.7f, 0.7f, 0.7f), TextAnchor.MiddleLeft);

        MakeText(row.transform, "Price", price,
            new Vector2(-60, 0), new Vector2(110, 32),
            15, FontStyle.Bold, Color.yellow, TextAnchor.MiddleCenter);

        var buyBtn = MakeButton(row.transform, "BuyBtn", "구매",
            new Vector2(110, 0), new Vector2(100, 46), BuyColor);
        UnityEventTools.AddPersistentListener(buyBtn.onClick, onBuy);
        UnityEventTools.AddPersistentListener(buyBtn.onClick, onRefresh);
    }

    // ══════════════════════════════════════════
    //  헬퍼
    // ══════════════════════════════════════════
    static GameObject MakePanelRoot(Transform parent, string name, Vector2 size)
    {
        var go = MakeRect(parent, name, Vector2.zero, size);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);

        var img   = go.AddComponent<Image>();
        img.color = BgColor;

        var outline          = go.AddComponent<Outline>();
        outline.effectColor    = new Color(0.3f, 0.6f, 1f, 0.7f);
        outline.effectDistance = new Vector2(2, -2);
        return go;
    }

    static Button MakeButton(Transform parent, string name, string label,
        Vector2 pos, Vector2 size, Color bg)
    {
        var go    = MakeRect(parent, name, pos, size);
        var img   = go.AddComponent<Image>();
        img.color = bg;

        var btn = go.AddComponent<Button>();
        var cb  = btn.colors;
        cb.normalColor      = bg;
        cb.highlightedColor = new Color(Mathf.Min(bg.r + 0.15f, 1f), Mathf.Min(bg.g + 0.15f, 1f), Mathf.Min(bg.b + 0.15f, 1f));
        cb.pressedColor     = new Color(Mathf.Max(bg.r - 0.1f, 0f),  Mathf.Max(bg.g - 0.1f, 0f),  Mathf.Max(bg.b - 0.1f, 0f));
        cb.selectedColor    = bg;
        btn.colors = cb;

        var txtGo = MakeRect(go.transform, "Label", Vector2.zero, size);
        var txt   = txtGo.AddComponent<Text>();
        txt.text      = label;
        txt.fontSize  = 14;
        txt.fontStyle = FontStyle.Bold;
        txt.color     = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font      = font;
        return btn;
    }

    static Text MakeText(Transform parent, string name, string content,
        Vector2 pos, Vector2 size, int fontSize, FontStyle style, Color color, TextAnchor anchor)
    {
        var go = MakeRect(parent, name, pos, size);
        var t  = go.AddComponent<Text>();
        t.text      = content;
        t.fontSize  = fontSize;
        t.fontStyle = style;
        t.color     = color;
        t.alignment = anchor;
        t.font      = font;
        return t;
    }

    static void MakeDivider(Transform parent, Vector2 pos)
    {
        var go    = MakeRect(parent, "Divider", pos, new Vector2(860, 1));
        var img   = go.AddComponent<Image>();
        img.color = DividerColor;
    }

    static GameObject MakeRect(Transform parent, string name, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;
        return go;
    }
}
