using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Central registry of all piece types in the game
/// Unity 6 best practice: Data-driven, easily extensible
/// Add new gems by creating PieceTypeData assets and adding them here
/// </summary>
[CreateAssetMenu(fileName = "PieceTypeDatabase", menuName = "Match3/Piece Type Database")]
public class PieceTypeDatabase : ScriptableObject
{
    [Header("Available Piece Types")]
    [Tooltip("Add or remove piece types here - no code changes needed!")]
    public List<PieceTypeData> pieceTypes = new List<PieceTypeData>();

    [Header("Spawn Settings")]
    [Tooltip("Only spawn from this many types (0 = use all)")]
    [Range(0, 20)]
    public int maxTypesInPlay = 6;

    // Cache for quick lookups
    private Dictionary<int, PieceTypeData> typeCache;
    private List<PieceTypeData> spawnableTypes;
    private float totalSpawnWeight;

    /// <summary>
    /// Initialize the database (call at game start)
    /// </summary>
    public void Initialize()
    {
        BuildCache();
        ValidateDatabase();
    }

    private void BuildCache()
    {
        typeCache = new Dictionary<int, PieceTypeData>();

        foreach (var type in pieceTypes)
        {
            if (type != null)
            {
                if (typeCache.ContainsKey(type.typeID))
                {
                    Debug.LogWarning($"Duplicate piece type ID {type.typeID}: {type.pieceName}");
                }
                else
                {
                    typeCache[type.typeID] = type;
                }
            }
        }

        // Determine which types can spawn
        spawnableTypes = pieceTypes
            .Where(t => t != null && !t.isSpecial)
            .Take(maxTypesInPlay > 0 ? maxTypesInPlay : pieceTypes.Count)
            .ToList();

        totalSpawnWeight = spawnableTypes.Sum(t => t.spawnWeight);
    }

    private void ValidateDatabase()
    {
        if (pieceTypes.Count == 0)
        {
            Debug.LogError("PieceTypeDatabase has no piece types!");
        }

        if (spawnableTypes.Count < 3)
        {
            Debug.LogWarning("Less than 3 spawnable piece types - matches may be difficult!");
        }

        // Check for missing sprites
        foreach (var type in pieceTypes)
        {
            if (type != null && type.sprite == null)
            {
                Debug.LogWarning($"Piece type '{type.pieceName}' has no sprite assigned");
            }
        }
    }

    /// <summary>
    /// Get a piece type by ID
    /// </summary>
    public PieceTypeData GetTypeByID(int id)
    {
        if (typeCache == null) Initialize();
        return typeCache.TryGetValue(id, out var type) ? type : null;
    }

    /// <summary>
    /// Get a random spawnable piece type based on spawn weights
    /// </summary>
    public PieceTypeData GetRandomType()
    {
        if (spawnableTypes == null || spawnableTypes.Count == 0)
        {
            Initialize();
        }

        if (spawnableTypes.Count == 0)
        {
            Debug.LogError("No spawnable piece types available!");
            return null;
        }

        // Weighted random selection
        float randomValue = Random.Range(0f, totalSpawnWeight);
        float currentWeight = 0f;

        foreach (var type in spawnableTypes)
        {
            currentWeight += type.spawnWeight;
            if (randomValue <= currentWeight)
            {
                return type;
            }
        }

        // Fallback to first type
        return spawnableTypes[0];
    }

    /// <summary>
    /// Get all spawnable types (non-special)
    /// </summary>
    public List<PieceTypeData> GetSpawnableTypes()
    {
        if (spawnableTypes == null) Initialize();
        return new List<PieceTypeData>(spawnableTypes);
    }

    /// <summary>
    /// Get all piece types including special ones
    /// </summary>
    public List<PieceTypeData> GetAllTypes()
    {
        return new List<PieceTypeData>(pieceTypes);
    }

    /// <summary>
    /// Get count of spawnable types
    /// </summary>
    public int SpawnableTypeCount
    {
        get
        {
            if (spawnableTypes == null) Initialize();
            return spawnableTypes.Count;
        }
    }

    /// <summary>
    /// Add a new piece type at runtime (for DLC, events, etc.)
    /// </summary>
    public void AddPieceType(PieceTypeData newType)
    {
        if (newType == null) return;

        if (!pieceTypes.Contains(newType))
        {
            pieceTypes.Add(newType);
            BuildCache(); // Rebuild cache
        }
    }

    /// <summary>
    /// Remove a piece type at runtime
    /// </summary>
    public void RemovePieceType(int typeID)
    {
        var type = GetTypeByID(typeID);
        if (type != null)
        {
            pieceTypes.Remove(type);
            BuildCache(); // Rebuild cache
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Rebuild cache when changed in Inspector
        BuildCache();
    }
#endif
}
