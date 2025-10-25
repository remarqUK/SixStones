using UnityEngine;
using UnityEditor;

/// <summary>
/// Diagnostic tool to check board and camera positions
/// </summary>
public class DiagnoseBoardPosition
{
    [MenuItem("Tools/Diagnose Board Position")]
    public static void Diagnose()
    {
        // Find the board
        Board board = GameObject.FindFirstObjectByType<Board>();
        if (board == null)
        {
            Debug.LogError("No Board found in scene!");
            EditorUtility.DisplayDialog("No Board", "No Board component found in scene.", "OK");
            return;
        }

        // Find the camera
        Camera cam = Camera.main;
        if (cam == null) cam = GameObject.FindFirstObjectByType<Camera>();

        if (cam == null)
        {
            Debug.LogError("No Camera found in scene!");
            EditorUtility.DisplayDialog("No Camera", "No Camera found in scene.", "OK");
            return;
        }

        // Get board info
        SerializedObject boardSO = new SerializedObject(board);
        int width = boardSO.FindProperty("width").intValue;
        int height = boardSO.FindProperty("height").intValue;
        float cellSize = boardSO.FindProperty("cellSize").floatValue;

        Vector3 boardPos = board.transform.position;
        Vector3 cameraPos = cam.transform.position;
        float orthoSize = cam.orthographicSize;

        // Calculate piece offset (Board.GridToWorld applies this)
        float offsetX = -(width - 1) * cellSize / 2f;
        float offsetY = -(height - 1) * cellSize / 2f;

        // Camera should be at origin since Board centers pieces around (0,0)
        float expectedCenterX = 0f;
        float expectedCenterY = 0f;

        // Build diagnostic message
        string message = "=== BOARD POSITION DIAGNOSTIC ===\n\n";
        message += $"Board GameObject Position: {boardPos}\n";
        message += $"Board Size: {width}x{height}, Cell Size: {cellSize}\n";
        message += $"Board.GridToWorld offset: ({offsetX:F2}, {offsetY:F2})\n";
        message += $"Pieces will be at WORLD positions: ({offsetX:F2},{offsetY:F2}) to ({width-1+offsetX:F2},{height-1+offsetY:F2})\n\n";

        message += $"Expected camera position: ({expectedCenterX}, {expectedCenterY}, -10)\n\n";

        message += $"Camera Position: {cameraPos}\n";
        message += $"Camera Orthographic Size: {orthoSize}\n\n";

        // Check if positions are correct
        bool boardOK = boardPos == Vector3.zero;
        bool cameraOK = Mathf.Approximately(cameraPos.x, expectedCenterX) &&
                        Mathf.Approximately(cameraPos.y, expectedCenterY) &&
                        Mathf.Approximately(cameraPos.z, -10f);

        if (boardOK && cameraOK)
        {
            message += "<color=green>✓ Board and Camera positions are CORRECT!</color>\n";
        }
        else
        {
            message += "<color=red>✗ Positions are INCORRECT:</color>\n";
            if (!boardOK)
            {
                message += $"  - Board should be at (0, 0, 0) but is at {boardPos}\n";
            }
            if (!cameraOK)
            {
                message += $"  - Camera should be at ({expectedCenterX}, {expectedCenterY}, -10) but is at {cameraPos}\n";
            }
            message += "\nRun 'Tools > Center Camera on Board' to fix!";
        }

        Debug.Log(message);

        EditorUtility.DisplayDialog("Board Position Diagnostic",
            $"Board: {boardPos}\n" +
            $"Piece Offset: ({offsetX:F1}, {offsetY:F1})\n" +
            $"Pieces at: ({offsetX:F1},{offsetY:F1}) to ({width-1+offsetX:F1},{height-1+offsetY:F1})\n\n" +
            $"Expected Camera: (0, 0, -10)\n" +
            $"Actual Camera: {cameraPos}\n\n" +
            (boardOK && cameraOK ? "Positions are CORRECT!" : "Positions need fixing!\n\nRun 'Tools > Center Camera on Board'"),
            "OK");
    }
}
