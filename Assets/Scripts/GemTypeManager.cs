using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages gem type configurations and properties
/// </summary>
public class GemTypeManager : MonoBehaviour
{
    private static GemTypeManager instance;
    public static GemTypeManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<GemTypeManager>();
            }
            return instance;
        }
    }

    [Header("Gem Type Configurations")]
    [SerializeField] private List<GemTypeData> gemTypes = new List<GemTypeData>();

    private Dictionary<GamePiece.PieceType, GemTypeData> gemTypeLookup = new Dictionary<GamePiece.PieceType, GemTypeData>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        InitializeLookup();
    }

    private void InitializeLookup()
    {
        gemTypeLookup.Clear();
        foreach (var gemTypeData in gemTypes)
        {
            if (gemTypeData != null)
            {
                gemTypeLookup[gemTypeData.gemType] = gemTypeData;
            }
        }
    }

    /// <summary>
    /// Get gem type data for a specific gem
    /// </summary>
    public GemTypeData GetGemTypeData(GamePiece.PieceType gemType)
    {
        if (gemTypeLookup.TryGetValue(gemType, out GemTypeData data))
        {
            return data;
        }
        return null;
    }

    /// <summary>
    /// Get gold value for a specific gem type
    /// </summary>
    public int GetGoldPerGem(GamePiece.PieceType gemType)
    {
        GemTypeData data = GetGemTypeData(gemType);
        return data != null ? data.goldPerGem : 0;
    }

    /// <summary>
    /// Get XP multiplier for a specific gem type
    /// </summary>
    public float GetXPMultiplier(GamePiece.PieceType gemType)
    {
        GemTypeData data = GetGemTypeData(gemType);
        return data != null ? data.xpMultiplier : 1.0f;
    }

    /// <summary>
    /// Calculate total gold from a list of matched pieces
    /// </summary>
    public int CalculateTotalGold(List<GamePiece> matchedPieces)
    {
        int totalGold = 0;
        foreach (var piece in matchedPieces)
        {
            if (piece != null)
            {
                totalGold += GetGoldPerGem(piece.Type);
            }
        }
        return totalGold;
    }

    /// <summary>
    /// Calculate total gold from a dictionary of gem types and counts
    /// </summary>
    public int CalculateTotalGold(Dictionary<GamePiece.PieceType, List<GamePiece>> colorGroups)
    {
        int totalGold = 0;
        foreach (var kvp in colorGroups)
        {
            GamePiece.PieceType gemType = kvp.Key;
            int count = kvp.Value.Count;
            int goldPerGem = GetGoldPerGem(gemType);
            totalGold += goldPerGem * count;
        }
        return totalGold;
    }

    /// <summary>
    /// Register a gem type at runtime
    /// </summary>
    public void RegisterGemType(GemTypeData gemTypeData)
    {
        if (gemTypeData != null && !gemTypes.Contains(gemTypeData))
        {
            gemTypes.Add(gemTypeData);
            gemTypeLookup[gemTypeData.gemType] = gemTypeData;
        }
    }

    /// <summary>
    /// Get all registered gem types
    /// </summary>
    public List<GemTypeData> GetAllGemTypes()
    {
        return new List<GemTypeData>(gemTypes);
    }
}
