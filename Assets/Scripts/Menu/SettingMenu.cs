using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingMenu : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private TMP_Dropdown textureDropdown;
    [SerializeField] private TMP_Dropdown antialiasingDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Button saveSettings;
    [SerializeField] private Button returnToMainMenu;
    [SerializeField] private Button manualSaveButton;  
    [SerializeField] private GameObject GraphicSettings;
    [SerializeField] private Button openGraphicSettings;
    [SerializeField] public GameObject OptionmenuUI;
    [SerializeField] private Button ReturntoOptionmenu;
    [SerializeField] private GameObject mainMenu;

    private bool isApplyingSettings = false;
    Resolution[] resolutions;

    void Start()
    {
        SetupResolutionDropdown();
        SetupQualityDropdown();
        SetupTextureDropdown();
        SetupAntialiasingDropdown();

        LoadSettings();

        AddEventListeners();
    }

    void SetupResolutionDropdown()
    {
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        resolutions = Screen.resolutions;

        if (resolutions == null || resolutions.Length == 0)
        {
            Debug.LogWarning("No resolutions found!");
            return;
        }

        HashSet<string> uniqueResolutions = new HashSet<string>();
        int currentResolutionIndex = 0;
        bool foundCurrentResolution = false;

        for (int i = 0; i < resolutions.Length; i++)
        {
            float refreshRate = (float)resolutions[i].refreshRateRatio.value;
            string option = $"{resolutions[i].width} x {resolutions[i].height} @ {refreshRate:0}Hz";

            if (uniqueResolutions.Add(option))
            {
                int optionIndex = options.Count;
                options.Add(option);

                float currentRefreshRate = (float)Screen.currentResolution.refreshRateRatio.value;

                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height &&
                    Mathf.Approximately(refreshRate, currentRefreshRate))
                {
                    currentResolutionIndex = optionIndex;
                    foundCurrentResolution = true;
                }
            }
        }

        resolutionDropdown.AddOptions(options);

        // Set the current resolution if found, otherwise default to first
        if (foundCurrentResolution && currentResolutionIndex < options.Count)
        {
            resolutionDropdown.value = currentResolutionIndex;
        }
        else if (options.Count > 0)
        {
            resolutionDropdown.value = 0;
        }

        resolutionDropdown.RefreshShownValue();
    }

    void SetupQualityDropdown()
    {
        qualityDropdown.ClearOptions();
        List<string> options = new List<string>
        {
            "Very Low", "Low", "Medium", "High", "Very High", "Ultra", "Custom"
        };
        qualityDropdown.AddOptions(options);
    }

    void SetupTextureDropdown()
    {
        textureDropdown.ClearOptions();
        List<string> options = new List<string>
        {
            "Full", "Half", "Quarter", "Eighth"
        };
        textureDropdown.AddOptions(options);
    }

    void SetupAntialiasingDropdown()
    {
        antialiasingDropdown.ClearOptions();
        List<string> options = new List<string>
        {
            "Disabled", "2x", "4x", "8x"
        };
        antialiasingDropdown.AddOptions(options);
    }

    void AddEventListeners()
    {
        // Remove existing listeners to prevent duplicates
        resolutionDropdown.onValueChanged.RemoveAllListeners();
        qualityDropdown.onValueChanged.RemoveAllListeners();
        textureDropdown.onValueChanged.RemoveAllListeners();
        antialiasingDropdown.onValueChanged.RemoveAllListeners();
        fullscreenToggle.onValueChanged.RemoveAllListeners();

        // Add new listeners
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        qualityDropdown.onValueChanged.AddListener(SetQuality);
        textureDropdown.onValueChanged.AddListener(SetTextureQuality);
        antialiasingDropdown.onValueChanged.AddListener(SetAntialiasing);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);

        if (saveSettings != null)
            saveSettings.onClick.AddListener(SaveSettings);
        if (returnToMainMenu != null)
            returnToMainMenu.onClick.AddListener(ReturnToMainMenuFromTestScene);
        if (openGraphicSettings != null)
            openGraphicSettings.onClick.AddListener(ToggleGraphicSettings);
        if (ReturntoOptionmenu != null)
            ReturntoOptionmenu.onClick.AddListener(ReturnToOptionsMenu);
        if (manualSaveButton != null)
            manualSaveButton.onClick.AddListener(OnManualSave);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        if (isApplyingSettings) return;
        Screen.fullScreen = isFullscreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        if (isApplyingSettings) return;

        List<Resolution> uniqueResolutions = new List<Resolution>();
        HashSet<string> seenResolutions = new HashSet<string>();

        foreach (var res in resolutions)
        {
            string key = $"{res.width}x{res.height}";
            if (seenResolutions.Add(key))
            {
                uniqueResolutions.Add(res);
            }
        }

        if (resolutionIndex >= 0 && resolutionIndex < uniqueResolutions.Count)
        {
            Resolution resolution = uniqueResolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }
    }

    public void SetTextureQuality(int textureIndex)
    {
        if (isApplyingSettings) return;

        QualitySettings.globalTextureMipmapLimit = textureIndex;

        // If texture quality is changed manually, set quality to "Custom"
        if (qualityDropdown.value != 6)
        {
            isApplyingSettings = true;
            qualityDropdown.value = 6;
            isApplyingSettings = false;
        }
    }

    public void SetAntialiasing(int antialiasingIndex)
    {
        if (isApplyingSettings) return;

        // Map dropdown index to actual antialiasing values
        int[] aaValues = { 0, 2, 4, 8 };
        if (antialiasingIndex >= 0 && antialiasingIndex < aaValues.Length)
        {
            QualitySettings.antiAliasing = aaValues[antialiasingIndex];
        }

        // If antialiasing is changed manually, set quality to "Custom"
        if (qualityDropdown.value != 6)
        {
            isApplyingSettings = true;
            qualityDropdown.value = 6;
            isApplyingSettings = false;
        }
    }

    public void SetQuality(int qualityIndex)
    {
        if (isApplyingSettings) return;

        isApplyingSettings = true;

        if (qualityIndex != 6) // If not using custom quality
        {
            QualitySettings.SetQualityLevel(qualityIndex);

            // Update the other dropdowns to reflect the preset
            switch (qualityIndex)
            {
                case 0: // Very Low
                    textureDropdown.value = 3;
                    antialiasingDropdown.value = 0;
                    break;
                case 1: // Low
                    textureDropdown.value = 2;
                    antialiasingDropdown.value = 0;
                    break;
                case 2: // Medium
                    textureDropdown.value = 1;
                    antialiasingDropdown.value = 0;
                    break;
                case 3: // High
                    textureDropdown.value = 0;
                    antialiasingDropdown.value = 0;
                    break;
                case 4: // Very High
                    textureDropdown.value = 0;
                    antialiasingDropdown.value = 1; // 2x
                    break;
                case 5: // Ultra
                    textureDropdown.value = 0;
                    antialiasingDropdown.value = 2; // 4x
                    break;
            }
        }

        isApplyingSettings = false;
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("QualitySetting", qualityDropdown.value);
        PlayerPrefs.SetInt("TextureSetting", textureDropdown.value);
        PlayerPrefs.SetInt("AntialiasingSetting", antialiasingDropdown.value);
        PlayerPrefs.SetInt("ResolutionSetting", resolutionDropdown.value);
        PlayerPrefs.SetInt("FullscreenSetting", Convert.ToInt32(fullscreenToggle.isOn));
        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        isApplyingSettings = true;

        // Load quality setting
        if (PlayerPrefs.HasKey("QualitySetting"))
            qualityDropdown.value = PlayerPrefs.GetInt("QualitySetting");
        else
            qualityDropdown.value = 3;

        // Load texture setting
        if (PlayerPrefs.HasKey("TextureSetting"))
            textureDropdown.value = PlayerPrefs.GetInt("TextureSetting");
        else
            textureDropdown.value = 0;

        // Load antialiasing setting
        if (PlayerPrefs.HasKey("AntialiasingSetting"))
            antialiasingDropdown.value = PlayerPrefs.GetInt("AntialiasingSetting");
        else
            antialiasingDropdown.value = 0;

        // Load resolution setting
        if (PlayerPrefs.HasKey("ResolutionSetting"))
            resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionSetting");

        // Load fullscreen setting
        if (PlayerPrefs.HasKey("FullscreenSetting"))
            fullscreenToggle.isOn = Convert.ToBoolean(PlayerPrefs.GetInt("FullscreenSetting"));
        else
            fullscreenToggle.isOn = true;

        // Apply the loaded settings
        SetQuality(qualityDropdown.value);
        SetFullscreen(fullscreenToggle.isOn);

        isApplyingSettings = false;
    }

    public void ToggleGraphicSettings()
    {
        GraphicSettings.SetActive(true);
        OptionmenuUI.SetActive(false);
    }

    public void ReturnToMainMenuFromTestScene()
    {
        Debug.Log("Return to Main Menu button clicked from test scene");

        GameSaveManager.Instance?.SaveAndQuitToMenu();

        SaveSettings();

        if (Time.timeScale != 1f)
            Time.timeScale = 1f;

        SceneManager.LoadScene("MainMenu");
    }

    public void OnManualSave()
    {
        GameSaveManager.Instance?.SaveNow();
        Debug.Log("Manual save triggered from pause menu.");
    }

    public void ReturnToOptionsMenu()
    {
        GraphicSettings.SetActive(false);
        OptionmenuUI.SetActive(true);
    }

    void OnDestroy()
    {
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.RemoveAllListeners();
        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.RemoveAllListeners();
        if (textureDropdown != null)
            textureDropdown.onValueChanged.RemoveAllListeners();
        if (antialiasingDropdown != null)
            antialiasingDropdown.onValueChanged.RemoveAllListeners();
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.RemoveAllListeners();
    }

    public void OnApplicationQuit()
    {
        Application.Quit();
    }

}