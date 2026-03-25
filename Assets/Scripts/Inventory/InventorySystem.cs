using UnityEngine;
using System.Collections.Generic;
using System;


[Serializable]
public class InventorySlot
{
    public ItemData item;
    public int quantity;

    public bool IsEmpty => item == null || quantity <= 0;

    public void Clear()
    {
        item = null;
        quantity = 0;
    }
}

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance;

    public const int GRID_SIZE = 16;

    public InventorySlot[] slots = new InventorySlot[GRID_SIZE];

    public InventorySlot helmetSlot  = new InventorySlot();
    public InventorySlot chestSlot   = new InventorySlot();
    public InventorySlot glovesSlot  = new InventorySlot();
    public InventorySlot bootsSlot   = new InventorySlot();
    public InventorySlot weaponSlot  = new InventorySlot();

    public event Action OnInventoryChanged;
    public event Action OnEquipmentChanged;


    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        for (int i = 0; i < GRID_SIZE; i++)
            slots[i] = new InventorySlot();
    }


    public bool AddItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;

        if (item.isStackable)
        {
            foreach (var slot in slots)
            {
                if (!slot.IsEmpty && slot.item == item && slot.quantity < item.maxStackSize)
                {
                    int space = item.maxStackSize - slot.quantity;
                    int add = Mathf.Min(space, amount);
                    slot.quantity += add;
                    amount -= add;
                    if (amount <= 0) { NotifyInventory(); return true; }
                }
            }
        }

        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
            {
                slot.item = item;
                slot.quantity = Mathf.Min(amount, item.maxStackSize);
                amount -= slot.quantity;
                if (amount <= 0) { NotifyInventory(); return true; }
            }
        }

        Debug.LogWarning($"Inventory full – could not add {item.displayName}");
        return false;
    }

    public bool RemoveItem(ItemData item, int amount = 1)
    {
        for (int i = 0; i < GRID_SIZE; i++)
        {
            if (slots[i].item == item)
            {
                slots[i].quantity -= amount;
                if (slots[i].quantity <= 0) slots[i].Clear();
                NotifyInventory();
                return true;
            }
        }
        return false;
    }

    public bool HasItem(ItemData item) =>
        Array.Exists(slots, s => s.item == item && !s.IsEmpty);
    public bool EquipFromSlot(int gridIndex)
    {
        if (gridIndex < 0 || gridIndex >= GRID_SIZE) return false;
        var gridSlot = slots[gridIndex];
        if (gridSlot.IsEmpty) return false;

        InventorySlot equipTarget = GetEquipSlotForItem(gridSlot.item);
        if (equipTarget == null)
        {
            Debug.LogWarning($"{gridSlot.item.displayName} is not equippable.");
            return false;
        }

        if (!equipTarget.IsEmpty)
        {
            ItemData swapBack = equipTarget.item;
            int swapQty = equipTarget.quantity;
            equipTarget.item = gridSlot.item;
            equipTarget.quantity = 1;
            gridSlot.item = swapBack;
            gridSlot.quantity = swapQty;
        }
        else
        {
            equipTarget.item = gridSlot.item;
            equipTarget.quantity = 1;
            gridSlot.Clear();
        }

        NotifyEquipment();
        NotifyInventory();
        return true;
    }

    public bool UnequipSlot(ItemType slotType)
    {
        InventorySlot equipSlot = GetEquipSlotByType(slotType);
        if (equipSlot == null || equipSlot.IsEmpty) return false;

        if (AddItem(equipSlot.item, equipSlot.quantity))
        {
            equipSlot.Clear();
            NotifyEquipment();
            return true;
        }

        Debug.LogWarning("Cannot unequip – inventory full.");
        return false;
    }

    public WeaponType GetEquippedWeaponType() =>
        weaponSlot.IsEmpty ? WeaponType.None : weaponSlot.item.weaponType;


    public void PopulateFromSave(SaveDataFile save)
    {
        foreach (var s in slots) s.Clear();
        helmetSlot.Clear(); chestSlot.Clear();
        glovesSlot.Clear(); bootsSlot.Clear(); weaponSlot.Clear();

        var db = ItemDatabase.Instance;
        if (db == null) return;

        foreach (var saved in save.inventorySlots)
        {
            if (saved.slotIndex < 0 || saved.slotIndex >= GRID_SIZE) continue;
            var item = db.Get(saved.itemID);
            if (item == null) continue;
            slots[saved.slotIndex].item = item;
            slots[saved.slotIndex].quantity = saved.quantity;
        }

        if (save.equipment != null)
        {
            LoadEquipSlot(helmetSlot,  save.equipment.helmetItemID, db);
            LoadEquipSlot(chestSlot,   save.equipment.chestItemID,  db);
            LoadEquipSlot(glovesSlot,  save.equipment.glovesItemID, db);
            LoadEquipSlot(bootsSlot,   save.equipment.bootsItemID,  db);
            LoadEquipSlot(weaponSlot,  save.equipment.weaponItemID, db);
        }

        NotifyInventory();
        NotifyEquipment();
    }

    public void WriteToSave(SaveDataFile save)
    {
        save.inventorySlots.Clear();
        for (int i = 0; i < GRID_SIZE; i++)
        {
            if (!slots[i].IsEmpty)
            {
                save.inventorySlots.Add(new SavedInventorySlot
                {
                    slotIndex = i,
                    itemID    = slots[i].item.itemID,
                    quantity  = slots[i].quantity
                });
            }
        }

        save.equipment = new SavedEquipment
        {
            helmetItemID = helmetSlot.item?.itemID  ?? "",
            chestItemID  = chestSlot.item?.itemID   ?? "",
            glovesItemID = glovesSlot.item?.itemID  ?? "",
            bootsItemID  = bootsSlot.item?.itemID   ?? "",
            weaponItemID = weaponSlot.item?.itemID  ?? "",
            weaponType   = GetEquippedWeaponType().ToString()
        };
    }


    private InventorySlot GetEquipSlotForItem(ItemData item)
    {
        return item.itemType switch
        {
            ItemType.Helmet  => helmetSlot,
            ItemType.Chest   => chestSlot,
            ItemType.Gloves  => glovesSlot,
            ItemType.Boots   => bootsSlot,
            ItemType.Weapon  => weaponSlot,
            _                => null
        };
    }

    private InventorySlot GetEquipSlotByType(ItemType type)
    {
        return type switch
        {
            ItemType.Helmet  => helmetSlot,
            ItemType.Chest   => chestSlot,
            ItemType.Gloves  => glovesSlot,
            ItemType.Boots   => bootsSlot,
            ItemType.Weapon  => weaponSlot,
            _                => null
        };
    }

    private void LoadEquipSlot(InventorySlot slot, string id, ItemDatabase db)
    {
        if (string.IsNullOrEmpty(id)) return;
        var item = db.Get(id);
        if (item != null) { slot.item = item; slot.quantity = 1; }
    }

    private void NotifyInventory() => OnInventoryChanged?.Invoke();
    private void NotifyEquipment() => OnEquipmentChanged?.Invoke();
}
