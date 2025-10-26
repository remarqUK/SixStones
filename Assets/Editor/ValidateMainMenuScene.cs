using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Validate and re-save MainMenu scene to ensure Unity recognizes it
/// </summary>
public class ValidateMainMenuScene : EditorWindow
{
    [MenuItem("Tools/Validate MainMenu Scene")]
    public static void ValidateScene()
    {
        string scenePath = "Assets/Scenes/MainMenu.unity";

        try
        {
            // Try to open the scene
            var scene = EditorSceneManager.OpenScene(scenePath);

            if (scene.IsValid())
            {
                Debug.Log($"MainMenu scene is valid. Path: {scene.path}");

                // Mark the scene as dirty and save it
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);

                Debug.Log("<color=green>MainMenu scene validated and saved successfully!</color>");

                EditorUtility.DisplayDialog("Scene Validated",
                    "MainMenu scene has been validated and saved!\n\n" +
                    "Now try:\n" +
                    "1. Tools > Setup Build Scenes\n" +
                    "OR\n" +
                    "2. File > Build Profiles > Add Open Scenes",
                    "OK");
            }
            else
            {
                Debug.LogError("MainMenu scene is not valid!");
                EditorUtility.DisplayDialog("Error",
                    "MainMenu scene could not be validated!",
                    "OK");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to open MainMenu scene: {e.Message}");
            EditorUtility.DisplayDialog("Error",
                $"Failed to open MainMenu scene:\n{e.Message}",
                "OK");
        }
    }
}
