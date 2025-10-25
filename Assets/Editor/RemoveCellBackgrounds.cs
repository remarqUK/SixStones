using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor tool to remove cell background objects from the scene
/// </summary>
public class RemoveCellBackgrounds
{
    [MenuItem("Tools/Remove Cell Backgrounds")]
    public static void RemoveBackgrounds()
    {
        // Find the Board
        Board board = GameObject.FindFirstObjectByType<Board>();
        if (board == null)
        {
            EditorUtility.DisplayDialog("No Board", "No Board found in scene.", "OK");
            return;
        }

        // Find Background parent object
        Transform backgroundParent = board.transform.Find("Background");

        if (backgroundParent != null)
        {
            int cellCount = backgroundParent.childCount;
            GameObject.DestroyImmediate(backgroundParent.gameObject);

            Debug.Log($"<color=green>Removed Background object with {cellCount} cell backgrounds</color>");

            EditorUtility.DisplayDialog("Backgrounds Removed",
                $"Removed {cellCount} cell background objects.\n\n" +
                "The grid now has a clean, uniform dark background.",
                "OK");
        }
        else
        {
            Debug.Log("No Background object found - already clean!");
            EditorUtility.DisplayDialog("Already Clean",
                "No cell backgrounds found in the scene.\n\nThe grid is already using a clean background.",
                "OK");
        }
    }
}
