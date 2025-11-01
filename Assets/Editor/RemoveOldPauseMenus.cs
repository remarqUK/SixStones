using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Removes old scene-specific PauseMenu objects from all scenes
/// This is needed after converting to the global singleton PauseMenu pattern
/// </summary>
public class RemoveOldPauseMenus : EditorWindow
{
    [MenuItem("Tools/Remove Old Scene Pause Menus")]
    public static void RemovePauseMenus()
    {
        bool confirmed = EditorUtility.DisplayDialog(
            "Remove Old Pause Menus",
            "This will remove scene-specific PauseMenu objects from Match3 and Maze3D scenes.\n\n" +
            "The new global PauseMenu singleton will work automatically across all scenes.\n\n" +
            "Current scenes will be saved. Continue?",
            "Yes, Remove", "Cancel");

        if (!confirmed)
        {
            Debug.Log("Operation cancelled");
            return;
        }

        // Save current scene state
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        Scene originalScene = SceneManager.GetActiveScene();

        int removedCount = 0;

        // Process Match3 scene
        removedCount += ProcessScene("Assets/Scenes/Match3.unity");

        // Process Maze3D scene
        removedCount += ProcessScene("Assets/Scenes/Maze3D.unity");

        // Restore original scene
        if (!string.IsNullOrEmpty(originalScene.path))
        {
            EditorSceneManager.OpenScene(originalScene.path);
        }

        Debug.Log($"<color=green>Cleanup complete! Removed {removedCount} old PauseMenu objects.</color>");
        Debug.Log("The global PauseMenu singleton will now work consistently across all scenes.");
        Debug.Log("Next step: Run 'Tools > Create Global Pause Menu Prefab' if you haven't already.");
    }

    private static int ProcessScene(string scenePath)
    {
        int removedCount = 0;

        // Open the scene
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        Debug.Log($"\nProcessing scene: {scene.name}");

        // Find all root GameObjects
        GameObject[] rootObjects = scene.GetRootGameObjects();

        foreach (GameObject rootObj in rootObjects)
        {
            // Check for PauseMenu component
            PauseMenu pauseMenu = rootObj.GetComponent<PauseMenu>();
            if (pauseMenu != null)
            {
                Debug.Log($"  - Found and removing GameObject with PauseMenu: {rootObj.name}");
                Object.DestroyImmediate(rootObj);
                removedCount++;
                continue;
            }

            // Check for objects named "PauseMenu" or "PauseMenuManager"
            if (rootObj.name.Contains("PauseMenu") || rootObj.name == "PauseMenuManager")
            {
                Debug.Log($"  - Found and removing PauseMenu GameObject: {rootObj.name}");
                Object.DestroyImmediate(rootObj);
                removedCount++;
                continue;
            }

            // Also check for PauseMenuCanvas
            if (rootObj.name == "PauseMenuCanvas")
            {
                Debug.Log($"  - Found and removing PauseMenuCanvas: {rootObj.name}");
                Object.DestroyImmediate(rootObj);
                removedCount++;
                continue;
            }

            // Check children for PauseMenu components
            PauseMenu[] childPauseMenus = rootObj.GetComponentsInChildren<PauseMenu>(true);
            foreach (PauseMenu childPauseMenu in childPauseMenus)
            {
                Debug.Log($"  - Found and removing child GameObject with PauseMenu: {childPauseMenu.gameObject.name}");
                Object.DestroyImmediate(childPauseMenu.gameObject);
                removedCount++;
            }
        }

        // Save the scene
        if (removedCount > 0)
        {
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"  - Saved {scene.name} with {removedCount} objects removed");
        }
        else
        {
            Debug.Log($"  - No PauseMenu objects found in {scene.name}");
        }

        return removedCount;
    }
}
