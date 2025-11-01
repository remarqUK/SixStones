using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the main menu UI and navigation
///
/// BUTTONS:
/// - New Game: Starts a fresh game, deleting all saves
/// - Continue: Loads the most recent save file (disabled if no saves exist)
/// - Load Game: Opens a browser showing all 10 save slots for manual selection
/// - Settings: Opens the global options menu
/// - Exit: Quits the game
///
/// SAVE SLOT BROWSER:
/// - Shows all 10 save slots with metadata (level, zone, date, playtime)
/// - Empty slots are grayed out and disabled
/// - Click any slot to load that specific save
/// - Each slot shows: Slot #, Level, Zone, Date, and Playtime
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("UI References - Main Menu")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private TextMeshProUGUI continueButtonText;
    [SerializeField] private TextMeshProUGUI loadGameButtonText;

    [Header("UI References - Load Game Panel")]
    [SerializeField] private GameObject loadGamePanel;
    [SerializeField] private Transform saveSlotContainer;
    [SerializeField] private GameObject saveSlotButtonPrefab;
    [SerializeField] private Button closeBrowserButton;

    [Header("Settings")]
    [SerializeField] private SceneIdentifier gameScene = SceneIdentifier.Match3;
    [SerializeField] private SceneIdentifier loadingScene = SceneIdentifier.LoadingScreen;
    [SerializeField] private bool useLoadingScreen = false; // Go directly to Match3, not through loading screen

    private void Start()
    {
        // Wire up button events
        if (newGameButton != null)
            newGameButton.onClick.AddListener(OnNewGame);

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinue);

        if (loadGameButton != null)
            loadGameButton.onClick.AddListener(OnLoadGame);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettings);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExit);

        if (closeBrowserButton != null)
            closeBrowserButton.onClick.AddListener(CloseLoadGamePanel);

        // Check if save game exists and update buttons
        UpdateButtons();

        // Make sure load game panel is hidden initially
        if (loadGamePanel != null)
            loadGamePanel.SetActive(false);

        // Initialize global options manager (ensures it exists)
        _ = GlobalOptionsManager.Instance;
    }

    private void UpdateButtons()
    {
        bool hasSave = EnhancedGameSaveManager.HasSaveGame();
        bool hasAnySave = EnhancedGameSaveManager.HasAnySaveGame();

        Debug.Log($"[MainMenu] UpdateButtons: hasSave={hasSave}, hasAnySave={hasAnySave}");

        // Update Continue button (uses most recent save)
        if (continueButton != null)
        {
            continueButton.interactable = hasSave;

            if (continueButtonText != null)
            {
                continueButtonText.color = hasSave
                    ? new Color(0.9f, 0.9f, 0.9f, 1f)
                    : new Color(0.5f, 0.5f, 0.5f, 1f);
            }

            Debug.Log($"[MainMenu] Continue button: interactable={hasSave}");
        }

        // Update Load Game button (enabled if any save exists in any slot)
        if (loadGameButton != null)
        {
            loadGameButton.interactable = hasAnySave;

            if (loadGameButtonText != null)
            {
                loadGameButtonText.color = hasAnySave
                    ? new Color(0.9f, 0.9f, 0.9f, 1f)
                    : new Color(0.5f, 0.5f, 0.5f, 1f);
            }

            Debug.Log($"[MainMenu] Load Game button: interactable={hasAnySave}");
        }
        else
        {
            Debug.LogWarning("[MainMenu] Load Game button is not assigned!");
        }
    }

    private void OnNewGame()
    {
        Debug.Log("Starting new game...");

        // Clear any existing save game
        EnhancedGameSaveManager.DeleteSaveGame();

        // Load character selection scene
        SceneHelper.LoadScene(SceneIdentifier.CharacterSelection);
    }

    private void OnContinue()
    {
        Debug.Log("Continuing saved game...");

        // Load the saved game and switch to the saved scene
        EnhancedGameSaveManager.ContinueGame();
    }

    private void OnLoadGame()
    {
        Debug.Log("Opening load game browser...");

        // Check if any save games exist
        if (!EnhancedGameSaveManager.HasAnySaveGame())
        {
            Debug.LogWarning("[LoadGame] No save games found via HasAnySaveGame()");
            ShowNoSaveGamesDialog();
            return;
        }

        Debug.Log("[LoadGame] Save games exist, opening browser...");

        if (loadGamePanel == null)
        {
            Debug.LogError("[LoadGame] Load Game Panel is not assigned in MainMenuManager!");
            Debug.LogError("[LoadGame] Please run: Tools > Main Menu > Create Load Game Panel");
            return;
        }

        if (saveSlotContainer == null)
        {
            Debug.LogError("[LoadGame] Save Slot Container is not assigned in MainMenuManager!");
            return;
        }

        if (saveSlotButtonPrefab == null)
        {
            Debug.LogError("[LoadGame] Save Slot Button Prefab is not assigned in MainMenuManager!");
            Debug.LogError("[LoadGame] Please run: Tools > Main Menu > Create Save Slot Button Prefab");
            return;
        }

        // Everything is assigned, open the panel
        Debug.Log($"[LoadGame] Opening panel: {loadGamePanel.name}");
        loadGamePanel.SetActive(true);
        Debug.Log($"[LoadGame] Panel active state: {loadGamePanel.activeSelf}");

        PopulateSaveSlots();
    }

    private void ShowNoSaveGamesDialog()
    {
        Debug.LogWarning("No save games found!");

        // Create a simple dialog panel
        if (UnityEngine.Object.FindFirstObjectByType<Canvas>() != null)
        {
            StartCoroutine(ShowNoSaveGamesDialogCoroutine());
        }
    }

    private System.Collections.IEnumerator ShowNoSaveGamesDialogCoroutine()
    {
        Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) yield break;

        // Create dialog background
        GameObject dialogObj = new GameObject("NoSaveGamesDialog");
        dialogObj.transform.SetParent(canvas.transform, false);

        RectTransform dialogRect = dialogObj.AddComponent<RectTransform>();
        dialogRect.anchorMin = Vector2.zero;
        dialogRect.anchorMax = Vector2.one;
        dialogRect.sizeDelta = Vector2.zero;
        dialogRect.anchoredPosition = Vector2.zero;

        Image dialogBg = dialogObj.AddComponent<Image>();
        dialogBg.color = new Color(0, 0, 0, 0.8f);

        // Create message panel
        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(dialogObj.transform, false);

        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(500, 300);

        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.2f, 0.2f, 0.25f, 1f);

        // Create message text
        GameObject textObj = new GameObject("MessageText");
        textObj.transform.SetParent(panelObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = new Vector2(0, 40);
        textRect.sizeDelta = new Vector2(450, 150);

        TextMeshProUGUI messageText = textObj.AddComponent<TextMeshProUGUI>();
        messageText.text = "No Save Games Found\n\nThere are no saved games to load.\nStart a new game to begin playing.";
        messageText.fontSize = 28;
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.color = Color.white;

        // Create OK button
        GameObject buttonObj = new GameObject("OKButton");
        buttonObj.transform.SetParent(panelObj.transform, false);

        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0, -80);
        buttonRect.sizeDelta = new Vector2(200, 60);

        Button button = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.3f, 0.5f, 0.8f, 1f);

        // Add button click handler to destroy dialog
        button.onClick.AddListener(() => Destroy(dialogObj));

        // Create button text
        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(buttonObj.transform, false);

        RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.sizeDelta = Vector2.zero;
        btnTextRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI buttonText = btnTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "OK";
        buttonText.fontSize = 32;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;

        yield return null;
    }

    private void PopulateSaveSlots()
    {
        Debug.Log("[PopulateSaveSlots] Starting to populate save slots...");

        if (saveSlotContainer == null)
        {
            Debug.LogError("[PopulateSaveSlots] Save Slot Container is not assigned!");
            return;
        }

        if (saveSlotButtonPrefab == null)
        {
            Debug.LogError("[PopulateSaveSlots] Save Slot Button Prefab is not assigned!");
            return;
        }

        // Clear existing slot buttons
        int clearedCount = 0;
        foreach (Transform child in saveSlotContainer)
        {
            Destroy(child.gameObject);
            clearedCount++;
        }
        Debug.Log($"[PopulateSaveSlots] Cleared {clearedCount} existing buttons");

        // Get all save slot information
        var allSlots = EnhancedGameSaveManager.GetAllSaveSlots();
        Debug.Log($"[PopulateSaveSlots] Retrieved {allSlots.Count} slot info objects");

        // Check if all slots are empty
        bool hasAnySave = false;
        int nonEmptyCount = 0;
        foreach (var slot in allSlots)
        {
            if (!slot.isEmpty)
            {
                hasAnySave = true;
                nonEmptyCount++;
                Debug.Log($"[PopulateSaveSlots] Slot {slot.slotNumber} has save data: Level {slot.playerLevel}, Zone {slot.currentZone}/{slot.currentSubZone}");
            }
        }

        Debug.Log($"[PopulateSaveSlots] Found {nonEmptyCount} non-empty slots");

        if (!hasAnySave)
        {
            Debug.LogWarning("[PopulateSaveSlots] All save slots are empty!");
            // This shouldn't happen since we check HasAnySaveGame() before opening the panel
            // But if it does, close the panel and show the dialog
            if (loadGamePanel != null)
            {
                loadGamePanel.SetActive(false);
            }
            ShowNoSaveGamesDialog();
            return;
        }

        // Create a button for each save slot
        int buttonsCreated = 0;
        foreach (var slotInfo in allSlots)
        {
            GameObject slotButtonObj = Instantiate(saveSlotButtonPrefab, saveSlotContainer);
            if (slotButtonObj == null)
            {
                Debug.LogError($"[PopulateSaveSlots] Failed to instantiate button for slot {slotInfo.slotNumber}!");
                continue;
            }

            buttonsCreated++;
            slotButtonObj.name = $"SaveSlot_{slotInfo.slotNumber}";

            // Check button RectTransform
            RectTransform buttonRect = slotButtonObj.GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                Debug.Log($"[PopulateSaveSlots] Slot {slotInfo.slotNumber} Button RectTransform: size={buttonRect.sizeDelta}, rect.size={buttonRect.rect.size}");
            }

            Button slotButton = slotButtonObj.GetComponent<Button>();
            if (slotButton == null)
            {
                Debug.LogError($"[PopulateSaveSlots] Slot {slotInfo.slotNumber} button has no Button component!");
                continue;
            }

            // Get the existing TextMeshPro component from the prefab
            TextMeshProUGUI slotText = slotButtonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (slotText == null)
            {
                Debug.LogError($"[PopulateSaveSlots] Slot {slotInfo.slotNumber} button prefab has no TextMeshProUGUI component!");
                continue;
            }

            // DON'T destroy and recreate - the prefab already has font and material assigned!
            // Just ensure it's configured properly
            slotText.fontSize = 20;
            slotText.enableAutoSizing = false;
            slotText.alignment = TextAlignmentOptions.Left;

            if (slotInfo.isEmpty)
            {
                slotText.text = $"Slot {slotInfo.slotNumber} - Empty";
                slotButton.interactable = false;
                slotText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            }
            else
            {
                slotText.text = $"Slot {slotInfo.slotNumber}\n" +
                               $"Level {slotInfo.playerLevel} - Zone {slotInfo.currentZone}/{slotInfo.currentSubZone}\n" +
                               $"{slotInfo.saveDate}\n" +
                               $"Playtime: {slotInfo.GetPlaytimeFormatted()}";
                slotButton.interactable = true;
                slotText.color = new Color(0.9f, 0.9f, 0.9f, 1f);

                // Wire up the button to load this slot
                int slotNumber = slotInfo.slotNumber; // Capture for closure
                slotButton.onClick.AddListener(() => LoadSaveSlot(slotNumber));
            }

            // Force TextMeshPro to update the mesh
            slotText.ForceMeshUpdate();

            Debug.Log($"[PopulateSaveSlots] Slot {slotInfo.slotNumber} created: Text='{slotText.text.Substring(0, Mathf.Min(30, slotText.text.Length))}...', Font={slotText.font?.name}, SharedMat={slotText.fontSharedMaterial?.name}");
        }

        Debug.Log($"[PopulateSaveSlots] Completed: Created {buttonsCreated} buttons from {allSlots.Count} slots");
    }

    private void LoadSaveSlot(int slotNumber)
    {
        Debug.Log($"[LoadSaveSlot] Loading save slot {slotNumber}...");

        // Use ContinueGame to properly load the scene and restore save data
        EnhancedGameSaveManager.ContinueGame(slotNumber);
    }

    private void CloseLoadGamePanel()
    {
        Debug.Log("Closing load game browser...");

        if (loadGamePanel != null)
        {
            loadGamePanel.SetActive(false);
        }
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
        if (useLoadingScreen)
        {
            // Load through loading screen
            SceneHelper.LoadScene(loadingScene);
        }
        else
        {
            // Load game scene directly
            SceneHelper.LoadScene(gameScene);
        }
    }
}
