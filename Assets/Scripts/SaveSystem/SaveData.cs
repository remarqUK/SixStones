using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main save data container - holds all game state that needs to be persisted
/// </summary>
[Serializable]
public class SaveData
{
    // Save metadata
    public string saveVersion = "1.0";
    public string saveDate;
    public float playtimeSeconds;
    public SceneIdentifier savedScene = SceneIdentifier.Maze3D; // Scene where the game was saved

    // Player progression
    public PlayerProgressData playerProgress;

    // Combat & stats
    public PlayerStatsData playerStats;

    // Inventory system
    public InventoryData inventoryData;
    public EquipmentData equipmentData;

    // Spell & status effects
    public SpellSystemData spellData;
    public StatusEffectsSaveData statusEffectData;

    // Maze/dungeon state
    public MazeStateData mazeState;

    // Options & settings (optional - usually separate)
    public GameOptionsData gameOptions;

    public SaveData()
    {
        saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        playtimeSeconds = 0f;

        playerProgress = new PlayerProgressData();
        playerStats = new PlayerStatsData();
        inventoryData = new InventoryData();
        equipmentData = new EquipmentData();
        spellData = new SpellSystemData();
        statusEffectData = new StatusEffectsSaveData();
        mazeState = new MazeStateData();
        gameOptions = new GameOptionsData();
    }
}

/// <summary>
/// Player progression through zones/subzones/maps
/// </summary>
[Serializable]
public class PlayerProgressData
{
    public int currentZone;
    public int currentSubZone;
    public int currentMap;
    public int positionX;
    public int positionY;
}

/// <summary>
/// Player stats - health, scores, level, XP, gold
/// </summary>
[Serializable]
public class PlayerStatsData
{
    // Player 1 stats
    public int healthP1;
    public int maxHealthP1;
    public int scoreP1;

    // Player 2 stats
    public int healthP2;
    public int maxHealthP2;
    public int scoreP2;

    // Progression
    public int level;
    public int currentXP;
    public int gold;

    // Attributes (from equipment bonuses)
    public int bonusStrength;
    public int bonusDexterity;
    public int bonusConstitution;
    public int bonusIntelligence;
    public int bonusWisdom;
    public int bonusCharisma;
    public int totalArmorClass;
}

/// <summary>
/// Inventory state - all slots with items and quantities
/// </summary>
[Serializable]
public class InventoryData
{
    public int maxSlots;
    public List<InventorySlotData> slots;

    public InventoryData()
    {
        slots = new List<InventorySlotData>();
    }
}

/// <summary>
/// Single inventory slot data
/// </summary>
[Serializable]
public class InventorySlotData
{
    public string itemName; // Reference to Item ScriptableObject by name
    public int quantity;

    public InventorySlotData(string itemName, int quantity)
    {
        this.itemName = itemName;
        this.quantity = quantity;
    }
}

/// <summary>
/// Equipment state - all equipped items
/// </summary>
[Serializable]
public class EquipmentData
{
    public string headItem;
    public string chestItem;
    public string legsItem;
    public string feetItem;
    public string mainHandItem;
    public string offHandItem;
    public string twoHandItem;
    public string ringItem;

    public EquipmentData()
    {
        headItem = null;
        chestItem = null;
        legsItem = null;
        feetItem = null;
        mainHandItem = null;
        offHandItem = null;
        twoHandItem = null;
        ringItem = null;
    }
}

/// <summary>
/// Spell system state - learned spells, prepared spells, cooldowns
/// </summary>
[Serializable]
public class SpellSystemData
{
    public List<string> learnedSpells; // Spell names
    public List<string> preparedSpells; // Currently prepared spell names
    public Dictionary<string, float> spellCooldowns; // Spell name -> remaining cooldown
    public Dictionary<string, int> gemCharges; // Spell name -> current gem charges

    public SpellSystemData()
    {
        learnedSpells = new List<string>();
        preparedSpells = new List<string>();
        spellCooldowns = new Dictionary<string, float>();
        gemCharges = new Dictionary<string, int>();
    }
}

/// <summary>
/// Active status effects (poison, buffs, debuffs)
/// </summary>
[Serializable]
public class StatusEffectsSaveData
{
    public List<SavedStatusEffect> activeEffects;

    public StatusEffectsSaveData()
    {
        activeEffects = new List<SavedStatusEffect>();
    }
}

/// <summary>
/// Saved state of an active status effect
/// </summary>
[Serializable]
public class SavedStatusEffect
{
    public string effectName; // Reference to StatusEffect ScriptableObject
    public int player; // 1 or 2
    public int severity; // Same as stackCount/intensity
    public int remainingDuration; // Remaining turns

    public SavedStatusEffect(string effectName, int player, int severity, int remainingDuration)
    {
        this.effectName = effectName;
        this.player = player;
        this.severity = severity;
        this.remainingDuration = remainingDuration;
    }
}

/// <summary>
/// Maze/dungeon state - generation seed, explored cells, enemy/treasure states
/// </summary>
[Serializable]
public class MazeStateData
{
    // Generation data
    public int randomSeed;
    public int mazeWidth;
    public int mazeHeight;

    // Player position in maze
    public int playerX;
    public int playerY;
    public int playerFacing; // 0=North, 1=East, 2=South, 3=West

    // Maze cell states (which cells have been visited/explored)
    public List<MazeCellState> cellStates;

    // Minimap exploration state
    public List<Vector2IntSerializable> visitedCells;
    public List<Vector2IntSerializable> revealedCells;

    // Secret room states (opened/unopened)
    public List<Vector2IntSerializable> openedSecretRooms;

    public MazeStateData()
    {
        cellStates = new List<MazeCellState>();
        visitedCells = new List<Vector2IntSerializable>();
        revealedCells = new List<Vector2IntSerializable>();
        openedSecretRooms = new List<Vector2IntSerializable>();
    }
}

/// <summary>
/// State of a single maze cell (enemy defeated, treasure collected, etc.)
/// </summary>
[Serializable]
public class MazeCellState
{
    public int x;
    public int y;
    public bool explored; // Has player visited this cell?
    public bool enemyDefeated; // Has enemy been defeated?
    public bool treasureCollected; // Has treasure been collected?

    public MazeCellState(int x, int y)
    {
        this.x = x;
        this.y = y;
        this.explored = false;
        this.enemyDefeated = false;
        this.treasureCollected = false;
    }
}

/// <summary>
/// Serializable Vector2Int (Unity's Vector2Int isn't JSON serializable by default)
/// </summary>
[Serializable]
public class Vector2IntSerializable
{
    public int x;
    public int y;

    public Vector2IntSerializable(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public Vector2IntSerializable(Vector2Int vector)
    {
        this.x = vector.x;
        this.y = vector.y;
    }

    public Vector2Int ToVector2Int()
    {
        return new Vector2Int(x, y);
    }
}

/// <summary>
/// Game options/settings (audio, graphics, controls)
/// </summary>
[Serializable]
public class GameOptionsData
{
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;
    public string languageCode;
    public bool fullscreen;
    public int resolutionWidth;
    public int resolutionHeight;

    public GameOptionsData()
    {
        masterVolume = 1.0f;
        musicVolume = 0.7f;
        sfxVolume = 0.8f;
        languageCode = "en";
        fullscreen = true;
        resolutionWidth = 1920;
        resolutionHeight = 1080;
    }
}
