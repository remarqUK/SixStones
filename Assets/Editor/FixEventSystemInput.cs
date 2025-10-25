using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Editor tool to fix EventSystem using old Input system
/// Replaces StandaloneInputModule with InputSystemUIInputModule
/// </summary>
public class FixEventSystemInput
{
    [MenuItem("Tools/Fix EventSystem Input Module")]
    public static void FixEventSystem()
    {
        // Find all EventSystems in the scene
        EventSystem[] eventSystems = GameObject.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);

        if (eventSystems.Length == 0)
        {
            Debug.LogWarning("No EventSystem found in the current scene.");
            EditorUtility.DisplayDialog("No EventSystem Found",
                "No EventSystem found in the current scene.\n\nCreate one using GameObject > UI > Event System.",
                "OK");
            return;
        }

        int fixedCount = 0;

        foreach (EventSystem eventSystem in eventSystems)
        {
            GameObject obj = eventSystem.gameObject;

            // Check for old StandaloneInputModule
            StandaloneInputModule oldModule = obj.GetComponent<StandaloneInputModule>();

            if (oldModule != null)
            {
                Debug.Log($"Removing StandaloneInputModule from {obj.name}");
                Object.DestroyImmediate(oldModule);
            }

            // Check if InputSystemUIInputModule already exists
            InputSystemUIInputModule newModule = obj.GetComponent<InputSystemUIInputModule>();

            if (newModule == null)
            {
                newModule = obj.AddComponent<InputSystemUIInputModule>();
                Debug.Log($"<color=green>Added InputSystemUIInputModule to {obj.name}</color>");
                fixedCount++;

                // Mark scene as dirty
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(obj.scene);
            }
            else
            {
                Debug.Log($"EventSystem '{obj.name}' already has InputSystemUIInputModule.");
            }
        }

        if (fixedCount > 0)
        {
            Debug.Log($"<color=green>Fixed {fixedCount} EventSystem(s). Don't forget to save the scene!</color>");
            EditorUtility.DisplayDialog("EventSystem Fixed",
                $"Updated {fixedCount} EventSystem(s) to use the new Input System.\n\n" +
                "Make sure to save the scene (Ctrl+S).",
                "OK");
        }
        else
        {
            Debug.Log("All EventSystems already using InputSystemUIInputModule.");
            EditorUtility.DisplayDialog("No Changes Needed",
                "All EventSystems are already using the new Input System.",
                "OK");
        }
    }
}
