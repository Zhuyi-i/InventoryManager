using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// ─────────────────────────────────────────────
//  EquipmentSlotUI
//  Attach to each equipment slot image.
//  Set slotType in Inspector to match the slot.
//
//  Hierarchy per slot:
//   EquipSlot (Image + this script)
//     └── ItemIcon   (Image)
//     └── SlotLabel  (TMP, e.g. "Helmet")
// ─────────────────────────────────────────────
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

    // ── Refresh visual ────────────────────────

    public void Refresh()
    {
        var inv  = InventorySystem.Instance;
        if (inv == null) return;

        InventorySlot equipSlot = GetEquipSlot(inv);
        bool hasItem = equipSlot != null && !equipSlot.IsEmpty;

        itemIcon.enabled = hasItem;
        if (hasItem) itemIcon.sprite = equipSlot.item.icon;
    }

    // ── Hover ─────────────────────────────────

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

    // ── Drop from inventory grid ──────────────

    public void OnDrop(PointerEventData e)
    {
        // Find which grid slot was dragged
        var dragSrc = e.pointerDrag?.GetComponent<InventorySlotUI>();
        if (dragSrc == null) return;

        var inv = InventorySystem.Instance;
        if (inv == null) return;

        var gridSlot = inv.slots[dragSrc.slotIndex];
        if (gridSlot.IsEmpty) return;

        // Only accept if item type matches this equipment slot
        if (!ItemMatchesSlot(gridSlot.item)) return;

        inv.EquipFromSlot(dragSrc.slotIndex);
        InventoryUI.Instance?.RefreshAll();
    }

    // ── Click to unequip ──────────────────────

    public void OnPointerClick(PointerEventData e)
    {
        var inv = InventorySystem.Instance;
        if (inv == null) return;

        var slot = GetEquipSlot(inv);
        if (slot == null || slot.IsEmpty) return;

        inv.UnequipSlot(slotType);
        InventoryUI.Instance?.RefreshAll();
    }

    // ── Helpers ───────────────────────────────

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
