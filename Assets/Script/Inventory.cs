using UnityEngine;

/// <summary>
/// 게임 씬 HUD 표시 전용
/// 실제 데이터/입력은 InventoryManager(DontDestroyOnLoad)에서 관리
/// </summary>
public class Inventory : MonoBehaviour
{
    void OnGUI()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;
        var im = InventoryManager.Instance;
        if (im == null) return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 14;
        style.normal.textColor = Color.white;

        GUIStyle bombStyle = new GUIStyle(style);
        bombStyle.normal.textColor = im.BombCount > 0 ? Color.yellow : Color.gray;

        // 가방 개수
        GUILayout.BeginArea(new Rect(Screen.width - 210, 10, 200, 30));
        GUILayout.Label($"가방: {im.BagCount}/{im.bagMaxSlots}  (B: 상점)", style);
        GUILayout.EndArea();

        // 아이템 슬롯
        GUILayout.BeginArea(new Rect(Screen.width - 210, 50, 200, 160));
        GUILayout.Label($"=== 아이템 [{im.ItemCount}/{im.itemMaxSlots}] ===", style);
        var items = im.GetItems();
        for (int i = 0; i < items.Count; i++)
            GUILayout.Label($"  [{i + 1}] {items[i].itemName}", style);
        if (items.Count == 0)
            GUILayout.Label("  (없음)", style);
        GUILayout.EndArea();

        // 폭탄
        GUILayout.BeginArea(new Rect(Screen.width - 210, 220, 200, 30));
        GUILayout.Label($"폭탄 [Q]: {im.BombCount}/{im.maxBombs}", bombStyle);
        GUILayout.EndArea();
    }
}
