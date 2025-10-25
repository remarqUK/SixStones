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
    [SerializeField] private bool useLoadingScreen = true;

    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button settingsBackButton;

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

        if (settingsBackButton != null)
            settingsBackButton.onClick.AddListener(OnSettingsBack);

        // Check if save game exists
        UpdateContinueButton();

        // Hide settings panel initially
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void UpdateContinueButton()
    {
        bool hasSave = GameSaveManager.HasSaveGame();

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
        GameSaveManager.DeleteSaveGame();

        // Load game scene
        LoadGameScene();
    }

    private void OnContinue()
    {
        Debug.Log("Continuing saved game...");

        // Load game scene (save data will be loaded in the game scene)
        LoadGameScene();
    }

    private void OnSettings()
    {
        Debug.Log("Opening settings...");

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    private void OnSettingsBack()
    {
        Debug.Log("Closing settings...");

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
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
