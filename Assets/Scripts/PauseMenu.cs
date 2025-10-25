using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the pause menu - shows when Escape or controller menu button is pressed
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuPanel;

    private bool isPaused = false;

    private void Update()
    {
        // Check for Escape key or controller menu button (Start button)
        bool pausePressed = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;

        // Also check for gamepad Start button (menu button)
        if (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)
        {
            pausePressed = true;
        }

        if (pausePressed)
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            ShowPauseMenu();
        }
        else
        {
            HidePauseMenu();
        }
    }

    private void ShowPauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        // Pause the game
        Time.timeScale = 0f;
        Debug.Log("Game Paused");
    }

    private void HidePauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Resume the game
        Time.timeScale = 1f;
        Debug.Log("Game Resumed");
    }

    /// <summary>
    /// Public method to resume game (called by Resume button)
    /// </summary>
    public void ResumeGame()
    {
        Debug.Log("Resume Game clicked");
        isPaused = false;
        HidePauseMenu();
    }

    /// <summary>
    /// Placeholder for Options menu (not implemented yet)
    /// </summary>
    public void ShowOptions()
    {
        Debug.Log("Options clicked (not implemented yet)");
    }

    /// <summary>
    /// Placeholder for Save & Continue (not implemented yet)
    /// </summary>
    public void SaveAndContinue()
    {
        Debug.Log("Save & Continue clicked (not implemented yet)");
    }

    /// <summary>
    /// Placeholder for Save & Quit (not implemented yet)
    /// </summary>
    public void SaveAndQuit()
    {
        Debug.Log("Save & Quit clicked (not implemented yet)");
    }
}
