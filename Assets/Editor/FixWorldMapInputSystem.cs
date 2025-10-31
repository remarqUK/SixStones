using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;

public class FixWorldMapInputSystem : Editor
{
    [MenuItem("Tools/Fix WorldMap Input System")]
    public static void FixInputSystem()
    {
        // Check if we're in the WorldMap scene
        var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (activeScene.name != "WorldMap")
        {
            Debug.LogWarning("This tool should be run in the WorldMap scene!");
            return;
        }
        
        // Fix EventSystem - replace StandaloneInputModule with InputSystemUIInputModule
        EventSystem eventSystem = GameObject.FindFirstObjectByType<EventSystem>();
        if (eventSystem != null)
        {
            // Remove old StandaloneInputModule
            StandaloneInputModule oldModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (oldModule != null)
            {
                DestroyImmediate(oldModule);
                Debug.Log("Removed StandaloneInputModule");
            }
            
            // Add new InputSystemUIInputModule
            var inputModule = eventSystem.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            if (inputModule == null)
            {
                eventSystem.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                Debug.Log("Added InputSystemUIInputModule");
            }
        }
        
        // Fix Camera - add Universal Additional Camera Data if using URP
        Camera mainCamera = GameObject.FindFirstObjectByType<Camera>();
        if (mainCamera != null)
        {
            // Check if URP is being used
            var urpCameraType = System.Type.GetType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
            if (urpCameraType != null)
            {
                var cameraData = mainCamera.GetComponent(urpCameraType);
                if (cameraData == null)
                {
                    mainCamera.gameObject.AddComponent(urpCameraType);
                    Debug.Log("Added Universal Additional Camera Data");
                }
            }
            else
            {
                Debug.Log("URP not detected - camera is fine for built-in render pipeline");
            }
        }
        
        Debug.Log("WorldMap Input System fixed!");
        
        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(activeScene);
    }
}
