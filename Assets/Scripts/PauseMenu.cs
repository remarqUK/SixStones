using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Global persistent pause menu that works across all scenes (Match3, Maze3D, zone maps, etc.)
/// Uses DontDestroyOnLoad to persist across scene changes
///
/// SETUP INSTRUCTIONS:
/// 1. Run "Tools > Create Global Pause Menu Prefab" in Unity Editor (one time only)
/// 2. This creates a prefab at Assets/Resources/GlobalPauseMenu.prefab
/// 3. The system will auto-instantiate at runtime and persist across all scene changes
/// 4. No need to add to any scene - it auto-creates from the prefab
///
/// USAGE:
/// - Press Escape (keyboard) or Start button (gamepad) to toggle pause menu in any scene
/// - Automatically pauses game (Time.timeScale = 0) when opened
/// - Auto-saves game when pause menu is opened
/// - Works in 2D (Match3), 3D (Maze3D), and UI-only scenes (zone/subzone maps)
///
/// FEATURES:
/// - Persists across scene changes (DontDestroyOnLoad)
/// - Auto-instantiates at game start using [RuntimeInitializeOnLoadMethod]
/// - Singleton pattern ensures only one instance exists
/// - Persistent canvas overlay (renders on top of everything)
/// - Integrates with GlobalOptionsManager for Settings button
/// - Auto-saves progress when opened (never lose progress!)
/// </summary>
public class PauseMenu : MonoBehaviour
{
    private static PauseMenu instance;

    /// <summary>
    /// Automatically initializes the PauseMenu singleton when the game starts
    /// This ensures it's available in all scenes without manual setup
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInitialize()
    {
        // Access Instance to trigger auto-instantiation
        if (Instance != null)
        {
            Debug.Log("GlobalPauseMenu auto-initialized and ready");
        }
    }
    public static PauseMenu Instance
    {
        get
        {
            if (instance == null)
            {
                // Try to find existing instance
                instance = FindFirstObjectByType<PauseMenu>();

                // If still null, try to load from Resources
                if (instance == null)
                {
                    GameObject prefab = Resources.Load<GameObject>("GlobalPauseMenu");
                    if (prefab != null)
                    {
                        GameObject go = Instantiate(prefab);
                        instance = go.GetComponent<PauseMenu>();
                        DontDestroyOnLoad(go);
                        Debug.Log("GlobalPauseMenu instantiated from prefab and will persist across scenes");
                    }
                    else
                    {
                        // No prefab found - create basic instance
                        GameObject go = new GameObject("GlobalPauseMenu");
                        instance = go.AddComponent<PauseMenu>();
                        DontDestroyOnLoad(go);
                        instance.Initialize();
                        Debug.LogWarning("GlobalPauseMenu created without prefab. Run Tools > Create Global Pause Menu Prefab to set up the UI.");
                    }
                }
            }
            return instance;
        }
    }

    [Header("References - Auto-configured")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Canvas persistentCanvas;

    private bool isPaused = false;
    private bool isInitialized = false;

    private void Awake()
    {
        // Enforce singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"GlobalPauseMenu Awake: Setting as instance and persisting. Scene: {SceneManager.GetActiveScene().name}");
            Initialize();
        }
        else if (instance != this)
        {
            Debug.Log($"Duplicate GlobalPauseMenu found in {SceneManager.GetActiveScene().name} - destroying");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Initialize the persistent canvas and pause menu UI
    /// </summary>
    private void Initialize()
    {
        if (isInitialized) return;

        // Initialize global options manager (ensures it exists)
        _ = GlobalOptionsManager.Instance;

        // Create persistent canvas if it doesn't exist
        if (persistentCanvas == null)
        {
            CreatePersistentCanvas();
        }

        // Fix pause menu panel RectTransform (workaround for Unity scene caching issue)
        if (pauseMenuPanel != null)
        {
            RectTransform rectTransform = pauseMenuPanel.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // Set anchors to stretch across entire screen
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(1, 1);
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = Vector2.zero;
                Debug.Log("PauseMenu: Fixed panel RectTransform to fill screen");

                // Also fix the text
                Transform pausedText = pauseMenuPanel.transform.Find("PausedText");
                if (pausedText != null)
                {
                    RectTransform textRect = pausedText.GetComponent<RectTransform>();
                    if (textRect != null)
                    {
                        textRect.anchorMin = new Vector2(0, 0);
                        textRect.anchorMax = new Vector2(1, 1);
                        textRect.anchoredPosition = Vector2.zero;
                        textRect.sizeDelta = new Vector2(-40, -40);
                    }
                }

                // Fix the Image color to black
                UnityEngine.UI.Image image = pauseMenuPanel.GetComponent<UnityEngine.UI.Image>();
                if (image != null)
                {
                    image.color = new Color(0, 0, 0, 0.95f);
                }
            }

            // Make sure panel is initially hidden
            pauseMenuPanel.SetActive(false);
        }

        isInitialized = true;
        Debug.Log($"GlobalPauseMenu initialized - Canvas: {(persistentCanvas != null ? "Found" : "Missing")}, Panel: {(pauseMenuPanel != null ? "Found" : "Missing")}");
    }

    /// <summary>
    /// Creates a persistent canvas that will overlay all scenes
    /// </summary>
    private void CreatePersistentCanvas()
    {
        GameObject canvasObj = new GameObject("PersistentPauseCanvas");
        canvasObj.transform.SetParent(transform, false);

        persistentCanvas = canvasObj.AddComponent<Canvas>();
        persistentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        persistentCanvas.sortingOrder = 999; // Render on top (just below GlobalOptionsManager at 1000)

        var canvasScaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        canvasScaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        Debug.Log("Created persistent canvas for pause menu");
    }

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
        if (GlobalOptionsManager.Instance != null && GlobalOptionsManager.Instance.IsOpen)
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

        // Disable player controllers that might interfere with UI input
        DisablePlayerControllers();

        // Make cursor visible and unlocked for UI interaction
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Debug.Log("Game Paused");

        // Auto-save when pause menu opens (after pausing)
        StartCoroutine(AutoSaveWhenReady());
    }

