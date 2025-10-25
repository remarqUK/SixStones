using UnityEngine;

/// <summary>
/// Defines properties for a gem type
/// </summary>
[CreateAssetMenu(fileName = "New Gem Type", menuName = "Match3/Gem Type")]
public class GemTypeData : ScriptableObject
{
    [Header("Identity")]
    public GamePiece.PieceType gemType;
    public string gemName;

    [Header("Visual")]
    public Color gemColor = Color.white;
    public Sprite gemSprite;

    [Header("Rewards")]
    [Tooltip("Gold awarded per gem when matched")]
    public int goldPerGem = 0;

    [Tooltip("XP bonus multiplier (1.0 = normal, 2.0 = double XP)")]
    public float xpMultiplier = 1.0f;

    [Header("Special Properties")]
    [Tooltip("Does this gem have special match properties?")]
    public bool isSpecialGem = false;

    [Tooltip("Description of special behavior")]
    [TextArea(2, 4)]
    public string specialDescription = "";
}
