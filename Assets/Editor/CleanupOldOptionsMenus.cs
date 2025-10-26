using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Removes old scene-specific options menu setups
/// Run this after creating the new persistent options menu
/// </summary>
public class CleanupOldOptionsMenus : EditorWindow
{
    [MenuItem("Tools/Cleanup Old Options Menus")]
    public static void CleanupOldMenus()
    {
        bool confirm = EditorUtility.DisplayDialog("Cleanup Old Options Menus",
            "This will remove old scene-specific OptionsMenuController objects from MainMenu and Match3 scenes.\n\n" +
            "Make sure you've created the new persistent options menu first!\n\n" +
            "Do you want to continue?",
            "Yes, Clean Up", "Cancel");

        if (!confirm)
        {
            Debug.Log("Cleanup cancelled");
            return;
        }

        // Clean up MainMenu scene
        CleanupScene("Assets/Scenes/MainMenu.unity");

        // Clean up Match3 scene
        CleanupScene("Assets/Scenes/Match3.unity");

        Debug.Log("<color=green>Old options menus cleaned up successfully!</color>");
    }

    private static void CleanupScene(string scenePath)
    {
        // Check if scene exists
        if (!System.IO.File.Exists(scenePath))
        {
            Debug.LogWarning($"Scene not found: {scenePath}");
            return;
        }

        // Open the scene
        var scene = EditorSceneManager.OpenScene(scenePath);

        // Find old OptionsMenuController (not part of GlobalOptionsManager)
        OptionsMenuController[] controllers = GameObject.FindObjectsByType<OptionsMenuController>(FindObjectsSortMode.None);

        bool foundOld = false;
        foreach (var controller in controllers)
        {
            // Check if this is NOT part of GlobalOptionsManager
            GlobalOptionsManager manager = controller.GetComponent<GlobalOptionsManager>();
            if (manager == null)
            {
                // This is an old scene-specific controller
                Debug.Log($"Removing old OptionsMenuController from {scenePath}");

                // Find and remove the options panel
                SerializedObject so = new SerializedObject(controller);
                GameObject optionsPanel = so.FindProperty("optionsPanel").objectReferenceValue as GameObject;
                if (optionsPanel != null)
                {
                    DestroyImmediate(optionsPanel);
                }

                // Remove the controller GameObject
                DestroyImmediate(controller.gameObject);
                foundOld = true;
            }
        }

        // Find and remove old GlobalOptionsAccess components (deprecated)
        // Suppress obsolete warning since we intentionally need to reference it for cleanup
#pragma warning disable CS0618
        GlobalOptionsAccess[] oldAccess = GameObject.FindObjectsByType<GlobalOptionsAccess>(FindObjectsSortMode.None);
#pragma warning restore CS0618
        foreach (var access in oldAccess)
        {
            Debug.Log($"Removing deprecated GlobalOptionsAccess from {scenePath}");
            DestroyImmediate(access.gameObject);
            foundOld = true;
        }

        if (foundOld)
        {
            // Save scene
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"<color=green>Cleaned up {scenePath}</color>");
        }
        else
        {
            Debug.Log($"No old options menus found in {scenePath}");
        }
    }
}
