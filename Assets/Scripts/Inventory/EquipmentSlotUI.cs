using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class EquipmentSlotUI : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IDropHandler, IPointerClickHandler
{
    [Header("Slot identity")]
    public ItemType slotType;

    [Header("Child refs")]
    [SerializeField] private Image    itemIcon;
    [SerializeField] private TMP_Text slotLabel;

    private void Awake()
    {
        if (slotLabel != null)
            slotLabel.text = slotType.ToString();
    }

    public void Refresh()
    {
        var inv  = InventorySystem.Instance;
        if (inv == null) return;

        InventorySlot equipSlot = GetEquipSlot(inv);
        bool hasItem = equipSlot != null && !equipSlot.IsEmpty;

        itemIcon.enabled = hasItem;
        if (hasItem) itemIcon.sprite = equipSlot.item.icon;
    }

    public void OnPointerEnter(PointerEventData e)
    {
        var inv = InventorySystem.Instance;
        if (inv == null) return;
        var slot = GetEquipSlot(inv);
        if (slot != null && !slot.IsEmpty)
            ItemTooltipUI.Instance?.Show(slot.item);
    }

    public void OnPointerExit(PointerEventData e)
    {
        ItemTooltipUI.Instance?.Hide();
    }

    public void OnDrop(PointerEventData e)
    {
        var dragSrc = e.pointerDrag?.GetComponent<InventorySlotUI>();
        if (dragSrc == null) return;

        var inv = InventorySystem.Instance;
        if (inv == null) return;

        var gridSlot = inv.slots[dragSrc.slotIndex];
        if (gridSlot.IsEmpty) return;

        if (!ItemMatchesSlot(gridSlot.item)) return;

        inv.EquipFromSlot(dragSrc.slotIndex);
        InventoryUI.Instance?.RefreshAll();
    }

    public void OnPointerClick(PointerEventData e)
    {
        var inv = InventorySystem.Instance;
        if (inv == null) return;

        var slot = GetEquipSlot(inv);
        if (slot == null || slot.IsEmpty) return;

        inv.UnequipSlot(slotType);
        InventoryUI.Instance?.RefreshAll();
    }

    private InventorySlot GetEquipSlot(InventorySystem inv)
    {
        return slotType switch
        {
            ItemType.Helmet  => inv.helmetSlot,
            ItemType.Chest   => inv.chestSlot,
            ItemType.Gloves  => inv.glovesSlot,
            ItemType.Boots   => inv.bootsSlot,
            ItemType.Weapon  => inv.weaponSlot,
            _                => null
        };
    }

    private bool ItemMatchesSlot(ItemData item) => item.itemType == slotType;
}
