using UnityEngine;

/// <summary>
/// Defines a single piece/gem type with all its properties
/// Unity 6 best practice: Data-driven design for easy expansion
/// </summary>
[CreateAssetMenu(fileName = "NewPieceType", menuName = "Match3/Piece Type")]
public class PieceTypeData : ScriptableObject
{
    [Header("Identity")]
    public string pieceName = "Red Gem";
    public int typeID = 0; // Unique identifier

    [Header("Visual")]
    public Sprite sprite;
    public Color color = Color.white;

    [Header("Gameplay")]
    [Range(1, 100)]
    public int basePoints = 10;

    [Tooltip("Weight for random spawning (higher = more common)")]
    [Range(0.1f, 10f)]
    public float spawnWeight = 1f;

    [Header("Special Properties")]
    public bool isSpecial = false;
    public string specialAbility = "";

    [Header("Audio")]
    public AudioClip matchSound;

    [Header("Visual Effects")]
    public GameObject matchParticle;
    public float particleScale = 1f;
}
