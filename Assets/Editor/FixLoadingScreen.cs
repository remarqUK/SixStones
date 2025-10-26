using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Fix LoadingScreen to load MainMenu instead of Match3
/// </summary>
public class FixLoadingScreen : EditorWindow
{
    [MenuItem("Tools/Fix Loading Screen Scene")]
    public static void FixLoadingScreenScene()
    {
        // Open the LoadingScreen scene
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/LoadingScreen.unity");

        // Find the LoadingManager
        LoadingManager loadingManager = GameObject.FindFirstObjectByType<LoadingManager>();

        if (loadingManager != null)
        {
            // Use SerializedObject to modify private serialized fields
            SerializedObject so = new SerializedObject(loadingManager);
            SerializedProperty sceneToLoadProp = so.FindProperty("sceneToLoad");

            if (sceneToLoadProp != null)
            {
                string oldValue = sceneToLoadProp.stringValue;
                sceneToLoadProp.stringValue = "MainMenu";
                so.ApplyModifiedProperties();

                Debug.Log($"Updated LoadingManager: sceneToLoad changed from '{oldValue}' to 'MainMenu'");

                // Save the scene
                EditorSceneManager.SaveScene(scene);
                Debug.Log("LoadingScreen scene saved");

                EditorUtility.DisplayDialog("Loading Screen Fixed",
                    $"LoadingManager updated successfully!\n\n" +
                    $"Old value: {oldValue}\n" +
                    $"New value: MainMenu\n\n" +
                    "Now the flow is:\n" +
                    "LoadingScreen → MainMenu → Match3",
                    "OK");
            }
            else
            {
                Debug.LogError("Could not find 'sceneToLoad' field");
            }
        }
        else
        {
            Debug.LogError("Could not find LoadingManager in LoadingScreen scene");
            EditorUtility.DisplayDialog("Error",
                "Could not find LoadingManager in LoadingScreen scene!",
                "OK");
        }
    }
}
