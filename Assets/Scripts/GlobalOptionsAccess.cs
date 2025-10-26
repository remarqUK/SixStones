using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// DEPRECATED: This component has been replaced by GlobalOptionsManager
/// GlobalOptionsManager is a persistent singleton with DontDestroyOnLoad that survives scene changes
///
/// To migrate:
/// 1. Delete this component from your scenes
/// 2. Run Tools > Create Persistent Options Menu to create the new system
/// 3. Use GlobalOptionsManager.Instance.OpenOptions() in your code
///
/// This class is kept for backward compatibility but will be removed in a future version
/// </summary>
[System.Obsolete("Use GlobalOptionsManager instead. This component is deprecated.", false)]
public class GlobalOptionsAccess : MonoBehaviour
{
    [Header("Hotkey Settings")]
    [SerializeField] private bool enableGlobalHotkey = true;

    [Header("References")]
    [SerializeField] private OptionsMenuController optionsMenuController;

    private void Start()
    {
        // Find options menu controller if not assigned
        if (optionsMenuController == null)
        {
            optionsMenuController = FindFirstObjectByType<OptionsMenuController>();
        }

        if (optionsMenuController == null)
        {
            Debug.LogWarning("GlobalOptionsAccess: No OptionsMenuController found! Run Tools > Create Options Menu UI");
        }
    }

    private void Update()
    {
        if (!enableGlobalHotkey || optionsMenuController == null) return;

        // Check for options hotkey (Tab key or Select button on gamepad)
        bool optionsKeyPressed = false;

        // Keyboard: Tab key
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            optionsKeyPressed = true;
        }

        // Gamepad: Select button (usually the button with two overlapping squares)
        if (Gamepad.current != null && Gamepad.current.selectButton.wasPressedThisFrame)
        {
            optionsKeyPressed = true;
        }

        if (optionsKeyPressed)
        {
            ToggleOptions();
        }
    }

    private void ToggleOptions()
    {
        if (optionsMenuController.IsOpen)
        {
            // Close options
            optionsMenuController.CloseOptions();
        }
        else
        {
            // Open options - ALWAYS pause when opened globally
            // This ensures everything halts regardless of what the user is doing
            optionsMenuController.SetPauseTimeWhenOpen(true);
            optionsMenuController.ShowOptions(OnOptionsClosedGlobally);

            Debug.Log("Options opened globally - game paused");
        }
    }

    /// <summary>
    /// Called when options menu is closed via global hotkey
    /// </summary>
    private void OnOptionsClosedGlobally()
    {
        // Options closed - game will automatically unpause via OptionsMenuController
        Debug.Log("Options closed globally");
    }

    /// <summary>
    /// Public method to open options (can be called from UI buttons)
    /// </summary>
    public void OpenOptions()
    {
        if (optionsMenuController != null && !optionsMenuController.IsOpen)
        {
            // Always pause when opening globally - halt everything
            optionsMenuController.SetPauseTimeWhenOpen(true);
            optionsMenuController.ShowOptions(OnOptionsClosedGlobally);
        }
    }

    /// <summary>
    /// Public method to close options (can be called from UI buttons)
    /// </summary>
    public void CloseOptions()
    {
        if (optionsMenuController != null && optionsMenuController.IsOpen)
        {
            optionsMenuController.CloseOptions();
        }
    }
}
