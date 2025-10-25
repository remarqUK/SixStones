using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor tool to update camera background color to match side panels
/// </summary>
public class FixCameraBackground
{
    [MenuItem("Tools/Fix Camera Background Color")]
    public static void FixBackground()
    {
        Camera cam = Camera.main;
        if (cam == null) cam = GameObject.FindFirstObjectByType<Camera>();

        if (cam == null)
        {
            EditorUtility.DisplayDialog("No Camera", "No camera found in scene.", "OK");
            return;
        }

        // Set to match side panel color
        Color bgColor = new Color(0.08f, 0.08f, 0.12f);
        cam.backgroundColor = bgColor;

        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(cam.gameObject.scene);

        Debug.Log($"<color=green>Camera background updated to match side panels: RGB({bgColor.r:F2}, {bgColor.g:F2}, {bgColor.b:F2})</color>");

        EditorUtility.DisplayDialog("Background Fixed",
            "Camera background color updated to match side panels.\n\nDon't forget to save the scene!",
            "OK");
    }
}
