using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Automatically add scenes to Build Settings in correct order
/// </summary>
public class SetupBuildScenes : EditorWindow
{
    [MenuItem("Tools/Setup Build Scenes")]
    public static void AddScenesToBuild()
    {
        // Define scenes in the correct order
        string[] scenePaths = new string[]
        {
            "Assets/Scenes/LoadingScreen.unity",  // Index 0 - First scene to load
            "Assets/Scenes/MainMenu.unity",       // Index 1
            "Assets/Scenes/Match3.unity"          // Index 2
        };

        // Create list of EditorBuildSettingsScene
        List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();

        foreach (string scenePath in scenePaths)
        {
            // Check if scene exists
            if (System.IO.File.Exists(scenePath))
            {
                buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                Debug.Log($"Added to build: {scenePath}");
            }
            else
            {
                Debug.LogWarning($"Scene not found: {scenePath}");
            }
        }

        // Set the build scenes
        EditorBuildSettings.scenes = buildScenes.ToArray();

        Debug.Log($"<color=green>Build settings updated! {buildScenes.Count} scenes added.</color>");

        string sceneList = string.Join("\n", scenePaths.Select((path, index) =>
            $"{index}. {System.IO.Path.GetFileNameWithoutExtension(path)}"));

        EditorUtility.DisplayDialog("Build Scenes Setup Complete",
            $"Successfully added {buildScenes.Count} scenes to Build Settings:\n\n" +
            sceneList + "\n\n" +
            "Flow: LoadingScreen → MainMenu → Match3",
            "OK");
    }
}
