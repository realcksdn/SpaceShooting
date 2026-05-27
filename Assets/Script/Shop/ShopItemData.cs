using UnityEngine;

public enum ShopCategory { Part, Consumable, Bomb }

public enum PartType
{
    MoveSpeedUp,        // 이동속도 증가
    MaxHpUp,            // 최대 HP 증가
    ReflectRadiusUp,    // 반사 범위 증가
    ReflectCooldownDown,// 반사 쿨타임 감소
    DashDamageUp        // 대쉬 데미지 증가
}

[CreateAssetMenu(fileName = "NewShopItem", menuName = "Shop/ShopItemData")]
public class ShopItemData : ScriptableObject
{
    public string itemName;
    [TextArea(2, 3)] public string description;
    public int price;
    public ShopCategory category;

    [Header("파츠 전용")]
    public PartType partType;
    public float partValue;         // 증가량 (예: 0.2 = 이동속도 20% 증가)

    [Header("소모품 전용 (category=Consumable)")]
    public ItemType consumableType;
    public int healAmount;          // Heal 전용
    public float duration;          // Invincibility / DefenseBoost 전용

    // Bomb은 추가 필드 없음 (category=Bomb이면 폭탄 1개 구매)
}
