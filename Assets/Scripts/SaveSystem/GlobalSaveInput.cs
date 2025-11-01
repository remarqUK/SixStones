using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Global persistent input handler for save/load shortcuts
/// Works from any scene, just like pressing Escape for pause menu
/// Add this to your first scene and it will persist throughout the game
/// </summary>
public class GlobalSaveInput : MonoBehaviour
{
    private static GlobalSaveInput instance;

    [Header("Settings")]
    [SerializeField] private bool enableKeyboardShortcuts = true;
    [SerializeField] private bool showDebugMessages = true;

    private Keyboard keyboard;

    private void Awake()
    {
        // Singleton pattern with DontDestroyOnLoad - persists across all scenes
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Get keyboard reference
            keyboard = Keyboard.current;

            if (showDebugMessages)
            {
                Debug.Log("[GlobalSaveInput] Initialized - Press F5 to Save, F9 to Load");
            }
        }
        else if (instance != this)
        {
            // Destroy duplicate instances
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!enableKeyboardShortcuts)
            return;

        // Check if keyboard is available
        if (keyboard == null)
        {
            keyboard = Keyboard.current;
            if (keyboard == null)
                return;
        }

        // F5 - Quick Save
        if (keyboard.f5Key.wasPressedThisFrame)
        {
            QuickSave();
        }

        // F9 - Quick Load
        if (keyboard.f9Key.wasPressedThisFrame)
        {
            QuickLoad();
        }
    }

    /// <summary>
    /// Quick save - can be called from anywhere
    /// </summary>
    public void QuickSave()
    {
        bool success = EnhancedGameSaveManager.SaveGame();

        if (success)
        {
            if (showDebugMessages)
            {
                Debug.Log($"<color=green>[QUICK SAVE] Game saved successfully!</color>");
            }

            // Optional: Show on-screen notification
            ShowSaveNotification("Game Saved!", Color.green);
        }
        else
        {
            if (showDebugMessages)
            {
                Debug.LogWarning("[QUICK SAVE] Failed to save game!");
            }

            ShowSaveNotification("Save Failed!", Color.red);
        }
    }

    /// <summary>
    /// Quick load - can be called from anywhere
    /// </summary>
    public void QuickLoad()
    {
        if (!EnhancedGameSaveManager.HasSaveGame())
        {
            if (showDebugMessages)
            {
                Debug.LogWarning("[QUICK LOAD] No save file found!");
            }

            ShowSaveNotification("No Save Found!", Color.yellow);
            return;
        }

        SaveData saveData = EnhancedGameSaveManager.LoadGame();

        if (saveData != null)
        {
            EnhancedGameSaveManager.RestoreSaveData(saveData);

            if (showDebugMessages)
            {
                Debug.Log($"<color=cyan>[QUICK LOAD] Game loaded successfully!</color>");
            }

            ShowSaveNotification("Game Loaded!", Color.cyan);
        }
        else
        {
            if (showDebugMessages)
            {
                Debug.LogWarning("[QUICK LOAD] Failed to load game!");
            }

            ShowSaveNotification("Load Failed!", Color.red);
        }
    }

    /// <summary>
    /// Show on-screen notification (optional - can be enhanced with UI)
    /// </summary>
    private void ShowSaveNotification(string message, Color color)
    {
        // For now, just debug log
        // You can enhance this to show a UI popup/toast notification
        Debug.Log($"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{message}</color>");

        // Optional: Trigger an event that UI can listen to
        // SaveNotificationEvent?.Invoke(message, color);
    }

    /// <summary>
    /// Enable/disable keyboard shortcuts at runtime
    /// </summary>
    public static void SetKeyboardShortcutsEnabled(bool enabled)
    {
        if (instance != null)
        {
            instance.enableKeyboardShortcuts = enabled;
        }
    }

    /// <summary>
    /// Check if save exists (useful for UI)
    /// </summary>
    public static bool HasSaveGame()
    {
        return EnhancedGameSaveManager.HasSaveGame();
    }

    /// <summary>
    /// Get the singleton instance
    /// </summary>
    public static GlobalSaveInput Instance => instance;
}
