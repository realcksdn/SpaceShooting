using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상점에서 구매한 파츠(스킬)를 관리하고 플레이어 능력치에 즉시 적용
/// Shop.cs에서 TryBuy() 호출
/// </summary>
public class PlayerParts : MonoBehaviour
{
    private HashSet<PartType> owned = new HashSet<PartType>();

    public bool HasPart(PartType type) => owned.Contains(type);

    /// <summary>코인 차감 후 파츠 적용. 성공 여부 반환</summary>
    public bool TryBuy(ShopItemData data)
    {
        if (data.category != ShopCategory.Part) return false;
        if (owned.Contains(data.partType))       return false; // 이미 보유
        if (GameManager.Instance == null)        return false;
        if (!GameManager.Instance.SpendCoins(data.price)) return false;

        owned.Add(data.partType);
        Apply(data);
        Debug.Log($"[파츠] {data.itemName} 구매 완료!");
        return true;
    }

    void Apply(ShopItemData data)
    {
        switch (data.partType)
        {
            case PartType.MoveSpeedUp:
                var mv = GetComponent<PlayerMovement>();
                if (mv != null) mv.speedMultiplier += data.partValue;
                break;

            case PartType.MaxHpUp:
                var ph = GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    ph.maxHp += (int)data.partValue;
                    ph.Heal((int)data.partValue); // 증가분만큼 즉시 회복
                }
                break;

            case PartType.ReflectRadiusUp:
                var rf = GetComponent<PlayerReflect>();
                if (rf != null) rf.reflectRadius += data.partValue;
                break;

            case PartType.ReflectCooldownDown:
                var rf2 = GetComponent<PlayerReflect>();
                if (rf2 != null) rf2.cooldown = Mathf.Max(0.1f, rf2.cooldown - data.partValue);
                break;

            case PartType.DashDamageUp:
                var dash = GetComponent<PlayerDash>();
                if (dash != null) dash.slamDamage += (int)data.partValue;
                break;
        }
    }
}
