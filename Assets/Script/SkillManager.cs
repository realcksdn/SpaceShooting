using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SkillType { None, SpeedBoost, Shield, Reflect }

[System.Serializable]
public class SkillData
{
    public SkillType type;
    public string    skillName;
    public int       price;
    public float     duration;
}

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    [Header("스킬 정보")]
    public List<SkillData> availableSkills = new List<SkillData>
    {
        new SkillData { type = SkillType.SpeedBoost, skillName = "스피드",   price = 200, duration = 4f },
        new SkillData { type = SkillType.Shield,     skillName = "무적",     price = 300, duration = 3f },
        new SkillData { type = SkillType.Reflect,    skillName = "반사",     price = 400, duration = 0f },
    };

    [Header("슬롯")]
    public SkillType slot1 = SkillType.None;
    public SkillType slot2 = SkillType.None;

    [Header("쿨타임")]
    public float slot1Cooldown = 8f;
    public float slot2Cooldown = 8f;

    private HashSet<SkillType> ownedSkills = new HashSet<SkillType>();
    private float slot1Timer = 999f;
    private float slot2Timer = 999f;

    public float Slot1Fill => Mathf.Clamp01(slot1Timer / slot1Cooldown);
    public float Slot2Fill => Mathf.Clamp01(slot2Timer / slot2Cooldown);

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;
        if (SceneManager.GetActiveScene().name == "Shop") return;

        slot1Timer += Time.deltaTime;
        slot2Timer += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Q)) UseSlot(1);
        if (Input.GetKeyDown(KeyCode.R)) UseSlot(2);
    }

    // ── 구매 ──────────────────────────────────
    public bool BuySkill(SkillType type)
    {
        if (ownedSkills.Contains(type)) return false;
        var data = GetData(type);
        if (data == null) return false;
        if (!GameManager.Instance.SpendCoins(data.price)) return false;

        ownedSkills.Add(type);
        return true;
    }

    public bool HasSkill(SkillType type) => ownedSkills.Contains(type);

    // ── 슬롯 장착 ─────────────────────────────
    public void AssignSlot(int slot, SkillType type)
    {
        if (!ownedSkills.Contains(type)) return;
        if (slot == 1) slot1 = type;
        else           slot2 = type;
    }

    // ── 사용 ──────────────────────────────────
    void UseSlot(int slot)
    {
        SkillType type  = slot == 1 ? slot1 : slot2;
        float     timer = slot == 1 ? slot1Timer : slot2Timer;
        float     cd    = slot == 1 ? slot1Cooldown : slot2Cooldown;

        if (type == SkillType.None || timer < cd) return;

        if (slot == 1) slot1Timer = 0f;
        else           slot2Timer = 0f;

        ActivateSkill(type);
    }

    void ActivateSkill(SkillType type)
    {
        var player = GameObject.FindWithTag("Player");
        var ph     = player?.GetComponent<PlayerHealth>();
        var pm     = player?.GetComponent<PlayerMovement>();
        var pr     = player?.GetComponent<PlayerReflect>();
        var data   = GetData(type);

        switch (type)
        {
            case SkillType.SpeedBoost:
                if (pm != null) StartCoroutine(SpeedRoutine(pm, data.duration));
                break;

            case SkillType.Shield:
                ph?.ApplyInvincibility(data.duration);
                break;

            case SkillType.Reflect:
                pr?.ReflectAll();
                break;
        }
    }

    IEnumerator SpeedRoutine(PlayerMovement pm, float duration)
    {
        pm.speedMultiplier += 1f;
        yield return new WaitForSeconds(duration);
        pm.speedMultiplier -= 1f;
    }

    public SkillData GetData(SkillType type) =>
        availableSkills.Find(s => s.type == type);

    public void ResetAll()
    {
        ownedSkills.Clear();
        slot1 = SkillType.None;
        slot2 = SkillType.None;
    }
}
