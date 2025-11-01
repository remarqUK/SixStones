using UnityEngine;
using TMPro;

/// <summary>
/// Debug component for testing save system
/// Shows real-time save status and provides keyboard shortcuts
/// </summary>
public class SaveSystemDebugger : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private bool showDebugUI = true;
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private KeyCode saveKey = KeyCode.F5;
    [SerializeField] private KeyCode loadKey = KeyCode.F9;
    [SerializeField] private KeyCode deleteKey = KeyCode.F10;
    [SerializeField] private KeyCode infoKey = KeyCode.F11;

    [Header("Debug Display")]
    [SerializeField] private bool showSaveStatus = true;
    [SerializeField] private bool showPlayerStats = true;
    [SerializeField] private bool showInventoryCount = true;
    [SerializeField] private bool showEquipmentCount = true;
    [SerializeField] private bool showMazeInfo = true;
    [SerializeField] private float updateInterval = 0.5f;

    private float updateTimer = 0f;
    private string statusText = "";

    private void Update()
    {
        // Update debug display
        if (showDebugUI)
        {
            updateTimer += Time.deltaTime;
            if (updateTimer >= updateInterval)
            {
                UpdateDebugDisplay();
                updateTimer = 0f;
            }
        }

        // Keyboard shortcuts
        if (Input.GetKeyDown(saveKey))
        {
            TestSave();
        }

        if (Input.GetKeyDown(loadKey))
        {
            TestLoad();
        }

        if (Input.GetKeyDown(deleteKey))
        {
            TestDelete();
        }

        if (Input.GetKeyDown(infoKey))
        {
            TestShowInfo();
        }
    }

    private void UpdateDebugDisplay()
    {
        if (debugText == null || !showDebugUI)
            return;

        statusText = "=== SAVE SYSTEM DEBUG ===\n\n";

        // Save status
        if (showSaveStatus)
        {
            bool hasSave = EnhancedGameSaveManager.HasAnySaveGame();
            statusText += $"<b>Save Files:</b> {(hasSave ? "<color=green>EXIST</color>" : "<color=red>NONE FOUND</color>")}\n";

            if (hasSave)
            {
                var usedSlots = EnhancedGameSaveManager.GetUsedSlots();
                statusText += $"Used Slots: {string.Join(", ", usedSlots)}\n";

                // Show most recent save
                int recentSlot = EnhancedGameSaveManager.GetMostRecentSlot();
                if (recentSlot != -1)
                {
                    SaveSlotInfo info = EnhancedGameSaveManager.GetSaveSlotInfo(recentSlot);
                    if (info != null && !info.isEmpty)
                    {
                        statusText += $"\n<b>Most Recent (Slot {recentSlot}):</b>\n";
                        statusText += $"Date: {info.saveDate}\n";
                        statusText += $"Level: {info.playerLevel}\n";
                        statusText += $"Zone: {info.currentZone}/{info.currentSubZone}\n";
                        statusText += $"Size: {info.fileSizeKB} KB\n";
                    }
                }
            }
            statusText += "\n";
        }

        // Player stats
        GameManager gameManager = UnityEngine.Object.FindFirstObjectByType<GameManager>();
        if (showPlayerStats && gameManager != null)
        {
            statusText += "<b>Player Stats:</b>\n";
            int p1Health = gameManager.GetCurrentHealth(PlayerManager.Player.Player1);
            int p1MaxHealth = gameManager.GetMaxHealth(PlayerManager.Player.Player1);
            int p2Health = gameManager.GetCurrentHealth(PlayerManager.Player.Player2);
            int p2MaxHealth = gameManager.GetMaxHealth(PlayerManager.Player.Player2);
            statusText += $"P1: {p1Health}/{p1MaxHealth} HP\n";
            statusText += $"P2: {p2Health}/{p2MaxHealth} HP\n";

            if (LevelSystem.Instance != null)
            {
                statusText += $"Level: {LevelSystem.Instance.CurrentLevel}, XP: {LevelSystem.Instance.CurrentXP}\n";
            }

            if (CurrencyManager.Instance != null)
            {
                statusText += $"Gold: {CurrencyManager.Instance.CurrentGold}\n";
            }
            statusText += "\n";
        }

        // Inventory
        if (showInventoryCount)
        {
            Inventory inventory = UnityEngine.Object.FindFirstObjectByType<Inventory>();
            if (inventory != null)
            {
                statusText += $"<b>Inventory:</b> {inventory.UsedSlots}/{inventory.MaxSlots} slots\n";
                statusText += $"Weight: {inventory.GetTotalWeight():F1} lbs\n";
                statusText += $"Value: {inventory.GetTotalValue()} gp\n\n";
            }
        }

        // Equipment
        if (showEquipmentCount)
        {
            PlayerEquipment equipment = UnityEngine.Object.FindFirstObjectByType<PlayerEquipment>();
            if (equipment != null)
            {
                int equippedCount = equipment.GetAllEquippedItems().Count;
                statusText += $"<b>Equipment:</b> {equippedCount} items equipped\n";
                statusText += $"Armor Class: {equipment.GetTotalArmorClass()}\n\n";
            }
        }

        // Maze info
        if (showMazeInfo)
        {
            MapGenerator mapGen = UnityEngine.Object.FindFirstObjectByType<MapGenerator>();
            FirstPersonMazeController mazeController = UnityEngine.Object.FindFirstObjectByType<FirstPersonMazeController>();

            if (mapGen != null && mapGen.grid != null)
            {
                statusText += "<b>Maze:</b>\n";
                statusText += $"Size: {mapGen.grid.GetLength(0)}x{mapGen.grid.GetLength(1)}\n";
                statusText += $"Seed: {mapGen.randomSeed}\n";

                if (mazeController != null)
                {
                    statusText += $"Player Pos: ({mazeController.GridX}, {mazeController.GridZ})\n";
                }
            }
            statusText += "\n";
        }

        // Keyboard shortcuts
        statusText += "<b>Shortcuts:</b>\n";
        statusText += $"{saveKey} = Save | {loadKey} = Load | {deleteKey} = Delete | {infoKey} = Info\n";

        debugText.text = statusText;
    }

    #region Test Methods

    public void TestSave()
    {
        Debug.Log("[SaveSystemDebugger] Testing save...");

        bool success = EnhancedGameSaveManager.SaveGame();

        if (success)
        {
            Debug.Log("<color=green>[SaveSystemDebugger] Save SUCCESS!</color>");
            ShowNotification("SAVED!", Color.green);
        }
        else
        {
            Debug.LogError("[SaveSystemDebugger] Save FAILED!");
            ShowNotification("SAVE FAILED!", Color.red);
        }
    }

    public void TestLoad()
    {
        Debug.Log("[SaveSystemDebugger] Testing load...");

        if (!EnhancedGameSaveManager.HasSaveGame())
        {
            Debug.LogWarning("[SaveSystemDebugger] No save file found!");
            ShowNotification("NO SAVE FOUND!", Color.yellow);
            return;
        }

        SaveData data = EnhancedGameSaveManager.LoadGame();

        if (data != null)
        {
            EnhancedGameSaveManager.RestoreSaveData(data);
            Debug.Log("<color=green>[SaveSystemDebugger] Load SUCCESS!</color>");
            ShowNotification("LOADED!", Color.green);
        }
        else
        {
            Debug.LogError("[SaveSystemDebugger] Load FAILED!");
            ShowNotification("LOAD FAILED!", Color.red);
        }
    }

    public void TestDelete()
    {
        Debug.Log("[SaveSystemDebugger] Testing delete...");

        if (!EnhancedGameSaveManager.HasSaveGame())
        {
            Debug.LogWarning("[SaveSystemDebugger] No save file to delete!");
            ShowNotification("NO SAVE TO DELETE!", Color.yellow);
            return;
        }

        EnhancedGameSaveManager.DeleteSaveGame();
        Debug.Log("<color=yellow>[SaveSystemDebugger] Save deleted!</color>");
        ShowNotification("DELETED!", Color.yellow);
    }

    public void TestShowInfo()
    {
        Debug.Log("[SaveSystemDebugger] Showing save info for all slots...");

        if (!EnhancedGameSaveManager.HasAnySaveGame())
        {
            Debug.LogWarning("[SaveSystemDebugger] No save files found!");
            return;
        }

        var allSlots = EnhancedGameSaveManager.GetAllSaveSlots();
        Debug.Log("=== ALL SAVE SLOTS ===");

        foreach (var slotInfo in allSlots)
        {
            if (!slotInfo.isEmpty)
            {
                Debug.Log($"\n--- Slot {slotInfo.slotNumber} ---");
                Debug.Log($"Save Date: {slotInfo.saveDate}");
                Debug.Log($"Player Level: {slotInfo.playerLevel}");
                Debug.Log($"Zone: {slotInfo.currentZone}/{slotInfo.currentSubZone}");
                Debug.Log($"Playtime: {slotInfo.GetPlaytimeFormatted()}");
                Debug.Log($"File Size: {slotInfo.fileSizeKB} KB");
            }
        }

        Debug.Log("======================");
        ShowNotification("CHECK CONSOLE", Color.cyan);
    }

    public void TestExportJSON()
    {
        Debug.Log("[SaveSystemDebugger] Exporting current game state to JSON...");

        string json = EnhancedGameSaveManager.ExportSaveToJson();

        Debug.Log("=== SAVE DATA JSON ===");
        Debug.Log(json);
        Debug.Log("=== END JSON ===");

        ShowNotification("EXPORTED TO CONSOLE", Color.cyan);
    }

    #endregion

    #region Notification System

    private GameObject notificationObject;
    private TextMeshProUGUI notificationText;
    private float notificationTimer = 0f;
    private const float NOTIFICATION_DURATION = 2f;

    private void ShowNotification(string message, Color color)
    {
        // Create notification UI if it doesn't exist
        if (notificationObject == null)
        {
            CreateNotificationUI();
        }

        if (notificationText != null)
        {
            notificationText.text = message;
            notificationText.color = color;
            notificationObject.SetActive(true);
            notificationTimer = NOTIFICATION_DURATION;
        }
    }

    private void CreateNotificationUI()
    {
        // Create a simple notification UI element
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            notificationObject = new GameObject("SaveNotification");
            notificationObject.transform.SetParent(canvas.transform, false);

            RectTransform rect = notificationObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.9f);
            rect.anchorMax = new Vector2(0.5f, 0.9f);
            rect.sizeDelta = new Vector2(300, 60);

            notificationText = notificationObject.AddComponent<TextMeshProUGUI>();
            notificationText.fontSize = 24;
            notificationText.alignment = TextAlignmentOptions.Center;
            notificationText.fontStyle = FontStyles.Bold;

            notificationObject.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        // Hide notification after duration
        if (notificationTimer > 0f)
        {
            notificationTimer -= Time.deltaTime;
            if (notificationTimer <= 0f && notificationObject != null)
            {
                notificationObject.SetActive(false);
            }
        }
    }

    #endregion

    private void OnGUI()
    {
        // Alternative on-screen debug display (if TextMeshPro not available)
        if (showDebugUI && debugText == null)
        {
            GUI.Box(new Rect(10, 10, 300, 200), "Save System Debug");

            GUILayout.BeginArea(new Rect(20, 40, 280, 160));

            GUILayout.Label($"Save exists: {EnhancedGameSaveManager.HasSaveGame()}");

            if (GUILayout.Button("Save (F5)"))
            {
                TestSave();
            }

            if (GUILayout.Button("Load (F9)"))
            {
                TestLoad();
            }

            if (GUILayout.Button("Delete (F10)"))
            {
                TestDelete();
            }

            if (GUILayout.Button("Show Info (F11)"))
            {
                TestShowInfo();
            }

            GUILayout.EndArea();
        }
    }
}
