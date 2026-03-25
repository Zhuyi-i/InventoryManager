using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChestUI : MonoBehaviour
{
    public static ChestUI Instance;

    [Header("References")]
    [SerializeField] private GameObject chestPanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button takeAllButton;
    [SerializeField] private Button closeButton;

    [Header("Chest slots (assign all in order)")]
    [SerializeField] private ChestSlotUI[] chestSlots;

    private Chest _currentChest;
    private Action<SavedChestSlot> _onItemTaken;
    private Action _onTakeAll;
    private List<SavedChestSlot> _currentItems;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        chestPanel.SetActive(false);
        takeAllButton?.onClick.AddListener(OnTakeAllClicked);
        closeButton?.onClick.AddListener(Close);
    }


    public void Open(Chest chest,
                     List<SavedChestSlot> items,
                     Action<SavedChestSlot> onItemTaken,
                     Action onTakeAll)
    {
        _currentChest = chest;
        _onItemTaken = onItemTaken;
        _onTakeAll = onTakeAll;
        _currentItems = items;

        if (titleText != null) titleText.text = "Chest";

        RefreshChestSlots(items);
        chestPanel.SetActive(true);

        if (InventoryUI.Instance != null && !InventoryUI.Instance.IsOpen)
            InventoryUI.Instance.Toggle();
    }

    public void Close()
    {
        chestPanel.SetActive(false);
        _currentChest = null;
        ClearAllSlots();
    }

    public void RefreshChestSlots(List<SavedChestSlot> items)
    {
        _currentItems = items;
        var db = ItemDatabase.Instance;
        ClearAllSlots();

        for (int i = 0; i < chestSlots.Length; i++)
        {
            if (chestSlots[i] == null) continue;
            if (i < items.Count)
            {
                var item = db?.Get(items[i].itemID);
                if (item != null)
                    chestSlots[i].SetItem(items[i], item, OnSlotTaken);
            }
        }
    }

    public void RefreshFromCurrentState()
    {
        if (_currentItems != null)
            RefreshChestSlots(_currentItems);
    }


    public void ReceiveItemFromInventory(InventorySlot invSlot, int invSlotIndex)
    {
        if (_currentChest == null || invSlot == null || invSlot.IsEmpty) return;
        if (_currentItems == null) _currentItems = new List<SavedChestSlot>();

        var existing = _currentItems.Find(s => s.itemID == invSlot.item.itemID);
        if (existing != null && invSlot.item.isStackable)
        {
            existing.quantity += invSlot.quantity;
        }
        else
        {
            _currentItems.Add(new SavedChestSlot
            {
                itemID = invSlot.item.itemID,
                quantity = invSlot.quantity
            });
        }

        var inv = InventorySystem.Instance;
        if (inv != null) inv.slots[invSlotIndex].Clear();

        _currentChest.UpdateItemsExternally(_currentItems);

        RefreshChestSlots(_currentItems);
        InventoryUI.Instance?.RefreshAll();
    }

    public bool IsOpen => chestPanel.activeSelf;
    public Chest CurrentChest => _currentChest;


    private void OnSlotTaken(SavedChestSlot slot)
    {
        _onItemTaken?.Invoke(slot);
    }

    private void OnTakeAllClicked()
    {
        _onTakeAll?.Invoke();
    }

    private void ClearAllSlots()
    {
        if (chestSlots == null) return;
        foreach (var s in chestSlots) s?.Clear();
    }
}