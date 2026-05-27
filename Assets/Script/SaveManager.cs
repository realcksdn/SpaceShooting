using System.Collections.Generic;
using System.IO;
using UnityEngine;

// JSON에 저장할 데이터 구조
[System.Serializable]
public class SaveData
{
    public int coins          = 0;
    public int unlockedStages = 1;
    public int score          = 0;
    public List<LootEntry> bag = new List<LootEntry>();
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── 저장 ──────────────────────────────────────
    public void Save()
    {
        var gm = GameManager.Instance;
        var im = InventoryManager.Instance;

        var data = new SaveData
        {
            coins          = gm != null ? gm.coins          : 0,
            unlockedStages = gm != null ? gm.unlockedStages : 1,
            score          = gm != null ? gm.score          : 0,
            bag            = im != null ? new List<LootEntry>(im.GetBag()) : new List<LootEntry>()
        };

        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(SavePath, json);
    }

    // ── 불러오기 ──────────────────────────────────
    public SaveData Load()
    {
        if (!File.Exists(SavePath)) return new SaveData();

        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<SaveData>(json);
    }

    // ── 초기화 ────────────────────────────────────
    public void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }

    // 저장 파일 위치 확인용
    public string GetSavePath() => SavePath;
}
