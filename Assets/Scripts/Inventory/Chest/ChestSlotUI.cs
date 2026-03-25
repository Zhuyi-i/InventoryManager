using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ChestSlotUI : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerClickHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text quantityText;

    private SavedChestSlot _slot;
    private ItemData _item;
    private Action<SavedChestSlot> _onTaken;
    private bool _isEmpty = true;

    private static GameObject _dragGhost;
    private Canvas _canvas;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
    }

    public void SetItem(SavedChestSlot slot, ItemData item, Action<SavedChestSlot> onTaken)
    {
        _slot = slot;
        _item = item;
        _onTaken = onTaken;
        _isEmpty = false;

        if (itemIcon != null)
        {
            itemIcon.sprite = item.icon;
            itemIcon.enabled = true;
        }
        if (quantityText != null)
            quantityText.text = slot.quantity > 1 ? slot.quantity.ToString() : "";
    }

    public void Clear()
    {
        _slot = null;
        _item = null;
        _onTaken = null;
        _isEmpty = true;

        if (itemIcon != null) itemIcon.enabled = false;
        if (quantityText != null) quantityText.text = "";
    }

    public bool IsEmpty => _isEmpty;


    private float _lastClickTime = 0f;
    private const float DOUBLE_CLICK_TIME = 0.3f;

    public void OnPointerClick(PointerEventData e)
    {
        if (e.button != PointerEventData.InputButton.Left) return;
        if (_isEmpty) return;

        float now = Time.unscaledTime;
        bool isDoubleClick = now - _lastClickTime < DOUBLE_CLICK_TIME;
        _lastClickTime = now;

        if (!isDoubleClick) return;

        var inv = InventorySystem.Instance;
        if (inv == null) return;

        bool hasSpace = false;

        if (_item.isStackable)
        {
            for (int i = 0; i < InventorySystem.GRID_SIZE; i++)
            {
                var t = inv.slots[i];
                if (!t.IsEmpty && t.item == _item && t.quantity < _item.maxStackSize)
                { hasSpace = true; break; }
            }
        }

        if (!hasSpace)
        {
            for (int i = 0; i < InventorySystem.GRID_SIZE; i++)
            {
                if (inv.slots[i].IsEmpty) { hasSpace = true; break; }
            }
        }

        if (!hasSpace)
        {
            Debug.Log("Inventory full — could not transfer item from chest.");
            return;
        }

        _onTaken?.Invoke(_slot);
        InventoryUI.Instance?.RefreshAll();
        ChestUI.Instance?.RefreshFromCurrentState();
    }

    public void OnPointerEnter(PointerEventData e)
    {
        if (InventorySlotUI._dragSource != null || _dragGhost != null) return;
        if (!_isEmpty && _item != null)
            ItemTooltipUI.Instance?.Show(_item);
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (InventorySlotUI._dragSource != null || _dragGhost != null) return;
        ItemTooltipUI.Instance?.Hide();
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (_isEmpty) { e.pointerDrag = null; return; }

        ItemTooltipUI.Instance?.Hide();

        _dragGhost = new GameObject("ChestDragGhost");
        _dragGhost.transform.SetParent(_canvas.transform, false);
        _dragGhost.transform.SetAsLastSibling();

        var rt = _dragGhost.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(60, 60);

        var img = _dragGhost.AddComponent<Image>();
        img.sprite = _item?.icon;
        img.raycastTarget = false;

        var cg = _dragGhost.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData e)
    {
        if (_dragGhost != null)
            _dragGhost.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (_dragGhost != null) { Destroy(_dragGhost); _dragGhost = null; }
        ChestUI.Instance?.RefreshFromCurrentState();
        InventoryUI.Instance?.RefreshAll();
    }

    public void OnDrop(PointerEventData e)
    {
        var invSrc = e.pointerDrag?.GetComponent<InventorySlotUI>();
        if (invSrc == null) return;

        ChestUI.Instance?.ReceiveItemFromInventory(
            InventorySystem.Instance?.slots[invSrc.slotIndex],
            invSrc.slotIndex);
    }

    public void TakeItem()
    {
        if (_isEmpty) return;
        _onTaken?.Invoke(_slot);
    }
}