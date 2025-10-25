using UnityEngine;
using UnityEditor;

/// <summary>
/// Diagnostic tool to show grid and gem measurements
/// </summary>
public class DiagnoseGridMeasurements
{
    [MenuItem("Tools/Diagnose Grid Measurements")]
    public static void ShowMeasurements()
    {
        // Find the board
        Board board = GameObject.FindFirstObjectByType<Board>();
        if (board == null)
        {
            EditorUtility.DisplayDialog("No Board", "No Board found in scene.", "OK");
            return;
        }

        // Get board properties
        SerializedObject boardSO = new SerializedObject(board);
        int width = boardSO.FindProperty("width").intValue;
        int height = boardSO.FindProperty("height").intValue;
        float cellSize = boardSO.FindProperty("cellSize").floatValue;

        // Calculate grid measurements
        float gridWidth = width * cellSize;
        float gridHeight = height * cellSize;

        // Find a gem to check its scale
        GamePiece[] pieces = GameObject.FindObjectsByType<GamePiece>(FindObjectsSortMode.None);
        float gemScale = 0.9f; // Default
        float actualGemScale = 0f;

        if (pieces.Length > 0)
        {
            actualGemScale = pieces[0].transform.localScale.x;
        }

        // Also check prefab
        string prefabPath = "Assets/Prefabs/GamePiece.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        float prefabGemScale = 0f;
        if (prefab != null)
        {
            prefabGemScale = prefab.transform.localScale.x;
        }

        // Build message
        string message = "=== GRID MEASUREMENTS ===\n\n";
        message += "GRID DIMENSIONS:\n";
        message += $"  Width: {width} cells × {cellSize} = {gridWidth} world units\n";
        message += $"  Height: {height} cells × {cellSize} = {gridHeight} world units\n\n";

        message += "GEM CELLS:\n";
        message += $"  Number of gems horizontally: {width}\n";
        message += $"  Number of gems vertically: {height}\n";
        message += $"  Height of each gem cell: {cellSize} world units\n";
        message += $"  Width of each gem cell: {cellSize} world units\n\n";

        message += "GEM SPRITE SIZE:\n";
        if (pieces.Length > 0)
        {
            message += $"  Gem scale in scene: {actualGemScale:F3} world units\n";
            message += $"  Gem fills {actualGemScale / cellSize * 100:F1}% of cell\n";
            message += $"  Gap between gems: {cellSize - actualGemScale:F3} world units ({(cellSize - actualGemScale) / cellSize * 100:F1}%)\n";
        }
        else
        {
            message += "  No gems in scene to measure\n";
        }

        if (prefab != null)
        {
            message += $"\n  Prefab gem scale: {prefabGemScale:F3} world units\n";
            message += $"  Prefab gem fills {prefabGemScale / cellSize * 100:F1}% of cell\n";
        }

        message += "\n";
        message += "CALCULATIONS:\n";
        message += $"  Grid Height = {height} gems × {cellSize} cell size = {gridHeight} units\n";
        message += $"  Gem Height = cell size × gem scale ratio\n";
        message += $"  Gem Height = {cellSize} × {(actualGemScale > 0 ? actualGemScale : prefabGemScale):F2} = {(actualGemScale > 0 ? actualGemScale : prefabGemScale):F3} units\n";

        Debug.Log(message);

        EditorUtility.DisplayDialog("Grid Measurements",
            $"Grid: {width}×{height} cells\n" +
            $"Grid Height: {gridHeight} world units\n" +
            $"Gems in Height: {height}\n\n" +
            $"Cell Size: {cellSize} units\n" +
            $"Gem Scale: {(actualGemScale > 0 ? actualGemScale : prefabGemScale):F3} units\n" +
            $"Gem fills {(actualGemScale > 0 ? actualGemScale : prefabGemScale) / cellSize * 100:F1}% of cell\n\n" +
            "See Console for detailed breakdown.",
            "OK");
    }
}
