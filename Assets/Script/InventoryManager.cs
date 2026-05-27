using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 전환에도 유지되는 인벤토리 싱글턴
/// 가방(부산물) / 아이템 / 폭탄 / 파츠 데이터 보관
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("슬롯 한도")]
    public int bagMaxSlots  = 10;
    public int itemMaxSlots = 5;
    public int maxBombs     = 9;

    // ── 데이터 ───────────────────────────────────
    private List<LootEntry>    bag       = new List<LootEntry>();
    private List<ItemEntry>    items     = new List<ItemEntry>();
    private int                bombCount = 0;

    // 파츠 (구매 목록 보관 → 씬 재로드 시 재적용)
    private HashSet<PartType>    ownedParts    = new HashSet<PartType>();
    private List<ShopItemData>   ownedPartData = new List<ShopItemData>();

    // ── 공개 프로퍼티 ────────────────────────────
    public int BagCount   => bag.Count;
    public int ItemCount  => items.Count;
    public int BombCount  => bombCount;

    // ══════════════════════════════════════════════
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 상점 씬이 아닌 경우(= 게임 씬) 파츠 효과 재적용
        if (scene.name != "Shop")
            StartCoroutine(ReapplyPartsNextFrame());
    }

    IEnumerator ReapplyPartsNextFrame()
    {
        yield return null; // 플레이어 Start() 완료 대기
        var player = GameObject.FindWithTag("Player");
        if (player == null) yield break;
        foreach (var data in ownedPartData)
            ApplyPartToPlayer(data, player);
    }

    // ── 게임 씬 입력 (1~5 아이템, Q 폭탄) ────────
    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;
        if (SceneManager.GetActiveScene().name == "Shop") return;

        for (int i = 0; i < items.Count && i < 5; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                UseItem(i);
                break;
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
            UseBomb();
    }

    // ══════════════════════════════════════════════
    //  가방
    // ══════════════════════════════════════════════
    public List<LootEntry> GetBag() => bag;

    public bool AddLoot(LootEntry entry)
    {
        if (bag.Count >= bagMaxSlots) return false;
        bag.Add(entry);
        return true;
    }

    public void SellLoot(int index)
    {
        if (index < 0 || index >= bag.Count) return;
        var loot = bag[index];
        bag.RemoveAt(index);
        GameManager.Instance?.AddCoins(loot.sellValue);
    }

    /// <summary>가방에서 부산물 버리기 (코인 없이 제거)</summary>
    public void DropLoot(int index)
    {
        if (index < 0 || index >= bag.Count) return;
        bag.RemoveAt(index);
    }

    public void SellAll()
    {
        int total = 0;
        foreach (var l in bag) total += l.sellValue;
        bag.Clear();
        GameManager.Instance?.AddCoins(total);
    }

    // ══════════════════════════════════════════════
    //  아이템
    // ══════════════════════════════════════════════
    public List<ItemEntry> GetItems() => items;

    public bool AddItem(ItemEntry entry)
    {
        if (items.Count >= itemMaxSlots) return false;
        items.Add(entry);
        return true;
    }

    /// <summary>BagUI에서 외부 호출용</summary>
    public void UseItemPublic(int index) => UseItem(index);

    void UseItem(int index)
    {
        if (index < 0 || index >= items.Count) return;
        var entry = items[index];
        items.RemoveAt(index);

        var player = GameObject.FindWithTag("Player");
        var ph = player?.GetComponent<PlayerHealth>();
        if (ph == null) return;

        switch (entry.type)
        {
            case ItemType.Heal:          ph.Heal(entry.healAmount);          break;
            case ItemType.Invincibility: ph.ApplyInvincibility(entry.duration); break;
            case ItemType.DefenseBoost:  ph.ApplyDefenseBoost(entry.duration);  break;
        }
    }

    // ══════════════════════════════════════════════
    //  폭탄
    // ══════════════════════════════════════════════
    public bool AddBomb()
    {
        if (bombCount >= maxBombs) return false;
        bombCount++;
        return true;
    }

    void UseBomb()
    {
        if (bombCount <= 0) return;
        bombCount--;
        var bullets = FindObjectsByType<Bullet>(FindObjectsSortMode.None);
        foreach (var b in bullets) Destroy(b.gameObject);
    }

    // ══════════════════════════════════════════════
    //  파츠
    // ══════════════════════════════════════════════
    public bool HasPart(PartType type) => ownedParts.Contains(type);

    /// <summary>ShopItemData 없이 파츠를 직접 추가 (ShopUI 전용)</summary>
    public void AddPartDirect(PartType type, float value)
    {
        if (ownedParts.Contains(type)) return;
        ownedParts.Add(type);

        var data = ScriptableObject.CreateInstance<ShopItemData>();
        data.partType  = type;
        data.partValue = value;
        ownedPartData.Add(data);

        var player = GameObject.FindWithTag("Player");
        if (player != null) ApplyPartToPlayer(data, player);
    }

    public bool BuyPart(ShopItemData data)
    {
        if (ownedParts.Contains(data.partType)) return false;
        if (GameManager.Instance == null || !GameManager.Instance.SpendCoins(data.price)) return false;

        ownedParts.Add(data.partType);
        ownedPartData.Add(data);

        // 현재 게임 씬 플레이어에 즉시 적용
        var player = GameObject.FindWithTag("Player");
        if (player != null) ApplyPartToPlayer(data, player);

        return true;
    }

    void ApplyPartToPlayer(ShopItemData data, GameObject player)
    {
        switch (data.partType)
        {
            case PartType.MoveSpeedUp:
                var mv = player.GetComponent<PlayerMovement>();
                if (mv != null) mv.speedMultiplier += data.partValue;
                break;
            case PartType.MaxHpUp:
                var ph = player.GetComponent<PlayerHealth>();
                if (ph != null) { ph.maxHp += (int)data.partValue; ph.Heal((int)data.partValue); }
                break;
            case PartType.ReflectRadiusUp:
                var rf = player.GetComponent<PlayerReflect>();
                if (rf != null) rf.reflectRadius += data.partValue;
                break;
            case PartType.ReflectCooldownDown:
                var rf2 = player.GetComponent<PlayerReflect>();
                if (rf2 != null) rf2.cooldown = Mathf.Max(0.1f, rf2.cooldown - data.partValue);
                break;
            case PartType.DashDamageUp:
                var dash = player.GetComponent<PlayerDash>();
                if (dash != null) dash.slamDamage += (int)data.partValue;
                break;
        }
    }

    // ── HUD용 (Inventory.cs에서 호출) ────────────
    public string GetItemName(int index) => index < items.Count ? items[index].itemName : "";

    /// <summary>게임 초기화 시 가방·아이템 전부 비움</summary>
    public void ResetAll()
    {
        bag.Clear();
        items.Clear();
        ownedPartData.Clear();
    }
}
