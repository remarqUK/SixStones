using UnityEngine;
using UnityEditor;

public class AddCameraToWorldMap : Editor
{
    [MenuItem("Tools/Add Camera to WorldMap")]
    public static void AddCamera()
    {
        // Check if we're in the WorldMap scene
        var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (activeScene.name != "WorldMap")
        {
            Debug.LogWarning("This tool should be run in the WorldMap scene!");
            return;
        }
        
        // Check if camera already exists
        Camera existingCamera = GameObject.FindFirstObjectByType<Camera>();
        if (existingCamera != null)
        {
            Debug.Log("Camera already exists in scene!");
            return;
        }
        
        // Create camera
        GameObject cameraObj = new GameObject("Main Camera");
        cameraObj.tag = "MainCamera";
        
        Camera camera = cameraObj.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f); // Match panel color
        camera.orthographic = false;
        camera.fieldOfView = 60f;
        camera.nearClipPlane = 0.3f;
        camera.farClipPlane = 1000f;
        
        // Add audio listener
        cameraObj.AddComponent<AudioListener>();
        
        // Position camera
        cameraObj.transform.position = new Vector3(0, 0, -10);
        cameraObj.transform.rotation = Quaternion.identity;
        
        Debug.Log("Camera added to WorldMap scene!");
        
        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(activeScene);
    }
}
