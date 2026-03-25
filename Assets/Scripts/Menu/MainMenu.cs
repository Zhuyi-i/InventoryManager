using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
       public Button playButton;
       public Button exitButton;
       public Button optionsButton;

       public SaveSlotMenu saveSlotMenu;

       [SerializeField] private GameObject mainMenuUI;

    void Start()
    {
        playButton.onClick.AddListener(OnPlayButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);
        optionsButton.onClick.AddListener(OnOptionsButtonClicked);
    }

    void Update()
    {
        
    }

    void OnPlayButtonClicked()
    {
        optionsButton.gameObject.SetActive(false);
        saveSlotMenu.OpenSaveSlotMenu();
    }

    void OnExitButtonClicked()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    void OnOptionsButtonClicked()
    {
        mainMenuUI.SetActive(false); // Hide main menu when opening settings

    }

    // Called by SettingMenu when returning to main menu
    public void ShowMainMenu()
    {
        mainMenuUI.SetActive(true);
        optionsButton.gameObject.SetActive(true); // Restore options button
    }

}
