using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Simple script to add CharacterSelection to build settings
/// </summary>
public class FixBuildSettings
{
    [MenuItem("Tools/Fix Build Settings")]
    public static void Fix()
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();

        string charSelectPath = "Assets/Scenes/CharacterSelection.unity";

        if (scenes.Any(s => s.path == charSelectPath))
        {
            Debug.Log("CharacterSelection already in build settings");
            return;
        }

        scenes.Add(new EditorBuildSettingsScene(charSelectPath, true));
        EditorBuildSettings.scenes = scenes.ToArray();

        Debug.Log("Added CharacterSelection to build settings!");
    }
}
