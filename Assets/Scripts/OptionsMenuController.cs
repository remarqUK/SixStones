using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Standalone options menu that can be used from any scene (MainMenu or Match3)
/// </summary>
public class OptionsMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Slider gameVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider dialogVolumeSlider;
    [SerializeField] private TMP_Dropdown gameSpeedDropdown;
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private Button closeButton;

    [Header("Settings")]
    [SerializeField] private bool pauseTimeWhenOpen = false; // Only pause in Match3, not in MainMenu

    private System.Action onCloseCallback;
    private bool isOpen = false;

    public bool IsOpen => isOpen;

    private void Start()
    {
        // Wire up close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseOptions);
        }

        // Setup volume sliders
        SetupVolumeSliders();

        // Setup dropdowns
        SetupGameSpeedDropdown();
        SetupLanguageDropdown();

        // Hide panel initially
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }

    private void SetupVolumeSliders()
    {
        // Game Volume
        if (gameVolumeSlider != null)
        {
            if (AudioManager.Instance != null)
            {
                gameVolumeSlider.value = AudioManager.Instance.GetGameVolume();
            }
            gameVolumeSlider.onValueChanged.AddListener(OnGameVolumeChanged);
        }

        // Music Volume
        if (musicVolumeSlider != null)
        {
            if (AudioManager.Instance != null)
            {
                musicVolumeSlider.value = AudioManager.Instance.GetMusicVolume();
            }
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        // Dialog Volume
        if (dialogVolumeSlider != null)
        {
            if (AudioManager.Instance != null)
            {
                dialogVolumeSlider.value = AudioManager.Instance.GetDialogVolume();
            }
            dialogVolumeSlider.onValueChanged.AddListener(OnDialogVolumeChanged);
        }
    }

    private void OnGameVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetGameVolume(value);
        }
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
    }

    private void OnDialogVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetDialogVolume(value);
        }
    }

    private void SetupGameSpeedDropdown()
    {
        if (gameSpeedDropdown == null) return;

        // Setup options
        gameSpeedDropdown.ClearOptions();
        List<string> options = new List<string> { "Slow", "Medium", "Fast" };
        gameSpeedDropdown.AddOptions(options);

        // Set current value from GameSpeedSettings
        if (GameSpeedSettings.Instance != null)
        {
            gameSpeedDropdown.value = (int)GameSpeedSettings.Instance.GetCurrentSpeed();
            gameSpeedDropdown.RefreshShownValue();
        }

        // Wire up change event
        gameSpeedDropdown.onValueChanged.AddListener(OnGameSpeedChanged);
    }

    private void SetupLanguageDropdown()
    {
        if (languageDropdown == null) return;

        // Setup options
        languageDropdown.ClearOptions();
        List<string> options = new List<string> { "English", "Español", "Français", "Deutsch" };
        languageDropdown.AddOptions(options);

        // Set current value from LocalizationManager
        if (LocalizationManager.Instance != null)
        {
            languageDropdown.value = LocalizationManager.Instance.GetCurrentLanguageIndex();
            languageDropdown.RefreshShownValue();
        }

        // Wire up change event
        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    private void OnGameSpeedChanged(int index)
    {
        if (GameSpeedSettings.Instance != null)
        {
            GameSpeedSettings.Instance.SetGameSpeedFromDropdown(index);
        }
    }

    private void OnLanguageChanged(int index)
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.SetLanguageFromDropdown(index);
        }
    }

    /// <summary>
    /// Show the options menu
    /// </summary>
    public void ShowOptions(System.Action onClose = null)
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
        }

        isOpen = true;
        onCloseCallback = onClose;

        // Pause game time if in Match3 scene
        if (pauseTimeWhenOpen)
        {
            Time.timeScale = 0f;
        }

        Debug.Log("Options menu opened");
    }

    /// <summary>
    /// Close the options menu
    /// </summary>
    public void CloseOptions()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }

        isOpen = false;

        // Resume game time if paused
        if (pauseTimeWhenOpen)
        {
            Time.timeScale = 1f;
        }

        // Invoke callback (e.g., return to main menu or pause menu)
        onCloseCallback?.Invoke();

        Debug.Log("Options menu closed");
    }

    /// <summary>
    /// Set whether to pause time when options are open
    /// (true for Match3 scene, false for MainMenu)
    /// </summary>
    public void SetPauseTimeWhenOpen(bool pause)
    {
        pauseTimeWhenOpen = pause;
    }
}
