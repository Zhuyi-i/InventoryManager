using UnityEngine;


public enum ItemType
{
    Consumable,
    Weapon,
    Helmet,
    Chest,
    Gloves,
    Boots,
    Misc
}

public enum WeaponType
{
    None,
    Melee,
    Ranged
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Identity")]
    public string itemID;           
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Type")]
    public ItemType itemType;
    public WeaponType weaponType;  

    [Header("Stack")]
    public bool isStackable = false;
    public int maxStackSize = 1;

    [Header("Stats (applied when equipped / in inventory)")]
    public int bonusHP;
    public int bonusAttack;
    public int bonusDefense;
    public int bonusSpeed;
}