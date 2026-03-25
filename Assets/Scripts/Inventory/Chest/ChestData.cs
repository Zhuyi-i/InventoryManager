using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewChest", menuName = "Inventory/Chest Data")]
public class ChestData : ScriptableObject
{
    public List<ChestLootEntry> defaultLoot = new List<ChestLootEntry>();
}

[Serializable]
public class ChestLootEntry
{
    public ItemData item;
    public int quantity = 1;
}
