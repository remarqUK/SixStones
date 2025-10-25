using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Editor tool to fix cameras missing URP Additional Camera Data component
/// </summary>
public class FixCameraURP
{
    [MenuItem("Tools/Fix Camera URP Components")]
    public static void FixAllCameras()
    {
        // Find all cameras in the scene (Unity 6+ method)
        Camera[] cameras = GameObject.FindObjectsByType<Camera>(FindObjectsSortMode.None);

        if (cameras.Length == 0)
        {
            Debug.LogWarning("No cameras found in the current scene.");
            return;
        }

        int fixedCount = 0;

        foreach (Camera camera in cameras)
        {
            // Check if it already has the component
            UniversalAdditionalCameraData cameraData = camera.GetComponent<UniversalAdditionalCameraData>();

            if (cameraData == null)
            {
                // Add the component
                cameraData = camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
                cameraData.renderType = CameraRenderType.Base;
                cameraData.renderShadows = false; // 2D game doesn't need shadows
                cameraData.requiresColorOption = CameraOverrideOption.Off;
                cameraData.requiresDepthOption = CameraOverrideOption.Off;

                Debug.Log($"Added URP Additional Camera Data to: {camera.gameObject.name}");
                fixedCount++;

                // Mark scene as dirty so Unity knows to save changes
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(camera.gameObject.scene);
            }
            else
            {
                Debug.Log($"Camera '{camera.gameObject.name}' already has URP Additional Camera Data.");
            }
        }

        if (fixedCount > 0)
        {
            Debug.Log($"<color=green>Fixed {fixedCount} camera(s). Don't forget to save the scene!</color>");
            EditorUtility.DisplayDialog("Cameras Fixed",
                $"Added URP Additional Camera Data to {fixedCount} camera(s).\n\nMake sure to save the scene (Ctrl+S).",
                "OK");
        }
        else
        {
            Debug.Log("All cameras already have URP Additional Camera Data.");
            EditorUtility.DisplayDialog("No Changes Needed",
                "All cameras already have URP Additional Camera Data.",
                "OK");
        }
    }
}
