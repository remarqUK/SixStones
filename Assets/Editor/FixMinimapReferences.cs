using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class FixMinimapReferences : EditorWindow
{
    [MenuItem("Tools/Fix Minimap References")]
    public static void FixReferences()
    {
        Debug.Log("Starting minimap reference fix...");

        // Find MinimapRenderer in the scene
        MinimapRenderer minimapRenderer = Object.FindFirstObjectByType<MinimapRenderer>();
        if (minimapRenderer == null)
        {
            Debug.LogError("No MinimapRenderer found in scene!");
            return;
        }

        Debug.Log($"Found MinimapRenderer on GameObject: {minimapRenderer.gameObject.name}");

        // Find MapGenerator
        MapGenerator mapGen = Object.FindFirstObjectByType<MapGenerator>();
        if (mapGen == null)
        {
            Debug.LogError("No MapGenerator found in scene!");
            return;
        }

        Debug.Log($"Found MapGenerator on GameObject: {mapGen.gameObject.name}");

        // Find FirstPersonMazeController (player)
        FirstPersonMazeController player = Object.FindFirstObjectByType<FirstPersonMazeController>();
        if (player == null)
        {
            Debug.LogError("No FirstPersonMazeController found in scene!");
            return;
        }

        Debug.Log($"Found Player on GameObject: {player.gameObject.name}");

        // Find the minimap RawImage (usually in Canvas hierarchy)
        RawImage[] rawImages = Object.FindObjectsByType<RawImage>(FindObjectsSortMode.None);
        RawImage minimapImage = null;

        foreach (RawImage img in rawImages)
        {
            // Look for RawImage in minimap-related objects
            if (img.gameObject.name.ToLower().Contains("minimap") ||
                img.transform.parent != null && img.transform.parent.name.ToLower().Contains("minimap"))
            {
                minimapImage = img;
                break;
            }
        }

        if (minimapImage == null && rawImages.Length > 0)
        {
            // If we didn't find by name, use the first RawImage we found
            minimapImage = rawImages[0];
            Debug.LogWarning($"Couldn't find minimap RawImage by name, using first found: {minimapImage.gameObject.name}");
        }

        if (minimapImage == null)
        {
            Debug.LogError("No RawImage found for minimap!");
            return;
        }

        Debug.Log($"Found minimap RawImage on GameObject: {minimapImage.gameObject.name}");

        // Mark the minimapRenderer for undo
        Undo.RecordObject(minimapRenderer, "Fix Minimap References");

        // Assign references using public properties
        minimapRenderer.mapGenerator = mapGen;
        minimapRenderer.player = player;
        minimapRenderer.minimapImage = minimapImage;

        // Mark the scene as dirty so Unity knows to save changes
        EditorUtility.SetDirty(minimapRenderer);

        Debug.Log("✓ Minimap references fixed successfully!");
        Debug.Log($"  - MapGenerator: {minimapRenderer.mapGenerator != null}");
        Debug.Log($"  - Player: {minimapRenderer.player != null}");
        Debug.Log($"  - MinimapImage: {minimapRenderer.minimapImage != null}");

        // Save the scene
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );

        Debug.Log("Scene marked as dirty. Remember to save!");
    }
}
