using UnityEngine;

/// <summary>
/// Persistent singleton that manages global options menu access across all scenes
/// Uses DontDestroyOnLoad to persist across scene changes
///
/// SETUP INSTRUCTIONS:
/// 1. Run "Tools > Create Persistent Options Menu Prefab" in Unity Editor (one time only)
/// 2. This creates a prefab at Assets/Resources/GlobalOptionsManager.prefab
/// 3. The system will auto-instantiate on first access and persist across all scene changes
/// 4. No need to add to any scene - it auto-creates from the prefab
///
/// USAGE:
/// - Press Escape to open pause menu, then click Settings button to open options
/// - Options menu always pauses the game when opened
/// - From code: GlobalOptionsManager.Instance.OpenOptions()
/// - With callback: GlobalOptionsManager.Instance.OpenOptions(() => Debug.Log("Options closed"))
///
/// FEATURES:
/// - Persists across scene changes (DontDestroyOnLoad)
/// - Auto-instantiates from Resources prefab on first access
/// - Always pauses game when opened
/// - Works from any scene without scene-specific setup
/// - Creates persistent canvas overlay (renders on top of everything)
/// - Singleton pattern ensures only one instance exists
///
/// INTEGRATION WITH EXISTING CODE:
/// - MainMenuManager: Uses Instance.OpenOptions() for Settings button
/// - PauseMenu: Uses Instance.OpenOptions(callback) to return to pause menu when closed
/// - Any script can call Instance.OpenOptions() to show options
/// </summary>
public class GlobalOptionsManager : MonoBehaviour
{
    private static GlobalOptionsManager instance;
    public static GlobalOptionsManager Instance
    {
        get
        {
            if (instance == null)
            {
                // Try to find existing instance
                instance = FindFirstObjectByType<GlobalOptionsManager>();

                // If still null, try to load from Resources
                if (instance == null)
                {
                    GameObject prefab = Resources.Load<GameObject>("GlobalOptionsManager");
                    if (prefab != null)
                    {
                        GameObject go = Instantiate(prefab);
                        instance = go.GetComponent<GlobalOptionsManager>();
                        DontDestroyOnLoad(go);
                        Debug.Log("GlobalOptionsManager instantiated from prefab and will persist across scenes");
                    }
                    else
                    {
                        // No prefab found - create basic instance
                        GameObject go = new GameObject("GlobalOptionsManager");
                        instance = go.AddComponent<GlobalOptionsManager>();
                        DontDestroyOnLoad(go);
                        instance.Initialize();
                        Debug.LogWarning("GlobalOptionsManager created without prefab. Run Tools > Create Persistent Options Menu Prefab to set up the UI.");
                    }
                }
            }
            return instance;
        }
    }

    [Header("References - Auto-configured")]
    [SerializeField] private OptionsMenuController optionsMenuController;
    [SerializeField] private Canvas persistentCanvas;
    private bool isInitialized = false;

    public bool IsOpen => optionsMenuController != null && optionsMenuController.IsOpen;

    private void Awake()
    {
        // Enforce singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"GlobalOptionsManager Awake: Setting as instance and persisting. Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
            Initialize();
        }
        else if (instance != this)
        {
            Debug.Log($"Duplicate GlobalOptionsManager found in {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name} - destroying");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Initialize the persistent canvas and options controller
    /// </summary>
    private void Initialize()
    {
        if (isInitialized) return;

        // Create persistent canvas if it doesn't exist
        if (persistentCanvas == null)
        {
            CreatePersistentCanvas();
        }

        // Get or add OptionsMenuController
        if (optionsMenuController == null)
        {
            optionsMenuController = GetComponent<OptionsMenuController>();
            if (optionsMenuController == null)
            {
                Debug.LogWarning("GlobalOptionsManager: OptionsMenuController not found. Options menu UI needs to be created.");
            }
            else
            {
                Debug.Log($"GlobalOptionsManager: Found OptionsMenuController on {gameObject.name}");
            }
        }

        isInitialized = true;
        Debug.Log($"GlobalOptionsManager initialized - Canvas: {(persistentCanvas != null ? "Found" : "Missing")}, Controller: {(optionsMenuController != null ? "Found" : "Missing")}");
    }

    /// <summary>
    /// Creates a persistent canvas that will overlay all scenes
    /// </summary>
    private void CreatePersistentCanvas()
    {
        GameObject canvasObj = new GameObject("PersistentOptionsCanvas");
        canvasObj.transform.SetParent(transform, false);

        persistentCanvas = canvasObj.AddComponent<Canvas>();
        persistentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        persistentCanvas.sortingOrder = 1000; // Render on top of everything

        var canvasScaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        canvasScaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        Debug.Log("Created persistent canvas for options menu");
    }

    /// <summary>
    /// Toggle options menu open/closed
    /// </summary>
    public void ToggleOptions()
    {
        if (optionsMenuController == null)
        {
            Debug.LogWarning("GlobalOptionsManager: Cannot toggle options - OptionsMenuController not found");
            return;
        }

        if (optionsMenuController.IsOpen)
        {
            CloseOptions();
        }
        else
        {
            OpenOptions();
        }
    }

    /// <summary>
    /// Open the options menu globally (always pauses)
    /// </summary>
    /// <param name="onClose">Optional callback to invoke when options are closed</param>
    public void OpenOptions(System.Action onClose = null)
    {
        if (optionsMenuController == null)
        {
            Debug.LogError("GlobalOptionsManager: Cannot open options - OptionsMenuController is null!\n" +
                          "Run Tools > Create Persistent Options Menu to set up the options system.");
            return;
        }

        if (optionsMenuController.IsOpen)
        {
            Debug.LogWarning("GlobalOptionsManager: Options menu is already open");
            return;
        }

        // ALWAYS pause when opening globally - halt everything
        optionsMenuController.SetPauseTimeWhenOpen(true);

        // Wrap callback to include our own logic
        System.Action wrappedCallback = () =>
        {
            OnOptionsClosedGlobally();
            onClose?.Invoke();
        };

        optionsMenuController.ShowOptions(wrappedCallback);

        Debug.Log("Options opened globally - game paused");
    }

    /// <summary>
    /// Close the options menu
    /// </summary>
    public void CloseOptions()
    {
        if (optionsMenuController == null || !optionsMenuController.IsOpen)
            return;

        optionsMenuController.CloseOptions();
    }

    /// <summary>
    /// Called when options menu is closed via global hotkey
    /// </summary>
    private void OnOptionsClosedGlobally()
    {
        Debug.Log("Options closed globally");
    }

    /// <summary>
    /// Get the persistent canvas for options UI
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
    /// Set the options controller (called by setup scripts)
    /// </summary>
    public void SetOptionsController(OptionsMenuController controller)
    {
        optionsMenuController = controller;
        Debug.Log("OptionsMenuController registered with GlobalOptionsManager");
    }
}
