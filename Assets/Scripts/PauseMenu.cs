using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the pause menu - shows when Escape or controller menu button is pressed
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameManager gameManager;

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
        // If options menu is open, close it and return to pause menu
        if (optionsPanel != null && optionsPanel.activeSelf)
        {
            CloseOptions();
            return;
        }

        // Otherwise, toggle pause menu normally
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
    /// Show options menu (hides pause menu)
    /// </summary>
    public void ShowOptions()
    {
        Debug.Log("Options clicked - showing options panel");

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Close options menu and return to pause menu
    /// </summary>
    public void CloseOptions()
    {
        Debug.Log("Closing options panel");

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }
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

    /// <summary>
    /// Restart the game (called by Restart Game button)
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("Restart Game clicked");

        // Unpause first
        isPaused = false;
        Time.timeScale = 1f;

        // Call GameManager's reset
        if (gameManager != null)
        {
            gameManager.ResetGame();
        }
        else
        {
            Debug.LogError("GameManager reference not set in PauseMenu!");
        }
    }
}
