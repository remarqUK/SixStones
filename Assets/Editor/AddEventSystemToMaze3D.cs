using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Adds an EventSystem to Maze3D scene if it's missing
/// EventSystem is required for UI button clicks to work
/// Uses InputSystemUIInputModule for new Input System compatibility
/// </summary>
public class AddEventSystemToMaze3D : EditorWindow
{
    [MenuItem("Tools/Fix Maze3D Event System")]
    public static void FixEventSystem()
    {
        // Save current scene state
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        Scene originalScene = SceneManager.GetActiveScene();

        // Open Maze3D scene
        Scene maze3DScene = EditorSceneManager.OpenScene("Assets/Scenes/Maze3D.unity", OpenSceneMode.Single);

        // Check if EventSystem already exists
        EventSystem existingEventSystem = Object.FindFirstObjectByType<EventSystem>();

        if (existingEventSystem != null)
        {
            Debug.Log("<color=yellow>EventSystem already exists - checking input module...</color>");

            // Check if it has the correct input module
            var oldModule = existingEventSystem.GetComponent<StandaloneInputModule>();
            var newModule = existingEventSystem.GetComponent<InputSystemUIInputModule>();

            if (oldModule != null)
            {
                Debug.Log("Removing old StandaloneInputModule and adding InputSystemUIInputModule");
                Object.DestroyImmediate(oldModule);
                existingEventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                EditorSceneManager.SaveScene(maze3DScene);
                Debug.Log("<color=green>Updated EventSystem to use new Input System!</color>");
            }
            else if (newModule == null)
            {
                Debug.Log("Adding InputSystemUIInputModule");
                existingEventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                EditorSceneManager.SaveScene(maze3DScene);
                Debug.Log("<color=green>Added InputSystemUIInputModule!</color>");
            }
            else
            {
                Debug.Log("<color=green>EventSystem already has correct InputSystemUIInputModule</color>");
            }
        }
        else
        {
            // Create EventSystem with new Input System module
            GameObject eventSystemObj = new GameObject("EventSystem");
            EventSystem eventSystem = eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<InputSystemUIInputModule>();

            Debug.Log("<color=green>EventSystem with InputSystemUIInputModule added to Maze3D scene!</color>");

            // Save the scene
            EditorSceneManager.SaveScene(maze3DScene);
            Debug.Log("Maze3D scene saved");
        }

        // Restore original scene
        if (!string.IsNullOrEmpty(originalScene.path))
        {
            EditorSceneManager.OpenScene(originalScene.path);
        }

        Debug.Log("<color=green>Done! UI buttons in Maze3D should now be clickable.</color>");
    }
}
