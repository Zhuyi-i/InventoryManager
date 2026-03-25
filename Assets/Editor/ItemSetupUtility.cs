using UnityEngine;
using UnityEditor;
using System.IO;

public class ItemSetupUtility : EditorWindow
{
    [MenuItem("Tools/Setup Placeholder Items")]
    public static void ShowWindow()
    {
        GetWindow<ItemSetupUtility>("Item Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("Placeholder Item Generator", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Creates placeholder ItemData assets in Assets/Items/ and registers them in the ItemDatabase at Assets/Resources/ItemDatabase.asset.",
            MessageType.Info);

        GUILayout.Space(10);

        if (GUILayout.Button("Generate All Placeholder Items", GUILayout.Height(40)))
        {
            GenerateItems();
        }
    }

    private static void GenerateItems()
    {
        EnsureFolder("Assets/Items");
        EnsureFolder("Assets/Items/Armor");
        EnsureFolder("Assets/Items/Misc");
        EnsureFolder("Assets/Resources");

        var items = new System.Collections.Generic.List<ItemData>();

        items.Add(MakeItem("Assets/Items/Armor/IronHelmet.asset",
            "iron_helmet", "Iron Helmet", ItemType.Helmet, WeaponType.None,
            "A sturdy iron helmet.", bonusHP: 10, bonusDef: 5));

        items.Add(MakeItem("Assets/Items/Armor/LeatherHelmet.asset",
            "leather_helmet", "Leather Helmet", ItemType.Helmet, WeaponType.None,
            "A light leather cap.", bonusHP: 5, bonusDef: 2));

        items.Add(MakeItem("Assets/Items/Armor/IronChest.asset",
            "iron_chest", "Iron Chestplate", ItemType.Chest, WeaponType.None,
            "Heavy iron chest armor.", bonusHP: 20, bonusDef: 10));

        items.Add(MakeItem("Assets/Items/Armor/LeatherChest.asset",
            "leather_chest", "Leather Vest", ItemType.Chest, WeaponType.None,
            "Flexible leather armor.", bonusHP: 10, bonusDef: 5, bonusSpeed: 1));

        items.Add(MakeItem("Assets/Items/Armor/IronGloves.asset",
            "iron_gloves", "Iron Gauntlets", ItemType.Gloves, WeaponType.None,
            "Heavy iron gauntlets.", bonusDef: 3, bonusAttack: 2));

        items.Add(MakeItem("Assets/Items/Armor/LeatherGloves.asset",
            "leather_gloves", "Leather Gloves", ItemType.Gloves, WeaponType.None,
            "Light leather gloves.", bonusDef: 1, bonusAttack: 1, bonusSpeed: 1));

        items.Add(MakeItem("Assets/Items/Armor/IronBoots.asset",
            "iron_boots", "Iron Boots", ItemType.Boots, WeaponType.None,
            "Heavy iron boots.", bonusDef: 4));

        items.Add(MakeItem("Assets/Items/Armor/LeatherBoots.asset",
            "leather_boots", "Leather Boots", ItemType.Boots, WeaponType.None,
            "Light leather boots.", bonusDef: 2, bonusSpeed: 2));

        items.Add(MakeItem("Assets/Items/Misc/OldKey.asset",
            "old_key", "Old Key", ItemType.Misc, WeaponType.None,
            "A rusty key. Opens something.", isStackable: false));

        items.Add(MakeItem("Assets/Items/Misc/GoldCoin.asset",
            "gold_coin", "Gold Coin", ItemType.Misc, WeaponType.None,
            "Shiny gold coin.", isStackable: true, maxStack: 99));

        items.Add(MakeItem("Assets/Items/Misc/MysteryOrb.asset",
            "mystery_orb", "Mystery Orb", ItemType.Misc, WeaponType.None,
            "Glows faintly. Purpose unknown.", isStackable: false));

        string dbPath = "Assets/Resources/ItemDatabase.asset";
        ItemDatabase db = AssetDatabase.LoadAssetAtPath<ItemDatabase>(dbPath);

        if (db == null)
        {
            db = ScriptableObject.CreateInstance<ItemDatabase>();
            AssetDatabase.CreateAsset(db, dbPath);
            Debug.Log("Created new ItemDatabase at " + dbPath);
        }

        db.items.Clear();
        db.items.AddRange(items);

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Item setup complete — {items.Count} items created and added to ItemDatabase.");
        EditorUtility.DisplayDialog("Done!",
            $"{items.Count} placeholder items created.\nItemDatabase populated at Assets/Resources/ItemDatabase.asset.",
            "OK");
    }


    private static ItemData MakeItem(
        string path,
        string id,
        string displayName,
        ItemType type,
        WeaponType weaponType,
        string description,
        int bonusHP = 0, int bonusAttack = 0, int bonusDef = 0, int bonusSpeed = 0,
        bool isStackable = false, int maxStack = 1)
    {
        ItemData existing = AssetDatabase.LoadAssetAtPath<ItemData>(path);
        if (existing != null)
        {
            Debug.Log($"Skipping existing item: {path}");
            return existing;
        }

        var item = ScriptableObject.CreateInstance<ItemData>();
        item.itemID       = id;
        item.displayName  = displayName;
        item.description  = description;
        item.itemType     = type;
        item.weaponType   = weaponType;
        item.bonusHP      = bonusHP;
        item.bonusAttack  = bonusAttack;
        item.bonusDefense = bonusDef;
        item.bonusSpeed   = bonusSpeed;
        item.isStackable  = isStackable;
        item.maxStackSize = maxStack;
        item.icon         = null;   // assign sprites later

        AssetDatabase.CreateAsset(item, path);
        return item;
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string folder = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
