using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Editor script to add CharacterSelection scene to Build Settings
/// Automatically runs when Unity loads
/// </summary>
[InitializeOnLoad]
public class AddCharacterSelectionToBuild
{
    static AddCharacterSelectionToBuild()
    {
        // Delay execution to avoid issues during Unity initialization
        EditorApplication.delayCall += AddCharacterSelectionSceneIfNeeded;
    }

    [MenuItem("Tools/Build Settings/Add CharacterSelection Scene")]
    public static void AddCharacterSelectionScene()
    {
        AddCharacterSelectionSceneIfNeeded();
    }

    private static void AddCharacterSelectionSceneIfNeeded()
    {
        // Get the list of current scenes in build settings
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

        // Path to the CharacterSelection scene
        string characterSelectionPath = "Assets/Scenes/CharacterSelection.unity";

        // Check if scene already exists in build settings
        bool alreadyExists = false;
        foreach (var scene in scenes)
        {
            if (scene.path == characterSelectionPath)
            {
                alreadyExists = true;
                break;
            }
        }

        if (alreadyExists)
        {
            return;
        }

        // Add the CharacterSelection scene to build settings
        EditorBuildSettingsScene newScene = new EditorBuildSettingsScene(characterSelectionPath, true);
        scenes.Add(newScene);

        // Update build settings
        EditorBuildSettings.scenes = scenes.ToArray();

        Debug.Log($"[AutoBuildSettings] Added CharacterSelection scene to Build Settings at index {scenes.Count - 1}");
    }
}
