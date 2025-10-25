using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor tool to center the camera on the game board
/// </summary>
public class CenterCamera
{
    [MenuItem("Tools/Center Camera on Board")]
    public static void CenterCameraOnBoard()
    {
        // Find the board
        Board board = GameObject.FindFirstObjectByType<Board>();
        if (board == null)
        {
            EditorUtility.DisplayDialog("No Board Found",
                "No Board component found in the current scene.",
                "OK");
            return;
        }

        // Find the camera
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

        // Ensure board is at origin
        board.transform.position = Vector3.zero;

        // Get board properties using SerializedObject
        SerializedObject boardSO = new SerializedObject(board);
        int width = boardSO.FindProperty("width").intValue;
        int height = boardSO.FindProperty("height").intValue;
        float cellSize = boardSO.FindProperty("cellSize").floatValue;

        // Calculate center position
        // NOTE: Board.GridToWorld() applies offset to center pieces around (0,0)
        // For 8x8 board: offset = -(width-1)/2 = -3.5
        // So pieces are positioned from (-3.5,-3.5) to (3.5,3.5)
        // Therefore camera should be at (0, 0, -10)

        // Position the camera at origin
        Vector3 cameraPos = mainCamera.transform.position;
        cameraPos.x = 0f;
        cameraPos.y = 0f;
        cameraPos.z = -10f;
        mainCamera.transform.position = cameraPos;

        // Set orthographic size for 95% coverage
        float boardHeight = height * cellSize;
        float cameraHeight = boardHeight / 0.95f;
        mainCamera.orthographicSize = cameraHeight / 2f;

        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(mainCamera.gameObject.scene);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(board.gameObject.scene);

        float offsetX = -(width - 1) * cellSize / 2f;
        float offsetY = -(height - 1) * cellSize / 2f;

        Debug.Log($"<color=green>Board positioned at origin: {board.transform.position}</color>");
        Debug.Log($"<color=green>Camera centered at: (0, 0, -10)</color>");
        Debug.Log($"Board: {width}x{height}, Cell Size: {cellSize}, Ortho Size: {mainCamera.orthographicSize:F2}");
        Debug.Log($"Board.GridToWorld applies offset: ({offsetX:F2}, {offsetY:F2})");
        Debug.Log($"Pieces will be at world positions {offsetX:F2} to {width-1+offsetX:F2} (X) and {offsetY:F2} to {height-1+offsetY:F2} (Y)");

        EditorUtility.DisplayDialog("Camera Centered",
            $"Board Position: (0, 0, 0)\n" +
            $"Camera Position: (0, 0, -10)\n\n" +
            $"Board Size: {width}x{height}\n" +
            $"Piece Offset: ({offsetX:F1}, {offsetY:F1})\n" +
            $"Pieces at: ({offsetX:F1},{offsetY:F1}) to ({width-1+offsetX:F1},{height-1+offsetY:F1})\n" +
            $"Orthographic Size: {mainCamera.orthographicSize:F2}\n\n" +
            $"The grid should now be perfectly centered!\n\n" +
            "Don't forget to save the scene!",
            "OK");
    }
}
