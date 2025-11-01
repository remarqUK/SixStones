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
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Button closeButton;

    [Header("Settings")]
    [SerializeField] private bool pauseTimeWhenOpen = false; // Only pause in Match3, not in MainMenu

    private System.Action onCloseCallback;
    private bool isOpen = false;

    public bool IsOpen => isOpen;

    private void Start()
    {
        // Apply saved resolution first (before UI setup)
        ApplySavedResolution();

        // Wire up close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseOptions);
        }

        // Auto-create resolution dropdown if not assigned
        if (resolutionDropdown == null && languageDropdown != null)
        {
            CreateResolutionDropdownUI();
        }

        // Auto-create fullscreen toggle if not assigned
        if (fullscreenToggle == null && resolutionDropdown != null)
        {
            CreateFullscreenToggleUI();
        }

        // Setup volume sliders
        SetupVolumeSliders();

        // Setup dropdowns
        SetupGameSpeedDropdown();
        SetupLanguageDropdown();
        SetupResolutionDropdown();
        SetupFullscreenToggle();

        // Hide panel initially
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }

    private void ApplySavedResolution()
    {
        // Apply saved fullscreen preference first
        if (PlayerPrefs.HasKey("Fullscreen"))
        {
            bool isFullscreen = PlayerPrefs.GetInt("Fullscreen") == 1;
            Screen.fullScreen = isFullscreen;
        }

        // Check if there's a saved resolution preference
        if (!PlayerPrefs.HasKey("ResolutionIndex")) return;

        int savedIndex = PlayerPrefs.GetInt("ResolutionIndex");

        // Get available resolutions
        Resolution[] resolutions = Screen.resolutions;
        List<Resolution> uniqueResolutions = new List<Resolution>();
        HashSet<string> uniqueResKeys = new HashSet<string>();

        foreach (Resolution res in resolutions)
        {
            string key = $"{res.width}x{res.height}";
            if (!uniqueResKeys.Contains(key))
            {
                uniqueResKeys.Add(key);
                uniqueResolutions.Add(res);
            }
        }

        // Apply the saved resolution
        if (savedIndex >= 0 && savedIndex < uniqueResolutions.Count)
        {
            Resolution savedResolution = uniqueResolutions[savedIndex];
            Screen.SetResolution(savedResolution.width, savedResolution.height, Screen.fullScreen);
            Debug.Log($"Applied saved resolution: {savedResolution.width} x {savedResolution.height} (Fullscreen: {Screen.fullScreen})");
        }
    }

    private void CreateResolutionDropdownUI()
    {
        // Duplicate the language dropdown container
        GameObject languageContainer = languageDropdown.transform.parent.gameObject;
        GameObject resolutionContainer = Instantiate(languageContainer, languageContainer.transform.parent);
        resolutionContainer.name = "ResolutionDropdownContainer";

        // Position it below the language dropdown
        RectTransform resRect = resolutionContainer.GetComponent<RectTransform>();
        resRect.anchoredPosition = new Vector2(0, -210);

        // Find and update the label
        TMP_Text[] texts = resolutionContainer.GetComponentsInChildren<TMP_Text>();
        foreach (TMP_Text text in texts)
        {
            if (text.name.Contains("Label"))
            {
                text.text = "Resolution:";
                break;
            }
        }

        // Get the dropdown component and assign it
        resolutionDropdown = resolutionContainer.GetComponentInChildren<TMP_Dropdown>();
        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
        }

        Debug.Log("Resolution dropdown UI created automatically");
    }

    private void CreateFullscreenToggleUI()
    {
        // Duplicate the resolution dropdown container
        GameObject resolutionContainer = resolutionDropdown.transform.parent.gameObject;
        GameObject fullscreenContainer = Instantiate(resolutionContainer, resolutionContainer.transform.parent);
        fullscreenContainer.name = "FullscreenToggleContainer";

        // Position it below the resolution dropdown
        RectTransform fsRect = fullscreenContainer.GetComponent<RectTransform>();
        fsRect.anchoredPosition = new Vector2(0, -270);

        // Find and update the label
        TMP_Text[] texts = fullscreenContainer.GetComponentsInChildren<TMP_Text>();
        foreach (TMP_Text text in texts)
        {
            if (text.name.Contains("Label"))
            {
                text.text = "Fullscreen:";
                break;
            }
        }

        // Remove the dropdown component and replace with toggle
        TMP_Dropdown dropdown = fullscreenContainer.GetComponentInChildren<TMP_Dropdown>();
        if (dropdown != null)
        {
            GameObject dropdownObj = dropdown.gameObject;
            DestroyImmediate(dropdown);

            // Add Toggle component
            fullscreenToggle = dropdownObj.AddComponent<Toggle>();

            // Configure the toggle to use its background as the graphic
            UnityEngine.UI.Image background = dropdownObj.GetComponent<UnityEngine.UI.Image>();
            if (background != null)
            {
                fullscreenToggle.targetGraphic = background;
            }

            // Find or create a checkmark GameObject
            Transform checkmark = dropdownObj.transform.Find("Checkmark");
            if (checkmark == null)
            {
                // Create a simple checkmark indicator
                GameObject checkmarkObj = new GameObject("Checkmark");
                checkmarkObj.transform.SetParent(dropdownObj.transform, false);
                RectTransform checkmarkRect = checkmarkObj.AddComponent<RectTransform>();
                checkmarkRect.anchorMin = new Vector2(0.5f, 0.5f);
                checkmarkRect.anchorMax = new Vector2(0.5f, 0.5f);
                checkmarkRect.sizeDelta = new Vector2(20, 20);
                checkmarkRect.anchoredPosition = Vector2.zero;

                UnityEngine.UI.Image checkmarkImage = checkmarkObj.AddComponent<UnityEngine.UI.Image>();
                checkmarkImage.color = Color.green;
                fullscreenToggle.graphic = checkmarkImage;
            }
            else
            {
                fullscreenToggle.graphic = checkmark.GetComponent<UnityEngine.UI.Image>();
            }
        }

        Debug.Log("Fullscreen toggle UI created automatically");
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

    private void SetupResolutionDropdown()
    {
        if (resolutionDropdown == null) return;

        // Get available resolutions
        Resolution[] resolutions = Screen.resolutions;

        // Filter to unique resolutions (remove duplicates with different refresh rates)
        List<Resolution> uniqueResolutions = new List<Resolution>();
        HashSet<string> uniqueResKeys = new HashSet<string>();

        foreach (Resolution res in resolutions)
        {
            string key = $"{res.width}x{res.height}";
            if (!uniqueResKeys.Contains(key))
            {
                uniqueResKeys.Add(key);
                uniqueResolutions.Add(res);
            }
        }

        // Create dropdown options
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < uniqueResolutions.Count; i++)
        {
            Resolution res = uniqueResolutions[i];
            string option = $"{res.width} x {res.height}";
            options.Add(option);

            // Check if this is the current resolution
            if (res.width == Screen.width && res.height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);

        // Load saved resolution preference or use current
        int savedResIndex = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);

        // Set value without triggering events
        resolutionDropdown.SetValueWithoutNotify(savedResIndex);
        resolutionDropdown.RefreshShownValue();

        // Wire up change event AFTER setting initial value
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);

        Debug.Log($"Resolution dropdown setup with {options.Count} options, current index: {savedResIndex}");
    }

    private void OnResolutionChanged(int index)
    {
        Resolution[] resolutions = Screen.resolutions;

        // Filter to unique resolutions again
        List<Resolution> uniqueResolutions = new List<Resolution>();
        HashSet<string> uniqueResKeys = new HashSet<string>();

        foreach (Resolution res in resolutions)
        {
            string key = $"{res.width}x{res.height}";
            if (!uniqueResKeys.Contains(key))
            {
                uniqueResKeys.Add(key);
                uniqueResolutions.Add(res);
            }
        }

        if (index >= 0 && index < uniqueResolutions.Count)
        {
            Resolution selectedResolution = uniqueResolutions[index];

            Debug.Log($"Attempting to change resolution to: {selectedResolution.width} x {selectedResolution.height}, Fullscreen: {Screen.fullScreen}");
            Debug.Log($"Current resolution: {Screen.width} x {Screen.height}");

            // Use FullScreenMode for better compatibility
            FullScreenMode fullScreenMode = Screen.fullScreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed;
            Screen.SetResolution(selectedResolution.width, selectedResolution.height, fullScreenMode);

            // Save preference
            PlayerPrefs.SetInt("ResolutionIndex", index);
            PlayerPrefs.Save();

            Debug.Log($"Resolution change requested. New screen size should be: {selectedResolution.width} x {selectedResolution.height}");

            // Notify all systems to update
            StartCoroutine(NotifyResolutionChange());
        }
    }

    private System.Collections.IEnumerator NotifyResolutionChange()
    {
        // Wait one frame for resolution to actually change
        yield return null;

        // Force camera scalers to update
        CameraScaler[] cameraScalers = FindObjectsByType<CameraScaler>(FindObjectsSortMode.None);
        foreach (CameraScaler scaler in cameraScalers)
        {
            scaler.enabled = false;
            scaler.enabled = true;
        }

        // Force canvas scalers to update
        UnityEngine.UI.CanvasScaler[] canvasScalers = FindObjectsByType<UnityEngine.UI.CanvasScaler>(FindObjectsSortMode.None);
        foreach (UnityEngine.UI.CanvasScaler scaler in canvasScalers)
        {
            scaler.enabled = false;
            scaler.enabled = true;
        }

        // Refresh all canvases
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            canvas.enabled = false;
            canvas.enabled = true;
        }

        Debug.Log($"Resolution change applied - refreshed {cameraScalers.Length} camera scalers, {canvasScalers.Length} canvas scalers, and {canvases.Length} canvases");
    }

    private void SetupFullscreenToggle()
    {
        if (fullscreenToggle == null) return;

        // Set current value
        fullscreenToggle.isOn = Screen.fullScreen;

        // Wire up change event
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);

        Debug.Log($"Fullscreen toggle setup, current state: {Screen.fullScreen}");
    }

    private void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;

        // Save preference
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log($"Fullscreen changed to: {isFullscreen}");

        // Notify all systems to update
        StartCoroutine(NotifyResolutionChange());
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
