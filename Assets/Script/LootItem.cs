using Unity.VisualScripting;
using UnityEngine;

// 가방에 들어가는 부산물 데이터
[System.Serializable]
public class LootEntry
{
    public string lootName;
    public int sellValue;
    public int weight;  // 무게 (kg) - 이름 표시용

    public string DisplayName => weight > 0 ? $"{lootName} [{weight}kg]" : lootName;
}

// 적이 죽을 때 드롭하는 부산물 오브젝트
public class LootItem : MonoBehaviour
{
    public LootEntry data;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (InventoryManager.Instance != null)
        {
            // 가방에 여유 있으면 추가, 가득 차면 그냥 버림
            InventoryManager.Instance.AddLoot(data);
            BagUI.Instance?.Refresh();
        }

        Destroy(gameObject); // 항상 제거 (Item.cs와 동일한 방식)
    }
}
