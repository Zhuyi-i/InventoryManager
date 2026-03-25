using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSaveManager : MonoBehaviour
{
    public static GameSaveManager Instance;

    private int currentSlot = -1;
    private SaveDataFile currentSave;

    private PlayerController _playerController;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        currentSlot = PlayerPrefs.GetInt("CurrentSaveSlot", 0);
        currentSave = SaveDataSystem.Load(currentSlot);

        if (currentSave == null)
        {
            Debug.LogWarning($"No save found for slot {currentSlot}, creating default.");
            currentSave = MakeDefaultSave("Player");
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        _playerController = playerGO != null
            ? playerGO.GetComponent<PlayerController>()
            : null;

        if (_playerController == null)
            Debug.Log($"GameSaveManager: PlayerController not found in '{scene.name}' — OK if main menu.");

        ApplySaveToGame();
    }

    private void ApplySaveToGame()
    {
        if (currentSave == null) return;
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.PopulateFromSave(currentSave);
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.LoadFromSave(currentSave);
        if (ChestManager.Instance != null)
            ChestManager.Instance.LoadFromSave(currentSave);
    }

    public void LoadSlot(int slotIndex)
    {
        currentSlot = slotIndex;
        PlayerPrefs.SetInt("CurrentSaveSlot", slotIndex);
        PlayerPrefs.Save();

        currentSave = SaveDataSystem.Load(slotIndex);
        if (currentSave == null)
            currentSave = MakeDefaultSave("Player");

        if (InventorySystem.Instance != null)
            InventorySystem.Instance.PopulateFromSave(currentSave);
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.LoadFromSave(currentSave);
        if (ChestManager.Instance != null)
            ChestManager.Instance.LoadFromSave(currentSave);

        MapNavigation.Instance?.ResetMap();
    }

    public void StartNewGame(int slotIndex, string playerName)
    {
        currentSlot = slotIndex;
        PlayerPrefs.SetInt("CurrentSaveSlot", slotIndex);
        PlayerPrefs.Save();

        currentSave = MakeDefaultSave(playerName);
        SaveDataSystem.Save(currentSave, slotIndex);

        if (InventorySystem.Instance != null)
            InventorySystem.Instance.PopulateFromSave(currentSave);
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.LoadFromSave(currentSave);
        if (ChestManager.Instance != null)
            ChestManager.Instance.LoadFromSave(currentSave);

        MapNavigation.Instance?.ResetMap();
    }

    public void SaveOnMapEnter(int newMapID, string newSceneName, int spawnDoorID, Vector2 spawnPosition)
    {
        if (currentSave == null || currentSlot < 0) return;

        currentSave.currentMapID = newMapID;
        currentSave.currentSceneName = newSceneName;
        currentSave.spawnDoorID = spawnDoorID;
        currentSave.isNewGame = false;

        currentSave.playerX = spawnPosition.x;
        currentSave.playerY = spawnPosition.y;
        currentSave.hasPositionSave = true;
        Debug.Log($"GameSaveManager: Saved spawn position ({spawnPosition.x}, {spawnPosition.y}) in map {newMapID}");

        if (PlayerStats.Instance != null)
        {
            currentSave.currentHP = PlayerStats.Instance.currentHP;
            currentSave.maxHP = PlayerStats.Instance.MaxHP;
        }

        if (InventorySystem.Instance != null)
            InventorySystem.Instance.WriteToSave(currentSave);
        if (ChestManager.Instance != null)
            ChestManager.Instance.WriteToSave(currentSave);

        SaveDataSystem.Save(currentSave, currentSlot);
        Debug.Log($"Auto-saved: map {newMapID}, scene '{newSceneName}', door {spawnDoorID}");
    }

    public void SaveNow()
    {
        if (currentSave == null || currentSlot < 0) return;

        currentSave.currentSceneName = SceneManager.GetActiveScene().name;
        currentSave.isNewGame = false;

        if (_playerController != null)
        {
            currentSave.playerX = _playerController.LastGroundedPosition.x;
            currentSave.playerY = _playerController.LastGroundedPosition.y;
            currentSave.hasPositionSave = true;
        }

        if (PlayerStats.Instance != null)
        {
            currentSave.currentHP = PlayerStats.Instance.currentHP;
            currentSave.maxHP = PlayerStats.Instance.MaxHP;
        }

        if (InventorySystem.Instance != null)
            InventorySystem.Instance.WriteToSave(currentSave);
        if (ChestManager.Instance != null)
            ChestManager.Instance.WriteToSave(currentSave);

        SaveDataSystem.Save(currentSave, currentSlot);
        Debug.Log($"Manual save — position ({currentSave.playerX}, {currentSave.playerY})");
    }

    public void SaveAndQuitToMenu()
    {
        if (currentSave == null || currentSlot < 0) return;

        currentSave.currentSceneName = SceneManager.GetActiveScene().name;
        currentSave.isNewGame = false;

        if (_playerController != null)
        {
            currentSave.playerX = _playerController.LastGroundedPosition.x;
            currentSave.playerY = _playerController.LastGroundedPosition.y;
            currentSave.hasPositionSave = true;
            Debug.Log($"SaveAndQuitToMenu — saved position ({currentSave.playerX}, {currentSave.playerY})");
        }
        else
        {
            Debug.LogWarning("SaveAndQuitToMenu: PlayerController not cached — position not updated.");
        }

        if (PlayerStats.Instance != null)
        {
            currentSave.currentHP = PlayerStats.Instance.currentHP;
            currentSave.maxHP = PlayerStats.Instance.MaxHP;
        }

        if (InventorySystem.Instance != null)
            InventorySystem.Instance.WriteToSave(currentSave);
        if (ChestManager.Instance != null)
            ChestManager.Instance.WriteToSave(currentSave);

        SaveDataSystem.Save(currentSave, currentSlot);
        Debug.Log($"Saved before quit to menu — map {currentSave.currentMapID}");
    }


    private SaveDataFile MakeDefaultSave(string playerName) => new SaveDataFile
    {
        playerName = playerName,
        isNewGame = true,
        currentHP = 100,
        maxHP = 100,
        currentMapID = 0,
        spawnDoorID = 0,
        currentSceneName = "",
        hasPositionSave = false,
        lastSaved = DateTime.Now.ToString("MM/dd/yyyy HH:mm"),
        inventorySlots = new System.Collections.Generic.List<SavedInventorySlot>(),
        equipment = new SavedEquipment(),
        chestStates = new System.Collections.Generic.List<SavedChestState>()
    };

    public SaveDataFile GetCurrentSave() => currentSave;
    public int GetCurrentSlot() => currentSlot;
}