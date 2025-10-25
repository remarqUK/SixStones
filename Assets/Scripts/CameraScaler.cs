using UnityEngine;

/// <summary>
/// Automatically scales the camera's orthographic size based on screen dimensions
/// Makes the board square and occupy 95% of screen height
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraScaler : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField] private int boardWidth = 8;
    [SerializeField] private int boardHeight = 8;
    [SerializeField] private float cellSize = 1f;

    [Header("Screen Coverage")]
    [Tooltip("Percentage of screen height the board should occupy (0-1)")]
    [SerializeField] [Range(0.5f, 1f)] private float boardHeightPercent = 0.95f;

    private Camera cam;
    private float lastAspect;
    private Vector2 lastScreenSize;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        UpdateCameraSize();
    }

    private void Update()
    {
        // Check if screen size or aspect ratio changed
        Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);
        if (currentScreenSize != lastScreenSize)
        {
            UpdateCameraSize();
            lastScreenSize = currentScreenSize;
        }
    }

    private void UpdateCameraSize()
    {
        if (cam == null) return;

        // Calculate board dimensions in world units (always square)
        float boardWorldHeight = boardHeight * cellSize;
        float boardWorldWidth = boardWidth * cellSize;

        // Board should occupy boardHeightPercent of screen height
        // orthographicSize is half the camera's vertical view
        // So if board is 8 units tall and should be 95% of screen:
        // cameraHeight = boardHeight / boardHeightPercent
        // orthographicSize = cameraHeight / 2
        float cameraHeight = boardWorldHeight / boardHeightPercent;
        cam.orthographicSize = cameraHeight / 2f;

        // Center the camera on the board
        // NOTE: Board.GridToWorld() applies offsets to center pieces around origin!
        // For 8x8 board: offset = -(7/2) = -3.5, so pieces are at (-3.5,-3.5) to (3.5,3.5)
        // Therefore camera should be at (0, 0, -10) not (3.5, 3.5, -10)
        Vector3 newPos = transform.position;
        newPos.x = 0f;
        newPos.y = 0f;
        newPos.z = -10f;
        transform.position = newPos;

        Debug.Log($"Camera centered at origin: pos=(0, 0, -10), orthoSize={cam.orthographicSize:F2}, boardSize={boardWorldWidth}x{boardWorldHeight}");
    }

    /// <summary>
    /// Update camera when board size changes
    /// </summary>
    public void SetBoardSize(int width, int height, float size)
    {
        boardWidth = width;
        boardHeight = height;
        cellSize = size;
        UpdateCameraSize();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Update in editor when values change
        if (Application.isPlaying && cam != null)
        {
            UpdateCameraSize();
        }
    }
#endif
}
