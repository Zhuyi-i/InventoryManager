using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Base Stats")]
    public int baseMaxHP   = 100;
    public int baseAttack  = 10;
    public int baseDefense = 5;
    public int baseSpeed   = 5;

    public int MaxHP    { get; private set; }
    public int Attack   { get; private set; }
    public int Defense  { get; private set; }
    public int Speed    { get; private set; }

    public int currentHP;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.OnEquipmentChanged += RecalculateStats;

        RecalculateStats();
    }

    private void OnDestroy()
    {
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.OnEquipmentChanged -= RecalculateStats;
    }


    public void LoadFromSave(SaveDataFile save)
    {
        baseMaxHP = save.maxHP;
        currentHP = save.currentHP;
        RecalculateStats();
    }


    public void RecalculateStats()
    {
        int bonusHP = 0, bonusAtk = 0, bonusDef = 0, bonusSpd = 0;

        if (InventorySystem.Instance != null)
        {
            var inv = InventorySystem.Instance;
            AccumBonus(inv.helmetSlot,  ref bonusHP, ref bonusAtk, ref bonusDef, ref bonusSpd);
            AccumBonus(inv.chestSlot,   ref bonusHP, ref bonusAtk, ref bonusDef, ref bonusSpd);
            AccumBonus(inv.glovesSlot,  ref bonusHP, ref bonusAtk, ref bonusDef, ref bonusSpd);
            AccumBonus(inv.bootsSlot,   ref bonusHP, ref bonusAtk, ref bonusDef, ref bonusSpd);
            AccumBonus(inv.weaponSlot,  ref bonusHP, ref bonusAtk, ref bonusDef, ref bonusSpd);
        }

        MaxHP   = baseMaxHP   + bonusHP;
        Attack  = baseAttack  + bonusAtk;
        Defense = baseDefense + bonusDef;
        Speed   = baseSpeed   + bonusSpd;

        currentHP = Mathf.Clamp(currentHP, 0, MaxHP);
    }


    public void TakeDamage(int raw)
    {
        int dmg = Mathf.Max(1, raw - Defense);
        currentHP = Mathf.Max(0, currentHP - dmg);
        Debug.Log($"Player took {dmg} damage. HP: {currentHP}/{MaxHP}");

        if (currentHP <= 0) OnDeath();
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Min(MaxHP, currentHP + amount);
    }

    private void OnDeath()
    {
        Debug.Log("Player died.");
        // TODO: trigger death animation / game-over screen
    }


    private void AccumBonus(InventorySlot slot,
        ref int hp, ref int atk, ref int def, ref int spd)
    {
        if (slot == null || slot.IsEmpty) return;
        hp  += slot.item.bonusHP;
        atk += slot.item.bonusAttack;
        def += slot.item.bonusDefense;
        spd += slot.item.bonusSpeed;
    }
}
