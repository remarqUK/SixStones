using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Fix MainMenu camera by adding URP Additional Camera Data
/// </summary>
public class FixMainMenuCamera : EditorWindow
{
    [MenuItem("Tools/Fix MainMenu Camera")]
    public static void FixCamera()
    {
        // Open the MainMenu scene
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");

        // Find the Main Camera
        Camera cam = GameObject.FindFirstObjectByType<Camera>();

        if (cam != null)
        {
            // Check if it already has the component
            var cameraData = cam.GetComponent<UniversalAdditionalCameraData>();

            if (cameraData == null)
            {
                // Add the URP Additional Camera Data component
                cameraData = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
                Debug.Log("Added UniversalAdditionalCameraData to Main Camera");
            }
            else
            {
                Debug.Log("Main Camera already has UniversalAdditionalCameraData");
            }

            // Save the scene
            EditorSceneManager.SaveScene(scene);
            Debug.Log("MainMenu scene saved");

            EditorUtility.DisplayDialog("Camera Fixed",
                "Main Camera in MainMenu scene has been fixed!\n\n" +
                "The URP Additional Camera Data component has been added.",
                "OK");
        }
        else
        {
            Debug.LogError("Could not find Main Camera in MainMenu scene");
            EditorUtility.DisplayDialog("Error",
                "Could not find Main Camera in MainMenu scene!",
                "OK");
        }
    }
}
