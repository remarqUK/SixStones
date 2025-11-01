using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Component to trigger save/load operations
/// Attach to UI buttons or call methods from other scripts
/// </summary>
public class SaveTrigger : MonoBehaviour
{
    [Header("UI References (Optional)")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button deleteSaveButton;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Settings")]
    [SerializeField] private int saveSlot = 1; // Which save slot to use (1-10)
    [SerializeField] private bool enableKeyboardShortcuts = false; // Disabled by default - use GlobalSaveInput instead
    [SerializeField] private bool autoSaveEnabled = true;
    [SerializeField] private float autoSaveIntervalSeconds = 300f; // 5 minutes
    [SerializeField] private bool saveOnSceneUnload = true;

    private float autoSaveTimer = 0f;

    private void Start()
    {
        // Hook up button listeners if assigned
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(OnSaveButtonClicked);
        }

        if (loadButton != null)
        {
            loadButton.onClick.AddListener(OnLoadButtonClicked);
        }

        if (deleteSaveButton != null)
        {
            deleteSaveButton.onClick.AddListener(OnDeleteSaveButtonClicked);
        }

        // Update UI based on save existence
        UpdateButtonStates();

        // Reset auto-save timer
        autoSaveTimer = autoSaveIntervalSeconds;
    }

    private void Update()
    {
        // Auto-save countdown
        if (autoSaveEnabled)
        {
            autoSaveTimer -= Time.deltaTime;
            if (autoSaveTimer <= 0f)
            {
                Debug.Log("Auto-saving game...");
                SaveGame();
                autoSaveTimer = autoSaveIntervalSeconds;
            }
        }

        // Keyboard shortcuts (only if enabled - use GlobalSaveInput for global shortcuts)
        if (enableKeyboardShortcuts)
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                SaveGame();
            }

            if (Input.GetKeyDown(KeyCode.F9))
            {
                LoadGame();
            }
        }
    }

    private void OnApplicationQuit()
    {
        if (saveOnSceneUnload)
        {
            Debug.Log("Saving game on application quit...");
            SaveGame();
        }
    }

    #region Public Methods

    /// <summary>
    /// Save the game to the configured slot
    /// </summary>
    public void SaveGame()
    {
        bool success = EnhancedGameSaveManager.SaveGame(saveSlot);

        if (success)
        {
            ShowFeedback($"Game Saved to Slot {saveSlot}!", Color.green);
            UpdateButtonStates();
        }
        else
        {
            ShowFeedback("Save Failed!", Color.red);
        }
    }

    /// <summary>
    /// Load the game from the configured slot
    /// </summary>
    public void LoadGame()
    {
        if (!EnhancedGameSaveManager.HasSaveGame(saveSlot))
        {
            ShowFeedback($"No Save in Slot {saveSlot}!", Color.yellow);
            return;
        }

        SaveData saveData = EnhancedGameSaveManager.LoadGame(saveSlot);

        if (saveData != null)
        {
            EnhancedGameSaveManager.RestoreSaveData(saveData);
            ShowFeedback($"Game Loaded from Slot {saveSlot}!", Color.green);
        }
        else
        {
            ShowFeedback("Load Failed!", Color.red);
        }
    }

    /// <summary>
    /// Delete the save file from the configured slot
    /// </summary>
    public void DeleteSave()
    {
        EnhancedGameSaveManager.DeleteSaveGame(saveSlot);
        ShowFeedback($"Save Deleted from Slot {saveSlot}!", Color.yellow);
        UpdateButtonStates();
    }

    /// <summary>
    /// Check if save exists in the configured slot
    /// </summary>
    public bool HasSave()
    {
        return EnhancedGameSaveManager.HasSaveGame(saveSlot);
    }

    /// <summary>
    /// Get save slot info for display
    /// </summary>
    public SaveSlotInfo GetSaveInfo()
    {
        return EnhancedGameSaveManager.GetSaveSlotInfo(saveSlot);
    }

    /// <summary>
    /// Set which save slot to use
    /// </summary>
    public void SetSaveSlot(int slot)
    {
        saveSlot = slot;
        UpdateButtonStates();
    }

    /// <summary>
    /// Enable/disable auto-save
    /// </summary>
    public void SetAutoSave(bool enabled)
    {
        autoSaveEnabled = enabled;
        if (enabled)
        {
            autoSaveTimer = autoSaveIntervalSeconds;
        }
    }

    #endregion

    #region Button Callbacks

    private void OnSaveButtonClicked()
    {
        SaveGame();
    }

    private void OnLoadButtonClicked()
    {
        LoadGame();
    }

    private void OnDeleteSaveButtonClicked()
    {
        // Show confirmation dialog (you can implement this)
        if (ConfirmDelete())
        {
            DeleteSave();
        }
    }

    private bool ConfirmDelete()
    {
        // Simple confirmation - you can replace with a proper UI dialog
        return true;
    }

    #endregion

    #region UI Helpers

    private void UpdateButtonStates()
    {
        bool hasSave = HasSave();

        if (loadButton != null)
        {
            loadButton.interactable = hasSave;
        }

        if (deleteSaveButton != null)
        {
            deleteSaveButton.interactable = hasSave;
        }

        // Save button always enabled
        if (saveButton != null)
        {
            saveButton.interactable = true;
        }
    }

    private void ShowFeedback(string message, Color color)
    {
        Debug.Log($"[SaveTrigger] {message}");

        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
            feedbackText.gameObject.SetActive(true);

            // Auto-hide after 3 seconds
            CancelInvoke(nameof(HideFeedback));
            Invoke(nameof(HideFeedback), 3f);
        }
    }

    private void HideFeedback()
    {
        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Event-Based Saving

    /// <summary>
    /// Subscribe to game events for automatic saving
    /// </summary>
    private void OnEnable()
    {
        // Subscribe to game events
        // Example: GameEvents.OnLevelComplete += OnLevelComplete;
        // Example: GameEvents.OnPlayerDeath += OnPlayerDeath;
    }

    private void OnDisable()
    {
        // Unsubscribe from game events
        // Example: GameEvents.OnLevelComplete -= OnLevelComplete;
        // Example: GameEvents.OnPlayerDeath -= OnPlayerDeath;
    }

    // Example event handlers
    private void OnLevelComplete()
    {
        Debug.Log("Level complete - auto-saving...");
        SaveGame();
    }

    private void OnPlayerDeath()
    {
        Debug.Log("Player died - game not saved");
        // Don't save on death (or save separately as "death save")
    }

    #endregion
}
