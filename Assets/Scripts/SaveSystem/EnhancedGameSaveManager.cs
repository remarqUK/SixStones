using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Enhanced save/load system using JSON serialization
/// Handles all game state including inventory, equipment, and maze state
/// Supports multiple save slots
///
/// SAFETY FEATURES:
/// - Prevents saving while board is processing (gems moving, matching, falling)
/// - Ensures saved state is always stable and not corrupted
/// - Returns false if save is blocked due to unstable game state
/// </summary>
public static class EnhancedGameSaveManager
{
    private const string SAVE_FOLDER = "Saves";
    private const int MAX_SAVE_SLOTS = 10; // Maximum number of save slots
    private const int DEFAULT_SLOT = 1; // Default slot for backward compatibility

    // PlayerPrefs key for tracking save existence (backwards compatibility)
    private const string SAVE_KEY = "GameSave_Exists";

    /// <summary>
    /// Get the save file path for a specific slot
    /// </summary>
    private static string GetSavePath(int slot)
    {
        ValidateSlot(slot);
        string fileName = $"gamesave_slot{slot}.json";
        return Path.Combine(Application.persistentDataPath, SAVE_FOLDER, fileName);
    }

    /// <summary>
    /// Get the backup file path for a specific slot
    /// </summary>
    private static string GetBackupPath(int slot)
    {
        ValidateSlot(slot);
        string fileName = $"gamesave_slot{slot}_backup.json";
        return Path.Combine(Application.persistentDataPath, SAVE_FOLDER, fileName);
    }

    /// <summary>
    /// Validate that slot number is within valid range
    /// </summary>
    private static void ValidateSlot(int slot)
    {
        if (slot < 1 || slot > MAX_SAVE_SLOTS)
        {
            throw new ArgumentException($"Save slot must be between 1 and {MAX_SAVE_SLOTS}, got {slot}");
        }
    }

    /// <summary>
    /// Check if a save game exists in the specified slot
    /// </summary>
    public static bool HasSaveGame(int slot = DEFAULT_SLOT)
    {
        ValidateSlot(slot);
        return File.Exists(GetSavePath(slot));
    }

