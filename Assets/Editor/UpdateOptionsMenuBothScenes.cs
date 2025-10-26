using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Updates options menu in both MainMenu and Match3 scenes
/// Can be called from command line or Unity Editor
/// </summary>
public class UpdateOptionsMenuBothScenes : EditorWindow
{
    [MenuItem("Tools/Update Options Menu (Both Scenes)")]
    public static void UpdateBothScenes()
    {
        Debug.Log("Updating options menu in both MainMenu and Match3 scenes...");

        // Update MainMenu scene (no pause)
        UpdateScene("Assets/Scenes/MainMenu.unity", false);

        // Update Match3 scene (with pause)
        UpdateScene("Assets/Scenes/Match3.unity", true);

        Debug.Log("<color=green>Options menu updated in both scenes!</color>");
    }

    /// <summary>
    /// Command-line method for batch mode
    /// </summary>
    public static void UpdateOptionsMenuCommandLine()
    {
        UpdateBothScenes();
    }

    private static void UpdateScene(string scenePath, bool pauseWhenOpen)
    {
        // Open the scene
        var scene = EditorSceneManager.OpenScene(scenePath);

        // Find existing Canvas
        Canvas canvas = GameObject.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError($"No Canvas found in {scenePath}!");
            return;
        }

        // Find OptionsMenuController
        OptionsMenuController controller = GameObject.FindFirstObjectByType<OptionsMenuController>();
        if (controller == null)
        {
            Debug.LogWarning($"No OptionsMenuController found in {scenePath}. Run Tools > Create Options Menu UI first.");
            return;
        }

        Debug.Log($"Found OptionsMenuController in {scenePath}. Updating...");

        // Update pauseTimeWhenOpen setting
        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("pauseTimeWhenOpen").boolValue = pauseWhenOpen;
        so.ApplyModifiedProperties();

        // Ensure GlobalOptionsAccess component exists
        // Suppress obsolete warning - this script is for backward compatibility with old system
#pragma warning disable CS0618
        GlobalOptionsAccess globalAccess = controller.GetComponent<GlobalOptionsAccess>();
        if (globalAccess == null)
        {
            Debug.Log($"Adding GlobalOptionsAccess component to {scenePath}");
            globalAccess = controller.gameObject.AddComponent<GlobalOptionsAccess>();
        }

        // Wire up the reference
        SerializedObject globalSO = new SerializedObject(globalAccess);
        globalSO.FindProperty("optionsMenuController").objectReferenceValue = controller;
        globalSO.FindProperty("enableGlobalHotkey").boolValue = true;
        globalSO.ApplyModifiedProperties();
#pragma warning restore CS0618

        // Save scene
        EditorSceneManager.SaveScene(scene);

        Debug.Log($"<color=green>Updated {scenePath} - Pause: {pauseWhenOpen}, Global hotkey: Tab</color>");
    }
}
