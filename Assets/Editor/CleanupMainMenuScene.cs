using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Removes obsolete settings panel from MainMenu scene
/// </summary>
public class CleanupMainMenuScene : EditorWindow
{
    [MenuItem("Tools/Cleanup MainMenu Scene")]
    public static void CleanupMainMenu()
    {
        // Check if MainMenu scene exists
        string scenePath = "Assets/Scenes/MainMenu.unity";
        if (!System.IO.File.Exists(scenePath))
        {
            Debug.LogError("MainMenu scene not found at " + scenePath);
            return;
        }

        // Open the scene
        var scene = EditorSceneManager.OpenScene(scenePath);

        bool foundSomething = false;

        // Find and remove SettingsPanel (obsolete overlay)
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "SettingsPanel" && obj.transform.parent != null && obj.transform.parent.name == "Canvas")
            {
                Debug.Log("Removing obsolete SettingsPanel from MainMenu");
                DestroyImmediate(obj);
                foundSomething = true;
                break;
            }
        }

        // Find and remove any old OptionsMenuController that's not part of GlobalOptionsManager
        OptionsMenuController[] controllers = GameObject.FindObjectsByType<OptionsMenuController>(FindObjectsSortMode.None);
        foreach (var controller in controllers)
        {
            GlobalOptionsManager manager = controller.GetComponent<GlobalOptionsManager>();
            if (manager == null)
            {
                Debug.Log("Removing old OptionsMenuController from MainMenu");
                DestroyImmediate(controller.gameObject);
                foundSomething = true;
            }
        }

        if (foundSomething)
        {
            // Save scene
            EditorSceneManager.SaveScene(scene);
            Debug.Log("<color=green>MainMenu scene cleaned up successfully!</color>");
        }
        else
        {
            Debug.Log("MainMenu scene is already clean - no obsolete elements found");
        }
    }
}