    /// <summary>
    /// Check if any save game exists in any slot
    /// </summary>
    public static bool HasAnySaveGame()
    {
        for (int slot = 1; slot <= MAX_SAVE_SLOTS; slot++)
        {
            if (HasSaveGame(slot))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Get all save slots that have save data
    /// </summary>
    public static List<int> GetUsedSlots()
    {
        List<int> usedSlots = new List<int>();
        for (int slot = 1; slot <= MAX_SAVE_SLOTS; slot++)
        {
            if (HasSaveGame(slot))
                usedSlots.Add(slot);
        }
        return usedSlots;
    }

    /// <summary>
    /// Check if it's safe to save the game (no animations or processing in progress)
    /// </summary>
    private static bool IsSafeToSave()
    {
        // Check if board is processing (gems moving, matching, falling, etc.)
        Board board = UnityEngine.Object.FindFirstObjectByType<Board>();
        if (board != null && board.IsProcessing)
        {
            Debug.LogWarning("[Save System] Cannot save while board is processing (gems are moving/matching)");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Save complete game state to JSON file in the specified slot
    /// </summary>
    public static bool SaveGame(int slot = DEFAULT_SLOT)
    {
        try
        {
            ValidateSlot(slot);

            // Safety check: Don't save if board is processing
            if (!IsSafeToSave())
            {
                Debug.LogWarning($"[Save System] Skipping save to slot {slot} - game state is not stable (animations in progress)");
                return false;
            }

            // Create save data
            SaveData saveData = CaptureSaveData();

            // Add slot number to save data metadata
            saveData.saveSlot = slot;

            // Serialize to JSON
            string json = JsonUtility.ToJson(saveData, true);

            // Ensure save directory exists
            string saveDir = Path.Combine(Application.persistentDataPath, SAVE_FOLDER);
            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }

            string savePath = GetSavePath(slot);
            string backupPath = GetBackupPath(slot);

            // Backup existing save if it exists
            if (File.Exists(savePath))
            {
                File.Copy(savePath, backupPath, true);
                Debug.Log($"Backed up existing save to: {backupPath}");
            }

            // Write new save
            File.WriteAllText(savePath, json);

            // Mark save as existing in PlayerPrefs (backwards compatibility)
            PlayerPrefs.SetInt($"{SAVE_KEY}_Slot{slot}", 1);
            PlayerPrefs.Save();

            Debug.Log($"Game saved successfully to slot {slot}: {savePath}");
            Debug.Log($"Save data size: {json.Length} characters");

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save game to slot {slot}: {e.Message}");
            Debug.LogException(e);
            return false;
        }
    }

    /// <summary>
    /// Load complete game state from JSON file in the specified slot
    /// </summary>
    public static SaveData LoadGame(int slot = DEFAULT_SLOT)
    {
        try
        {
            ValidateSlot(slot);
            string savePath = GetSavePath(slot);

            if (!File.Exists(savePath))
            {
                Debug.LogWarning($"No save file found in slot {slot}");
                return null;
            }

            // Read JSON
            string json = File.ReadAllText(savePath);

            // Deserialize
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            if (saveData == null)
            {
                Debug.LogError($"Failed to deserialize save data from slot {slot}");
                return null;
            }

            Debug.Log($"Game loaded successfully from slot {slot}: {savePath}");
            Debug.Log($"Save date: {saveData.saveDate}, Version: {saveData.saveVersion}, Slot: {saveData.saveSlot}");

            return saveData;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load game from slot {slot}: {e.Message}");
            Debug.LogException(e);

            // Attempt to load backup
            return LoadBackup(slot);
        }
    }

    /// <summary>
    /// Load backup save file if main save fails
    /// </summary>
    private static SaveData LoadBackup(int slot)
    {
        try
        {
            string backupPath = GetBackupPath(slot);

            if (!File.Exists(backupPath))
            {
                Debug.LogError($"No backup save file found for slot {slot}");
                return null;
            }

            Debug.LogWarning($"Attempting to load backup save for slot {slot}...");

            string json = File.ReadAllText(backupPath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            if (saveData != null)
            {
                Debug.Log($"Backup save loaded successfully for slot {slot}");
                // Restore backup as main save
                File.Copy(backupPath, GetSavePath(slot), true);
            }

            return saveData;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load backup for slot {slot}: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Delete save game from the specified slot
    /// </summary>
    public static void DeleteSaveGame(int slot = DEFAULT_SLOT)
    {
        try
        {
            ValidateSlot(slot);
            string savePath = GetSavePath(slot);
            string backupPath = GetBackupPath(slot);

            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log($"Save file deleted from slot {slot}");
            }

            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
                Debug.Log($"Backup save file deleted from slot {slot}");
            }

            PlayerPrefs.DeleteKey($"{SAVE_KEY}_Slot{slot}");
            PlayerPrefs.Save();

            Debug.Log($"Save game deleted successfully from slot {slot}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to delete save from slot {slot}: {e.Message}");
        }
    }

    /// <summary>
    /// Delete all save games from all slots
    /// </summary>
    public static void DeleteAllSaveGames()
    {
        for (int slot = 1; slot <= MAX_SAVE_SLOTS; slot++)
        {
            if (HasSaveGame(slot))
            {
                DeleteSaveGame(slot);
            }
        }
        Debug.Log("All save games deleted");
    }

    /// <summary>
    /// Capture all game state into SaveData object
    /// </summary>
    private static SaveData CaptureSaveData()
    {
        SaveData saveData = new SaveData();

        // Capture current scene
        saveData.savedScene = SceneHelper.GetCurrentScene();

        // Capture player progression
        saveData.playerProgress = CapturePlayerProgress();

        // Capture player stats
        saveData.playerStats = CapturePlayerStats();

        // Capture inventory
        saveData.inventoryData = CaptureInventory();

        // Capture equipment
        saveData.equipmentData = CaptureEquipment();

        // Capture spell system
        saveData.spellData = CaptureSpellSystem();

        // Capture status effects
        saveData.statusEffectData = CaptureStatusEffects();

        // Capture maze state
        saveData.mazeState = CaptureMazeState();

        // Capture match3 state
        saveData.match3State = CaptureMatch3State();

        // Capture game options
        saveData.gameOptions = CaptureGameOptions();

        return saveData;
    }

    #region Capture Methods

    private static PlayerProgressData CapturePlayerProgress()
    {
        PlayerProgressData data = new PlayerProgressData();

        if (PlayerProgress.Instance != null)
        {
            data.currentZone = PlayerProgress.Instance.currentZone;
            data.currentSubZone = PlayerProgress.Instance.currentSubZone;
            data.currentMap = PlayerProgress.Instance.currentMap;
            data.positionX = PlayerProgress.Instance.positionX;
            data.positionY = PlayerProgress.Instance.positionY;
        }

        return data;
    }

    private static PlayerStatsData CapturePlayerStats()
    {
        PlayerStatsData data = new PlayerStatsData();

        GameManager gameManager = UnityEngine.Object.FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            data.healthP1 = gameManager.GetCurrentHealth(PlayerManager.Player.Player1);
            data.maxHealthP1 = gameManager.GetMaxHealth(PlayerManager.Player.Player1);
            // TODO: Add GetScore method to GameManager to save scores
            // data.scoreP1 = gameManager.GetScore(PlayerManager.Player.Player1);

            data.healthP2 = gameManager.GetCurrentHealth(PlayerManager.Player.Player2);
            data.maxHealthP2 = gameManager.GetMaxHealth(PlayerManager.Player.Player2);
            // TODO: Add GetScore method to GameManager to save scores
            // data.scoreP2 = gameManager.GetScore(PlayerManager.Player.Player2);
        }

        if (LevelSystem.Instance != null)
        {
            data.level = LevelSystem.Instance.CurrentLevel;
            data.currentXP = LevelSystem.Instance.CurrentXP;
        }

        if (CurrencyManager.Instance != null)
        {
            data.gold = CurrencyManager.Instance.CurrentGold;
        }

        // Capture equipment bonuses
        PlayerEquipment equipment = UnityEngine.Object.FindFirstObjectByType<PlayerEquipment>();
        if (equipment != null)
        {
            data.bonusStrength = equipment.GetAttributeBonus("str");
            data.bonusDexterity = equipment.GetAttributeBonus("dex");
            data.bonusConstitution = equipment.GetAttributeBonus("con");
            data.bonusIntelligence = equipment.GetAttributeBonus("int");
            data.bonusWisdom = equipment.GetAttributeBonus("wis");
            data.bonusCharisma = equipment.GetAttributeBonus("cha");
            data.totalArmorClass = equipment.GetTotalArmorClass();
        }

        return data;
    }

    private static InventoryData CaptureInventory()
    {
        InventoryData data = new InventoryData();

        Inventory inventory = UnityEngine.Object.FindFirstObjectByType<Inventory>();
        if (inventory != null)
        {
            data.maxSlots = inventory.MaxSlots;

            foreach (var slot in inventory.Slots)
            {
                if (!slot.IsEmpty && slot.Item != null)
                {
                    data.slots.Add(new InventorySlotData(slot.Item.ItemName, slot.Quantity));
                }
                else
                {
                    // Save empty slots to maintain slot indices
                    data.slots.Add(new InventorySlotData(null, 0));
                }
            }
        }

        return data;
    }

    private static EquipmentData CaptureEquipment()
    {
        EquipmentData data = new EquipmentData();

        PlayerEquipment equipment = UnityEngine.Object.FindFirstObjectByType<PlayerEquipment>();
        if (equipment != null)
        {
            var equippedItems = equipment.EquippedItems;

            data.headItem = GetItemName(equippedItems, EquipmentSlot.Head);
            data.chestItem = GetItemName(equippedItems, EquipmentSlot.Chest);
            data.legsItem = GetItemName(equippedItems, EquipmentSlot.Legs);
            data.feetItem = GetItemName(equippedItems, EquipmentSlot.Feet);
            data.mainHandItem = GetItemName(equippedItems, EquipmentSlot.MainHand);
            data.offHandItem = GetItemName(equippedItems, EquipmentSlot.OffHand);
            data.twoHandItem = GetItemName(equippedItems, EquipmentSlot.TwoHand);
            data.ringItem = GetItemName(equippedItems, EquipmentSlot.Ring);
        }

        return data;
    }

    private static string GetItemName(System.Collections.Generic.Dictionary<EquipmentSlot, Item> equippedItems, EquipmentSlot slot)
    {
        if (equippedItems.ContainsKey(slot) && equippedItems[slot] != null)
        {
            return equippedItems[slot].ItemName;
        }
        return null;
    }

    private static SpellSystemData CaptureSpellSystem()
    {
        SpellSystemData data = new SpellSystemData();

        // TODO: Uncomment when SpellManager has these methods:
        // - GetLearnedSpells()
        // - GetPreparedSpells()
        // - GetRemainingCooldown()
        // - GetGemCharges()
        /*
        if (SpellManager.Instance != null)
        {
            // Capture learned spells
            var learnedSpells = SpellManager.Instance.GetLearnedSpells();
            foreach (var spell in learnedSpells)
            {
                if (spell != null)
                {
                    data.learnedSpells.Add(spell.spellName);
                }
            }

            // Capture prepared spells
            var preparedSpells = SpellManager.Instance.GetPreparedSpells();
            foreach (var spell in preparedSpells)
            {
                if (spell != null)
                {
                    data.preparedSpells.Add(spell.spellName);

                    // Capture cooldown
                    float cooldown = SpellManager.Instance.GetRemainingCooldown(spell);
                    if (cooldown > 0)
                    {
                        data.spellCooldowns[spell.spellName] = cooldown;
                    }

                    // Capture gem charges
                    int charges = SpellManager.Instance.GetGemCharges(spell);
                    data.gemCharges[spell.spellName] = charges;
                }
            }
        }
        */

        return data;
    }

    private static StatusEffectsSaveData CaptureStatusEffects()
    {
        StatusEffectsSaveData data = new StatusEffectsSaveData();

        if (StatusEffectManager.Instance != null)
        {
            // Get all active effects for both players
            PlayerManager.Player[] players = { PlayerManager.Player.Player1, PlayerManager.Player.Player2 };
            foreach (var player in players)
            {
                var effects = StatusEffectManager.Instance.GetActiveEffects(player);
                int playerNum = (player == PlayerManager.Player.Player1) ? 1 : 2;
                foreach (var effect in effects)
                {
                    if (effect != null && effect.effectData != null)
                    {
                        data.activeEffects.Add(new SavedStatusEffect(
                            effect.effectData.effectName,
                            playerNum,
                            effect.severity,
                            effect.remainingDuration
                        ));
                    }
                }
            }
        }

        return data;
    }

    private static MazeStateData CaptureMazeState()
    {
        MazeStateData data = new MazeStateData();

        MapGenerator mapGenerator = UnityEngine.Object.FindFirstObjectByType<MapGenerator>();
        if (mapGenerator != null && mapGenerator.grid != null)
        {
            // Save generation parameters - use actualSeedUsed to ensure exact maze reproduction
            data.randomSeed = mapGenerator.actualSeedUsed;
            data.mazeWidth = mapGenerator.width;
            data.mazeHeight = mapGenerator.height;

            // Save explored cells and their states
            for (int x = 0; x < mapGenerator.grid.GetLength(0); x++)
            {
                for (int y = 0; y < mapGenerator.grid.GetLength(1); y++)
                {
                    MapCell cell = mapGenerator.grid[x, y];
                    if (cell != null && cell.visited)
                    {
                        MazeCellState cellState = new MazeCellState(x, y);
                        cellState.explored = true; // You'll need to add tracking for this
                        cellState.enemyDefeated = !cell.hasEnemy; // Assuming false = defeated
                        cellState.treasureCollected = !cell.hasTreasure; // Assuming false = collected

                        data.cellStates.Add(cellState);
                    }
                }
            }

            // Save opened secret rooms
            foreach (var secretPos in mapGenerator.secretRoomPositions)
            {
                // Track which secret rooms have been opened (you'll need to add this tracking)
                data.openedSecretRooms.Add(new Vector2IntSerializable(secretPos));
            }
        }

        // Save player position and facing in maze
        FirstPersonMazeController mazeController = UnityEngine.Object.FindFirstObjectByType<FirstPersonMazeController>();
        if (mazeController != null)
        {
            data.playerX = mazeController.GridX;
            data.playerY = mazeController.GridZ;
            data.playerFacing = mazeController.Facing;
        }

        // Save minimap exploration state
        MinimapRenderer minimapRenderer = UnityEngine.Object.FindFirstObjectByType<MinimapRenderer>();
        if (minimapRenderer != null)
        {
            List<Vector2Int> visited = minimapRenderer.GetVisitedCells();
            List<Vector2Int> revealed = minimapRenderer.GetRevealedCells();

            foreach (var cell in visited)
            {
                data.visitedCells.Add(new Vector2IntSerializable(cell));
            }

            foreach (var cell in revealed)
            {
                data.revealedCells.Add(new Vector2IntSerializable(cell));
            }

            Debug.Log($"Saved minimap state: {data.visitedCells.Count} visited cells, {data.revealedCells.Count} revealed cells");
        }

        return data;
    }

    private static Match3StateData CaptureMatch3State()
    {
        Match3StateData data = new Match3StateData();

        Board board = UnityEngine.Object.FindFirstObjectByType<Board>();
        GameManager gameManager = UnityEngine.Object.FindFirstObjectByType<GameManager>();

        if (board != null)
        {
            // Save board dimensions
            data.boardWidth = board.Width;
            data.boardHeight = board.Height;

            // Save board pieces (convert 2D array to 1D for serialization)
            data.boardPieces = board.GetBoardStateAsArray();

            Debug.Log($"Captured Match3 board state: {data.boardWidth}x{data.boardHeight}, {data.boardPieces?.Length ?? 0} pieces");
        }

        if (gameManager != null)
        {
            // Save remaining moves
            data.player1RemainingMoves = gameManager.GetRemainingMoves(PlayerManager.Player.Player1);
            data.player2RemainingMoves = gameManager.GetRemainingMoves(PlayerManager.Player.Player2);

            // Save color scores
            var p1ColorScores = gameManager.GetColorScores(PlayerManager.Player.Player1);
            var p2ColorScores = gameManager.GetColorScores(PlayerManager.Player.Player2);

            if (p1ColorScores != null)
            {
                foreach (var kvp in p1ColorScores)
                {
                    data.player1ColorScores.Add(new ColorScoreEntry((int)kvp.Key, kvp.Value));
                }
            }

            if (p2ColorScores != null)
            {
                foreach (var kvp in p2ColorScores)
                {
                    data.player2ColorScores.Add(new ColorScoreEntry((int)kvp.Key, kvp.Value));
                }
            }

            Debug.Log($"Captured Match3 game state: P1 moves={data.player1RemainingMoves}, P2 moves={data.player2RemainingMoves}");
        }

        return data;
    }

    private static GameOptionsData CaptureGameOptions()
    {
        GameOptionsData data = new GameOptionsData();

        // TODO: Uncomment when GlobalOptionsManager has these properties
        // if (GlobalOptionsManager.Instance != null)
        // {
        //     data.masterVolume = GlobalOptionsManager.Instance.MasterVolume;
        //     data.musicVolume = GlobalOptionsManager.Instance.MusicVolume;
        //     data.sfxVolume = GlobalOptionsManager.Instance.SFXVolume;
        // }

        // TODO: Uncomment when LocalizationManager has CurrentLanguage property
        // if (LocalizationManager.Instance != null)
        // {
        //     data.languageCode = LocalizationManager.Instance.CurrentLanguage;
        // }

        data.fullscreen = Screen.fullScreen;
        data.resolutionWidth = Screen.width;
        data.resolutionHeight = Screen.height;

        return data;
    }

    #endregion

    #region Restore Methods

    /// <summary>
    /// Restore all game state from SaveData
    /// </summary>
    public static void RestoreSaveData(SaveData saveData)
    {
        if (saveData == null)
        {
            Debug.LogError("Cannot restore null save data");
            return;
        }

        Debug.Log("Restoring save data...");

        RestorePlayerProgress(saveData.playerProgress);
        RestorePlayerStats(saveData.playerStats);
        RestoreInventory(saveData.inventoryData);
        RestoreEquipment(saveData.equipmentData);
        RestoreSpellSystem(saveData.spellData);
        RestoreStatusEffects(saveData.statusEffectData);
        RestoreMazeState(saveData.mazeState);
        RestoreMatch3State(saveData.match3State);
        RestoreGameOptions(saveData.gameOptions);

        Debug.Log("Save data restored successfully");
    }

    private static void RestorePlayerProgress(PlayerProgressData data)
    {
        if (PlayerProgress.Instance != null && data != null)
        {
            // Use reflection or add public setters to PlayerProgress
            // For now, assuming there are setter methods
            Debug.Log($"Restoring player progress: Zone {data.currentZone}, SubZone {data.currentSubZone}, Map {data.currentMap}");

            // You may need to add these methods to PlayerProgress
            // PlayerProgress.Instance.SetProgress(data.currentZone, data.currentSubZone, data.currentMap);
            // PlayerProgress.Instance.SetPosition(data.positionX, data.positionY);
        }
    }

    private static void RestorePlayerStats(PlayerStatsData data)
    {
        GameManager gameManager = UnityEngine.Object.FindFirstObjectByType<GameManager>();
        if (gameManager != null && data != null)
        {
            // Restore max health first
            gameManager.SetMaxHealth(PlayerManager.Player.Player1, data.maxHealthP1);
            gameManager.SetMaxHealth(PlayerManager.Player.Player2, data.maxHealthP2);

            // Restore current health by calculating the difference and healing/damaging
            int currentP1Health = gameManager.GetCurrentHealth(PlayerManager.Player.Player1);
            int healthDiffP1 = data.healthP1 - currentP1Health;
            if (healthDiffP1 > 0) {
                gameManager.Heal(PlayerManager.Player.Player1, healthDiffP1);
            } else if (healthDiffP1 < 0) {
                gameManager.TakeDamage(PlayerManager.Player.Player1, -healthDiffP1);
            }

            int currentP2Health = gameManager.GetCurrentHealth(PlayerManager.Player.Player2);
            int healthDiffP2 = data.healthP2 - currentP2Health;
            if (healthDiffP2 > 0) {
                gameManager.Heal(PlayerManager.Player.Player2, healthDiffP2);
            } else if (healthDiffP2 < 0) {
                gameManager.TakeDamage(PlayerManager.Player.Player2, -healthDiffP2);
            }

            // TODO: Add SetScore method to GameManager to restore scores
            // gameManager.SetScore(PlayerManager.Player.Player1, data.scoreP1);
            // gameManager.SetScore(PlayerManager.Player.Player2, data.scoreP2);

            gameManager.UpdateUI();
        }

        if (LevelSystem.Instance != null && data != null)
        {
            LevelSystem.Instance.SetLevel(data.level);
            LevelSystem.Instance.SetXP(data.currentXP);
        }

        if (CurrencyManager.Instance != null && data != null)
        {
            CurrencyManager.Instance.SetGold(data.gold);
        }

        Debug.Log($"Restored player stats: HP1={data.healthP1}/{data.maxHealthP1}, HP2={data.healthP2}/{data.maxHealthP2}, Level={data.level}, Gold={data.gold}");
    }

    private static void RestoreInventory(InventoryData data)
    {
        Inventory inventory = UnityEngine.Object.FindFirstObjectByType<Inventory>();
        if (inventory != null && data != null)
        {
            inventory.ClearInventory();

            ItemDatabase itemDatabase = UnityEngine.Object.FindFirstObjectByType<ItemDatabase>();
            if (itemDatabase == null)
            {
                Debug.LogError("ItemDatabase not found - cannot restore inventory");
                return;
            }

            for (int i = 0; i < data.slots.Count && i < inventory.MaxSlots; i++)
            {
                InventorySlotData slotData = data.slots[i];
                if (!string.IsNullOrEmpty(slotData.itemName))
                {
                    Item item = itemDatabase.GetItemByName(slotData.itemName);
                    if (item != null)
                    {
                        inventory.AddItem(item, slotData.quantity);
                    }
                    else
                    {
                        Debug.LogWarning($"Could not find item: {slotData.itemName}");
                    }
                }
            }

            Debug.Log($"Restored inventory with {data.slots.Count} slots");
        }
    }

    private static void RestoreEquipment(EquipmentData data)
    {
        PlayerEquipment equipment = UnityEngine.Object.FindFirstObjectByType<PlayerEquipment>();
        if (equipment != null && data != null)
        {
            equipment.UnequipAll();

            ItemDatabase itemDatabase = UnityEngine.Object.FindFirstObjectByType<ItemDatabase>();
            if (itemDatabase == null)
            {
                Debug.LogError("ItemDatabase not found - cannot restore equipment");
                return;
            }

            TryEquipItem(equipment, itemDatabase, data.headItem);
            TryEquipItem(equipment, itemDatabase, data.chestItem);
            TryEquipItem(equipment, itemDatabase, data.legsItem);
            TryEquipItem(equipment, itemDatabase, data.feetItem);
            TryEquipItem(equipment, itemDatabase, data.mainHandItem);
            TryEquipItem(equipment, itemDatabase, data.offHandItem);
            TryEquipItem(equipment, itemDatabase, data.twoHandItem);
            TryEquipItem(equipment, itemDatabase, data.ringItem);

            Debug.Log("Restored equipment");
        }
    }

    private static void TryEquipItem(PlayerEquipment equipment, ItemDatabase itemDatabase, string itemName)
    {
        if (!string.IsNullOrEmpty(itemName))
        {
            Item item = itemDatabase.GetItemByName(itemName);
            if (item != null)
            {
                equipment.EquipItem(item);
            }
            else
            {
                Debug.LogWarning($"Could not find equipped item: {itemName}");
            }
        }
    }

    private static void RestoreSpellSystem(SpellSystemData data)
    {
        // TODO: Uncomment when SpellManager has these methods:
        // - ClearAllSpells()
        // - GetSpellByName()
        // - SetCooldown()
        // - SetGemCharges()
        /*
        if (SpellManager.Instance != null && data != null)
        {
            // Clear existing spells
            SpellManager.Instance.ClearAllSpells();

            // Restore learned spells
            foreach (string spellName in data.learnedSpells)
            {
                var spell = SpellManager.Instance.GetSpellByName(spellName);
                if (spell != null)
                {
                    SpellManager.Instance.LearnSpell(spell);
                }
            }

            // Restore prepared spells
            foreach (string spellName in data.preparedSpells)
            {
                var spell = SpellManager.Instance.GetSpellByName(spellName);
                if (spell != null)
                {
                    SpellManager.Instance.PrepareSpell(spell);

                    // Restore cooldowns
                    if (data.spellCooldowns.ContainsKey(spellName))
                    {
                        SpellManager.Instance.SetCooldown(spell, data.spellCooldowns[spellName]);
                    }

                    // Restore gem charges
                    if (data.gemCharges.ContainsKey(spellName))
                    {
                        SpellManager.Instance.SetGemCharges(spell, data.gemCharges[spellName]);
                    }
                }
            }

            Debug.Log($"Restored {data.learnedSpells.Count} learned spells, {data.preparedSpells.Count} prepared spells");
        }
        */
    }

    private static void RestoreStatusEffects(StatusEffectsSaveData data)
    {
        if (StatusEffectManager.Instance != null && data != null)
        {
            // Note: Status effect restoration needs GetStatusEffectByName and ApplyEffect methods
            // These need to be added to StatusEffectManager

            // StatusEffectManager.Instance.ClearAllEffects();
            //
            // foreach (var effectData in data.activeEffects)
            // {
            //     var statusEffect = StatusEffectManager.Instance.GetStatusEffectByName(effectData.effectName);
            //     if (statusEffect != null)
            //     {
            //         StatusEffectManager.Instance.ApplyEffect(
            //             effectData.player,
            //             statusEffect,
            //             effectData.severity,
            //             effectData.remainingDuration
            //         );
            //     }
            // }

            Debug.Log($"Status effects restoration pending - {data.activeEffects.Count} effects saved (needs StatusEffectManager implementation)");
        }
    }

    private static void RestoreMazeState(MazeStateData data)
    {
        if (data == null) return;

        // IMPORTANT: Reset minimap BEFORE regenerating maze to prevent initialization with wrong maze
        MinimapRenderer minimapRenderer = UnityEngine.Object.FindFirstObjectByType<MinimapRenderer>();
        if (minimapRenderer != null)
        {
            minimapRenderer.ResetForLoad();
        }

        MapGenerator mapGenerator = UnityEngine.Object.FindFirstObjectByType<MapGenerator>();
        if (mapGenerator != null)
        {
            // Regenerate maze with same seed
            mapGenerator.randomSeed = data.randomSeed;
            mapGenerator.width = data.mazeWidth;
            mapGenerator.height = data.mazeHeight;
            mapGenerator.GenerateMaze();

            // Restore cell states (enemies defeated, treasure collected)
            foreach (var cellState in data.cellStates)
            {
                MapCell cell = mapGenerator.GetCell(cellState.x, cellState.y);
                if (cell != null)
                {
                    cell.hasEnemy = !cellState.enemyDefeated;
                    cell.hasTreasure = !cellState.treasureCollected;
                    // You may need to add cell.explored = cellState.explored;
                }
            }

            Debug.Log($"Restored maze state: Seed={data.randomSeed}, Size={data.mazeWidth}x{data.mazeHeight}, {data.cellStates.Count} cells");
        }

        // Restore minimap exploration state BEFORE player position to avoid event interference
        if (minimapRenderer != null && data != null)
        {
            // Convert serialized vectors back to Vector2Int
            List<Vector2Int> visited = new List<Vector2Int>();
            List<Vector2Int> revealed = new List<Vector2Int>();

            if (data.visitedCells != null)
            {
                foreach (var cell in data.visitedCells)
                {
                    visited.Add(cell.ToVector2Int());
                }
            }

            if (data.revealedCells != null)
            {
                foreach (var cell in data.revealedCells)
                {
                    revealed.Add(cell.ToVector2Int());
                }
            }

            minimapRenderer.RestoreExploredState(visited, revealed);
            Debug.Log($"Minimap exploration state restored: {visited.Count} visited cells, {revealed.Count} revealed cells");
        }

        // Restore player position and facing AFTER minimap data
        FirstPersonMazeController mazeController = UnityEngine.Object.FindFirstObjectByType<FirstPersonMazeController>();
        if (mazeController != null)
        {
            mazeController.SetPositionAndFacing(data.playerX, data.playerY, data.playerFacing);
            Debug.Log($"Maze player position and facing restored to: ({data.playerX}, {data.playerY}), facing: {data.playerFacing}");
        }
    }

    private static void RestoreMatch3State(Match3StateData data)
    {
        if (data == null) return;

        Board board = UnityEngine.Object.FindFirstObjectByType<Board>();
        GameManager gameManager = UnityEngine.Object.FindFirstObjectByType<GameManager>();

        if (board != null && data.boardPieces != null)
        {
            // Restore board state from 1D array
            board.RestoreBoardState(data.boardPieces, data.boardWidth, data.boardHeight);
            Debug.Log($"Restored Match3 board: {data.boardWidth}x{data.boardHeight}");
        }

        if (gameManager != null)
        {
            // Restore remaining moves
            gameManager.SetRemainingMoves(PlayerManager.Player.Player1, data.player1RemainingMoves);
            gameManager.SetRemainingMoves(PlayerManager.Player.Player2, data.player2RemainingMoves);

            // Restore color scores
            if (data.player1ColorScores != null)
            {
                Dictionary<GamePiece.PieceType, int> p1Scores = new Dictionary<GamePiece.PieceType, int>();
                foreach (var entry in data.player1ColorScores)
                {
                    p1Scores[(GamePiece.PieceType)entry.pieceType] = entry.score;
                }
                gameManager.SetColorScores(PlayerManager.Player.Player1, p1Scores);
            }

            if (data.player2ColorScores != null)
            {
                Dictionary<GamePiece.PieceType, int> p2Scores = new Dictionary<GamePiece.PieceType, int>();
                foreach (var entry in data.player2ColorScores)
                {
                    p2Scores[(GamePiece.PieceType)entry.pieceType] = entry.score;
                }
                gameManager.SetColorScores(PlayerManager.Player.Player2, p2Scores);
            }

            Debug.Log($"Restored Match3 game state: P1 moves={data.player1RemainingMoves}, P2 moves={data.player2RemainingMoves}");

            // Update UI to reflect restored state
            gameManager.UpdateUI();
        }
    }

    private static void RestoreGameOptions(GameOptionsData data)
    {
        if (data == null) return;

        // Note: GlobalOptionsManager and LocalizationManager methods need to be implemented
        // For now, these are commented out. Uncomment when methods are available.

        /*
        if (GlobalOptionsManager.Instance != null)
        {
            // Add these methods to GlobalOptionsManager:
            // GlobalOptionsManager.Instance.SetMasterVolume(data.masterVolume);
            // GlobalOptionsManager.Instance.SetMusicVolume(data.musicVolume);
            // GlobalOptionsManager.Instance.SetSFXVolume(data.sfxVolume);
        }

        if (LocalizationManager.Instance != null)
        {
            // LocalizationManager.SetLanguage expects SystemLanguage enum
            // Need to convert string to enum or change implementation
            // LocalizationManager.Instance.SetLanguage(data.languageCode);
        }
        */

        Screen.SetResolution(data.resolutionWidth, data.resolutionHeight, data.fullscreen);

        Debug.Log("Restored game options (volume/language restoration pending implementation)");
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Get save file info for a specific slot (for UI display)
    /// </summary>
    public static SaveSlotInfo GetSaveSlotInfo(int slot)
    {
        if (!HasSaveGame(slot))
            return new SaveSlotInfo { slotNumber = slot, isEmpty = true };

        try
        {
            ValidateSlot(slot);
            string savePath = GetSavePath(slot);
            string json = File.ReadAllText(savePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            return new SaveSlotInfo
            {
                slotNumber = slot,
                isEmpty = false,
                saveDate = data.saveDate,
                playerLevel = data.playerStats.level,
                currentZone = data.playerProgress.currentZone,
                currentSubZone = data.playerProgress.currentSubZone,
                playtimeSeconds = data.playtimeSeconds,
                fileSizeKB = new FileInfo(savePath).Length / 1024,
                savedScene = data.savedScene
            };
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to get save file info for slot {slot}: {e.Message}");
            return new SaveSlotInfo { slotNumber = slot, isEmpty = true };
        }
    }

    /// <summary>
    /// Get all save slot info (for UI display)
    /// </summary>
    public static List<SaveSlotInfo> GetAllSaveSlots()
    {
        List<SaveSlotInfo> slots = new List<SaveSlotInfo>();
        for (int slot = 1; slot <= MAX_SAVE_SLOTS; slot++)
        {
            slots.Add(GetSaveSlotInfo(slot));
        }
        return slots;
    }

    /// <summary>
    /// Find the most recently saved slot
    /// </summary>
    public static int GetMostRecentSlot()
    {
        int mostRecentSlot = -1;
        DateTime mostRecentDate = DateTime.MinValue;

        for (int slot = 1; slot <= MAX_SAVE_SLOTS; slot++)
        {
            SaveSlotInfo info = GetSaveSlotInfo(slot);
            if (!info.isEmpty)
            {
                DateTime slotDate;
                if (DateTime.TryParse(info.saveDate, out slotDate))
                {
                    if (slotDate > mostRecentDate)
                    {
                        mostRecentDate = slotDate;
                        mostRecentSlot = slot;
                    }
                }
            }
        }

        return mostRecentSlot;
    }

    /// <summary>
    /// Export save to readable JSON string (for debugging)
    /// </summary>
    public static string ExportSaveToJson()
    {
        SaveData data = CaptureSaveData();
        return JsonUtility.ToJson(data, true);
    }

    #endregion

    #region Menu Integration

    /// <summary>
    /// Continue from main menu - loads the most recent saved game and switches to the saved scene
    /// Call this from the Continue button in the main menu
    /// </summary>
    public static void ContinueGame()
    {
        int mostRecentSlot = GetMostRecentSlot();
        if (mostRecentSlot == -1)
        {
            Debug.LogWarning("[ContinueGame] No save files found!");
            return;
        }

        ContinueGame(mostRecentSlot);
    }

    /// <summary>
    /// Continue from a specific save slot
    /// </summary>
    public static void ContinueGame(int slot)
    {
        if (!HasSaveGame(slot))
        {
            Debug.LogWarning($"[ContinueGame] No save file found in slot {slot}!");
            return;
        }

        SaveData saveData = LoadGame(slot);
        if (saveData == null)
        {
            Debug.LogError($"[ContinueGame] Failed to load save data from slot {slot}!");
            return;
        }

        // Get the target scene
        SceneIdentifier targetScene = saveData.savedScene;
        if (targetScene == SceneIdentifier.Unknown || !SceneHelper.IsGameplayScene(targetScene))
        {
            Debug.LogWarning($"[ContinueGame] Invalid scene in save data: {targetScene}, defaulting to Maze3D");
            targetScene = SceneIdentifier.Maze3D;
        }

        Debug.Log($"[ContinueGame] Loading scene: {targetScene} ({SceneHelper.GetSceneName(targetScene)})");

        // Load scene and restore data after scene loads
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoadedForContinue;
        pendingSaveData = saveData;
        SceneHelper.LoadScene(targetScene);
    }

    private static SaveData pendingSaveData = null;

    private static void OnSceneLoadedForContinue(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Unsubscribe immediately to avoid multiple calls
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoadedForContinue;

        if (pendingSaveData != null)
        {
            Debug.Log($"[ContinueGame] Scene loaded: {scene.name}, restoring save data...");

            // Use a temporary MonoBehaviour to start the coroutine
            GameObject tempObject = new GameObject("SaveRestoreHelper");
            tempObject.AddComponent<SaveRestoreHelper>().StartRestore(pendingSaveData);
        }
    }

    #endregion
}

/// <summary>
/// Save slot information for UI display
/// </summary>
public class SaveSlotInfo
{
    public int slotNumber;
    public bool isEmpty;
    public string saveDate;
    public int playerLevel;
    public int currentZone;
    public int currentSubZone;
    public float playtimeSeconds;
    public long fileSizeKB;
    public SceneIdentifier savedScene;

    public string GetPlaytimeFormatted()
    {
        int hours = (int)(playtimeSeconds / 3600);
        int minutes = (int)((playtimeSeconds % 3600) / 60);
        return $"{hours}h {minutes}m";
    }

    public string GetSlotDisplayName()
    {
        if (isEmpty)
            return $"Slot {slotNumber} - Empty";

        return $"Slot {slotNumber} - Level {playerLevel} - {saveDate}";
    }
}
