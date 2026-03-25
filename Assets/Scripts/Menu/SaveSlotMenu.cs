using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SaveSlotMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject saveSlotPanel;
    public GameObject overwriteConfirmDialog;
    public GameObject deleteConfirmDialog;

    [Header("Buttons")]
    public Button overwriteYesButton;
    public Button overwriteNoButton;
    public Button deleteYesButton;
    public Button deleteNoButton;
    public Button[] selectButtons = new Button[3];
    public Button[] overwriteButtons = new Button[3];
    public Button[] deleteButtons = new Button[3];

    [Header("UI")]
    public TMP_Text[] saveSlotTexts = new TMP_Text[3];
    public TMP_InputField nameInputField;

    [Header("Scene")]
    public string gameSceneName = "TestScene";

    private int selectedSlot = -1;
    private Action pendingAction;

    void Start()
    {
        saveSlotPanel.SetActive(false);
        overwriteConfirmDialog.SetActive(false);
        deleteConfirmDialog.SetActive(false);

        for (int i = 0; i < selectButtons.Length; i++)
        {
            int slotIndex = i;
            selectButtons[i].onClick.AddListener(() => OnSelectSlot(slotIndex));
            overwriteButtons[i].onClick.AddListener(() => OnOverwriteSlot(slotIndex));
            if (deleteButtons[i] != null)
                deleteButtons[i].onClick.AddListener(() => ShowDeleteConfirmation(slotIndex));
        }

        overwriteYesButton?.onClick.AddListener(ConfirmOverwrite);
        overwriteNoButton?.onClick.AddListener(CancelOverwrite);
        deleteYesButton?.onClick.AddListener(ConfirmDelete);
        deleteNoButton?.onClick.AddListener(CancelDelete);

        UpdateSaveSlotDisplays();
    }

    void UpdateSaveSlotDisplays()
    {
        for (int i = 0; i < selectButtons.Length; i++)
        {
            SaveDataFile save = SaveDataSystem.Load(i);

            if (save != null)
            {
                saveSlotTexts[i].text = $"SLOT {i + 1}\n" +
                                        $"Player: {save.playerName}\n" +
                                        $"Map: {save.currentMapID}\n" +
                                        $"HP: {save.currentHP}/{save.maxHP}\n" +
                                        $"Saved: {save.lastSaved}";
                overwriteButtons[i].gameObject.SetActive(true);

                var colors = selectButtons[i].colors;
                colors.normalColor = new Color(0.6f, 0.9f, 0.6f, 1f);
                selectButtons[i].colors = colors;
            }
            else
            {
                saveSlotTexts[i].text = $"SLOT {i + 1}\n[EMPTY]\nClick SELECT to start new game";
                overwriteButtons[i].gameObject.SetActive(false);

                var colors = selectButtons[i].colors;
                colors.normalColor = Color.white;
                selectButtons[i].colors = colors;
            }
        }
    }

    void OnSelectSlot(int slotIndex)
    {
        selectedSlot = slotIndex;
        SaveDataFile existing = SaveDataSystem.Load(slotIndex);

        if (existing != null)
        {
            GameSaveManager.Instance?.LoadSlot(slotIndex);
        }
        else
        {
            string playerName = GetInputFieldName() ?? $"Player{slotIndex + 1}";
            GameSaveManager.Instance?.StartNewGame(slotIndex, playerName);
        }

        LoadGameScene();
    }

    void OnOverwriteSlot(int slotIndex)
    {
        if (SaveDataSystem.Load(slotIndex) == null) return;

        selectedSlot = slotIndex;

        var msg = overwriteConfirmDialog.GetComponentInChildren<TextMeshProUGUI>();
        if (msg != null) msg.text = "Overwrite this save?";

        pendingAction = () =>
        {
            string playerName = GetInputFieldName() ?? $"Player{slotIndex + 1}";
            GameSaveManager.Instance?.StartNewGame(slotIndex, playerName);
            LoadGameScene();
        };

        overwriteConfirmDialog.SetActive(true);
    }

    void ShowDeleteConfirmation(int slotIndex)
    {
        if (SaveDataSystem.Load(slotIndex) == null) { Debug.Log("No save to delete."); return; }

        selectedSlot = slotIndex;

        var msg = deleteConfirmDialog.GetComponentInChildren<TextMeshProUGUI>();
        if (msg != null) msg.text = "Delete this save?";

        pendingAction = () =>
        {
            SaveDataSystem.DeleteSave(slotIndex);

            if (GameSaveManager.Instance != null &&
                GameSaveManager.Instance.GetCurrentSlot() == slotIndex)
            {
                GameSaveManager.Instance.StartNewGame(slotIndex, $"Player{slotIndex + 1}");
                SaveDataSystem.DeleteSave(slotIndex); 
            }

            UpdateSaveSlotDisplays();
        };

        deleteConfirmDialog.SetActive(true);
    }


    void ConfirmOverwrite() { overwriteConfirmDialog.SetActive(false); pendingAction?.Invoke(); pendingAction = null; }
    void CancelOverwrite() { overwriteConfirmDialog.SetActive(false); selectedSlot = -1; pendingAction = null; }
    void ConfirmDelete() { deleteConfirmDialog.SetActive(false); pendingAction?.Invoke(); pendingAction = null; }
    void CancelDelete() { deleteConfirmDialog.SetActive(false); selectedSlot = -1; pendingAction = null; }

    void LoadGameScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);
    }

    public void OpenSaveSlotMenu()
    {
        mainMenuPanel.SetActive(false);
        saveSlotPanel.SetActive(true);
        overwriteConfirmDialog.SetActive(false);
        deleteConfirmDialog.SetActive(false);
        if (nameInputField != null) nameInputField.text = "";
        UpdateSaveSlotDisplays();
    }

    public void BackToMainMenu()
    {
        saveSlotPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        overwriteConfirmDialog.SetActive(false);
        deleteConfirmDialog.SetActive(false);
    }

    public int GetSelectedSlot() => selectedSlot;

    private string GetInputFieldName()
    {
        if (nameInputField != null && !string.IsNullOrWhiteSpace(nameInputField.text))
            return nameInputField.text.Trim();
        return null;
    }
}