using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the main menu UI and navigation
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private TextMeshProUGUI continueButtonText;

    [Header("Settings")]
    [SerializeField] private string gameSceneName = "Match3";
    [SerializeField] private string loadingSceneName = "LoadingScreen";
    [SerializeField] private bool useLoadingScreen = false; // Go directly to Match3, not through loading screen

    private void Start()
    {
        // Wire up button events
        if (newGameButton != null)
            newGameButton.onClick.AddListener(OnNewGame);

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinue);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettings);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExit);

        // Check if save game exists
        UpdateContinueButton();

        // Initialize global options manager (ensures it exists)
        _ = GlobalOptionsManager.Instance;
    }

    private void UpdateContinueButton()
    {
        bool hasSave = EnhancedGameSaveManager.HasSaveGame();

        if (continueButton != null)
        {
            continueButton.interactable = hasSave;

            // Visual feedback for disabled state
            if (continueButtonText != null)
            {
                continueButtonText.color = hasSave
                    ? new Color(0.9f, 0.9f, 0.9f, 1f)
                    : new Color(0.5f, 0.5f, 0.5f, 1f);
            }
        }

        Debug.Log(hasSave ? "Save game found - Continue enabled" : "No save game - Continue disabled");
    }

    private void OnNewGame()
    {
        Debug.Log("Starting new game...");

        // Clear any existing save game
        EnhancedGameSaveManager.DeleteSaveGame();

        // Load game scene
        LoadGameScene();
    }

    private void OnContinue()
    {
        Debug.Log("Continuing saved game...");

        // Load the saved game and switch to the saved scene
        EnhancedGameSaveManager.ContinueGame();
    }

    private void OnSettings()
    {
        Debug.Log("Opening settings from main menu...");

        // Use global options manager singleton
        if (GlobalOptionsManager.Instance != null)
        {
            GlobalOptionsManager.Instance.OpenOptions();
        }
        else
        {
            Debug.LogError("GlobalOptionsManager not found! Run Tools > Create Persistent Options Menu");
        }
    }

    private void OnExit()
    {
        Debug.Log("Exiting game...");

#if UNITY_EDITOR
        // Stop play mode in editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Quit application in build
        Application.Quit();
#endif
    }

    private void LoadGameScene()
    {
        if (useLoadingScreen && !string.IsNullOrEmpty(loadingSceneName))
        {
            // Load through loading screen
            SceneManager.LoadScene(loadingSceneName);
        }
        else
        {
            // Load game scene directly
            SceneManager.LoadScene(gameSceneName);
        }
    }
}
