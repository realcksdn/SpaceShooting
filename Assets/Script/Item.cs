using UnityEngine;

public enum ItemType { Heal, Invincibility, DefenseBoost }

[System.Serializable]
public class ItemEntry
{
    public ItemType type;
    public string itemName;
    public int healAmount;   // Heal 전용
    public float duration;   // Invincibility / DefenseBoost 전용
}

// 월드에 배치되는 소비 아이템 - 플레이어가 닿으면 즉시 발동
public class Item : MonoBehaviour
{
    public ItemEntry data;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // 루트까지 올라가서 PlayerHealth를 찾음
        PlayerHealth ph = other.transform.root.GetComponentInChildren<PlayerHealth>();
        if (ph == null) return;

        // 가방에 넣기 (InventoryManager가 있으면 가방으로, 없으면 즉시 발동)
        if (InventoryManager.Instance != null)
        {
            var entry = new ItemEntry
            {
                type       = data.type,
                itemName   = data.itemName,
                healAmount = data.healAmount,
                duration   = data.duration
            };
            if (InventoryManager.Instance.AddItem(entry))
            {
                Debug.Log($"[아이템] {data.itemName} 가방에 추가");
                Destroy(gameObject);
            }
            // 슬롯 가득 차면 즉시 발동
            else
            {
                UseImmediate(ph);
                Destroy(gameObject);
            }
        }
        else
        {
            UseImmediate(ph);
            Destroy(gameObject);
        }
    }

    void UseImmediate(PlayerHealth ph)
    {
        switch (data.type)
        {
            case ItemType.Heal:          ph.Heal(data.healAmount);           break;
            case ItemType.Invincibility: ph.ApplyInvincibility(data.duration); break;
            case ItemType.DefenseBoost:  ph.ApplyDefenseBoost(data.duration);  break;
        }
        Debug.Log($"[아이템] {data.itemName} 즉시 발동!");
    }
}
