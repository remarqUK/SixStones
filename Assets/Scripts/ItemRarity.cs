using UnityEngine;

public enum ItemRarity
{
    Common,      // White/Gray
    Uncommon,    // Green
    Rare,        // Blue
    VeryRare,    // Purple
    Legendary,   // Orange/Gold
    Artifact     // Red/Unique
}

// Extension methods for rarity colors
public static class ItemRarityExtensions
{
    public static Color GetColor(this ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => new Color(0.8f, 0.8f, 0.8f),      // Light gray
            ItemRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f),    // Green
            ItemRarity.Rare => new Color(0.3f, 0.5f, 1f),          // Blue
            ItemRarity.VeryRare => new Color(0.7f, 0.3f, 1f),      // Purple
            ItemRarity.Legendary => new Color(1f, 0.6f, 0f),       // Orange
            ItemRarity.Artifact => new Color(1f, 0.2f, 0.2f),      // Red
            _ => Color.white
        };
    }

    public static string GetDisplayName(this ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.VeryRare => "Very Rare",
            _ => rarity.ToString()
        };
    }
}
