using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor tool to add CameraScaler to the main camera in existing scenes
/// </summary>
public class AddCameraScaler
{
    [MenuItem("Tools/Add Camera Scaler")]
    public static void AddScalerToCamera()
    {
        // Find main camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = GameObject.FindFirstObjectByType<Camera>();
        }

        if (mainCamera == null)
        {
            EditorUtility.DisplayDialog("No Camera Found",
                "No camera found in the current scene.",
                "OK");
            return;
        }

        // Check if it already has CameraScaler
        CameraScaler existing = mainCamera.GetComponent<CameraScaler>();
        if (existing != null)
        {
            EditorUtility.DisplayDialog("Already Has Scaler",
                $"Camera '{mainCamera.gameObject.name}' already has a CameraScaler component.",
                "OK");
            return;
        }

        // Add the component
        CameraScaler scaler = mainCamera.gameObject.AddComponent<CameraScaler>();

        // Configure with default values for 8x8 board at 95% screen height
        SerializedObject scalerSO = new SerializedObject(scaler);
        scalerSO.FindProperty("boardWidth").intValue = 8;
        scalerSO.FindProperty("boardHeight").intValue = 8;
        scalerSO.FindProperty("cellSize").floatValue = 1f;
        scalerSO.FindProperty("boardHeightPercent").floatValue = 0.95f;
        scalerSO.ApplyModifiedProperties();

        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(mainCamera.gameObject.scene);

        Debug.Log($"<color=green>Added CameraScaler to {mainCamera.gameObject.name}</color>");
        EditorUtility.DisplayDialog("Camera Scaler Added",
            $"Added CameraScaler to '{mainCamera.gameObject.name}'.\n\n" +
            "The camera will now automatically scale to fit the board on any screen size.\n\n" +
            "Don't forget to save the scene!",
            "OK");
    }
}
