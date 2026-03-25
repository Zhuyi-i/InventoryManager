using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlotUI : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerClickHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("Slot identity")]
    public int slotIndex;

    [Header("Child refs")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text quantityText;

    public static GameObject _dragGhost;
    public static InventorySlotUI _dragSource;
    private static Canvas _canvas;
    private Image _background;

    private static bool _splitMode = false;

    private void Awake()
    {
        _background = GetComponent<Image>();
        if (_canvas == null)
            _canvas = GetComponentInParent<Canvas>();
    }


    public void Refresh()
    {
        var inv = InventorySystem.Instance;
        if (inv == null) return;

        if (slotIndex < 0 || slotIndex >= InventorySystem.GRID_SIZE)
        {
            Debug.LogWarning($"InventorySlotUI '{gameObject.name}': invalid slotIndex {slotIndex}.");
            return;
        }

        var slot = inv.slots[slotIndex];
        bool empty = slot == null || slot.IsEmpty;

        itemIcon.enabled = !empty;
        quantityText.enabled = !empty;

        if (!empty)
        {
            itemIcon.sprite = slot.item.icon;
            quantityText.text = slot.quantity > 1 ? slot.quantity.ToString() : "";
        }
    }

    public void OnPointerEnter(PointerEventData e)
    {
        if (_dragSource != null) return;
        if (slotIndex < 0 || slotIndex >= InventorySystem.GRID_SIZE) return;

        var slot = InventorySystem.Instance?.slots[slotIndex];
        if (slot != null && !slot.IsEmpty)
            ItemTooltipUI.Instance?.Show(slot.item);
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (_dragSource != null) return;
        ItemTooltipUI.Instance?.Hide();
    }


    private float _lastClickTime = 0f;
    private int _lastClickIndex = -1;
    private const float DOUBLE_CLICK_TIME = 0.3f;

    public void OnPointerClick(PointerEventData e)
    {
        if (e.button == PointerEventData.InputButton.Right)
        {
            SplitStack();
            return;
        }

        if (e.button != PointerEventData.InputButton.Left) return;

        float now = Time.unscaledTime;
        bool isDoubleClick = (slotIndex == _lastClickIndex)
                          && (now - _lastClickTime < DOUBLE_CLICK_TIME);

        _lastClickTime = now;
        _lastClickIndex = slotIndex;

        if (!isDoubleClick) return;

        if (slotIndex < 0 || slotIndex >= InventorySystem.GRID_SIZE) return;
        var inv = InventorySystem.Instance;
        if (inv == null) return;
        var slot = inv.slots[slotIndex];
        if (slot == null || slot.IsEmpty) return;

        if (IsEquippable(slot.item))
        {
            InventorySlot equipSlot = GetEquipSlotForItem(inv, slot.item);
            bool slotOccupied = equipSlot != null && !equipSlot.IsEmpty;

            inv.EquipFromSlot(slotIndex);
            InventoryUI.Instance?.RefreshAll();

            if (slotOccupied && ChestUI.Instance != null && ChestUI.Instance.IsOpen)
            {
                var displaced = inv.slots[slotIndex];
                if (displaced != null && !displaced.IsEmpty)
                {
                    ChestUI.Instance.ReceiveItemFromInventory(displaced, slotIndex);
                    InventoryUI.Instance?.RefreshAll();
                    ChestUI.Instance?.RefreshFromCurrentState();
                }
            }
            return;
        }

        if (ChestUI.Instance != null && ChestUI.Instance.IsOpen)
        {
            ChestUI.Instance.ReceiveItemFromInventory(slot, slotIndex);
            InventoryUI.Instance?.RefreshAll();
            ChestUI.Instance?.RefreshFromCurrentState();
            return;
        }

        if (slot.item.isStackable)
        {
            for (int i = 0; i < InventorySystem.GRID_SIZE; i++)
            {
                if (i == slotIndex) continue;
                var target = inv.slots[i];
                if (target.IsEmpty) continue;
                if (target.item != slot.item) continue;
                if (target.quantity >= slot.item.maxStackSize) continue;

                int space = slot.item.maxStackSize - target.quantity;
                int move = Mathf.Min(space, slot.quantity);
                target.quantity += move;
                slot.quantity -= move;
                if (slot.quantity <= 0) { slot.Clear(); break; }
            }
        }

        if (!slot.IsEmpty)
        {
            for (int i = 0; i < InventorySystem.GRID_SIZE; i++)
            {
                if (i == slotIndex) continue;
                if (inv.slots[i].IsEmpty)
                {
                    inv.slots[i].item = slot.item;
                    inv.slots[i].quantity = slot.quantity;
                    slot.Clear();
                    break;
                }
            }
        }

        InventoryUI.Instance?.RefreshAll();
    }

    private void SplitStack()
    {
        if (slotIndex < 0 || slotIndex >= InventorySystem.GRID_SIZE) return;
        var inv = InventorySystem.Instance;
        if (inv == null) return;
        var slot = inv.slots[slotIndex];
        if (slot == null || slot.IsEmpty) return;
        if (!slot.item.isStackable || slot.quantity <= 1) return;

        int half = slot.quantity / 2;
        for (int i = 0; i < InventorySystem.GRID_SIZE; i++)
        {
            if (i == slotIndex) continue;
            if (inv.slots[i].IsEmpty)
            {
                inv.slots[i].item = slot.item;
                inv.slots[i].quantity = half;
                slot.quantity -= half;
                InventoryUI.Instance?.RefreshAll();
                return;
            }
        }
        Debug.Log("No empty slot to split into.");
    }

    private bool IsEquippable(ItemData item)
    {
        return item.itemType == ItemType.Helmet
            || item.itemType == ItemType.Chest
            || item.itemType == ItemType.Gloves
            || item.itemType == ItemType.Boots
            || item.itemType == ItemType.Weapon;
    }

    private InventorySlot GetEquipSlotForItem(InventorySystem inv, ItemData item)
    {
        return item.itemType switch
        {
            ItemType.Helmet => inv.helmetSlot,
            ItemType.Chest => inv.chestSlot,
            ItemType.Gloves => inv.glovesSlot,
            ItemType.Boots => inv.bootsSlot,
            ItemType.Weapon => inv.weaponSlot,
            _ => null
        };
    }


    public void OnBeginDrag(PointerEventData e)
    {
        if (slotIndex < 0 || slotIndex >= InventorySystem.GRID_SIZE) return;
        var slot = InventorySystem.Instance?.slots[slotIndex];
        if (slot == null || slot.IsEmpty) { e.pointerDrag = null; return; }

        _dragSource = this;
        ItemTooltipUI.Instance?.Hide();

        _dragGhost = new GameObject("DragGhost");
        _dragGhost.transform.SetParent(_canvas.transform, false);
        _dragGhost.transform.SetAsLastSibling();

        var rt = _dragGhost.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(60, 60);

        var img = _dragGhost.AddComponent<Image>();
        img.sprite = slot.item.icon;
        img.raycastTarget = false;

        var cg = _dragGhost.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData e)
    {
        if (_dragGhost == null) return;
        _dragGhost.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (_dragGhost != null) { Destroy(_dragGhost); _dragGhost = null; }
        _dragSource = null;
        InventoryUI.Instance?.RefreshAll();
        ChestUI.Instance?.RefreshFromCurrentState();
    }


    public void OnDrop(PointerEventData e)
    {
        var inv = InventorySystem.Instance;
        if (inv == null) return;

        var chestSrc = e.pointerDrag?.GetComponent<ChestSlotUI>();
        if (chestSrc != null && !chestSrc.IsEmpty)
        {
            chestSrc.TakeItem();
            InventoryUI.Instance?.RefreshAll();
            ChestUI.Instance?.RefreshFromCurrentState();
            return;
        }

        if (_dragSource == null) return;

        int from = _dragSource.slotIndex;
        int to = slotIndex;
        if (from == to) return;

        if (from < 0 || from >= InventorySystem.GRID_SIZE) return;
        if (to < 0 || to >= InventorySystem.GRID_SIZE) return;

        var slotFrom = inv.slots[from];
        var slotTo = inv.slots[to];

        if (slotFrom == null || slotFrom.IsEmpty) return;

        if (!slotTo.IsEmpty
            && slotTo.item == slotFrom.item
            && slotFrom.item.isStackable)
        {
            int space = slotFrom.item.maxStackSize - slotTo.quantity;
            if (space > 0)
            {
                int move = Mathf.Min(space, slotFrom.quantity);
                slotTo.quantity += move;
                slotFrom.quantity -= move;
                if (slotFrom.quantity <= 0) slotFrom.Clear();
            }
            else
            {
                SwapSlots(slotFrom, slotTo);
            }
        }
        else
        {
            SwapSlots(slotFrom, slotTo);
        }

        InventoryUI.Instance?.RefreshAll();
        ChestUI.Instance?.RefreshFromCurrentState();
    }

    private void SwapSlots(InventorySlot a, InventorySlot b)
    {
        var tempItem = a.item;
        var tempQty = a.quantity;
        a.item = b.item;
        a.quantity = b.quantity;
        b.item = tempItem;
        b.quantity = tempQty;
    }
}