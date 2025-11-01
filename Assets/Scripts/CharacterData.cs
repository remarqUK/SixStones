using UnityEngine;

/// <summary>
/// ScriptableObject that defines a character archetype with D&D-style attributes
/// </summary>
[CreateAssetMenu(fileName = "NewCharacter", menuName = "Character/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Basic Info")]
    public string characterName = "Knight";
    public string description = "A noble warrior skilled in combat";
    public Sprite characterSprite; // Character portrait/image

    [Header("Core Attributes")]
    [Tooltip("Physical power and melee damage")]
    public int strength = 10;

    [Tooltip("Agility, evasion, and ranged damage")]
    public int dexterity = 10;

    [Tooltip("Health points and physical resistance")]
    public int constitution = 10;

    [Tooltip("Magical power and spell damage")]
    public int intelligence = 10;

    [Tooltip("Perception and awareness")]
    public int wisdom = 10;

    [Tooltip("Social skills and leadership")]
    public int charisma = 10;

    [Header("Derived Stats")]
    [Tooltip("Starting health points")]
    public int baseHealth = 100;

    [Tooltip("Starting mana/energy")]
    public int baseMana = 50;

    [Tooltip("Movement speed")]
    public float moveSpeed = 5f;

    [Header("Class Features")]
    [Tooltip("Special abilities or passive traits")]
    public string[] classFeatures = new string[0];

    /// <summary>
    /// Get a formatted string of all attributes
    /// </summary>
    public string GetAttributesSummary()
    {
        return $"<b>{characterName}</b>\n\n" +
               $"<i>{description}</i>\n\n" +
               $"<b>Attributes:</b>\n" +
               $"Strength: {strength}\n" +
               $"Dexterity: {dexterity}\n" +
               $"Constitution: {constitution}\n" +
               $"Intelligence: {intelligence}\n" +
               $"Wisdom: {wisdom}\n" +
               $"Charisma: {charisma}\n\n" +
               $"<b>Derived Stats:</b>\n" +
               $"Health: {baseHealth}\n" +
               $"Mana: {baseMana}\n" +
               $"Move Speed: {moveSpeed}";
    }
}
