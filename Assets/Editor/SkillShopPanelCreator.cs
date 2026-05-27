using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;

/// <summary>
/// Unity 메뉴 → Tools → 스킬 상점 패널 생성
/// Shop씬을 열어놓은 상태에서 실행하세요.
/// Canvas가 씬에 있어야 합니다.
/// </summary>
public static class SkillShopPanelCreator
{
    static Font font;

    [MenuItem("Tools/스킬 상점 패널 생성")]
    static void Create()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("오류", "씬에 Canvas가 없습니다.\nCanvas를 먼저 추가하세요.", "확인");
            return;
        }

        // 기존 패널이 있으면 제거 후 재생성
        var old = canvas.transform.Find("SkillShopPanel");
        if (old != null) Undo.DestroyObjectImmediate(old.gameObject);

        // ── 패널 루트 ─────────────────────────────
        var panelGO = MakePanelRoot(canvas.transform);
        var shopUI  = panelGO.AddComponent<SkillShopUI>();
        panelGO.SetActive(false); // 기본값: 숨김

        // ── 헤더 ─────────────────────────────────
        MakeText(panelGO.transform, "Title", "⚡ 스킬 상점",
            new Vector2(-40, 268), new Vector2(300, 46),
            25, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);

        shopUI.coinText = MakeText(panelGO.transform, "CoinText", "0 C",
            new Vector2(310, 268), new Vector2(160, 42),
            20, FontStyle.Bold, Color.yellow, TextAnchor.MiddleRight);

        // 닫기 버튼
        var closeBtn = MakeButton(panelGO.transform, "CloseBtn", "✕",
            new Vector2(446, 268), new Vector2(42, 42), new Color(0.7f, 0.2f, 0.2f));
        UnityEventTools.AddPersistentListener(closeBtn.onClick, shopUI.Close);

        MakeDivider(panelGO.transform, new Vector2(0, 232));

        // ── 스킬 행 3개 ──────────────────────────
        MakeSkillRow(panelGO.transform, shopUI,
            "SpeedBoost", "스피드 부스트", "이동속도 +50%  /  4초", "200 C",
            new Vector2(0, 138),
            shopUI.BuySpeedBoost, shopUI.AssignSlot1SpeedBoost, shopUI.AssignSlot2SpeedBoost);

        MakeDivider(panelGO.transform, new Vector2(0, 72));

        MakeSkillRow(panelGO.transform, shopUI,
            "Shield", "무적 방어막", "피해 무효화  /  3초", "300 C",
            new Vector2(0, -10),
            shopUI.BuyShield, shopUI.AssignSlot1Shield, shopUI.AssignSlot2Shield);

        MakeDivider(panelGO.transform, new Vector2(0, -76));

        MakeSkillRow(panelGO.transform, shopUI,
            "Reflect", "탄 반사", "주변 탄환 즉시 반사  /  즉발", "400 C",
            new Vector2(0, -158),
            shopUI.BuyReflect, shopUI.AssignSlot1Reflect, shopUI.AssignSlot2Reflect);

        MakeDivider(panelGO.transform, new Vector2(0, -224));

        // ── 슬롯 상태 표시 ────────────────────────
        shopUI.slot1Text = MakeText(panelGO.transform, "Slot1Text", "슬롯1 (Q) : 없음",
            new Vector2(-150, -256), new Vector2(320, 32),
            16, FontStyle.Normal, new Color(0.6f, 0.9f, 1f), TextAnchor.MiddleLeft);

        shopUI.slot2Text = MakeText(panelGO.transform, "Slot2Text", "슬롯2 (R) : 없음",
            new Vector2(180, -256), new Vector2(320, 32),
            16, FontStyle.Normal, new Color(0.8f, 0.6f, 1f), TextAnchor.MiddleLeft);

        // ── ShopPanel 자동 연동 ───────────────────
        // ShopPanel이 씬에 있으면 mainShopPanel 참조 자동 설정
        var shopPanel = canvas.transform.Find("ShopPanel");
        if (shopPanel != null)
        {
            shopUI.mainShopPanel = shopPanel.gameObject;
            LinkOpenButton(shopPanel, shopUI);
        }

        // ── MenuUI 자동 연결 ─────────────────────
        var menuUI = Object.FindFirstObjectByType<MenuUI>();
        if (menuUI != null)
            menuUI.skillShopPanel = panelGO;

        // 생성 완료 처리
        Selection.activeGameObject = panelGO;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[스킬상점] SkillShopPanel 생성 완료!");
    }

    // ShopPanel 안에 "스킬 상점 열기" 버튼이 있으면 Open() 연결
    static void LinkOpenButton(Transform shopPanel, SkillShopUI skillShopUI)
    {
        var openBtn = shopPanel.GetComponentsInChildren<Button>(true);
        foreach (var btn in openBtn)
        {
            if (btn.name == "SkillShopOpenBtn")
            {
                UnityEventTools.AddPersistentListener(btn.onClick, skillShopUI.Open);
                return;
            }
        }
    }

    // ══════════════════════════════════════════
    //  스킬 한 행 생성
    // ══════════════════════════════════════════
    static void MakeSkillRow(Transform parent, SkillShopUI shopUI,
        string id, string skillName, string desc, string price, Vector2 pos,
        UnityEngine.Events.UnityAction onBuy,
        UnityEngine.Events.UnityAction onSlot1,
        UnityEngine.Events.UnityAction onSlot2)
    {
        var row = MakeRect(parent, id + "Row", pos, new Vector2(880, 72));

        MakeText(row.transform, "Name", skillName,
            new Vector2(-272, 11), new Vector2(220, 28),
            17, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);

        MakeText(row.transform, "Desc", desc,
            new Vector2(-272, -13), new Vector2(260, 22),
            12, FontStyle.Normal, new Color(0.7f, 0.7f, 0.7f), TextAnchor.MiddleLeft);

        MakeText(row.transform, "Price", price,
            new Vector2(-76, 0), new Vector2(110, 34),
            16, FontStyle.Bold, Color.yellow, TextAnchor.MiddleCenter);

        // 구매 버튼
        var buyBtn = MakeButton(row.transform, "BuyBtn", "구매",
            new Vector2(60, 0), new Vector2(96, 48), new Color(0.15f, 0.55f, 0.15f));
        UnityEventTools.AddPersistentListener(buyBtn.onClick, onBuy);
        UnityEventTools.AddPersistentListener(buyBtn.onClick, shopUI.Refresh);

        // Q슬롯 버튼
        var slot1Btn = MakeButton(row.transform, "Slot1Btn", "Q슬롯",
            new Vector2(170, 0), new Vector2(96, 48), new Color(0.15f, 0.35f, 0.7f));
        UnityEventTools.AddPersistentListener(slot1Btn.onClick, onSlot1);
        UnityEventTools.AddPersistentListener(slot1Btn.onClick, shopUI.Refresh);

        // R슬롯 버튼
        var slot2Btn = MakeButton(row.transform, "Slot2Btn", "R슬롯",
            new Vector2(280, 0), new Vector2(96, 48), new Color(0.45f, 0.15f, 0.7f));
        UnityEventTools.AddPersistentListener(slot2Btn.onClick, onSlot2);
        UnityEventTools.AddPersistentListener(slot2Btn.onClick, shopUI.Refresh);
    }

    // ══════════════════════════════════════════
    //  헬퍼
    // ══════════════════════════════════════════
    static GameObject MakePanelRoot(Transform parent)
    {
        var go = MakeRect(parent, "SkillShopPanel", Vector2.zero, new Vector2(960, 620));
        Undo.RegisterCreatedObjectUndo(go, "Create SkillShopPanel");

        var img     = go.AddComponent<Image>();
        img.color   = new Color(0.06f, 0.06f, 0.14f, 0.97f);

        var outline          = go.AddComponent<Outline>();
        outline.effectColor    = new Color(0.3f, 0.6f, 1f, 0.7f);
        outline.effectDistance = new Vector2(2, -2);
        return go;
    }

    static Button MakeButton(Transform parent, string name, string label,
        Vector2 pos, Vector2 size, Color bg)
    {
        var go  = MakeRect(parent, name, pos, size);
        var img = go.AddComponent<Image>();
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
        img.color = new Color(0.3f, 0.5f, 0.9f, 0.35f);
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
