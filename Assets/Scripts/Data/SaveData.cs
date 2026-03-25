using System.IO;
using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class SavedInventorySlot
{
    public int slotIndex;
    public string itemID;   
    public int quantity;
}

[Serializable]
public class SavedEquipment
{
    public string helmetItemID;
    public string chestItemID;
    public string glovesItemID;
    public string bootsItemID;
    public string weaponItemID;
    public string weaponType;   
}

[Serializable]
public class SaveDataFile
{
    public string playerName;
    public bool isNewGame = true;
    public string lastSaved;   

    public int currentHP;
    public int maxHP = 100;

    public int currentMapID;
    public string currentSceneName;
    public int spawnDoorID;

    public float playerX;
    public float playerY;
    public bool hasPositionSave;  

    public List<SavedInventorySlot> inventorySlots = new List<SavedInventorySlot>();
    public SavedEquipment equipment = new SavedEquipment();

    public List<SavedChestState> chestStates = new List<SavedChestState>();
}

public static class SaveDataSystem
{
    private static string GetSaveFilePath(int slotIndex)
    {
        string saveFolder = GetSaveFolderPath();
        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);
        return Path.Combine(saveFolder, $"saveData_slot{slotIndex}.json");
    }

    public static void Save(SaveDataFile dataFile, int slotIndex)
    {
        dataFile.lastSaved = DateTime.Now.ToString("MM/dd/yyyy HH:mm");
        string json = JsonUtility.ToJson(dataFile, true);
        File.WriteAllText(GetSaveFilePath(slotIndex), json);
        Debug.Log($"Game saved to slot {slotIndex}");
    }

    public static SaveDataFile Load(int slotIndex)
    {
        string path = GetSaveFilePath(slotIndex);
        if (!File.Exists(path)) return null;
        try
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveDataFile>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load save slot {slotIndex}: {e.Message}");
            return null;
        }
    }

    public static bool SaveExists(int slotIndex) => File.Exists(GetSaveFilePath(slotIndex));

    public static void DeleteSave(int slotIndex)
    {
        string path = GetSaveFilePath(slotIndex);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"Deleted save slot {slotIndex}");
        }
    }

    private static string GetSaveFolderPath()
    {
#if UNITY_EDITOR
        return Path.Combine(Application.dataPath, "..", "GameSaves");
#elif UNITY_STANDALONE_WIN
        return Path.Combine(Path.GetDirectoryName(Application.dataPath), "GameSaves");
#else
        return Application.persistentDataPath;
#endif
    }
}