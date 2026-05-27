using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 버튼 OnClick에 아래 메서드를 연결하면 끝.
/// coinText 하나만 연결하면 코인 표시도 됩니다.
/// </summary>
public class ShopUI : MonoBehaviour
{
    [Header("코인 표시 (선택)")]
    public Text coinText;

    [Header("가방 목록 표시 (선택)")]
    public Text bagListText;  // 가방 내용을 보여줄 Text

    void OnEnable() => Refresh();

    public void Refresh()
    {
        RefreshCoin();
        RefreshBagList();
    }

    public void Close() => gameObject.SetActive(false);

    // ══════════════════════════════════════════
    //  파츠 구매  (한 번 사면 중복 구매 불가)
    // ══════════════════════════════════════════

    public void Buy_MoveSpeedUp()
        => BuyPart("이동속도 증가", PartType.MoveSpeedUp, price: 100, value: 0.2f);

    public void Buy_MaxHpUp()
        => BuyPart("최대 HP 증가", PartType.MaxHpUp, price: 150, value: 1f);

    public void Buy_ReflectRadiusUp()
        => BuyPart("반사 범위 증가", PartType.ReflectRadiusUp, price: 120, value: 0.5f);

    public void Buy_ReflectCooldownDown()
        => BuyPart("반사 쿨타임 감소", PartType.ReflectCooldownDown, price: 120, value: 0.5f);

    public void Buy_DashDamageUp()
        => BuyPart("돌진 데미지 증가", PartType.DashDamageUp, price: 100, value: 1f);

    // ══════════════════════════════════════════
    //  소모품 구매  (가방에 들어감, 1키~로 사용)
    // ══════════════════════════════════════════

    public void Buy_HealPotion()
        => BuyConsumable("회복 포션", ItemType.Heal, price: 50, healAmount: 3, duration: 0);

    public void Buy_InvincibilityPotion()
        => BuyConsumable("무적 포션", ItemType.Invincibility, price: 80, healAmount: 0, duration: 5f);

    // ══════════════════════════════════════════
    //  폭탄 구매  (Q키로 사용)
    // ══════════════════════════════════════════

    public void Buy_Bomb()
        => BuyBomb(price: 60);

    // ══════════════════════════════════════════
    //  가방 판매
    // ══════════════════════════════════════════

    /// <summary>가방 아이템 전부 팔기 — 버튼 OnClick에 연결</summary>
    public void SellAll()
    {
        InventoryManager.Instance?.SellAll();
        RefreshCoin();
        RefreshBagList();
    }

    /// <summary>가방 특정 번호 팔기 (1번 = index 0) — 버튼 OnClick에 연결</summary>
    public void Sell1() => SellAt(0);
    public void Sell2() => SellAt(1);
    public void Sell3() => SellAt(2);
    public void Sell4() => SellAt(3);
    public void Sell5() => SellAt(4);
    public void Sell6() => SellAt(5);
    public void Sell7() => SellAt(6);
    public void Sell8() => SellAt(7);
    public void Sell9() => SellAt(8);
    public void Sell10() => SellAt(9);

    void SellAt(int index)
    {
        var im = InventoryManager.Instance;
        if (im == null || index >= im.GetBag().Count) return;
        im.SellLoot(index);
        RefreshCoin();
        RefreshBagList();
    }

    void RefreshBagList()
    {
        if (bagListText == null) return;
        var im = InventoryManager.Instance;
        if (im == null) return;

        var bag = im.GetBag();
        if (bag.Count == 0)
        {
            bagListText.text = "(가방이 비어있습니다)";
            return;
        }

        string result = "";
        for (int i = 0; i < bag.Count; i++)
            result += $"{i + 1}. {bag[i].DisplayName}  +{bag[i].sellValue}C\n";
        bagListText.text = result;
    }

    // ══════════════════════════════════════════
    //  내부 처리
    // ══════════════════════════════════════════

    void BuyPart(string name, PartType type, int price, float value)
    {
        var im = InventoryManager.Instance;
        var gm = GameManager.Instance;
        if (im == null || gm == null) return;

        if (im.HasPart(type))
        {
            Debug.Log($"[상점] {name} 이미 보유 중");
            return;
        }
        if (gm.coins < price)
        {
            Debug.Log($"[상점] 코인 부족 ({gm.coins}/{price})");
            return;
        }

        // ShopItemData 없이 직접 처리
        gm.SpendCoins(price);
        im.AddPartDirect(type, value);
        Debug.Log($"[상점] {name} 구매 완료!");
        RefreshCoin();
    }

    void BuyConsumable(string name, ItemType type, int price, int healAmount, float duration)
    {
        var im = InventoryManager.Instance;
        var gm = GameManager.Instance;
        if (im == null || gm == null) return;

        if (gm.coins < price)
        {
            Debug.Log($"[상점] 코인 부족 ({gm.coins}/{price})");
            return;
        }

        gm.SpendCoins(price);
        var entry = new ItemEntry
        {
            type       = type,
            itemName   = name,
            healAmount = healAmount,
            duration   = duration
        };
        if (!im.AddItem(entry))
        {
            gm.AddCoins(price); // 슬롯 가득 → 환불
            Debug.Log($"[상점] 아이템 슬롯이 가득 찼습니다.");
        }
        else
        {
            Debug.Log($"[상점] {name} 구매 완료!");
        }
        RefreshCoin();
    }

    void BuyBomb(int price)
    {
        var im = InventoryManager.Instance;
        var gm = GameManager.Instance;
        if (im == null || gm == null) return;

        if (gm.coins < price)
        {
            Debug.Log($"[상점] 코인 부족 ({gm.coins}/{price})");
            return;
        }

        gm.SpendCoins(price);
        if (!im.AddBomb())
        {
            gm.AddCoins(price); // 가득 → 환불
            Debug.Log($"[상점] 폭탄이 가득 찼습니다.");
        }
        else
        {
            Debug.Log($"[상점] 폭탄 구매 완료!");
        }
        RefreshCoin();
    }

    void RefreshCoin()
    {
        if (coinText == null) return;
        int c = GameManager.Instance != null ? GameManager.Instance.coins : 0;
        coinText.text = c + " C";
    }
}
