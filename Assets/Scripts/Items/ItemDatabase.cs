using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> items = new List<ItemData>();

    private Dictionary<string, ItemData> _lookup;

    public void Initialize()
    {
        _lookup = new Dictionary<string, ItemData>();
        foreach (var item in items)
        {
            if (item != null && !string.IsNullOrEmpty(item.itemID))
                _lookup[item.itemID] = item;
        }
    }

    public ItemData Get(string id)
    {
        if (_lookup == null) Initialize();
        _lookup.TryGetValue(id, out var result);
        return result;
    }

    private static ItemDatabase _instance;
    public static ItemDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<ItemDatabase>("ItemDatabase");
                if (_instance == null)
                    Debug.LogError("ItemDatabase not found in Resources folder!");
                else
                    _instance.Initialize();
            }
            return _instance;
        }
    }
}
