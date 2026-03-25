using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    [Header("Panels")]
    [SerializeField] private GameObject inventoryPanel;

    [Header("Grid slots (assign in order 0–15)")]
    [SerializeField] private InventorySlotUI[] gridSlots = new InventorySlotUI[16];

    [Header("Equipment slots")]
    [SerializeField] private EquipmentSlotUI helmetSlot;
    [SerializeField] private EquipmentSlotUI chestSlot;
    [SerializeField] private EquipmentSlotUI glovesSlot;
    [SerializeField] private EquipmentSlotUI bootsSlot;
    [SerializeField] private EquipmentSlotUI weaponSlot;

    [Header("Toggle keys")]
    [SerializeField] private KeyCode toggleKey1 = KeyCode.I;
    [SerializeField] private KeyCode toggleKey2 = KeyCode.Tab;

    private bool _isOpen = false;
    public bool IsOpen => _isOpen;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        inventoryPanel.SetActive(false);

        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged += RefreshAll;
            InventorySystem.Instance.OnEquipmentChanged += RefreshAll;
        }
    }

    private void OnDestroy()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged -= RefreshAll;
            InventorySystem.Instance.OnEquipmentChanged -= RefreshAll;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey1) || Input.GetKeyDown(toggleKey2))
            Toggle();
    }


    public void Toggle()
    {
        _isOpen = !_isOpen;
        inventoryPanel.SetActive(_isOpen);

        if (_isOpen)
        {
            RefreshAll();
        }
        else
        {
            ItemTooltipUI.Instance?.Hide();
        }
    }

    public void Close()
    {
        _isOpen = false;
        inventoryPanel.SetActive(false);
        ItemTooltipUI.Instance?.Hide();
    }


    public void RefreshAll()
    {
        for (int i = 0; i < gridSlots.Length; i++)
            if (gridSlots[i] != null) gridSlots[i].Refresh();

        helmetSlot?.Refresh();
        chestSlot?.Refresh();
        glovesSlot?.Refresh();
        bootsSlot?.Refresh();
        weaponSlot?.Refresh();
    }
}