    /// <summary>
    /// Waits for board to finish processing, then saves the game
    /// </summary>
    private System.Collections.IEnumerator AutoSaveWhenReady()
    {
        // Find the board (if in Match3 scene)
        Board board = FindFirstObjectByType<Board>();

        if (board != null && board.IsProcessing)
        {
            Debug.Log("[Auto-Save] Waiting for board animations to complete before saving...");

            // Wait for board to finish processing
            // Use real time since game is paused (Time.timeScale = 0)
            float timeout = 5f; // 5 second timeout
            float elapsed = 0f;

            while (board.IsProcessing && elapsed < timeout)
            {
                yield return null; // Wait one frame
                elapsed += Time.unscaledDeltaTime; // Use unscaled time since game is paused
            }

            if (elapsed >= timeout)
            {
                Debug.LogWarning("[Auto-Save] Timeout waiting for board to finish processing. Skipping save.");
                yield break;
            }
        }

        // Now it's safe to save
        bool saveSuccess = EnhancedGameSaveManager.SaveGame();
        if (saveSuccess)
        {
            Debug.Log("<color=green>[Auto-Save] Game saved when pause menu opened</color>");
        }
        else
        {
            Debug.LogWarning("[Auto-Save] Failed to save game when pause menu opened");
        }
    }

    private void HidePauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Resume the game
        Time.timeScale = 1f;

        // Re-enable player controllers
        EnablePlayerControllers();

        Debug.Log("Game Resumed");
    }

    private void DisablePlayerControllers()
    {
        // Disable FirstPersonMazeController if present (Maze3D scene)
        FirstPersonMazeController mazeController = FindFirstObjectByType<FirstPersonMazeController>();
        if (mazeController != null)
        {
            mazeController.enabled = false;
        }

        // Disable any other player controllers if needed
        // Add more controller types here as needed
    }

    private void EnablePlayerControllers()
    {
        // Re-enable FirstPersonMazeController if present
        FirstPersonMazeController mazeController = FindFirstObjectByType<FirstPersonMazeController>();
        if (mazeController != null)
        {
            mazeController.enabled = true;
        }

        // Re-enable any other player controllers if needed
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

        if (GlobalOptionsManager.Instance != null)
        {
            // Check if options controller is available
            if (GlobalOptionsManager.Instance.IsOpen || HasOptionsController())
            {
                // Open global options with callback to return to pause menu
                GlobalOptionsManager.Instance.OpenOptions(OnOptionsClose);
            }
            else
            {
                // Options controller not set up - restore pause menu
                Debug.LogError("GlobalOptionsManager exists but OptionsMenuController is not set up!\n" +
                              "Run Tools > Create Persistent Options Menu");

                if (pauseMenuPanel != null)
                {
                    pauseMenuPanel.SetActive(true);
                }
            }
        }
        else
        {
            Debug.LogError("GlobalOptionsManager not found! Run Tools > Create Persistent Options Menu");

            // Restore pause menu
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Check if GlobalOptionsManager has an OptionsMenuController set up
    /// </summary>
    private bool HasOptionsController()
    {
        if (GlobalOptionsManager.Instance == null)
            return false;

        // Try to get the OptionsMenuController component
        var controller = GlobalOptionsManager.Instance.GetComponent<OptionsMenuController>();
        return controller != null;
    }

    /// <summary>
    /// Called when options menu closes - return to pause menu
    /// </summary>
    private void OnOptionsClose()
    {
        Debug.Log("Closing options panel - returning to pause menu");

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        // Re-pause the game (we want pause menu to keep it paused)
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Close options menu and return to pause menu
    /// </summary>
    public void CloseOptions()
    {
        if (GlobalOptionsManager.Instance != null)
        {
            GlobalOptionsManager.Instance.CloseOptions();
        }

        // Return to pause menu
        OnOptionsClose();
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
    /// Restart the current scene (called by Restart Game button)
    /// Works generically - reloads whatever scene is currently active
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("Restart Game clicked - reloading current scene");

        // Unpause first
        isPaused = false;
        Time.timeScale = 1f;

        // Hide pause menu
        HidePauseMenu();

        // Reload the current scene
        SceneIdentifier currentScene = SceneHelper.GetCurrentScene();
        SceneHelper.LoadScene(currentScene);
    }

    /// <summary>
    /// Return to main menu (called by Return to Main Menu button)
    /// </summary>
    public void ReturnToMainMenu()
    {
        Debug.Log("Return to Main Menu clicked");

        // Unpause first
        isPaused = false;
        Time.timeScale = 1f;

        // Hide pause menu
        HidePauseMenu();

        // Load main menu scene
        SceneHelper.LoadScene(SceneIdentifier.MainMenu);
    }

    /// <summary>
    /// Get the persistent canvas for pause menu UI
    /// </summary>
    public Canvas GetPersistentCanvas()
    {
        if (persistentCanvas == null)
        {
            CreatePersistentCanvas();
        }
        return persistentCanvas;
    }

    /// <summary>
    /// Set the pause menu panel (called by setup scripts)
    /// </summary>
    public void SetPauseMenuPanel(GameObject panel)
    {
        pauseMenuPanel = panel;
        Debug.Log("Pause menu panel registered with GlobalPauseMenu");
    }
}
