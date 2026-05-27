using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Canvas 안에 BagPanel, BagPanel 안에 Text를 만들어두면
/// 이름으로 자동 탐색합니다. Inspector 연결 불필요.
/// B키 열고닫기 / T키 테스트 아이템 추가
/// </summary>
public class BagUI : MonoBehaviour
{
    public static BagUI Instance { get; private set; }

    // Inspector에서 직접 연결해도 되고, 비워두면 이름으로 자동 탐색
    public GameObject bagPanel;
    public Text       bagText;

    void Awake() => Instance = this;

    void Start()
    {
        // 비어있으면 이름으로 자동 탐색
        if (bagPanel == null)
        {
            var go = GameObject.Find("BagPanel");
            if (go != null) bagPanel = go;
            else Debug.LogWarning("[BagUI] 'BagPanel' 이름의 오브젝트를 찾지 못했습니다.");
        }

        if (bagText == null && bagPanel != null)
        {
            bagText = bagPanel.GetComponentInChildren<Text>();
            if (bagText == null)
                Debug.LogWarning("[BagUI] BagPanel 안에 Text 컴포넌트가 없습니다.");
        }

        if (InventoryManager.Instance == null)
            Debug.LogWarning("[BagUI] InventoryManager.Instance 가 없습니다. 씬에 InventoryManager 오브젝트를 추가하세요.");

        if (bagPanel != null) bagPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B)) Toggle();

        // T키: 테스트 아이템 강제 추가
        if (Input.GetKeyDown(KeyCode.T))
        {
            var im = InventoryManager.Instance;
            if (im == null) { Debug.LogError("[BagUI] InventoryManager 없음"); return; }
            im.AddLoot(new LootEntry { lootName = "테스트부품", sellValue = 50, weight = 2 });
            Debug.Log($"[BagUI] 테스트 아이템 추가. 현재 가방: {im.BagCount}개");
            if (bagPanel != null && bagPanel.activeSelf) Refresh();
        }
    }

    public void Toggle()
    {
        // 씬 재로드 후 참조가 끊어졌을 수 있으니 다시 탐색
        if (bagPanel == null) bagPanel = GameObject.Find("BagPanel");
        if (bagPanel == null) { Debug.LogError("[BagUI] BagPanel을 찾을 수 없습니다."); return; }
        if (bagText  == null) bagText  = bagPanel.GetComponentInChildren<Text>();

        bool next = !bagPanel.activeSelf;
        bagPanel.SetActive(next);
        if (next) Refresh();
    }

    public void Refresh()
    {
        if (bagPanel == null) bagPanel = GameObject.Find("BagPanel");
        if (bagText  == null && bagPanel != null) bagText = bagPanel.GetComponentInChildren<Text>();
        if (bagText  == null) return;

        var im = InventoryManager.Instance;
        if (im == null) { bagText.text = "InventoryManager 없음"; return; }

        var bag = im.GetBag();

        if (bag.Count == 0)
        {
            bagText.text = "가방이 비어있습니다.";
            return;
        }

        string result = $"가방  [{bag.Count}/{im.bagMaxSlots}]\n\n";
        for (int i = 0; i < bag.Count; i++)
            result += $"{i + 1}.  {bag[i].DisplayName}   +{bag[i].sellValue}C\n";

        bagText.text = result;
    }

    public void Open()  { if (bagPanel != null) { bagPanel.SetActive(true);  Refresh(); } }
    public void Close() { if (bagPanel != null)   bagPanel.SetActive(false); }
}
