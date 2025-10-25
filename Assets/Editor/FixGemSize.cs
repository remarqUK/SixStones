using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor tool to fix gem size in existing scenes
/// </summary>
public class FixGemSize
{
    [MenuItem("Tools/Fix Gem Size (Calculate from Grid)")]
    public static void FixGemSizes()
    {
        // Find the board to get cell size
        Board board = GameObject.FindFirstObjectByType<Board>();
        float cellSize = 1.0f; // Default

        if (board != null)
        {
            SerializedObject boardSO = new SerializedObject(board);
            cellSize = boardSO.FindProperty("cellSize").floatValue;
        }

        // Find all GamePiece components in the scene
        GamePiece[] pieces = GameObject.FindObjectsByType<GamePiece>(FindObjectsSortMode.None);

        if (pieces.Length == 0)
        {
            EditorUtility.DisplayDialog("No Gems Found",
                "No GamePiece components found in the current scene.\n\n" +
                "Make sure the game is not running and you have gems in the scene.",
                "OK");
            return;
        }

        // Scale gems to 75% of cell size (leaves 12.5% gap on each side)
        float gemSizeRatio = 0.75f;
        float gemScale = cellSize * gemSizeRatio;

        int fixedCount = 0;
        foreach (GamePiece piece in pieces)
        {
            piece.transform.localScale = new Vector3(gemScale, gemScale, 1f);
            fixedCount++;
        }

        // Also update the prefab if it exists
        string prefabPath = "Assets/Prefabs/GamePiece.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab != null)
        {
            prefab.transform.localScale = new Vector3(gemScale, gemScale, 1f);
            PrefabUtility.SavePrefabAsset(prefab);
            Debug.Log($"<color=green>Updated GamePiece prefab at {prefabPath}</color>");
        }

        Debug.Log($"<color=green>Scaled {fixedCount} gems to {gemScale:F2} (75% of cell size {cellSize:F2})</color>");

        EditorUtility.DisplayDialog("Gem Size Fixed",
            $"Scaled {fixedCount} gems based on grid:\n\n" +
            $"Cell Size: {cellSize:F2}\n" +
            $"Gem Scale: {gemScale:F2} (75% of cell)\n\n" +
            "The gems should now fit perfectly with proper spacing.\n\n" +
            "Note: Changes are applied immediately, no need to save.",
            "OK");
    }
}
