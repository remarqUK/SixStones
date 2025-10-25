using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor script to create default gem type configurations
/// </summary>
public class CreateDefaultGemTypes
{
    [MenuItem("Tools/Create Default Gem Types")]
    public static void CreateGemTypes()
    {
        string gemFolder = "Assets/GemTypes";

        // Create folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder(gemFolder))
        {
            AssetDatabase.CreateFolder("Assets", "GemTypes");
        }

        // Create gem types for all standard piece types
        CreateRedGem(gemFolder);
        CreateBlueGem(gemFolder);
        CreateGreenGem(gemFolder);
        CreateYellowGem(gemFolder);
        CreatePurpleGem(gemFolder);
        CreateOrangeGem(gemFolder);
        CreateWhiteGem(gemFolder);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Created default gem types in Assets/GemTypes folder!");
        Debug.Log("Yellow gems award 5 gold per gem. Other gems award 0 gold by default.");
    }

    private static void CreateRedGem(string folder)
    {
        GemTypeData gem = ScriptableObject.CreateInstance<GemTypeData>();
        gem.gemType = GamePiece.PieceType.Red;
        gem.gemName = "Red Gem";
        gem.gemColor = Color.red;
        gem.goldPerGem = 0;
        gem.xpMultiplier = 1.0f;

        AssetDatabase.CreateAsset(gem, $"{folder}/RedGem.asset");
    }

    private static void CreateBlueGem(string folder)
    {
        GemTypeData gem = ScriptableObject.CreateInstance<GemTypeData>();
        gem.gemType = GamePiece.PieceType.Blue;
        gem.gemName = "Blue Gem";
        gem.gemColor = Color.blue;
        gem.goldPerGem = 0;
        gem.xpMultiplier = 1.0f;

        AssetDatabase.CreateAsset(gem, $"{folder}/BlueGem.asset");
    }

    private static void CreateGreenGem(string folder)
    {
        GemTypeData gem = ScriptableObject.CreateInstance<GemTypeData>();
        gem.gemType = GamePiece.PieceType.Green;
        gem.gemName = "Green Gem";
        gem.gemColor = Color.green;
        gem.goldPerGem = 0;
        gem.xpMultiplier = 1.0f;

        AssetDatabase.CreateAsset(gem, $"{folder}/GreenGem.asset");
    }

    private static void CreateYellowGem(string folder)
    {
        GemTypeData gem = ScriptableObject.CreateInstance<GemTypeData>();
        gem.gemType = GamePiece.PieceType.Yellow;
        gem.gemName = "Yellow Gem";
        gem.gemColor = Color.yellow;
        gem.goldPerGem = 5; // Yellow gems give 5 gold per gem!
        gem.xpMultiplier = 1.0f;

        AssetDatabase.CreateAsset(gem, $"{folder}/YellowGem.asset");
    }

    private static void CreatePurpleGem(string folder)
    {
        GemTypeData gem = ScriptableObject.CreateInstance<GemTypeData>();
        gem.gemType = GamePiece.PieceType.Purple;
        gem.gemName = "Purple Gem";
        gem.gemColor = new Color(0.5f, 0f, 0.5f); // Purple
        gem.goldPerGem = 0;
        gem.xpMultiplier = 1.0f;

        AssetDatabase.CreateAsset(gem, $"{folder}/PurpleGem.asset");
    }

    private static void CreateOrangeGem(string folder)
    {
        GemTypeData gem = ScriptableObject.CreateInstance<GemTypeData>();
        gem.gemType = GamePiece.PieceType.Orange;
        gem.gemName = "Orange Gem";
        gem.gemColor = new Color(1f, 0.5f, 0f); // Orange
        gem.goldPerGem = 0;
        gem.xpMultiplier = 1.0f;

        AssetDatabase.CreateAsset(gem, $"{folder}/OrangeGem.asset");
    }

    private static void CreateWhiteGem(string folder)
    {
        GemTypeData gem = ScriptableObject.CreateInstance<GemTypeData>();
        gem.gemType = GamePiece.PieceType.White;
        gem.gemName = "White Gem";
        gem.gemColor = Color.white;
        gem.goldPerGem = 0;
        gem.xpMultiplier = 1.0f;

        AssetDatabase.CreateAsset(gem, $"{folder}/WhiteGem.asset");
    }
}
