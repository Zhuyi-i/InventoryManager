using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Chest : MonoBehaviour
{
    [Header("Identity")]
    public string chestID = "chest_01";
    public int mapID = 0;

    [Header("Loot")]
    public ChestData chestData;

    [Header("Visuals")]
    public GameObject openSprite;
    public GameObject closedSprite;
    public GameObject promptObject;  

    private bool _playerInRange = false;
    private bool _isLooted = false;

    private List<SavedChestSlot> _currentItems = new List<SavedChestSlot>();


    public string UID => $"{mapID}_{chestID}";


    private void Start()
    {
        LoadState();
        UpdateVisuals();
        if (promptObject != null) promptObject.SetActive(false);
    }

    private void Update()
    {
        if (!_playerInRange) return;
        if (Keyboard.current.eKey.wasPressedThisFrame)
            OpenChest();
    }

    private void LoadState()
    {
        var saved = ChestManager.Instance?.GetState(UID);

        if (saved != null)
        {
            _isLooted = saved.isOpened && saved.remainingItems.Count == 0;
            _currentItems = new List<SavedChestSlot>(saved.remainingItems);
        }
        else
        {
            _isLooted = false;
            _currentItems = new List<SavedChestSlot>();

            if (chestData != null)
            {
                foreach (var entry in chestData.defaultLoot)
                {
                    if (entry.item == null) continue;
                    _currentItems.Add(new SavedChestSlot
                    {
                        itemID = entry.item.itemID,
                        quantity = entry.quantity
                    });
                }
            }
        }
    }

    private void SaveState()
    {
        var state = new SavedChestState
        {
            chestUID = UID,
            isOpened = true,
            remainingItems = new List<SavedChestSlot>(_currentItems)
        };
        ChestManager.Instance?.SetState(state);
    }


    private void OpenChest()
    {
        UpdateVisuals();
        if (promptObject != null) promptObject.SetActive(false);
        ChestUI.Instance?.Open(this, _currentItems, OnItemTaken, OnTakeAll);
    }

    public void UpdateItemsExternally(List<SavedChestSlot> newItems)
    {
        _currentItems = newItems;
        _isLooted = _currentItems.Count == 0;
        SaveState();
        UpdateVisuals();
    }

    public void OnItemTaken(SavedChestSlot slot)
    {
        var db = ItemDatabase.Instance;
        var inv = InventorySystem.Instance;
        if (db == null || inv == null) return;

        var item = db.Get(slot.itemID);
        if (item == null) return;

        if (inv.AddItem(item, slot.quantity))
        {
            _currentItems.Remove(slot);
            SaveState();
            CheckIfEmpty();
        }
        else
        {
            Debug.LogWarning("Inventory full — item not taken.");
        }
    }

    public void OnItemDeposited(int inventorySlotIndex)
    {
        var inv = InventorySystem.Instance;
        if (inv == null) return;

        var invSlot = inv.slots[inventorySlotIndex];
        if (invSlot == null || invSlot.IsEmpty) return;

        foreach (var cs in _currentItems)
        {
            if (cs.itemID == invSlot.item.itemID && invSlot.item.isStackable)
            {
                int space = invSlot.item.maxStackSize - cs.quantity;
                if (space > 0)
                {
                    int move = Mathf.Min(space, invSlot.quantity);
                    cs.quantity += move;
                    invSlot.quantity -= move;
                    if (invSlot.quantity <= 0) invSlot.Clear();
                    SaveState();
                    InventoryUI.Instance?.RefreshAll();
                    ChestUI.Instance?.RefreshChestSlots(_currentItems);
                    return;
                }
            }
        }

        _currentItems.Add(new SavedChestSlot
        {
            itemID = invSlot.item.itemID,
            quantity = invSlot.quantity
        });
        invSlot.Clear();
        _isLooted = false;  
        SaveState();
        InventoryUI.Instance?.RefreshAll();
        ChestUI.Instance?.RefreshChestSlots(_currentItems);
    }

    public void OnTakeAll()
    {
        var db = ItemDatabase.Instance;
        var inv = InventorySystem.Instance;
        if (db == null || inv == null) return;

        var remaining = new List<SavedChestSlot>();

        foreach (var slot in _currentItems)
        {
            var item = db.Get(slot.itemID);
            if (item == null) continue;

            if (!inv.AddItem(item, slot.quantity))
            {
                remaining.Add(slot);   
                Debug.LogWarning($"Inventory full — could not take {item.displayName}.");
            }
        }

        _currentItems = remaining;
        SaveState();
        CheckIfEmpty();

        ChestUI.Instance?.RefreshChestSlots(_currentItems);
    }

    private void CheckIfEmpty()
    {
        if (_currentItems.Count == 0)
        {
            _isLooted = true;
            ChestManager.Instance?.MarkLooted(UID);
            UpdateVisuals();
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = true;
        if (promptObject != null) promptObject.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = false;
        if (promptObject != null) promptObject.SetActive(false);
        ChestUI.Instance?.Close();
    }


    private void UpdateVisuals()
    {
        if (openSprite != null) openSprite.SetActive(_isLooted);
        if (closedSprite != null) closedSprite.SetActive(!_isLooted);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.85f, 0f, 0.5f);
        Gizmos.DrawWireCube(transform.position, new Vector3(1f, 1f, 0.1f));
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.7f,
            $"Chest: {chestID} (map {mapID})");
#endif
    }
}