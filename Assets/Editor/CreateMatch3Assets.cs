using UnityEngine;
using UnityEditor;

/// <summary>
/// Creates all necessary ScriptableObject assets for the modern Match-3 system
/// </summary>
public class CreateMatch3Assets : EditorWindow
{
    [MenuItem("Tools/Create Match 3 Assets")]
    public static void CreateAssets()
    {
        // Create Resources folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        if (!AssetDatabase.IsValidFolder("Assets/Resources/Match3"))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "Match3");
        }

        // Create GameSettings
        GameSettings settings = ScriptableObject.CreateInstance<GameSettings>();
        AssetDatabase.CreateAsset(settings, "Assets/Resources/Match3/GameSettings.asset");

        // Create PieceTypeDatabase
        PieceTypeDatabase database = ScriptableObject.CreateInstance<PieceTypeDatabase>();
        database.maxTypesInPlay = 6;

        // Create 6 default piece types
        string[] colorNames = { "Red", "Blue", "Green", "Yellow", "Purple", "Orange" };
        Color[] colors = {
            new Color(0.9f, 0.2f, 0.2f),    // Red
            new Color(0.2f, 0.5f, 0.9f),    // Blue
            new Color(0.2f, 0.8f, 0.3f),    // Green
            new Color(0.95f, 0.85f, 0.2f),  // Yellow
            new Color(0.7f, 0.2f, 0.8f),    // Purple
            new Color(0.95f, 0.5f, 0.1f)    // Orange
        };

        for (int i = 0; i < colorNames.Length; i++)
        {
            PieceTypeData pieceType = ScriptableObject.CreateInstance<PieceTypeData>();
            pieceType.pieceName = colorNames[i];
            pieceType.typeID = i;
            pieceType.color = colors[i];
            pieceType.basePoints = 10;
            pieceType.spawnWeight = 1f;

            string path = $"Assets/Resources/Match3/{colorNames[i]}Piece.asset";
            AssetDatabase.CreateAsset(pieceType, path);

            database.pieceTypes.Add(pieceType);
        }

        AssetDatabase.CreateAsset(database, "Assets/Resources/Match3/PieceTypeDatabase.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Match-3 assets created successfully!");
        EditorUtility.DisplayDialog("Success",
            "Match-3 ScriptableObject assets created in Assets/Resources/Match3/\n\n" +
            "- GameSettings.asset\n" +
            "- PieceTypeDatabase.asset\n" +
            "- 6 piece type assets (Red, Blue, Green, Yellow, Purple, Orange)",
            "OK");
    }
}
