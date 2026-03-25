using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemTooltipUI : MonoBehaviour
{
    public static ItemTooltipUI Instance;

    [Header("Text fields")]
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemTypeText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private TMP_Text descriptionText;

    private Vector2 _offset = new Vector2(4f, -8f);

    private RectTransform _rect;
    private Canvas _canvas;
    private bool _visible = false;
    private ItemData _currentItem;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        _rect = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();

        _rect.pivot = new Vector2(0f, 1f);

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_visible) FollowMouse();
    }


    public void Show(ItemData item)
    {
        if (item == null) { Hide(); return; }

        if (item != _currentItem)
        {
            _currentItem = item;

            itemNameText.text = item.displayName;
            itemTypeText.text = item.itemType.ToString() +
                                (item.itemType == ItemType.Weapon ? $" ({item.weaponType})" : "");

            var sb = new System.Text.StringBuilder();
            if (item.bonusHP != 0) sb.AppendLine($"HP:   {item.bonusHP:+#;-#;0}");
            if (item.bonusAttack != 0) sb.AppendLine($"ATK:  {item.bonusAttack:+#;-#;0}");
            if (item.bonusDefense != 0) sb.AppendLine($"DEF:  {item.bonusDefense:+#;-#;0}");
            if (item.bonusSpeed != 0) sb.AppendLine($"SPD:  {item.bonusSpeed:+#;-#;0}");
            statsText.text = sb.Length > 0 ? sb.ToString().TrimEnd() : "No stat bonuses";
            descriptionText.text = item.description;
        }

        if (!_visible)
        {
            _visible = true;
            gameObject.SetActive(true);
        }

        FollowMouse();
    }

    public void Hide()
    {
        if (!_visible) return;
        _visible = false;
        _currentItem = null;
        gameObject.SetActive(false);
    }


    private void FollowMouse()
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            Input.mousePosition,
            _canvas.worldCamera,
            out pos);

        _rect.anchoredPosition = pos + _offset;
        ClampToScreen();
    }

    private void ClampToScreen()
    {
        var canvasRect = _canvas.transform as RectTransform;
        Vector2 canvasSize = canvasRect.rect.size;

        Vector2 tooltipSize = _rect.rect.size;

        Vector2 pos = _rect.anchoredPosition;

        float minX = -canvasSize.x * 0.5f;
        float maxX = canvasSize.x * 0.5f;
        float minY = -canvasSize.y * 0.5f;
        float maxY = canvasSize.y * 0.5f;
        if (pos.x + tooltipSize.x > maxX) pos.x = maxX - tooltipSize.x;
        if (pos.x < minX) pos.x = minX;
        if (pos.y - tooltipSize.y < minY) pos.y = minY + tooltipSize.y;
        if (pos.y > maxY) pos.y = maxY;

        _rect.anchoredPosition = pos;
    }
}