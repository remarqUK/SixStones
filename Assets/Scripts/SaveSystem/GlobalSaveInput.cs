using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Global persistent input handler for save/load shortcuts
/// Works from any scene, just like pressing Escape for pause menu
/// Auto-instantiates on game start and persists throughout the game
/// No need to add to any scene - creates itself automatically
///
/// CONTROLS:
/// - Keyboard: F5 to Save, F9 to Load
/// - Gamepad: Left Bumper to Save, Right Bumper to Load
///
/// Uses modern Unity 6 Input System with proper device handling
/// </summary>
public class GlobalSaveInput : MonoBehaviour
{
    private static GlobalSaveInput instance;

    public static GlobalSaveInput Instance
    {
        get
        {
            if (instance == null)
            {
                // Try to find existing instance
                instance = FindFirstObjectByType<GlobalSaveInput>();

                // If still null, create a new instance
                if (instance == null)
                {
                    GameObject go = new GameObject("GlobalSaveInput");
                    instance = go.AddComponent<GlobalSaveInput>();
                    DontDestroyOnLoad(go);
                    Debug.Log("[GlobalSaveInput] Auto-created and will persist across all scenes");
                }
            }
            return instance;
        }
    }

    [Header("Settings")]
    [SerializeField] private bool enableKeyboardShortcuts = true;
    [SerializeField] private bool showDebugMessages = true;

    private void Awake()
    {
        // Singleton pattern with DontDestroyOnLoad - persists across all scenes
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            if (showDebugMessages)
            {
                Debug.Log("[GlobalSaveInput] Initialized - Keyboard: F5 to Save, F9 to Load | Gamepad: LB to Save, RB to Load");
            }
        }
        else if (instance != this)
        {
            // Destroy duplicate instances
            Destroy(gameObject);
        }
    }

    // Ensure instance is created at startup
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        // Access the Instance property to trigger auto-creation
        var _ = Instance;
    }

    private void Update()
    {
        if (!enableKeyboardShortcuts)
            return;

        // F5 - Quick Save (using modern Unity Input System)
        if (Keyboard.current != null && Keyboard.current.f5Key.wasPressedThisFrame)
        {
            QuickSave();
        }

        // F9 - Quick Load (using modern Unity Input System)
        if (Keyboard.current != null && Keyboard.current.f9Key.wasPressedThisFrame)
        {
            QuickLoad();
        }

        // Gamepad support - use bumper buttons for save/load
        if (Gamepad.current != null)
        {
            // Left bumper - Quick Save
            if (Gamepad.current.leftShoulder.wasPressedThisFrame)
            {
                QuickSave();
            }

            // Right bumper - Quick Load
            if (Gamepad.current.rightShoulder.wasPressedThisFrame)
            {
                QuickLoad();
            }
        }
    }

    /// <summary>
    /// Quick save - can be called from anywhere
    /// Waits for board processing to complete before saving
    /// </summary>
    public void QuickSave()
    {
        StartCoroutine(QuickSaveCoroutine());
    }

    /// <summary>
    /// Coroutine that waits for board to finish processing, then saves
    /// </summary>
    private System.Collections.IEnumerator QuickSaveCoroutine()
    {
        // Find the board (if in Match3 scene)
        Board board = UnityEngine.Object.FindFirstObjectByType<Board>();

        if (board != null && board.IsProcessing)
        {
            if (showDebugMessages)
            {
                Debug.Log("[QUICK SAVE] Waiting for board animations to complete...");
            }

            ShowSaveNotification("Waiting for animations...", Color.yellow);

            // Wait for board to finish processing
            float timeout = 5f; // 5 second timeout
            float elapsed = 0f;

            while (board.IsProcessing && elapsed < timeout)
            {
                yield return null; // Wait one frame
                elapsed += Time.deltaTime;
            }

            if (elapsed >= timeout)
            {
                if (showDebugMessages)
                {
                    Debug.LogWarning("[QUICK SAVE] Timeout waiting for board to finish. Skipping save.");
                }
                ShowSaveNotification("Save Failed - Timeout", Color.red);
                yield break;
            }
        }

        // Now it's safe to save
        bool success = EnhancedGameSaveManager.SaveGame();

        if (success)
        {
            if (showDebugMessages)
            {
                Debug.Log($"<color=green>[QUICK SAVE] Game saved successfully!</color>");
            }

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
}
