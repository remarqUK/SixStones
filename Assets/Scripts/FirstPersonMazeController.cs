// Grid-based dungeon crawler controller
using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonMazeController : MonoBehaviour
{
    [Header("Grid Movement Settings")]
    public float transitionSpeed = 5f; // How fast to move between cells
    public float rotationSpeed = 10f; // How fast to rotate

    [Header("Camera")]
    public Camera playerCamera;
    public float cameraHeight = 1.6f;

    [Header("Maze Reference")]
    public Maze3DBuilder mazeBuilder;

    // Grid position
    private int gridX;
    private int gridZ;

    // Current facing direction (0 = North, 1 = East, 2 = South, 3 = West)
    private int facing = 0;

    // Public accessors for minimap
    public int GridX => gridX;
    public int GridZ => gridZ;
    public int Facing => facing;

    // Smooth movement state
    private bool isMoving = false;
    private Vector3 moveStartPos;
    private Vector3 moveTargetPos;
    private float moveProgress = 0f;

    // Smooth rotation state
    private bool isRotating = false;
    private Quaternion rotationStart;
    private Quaternion rotationTarget;
    private float rotationProgress = 0f;

    private Keyboard keyboard;
    private bool upKeyPressed = false;
    private bool downKeyPressed = false;
    private bool leftKeyPressed = false;
    private bool rightKeyPressed = false;

    private void Awake()
    {
        keyboard = Keyboard.current;

        // Create camera if not assigned (editor tool creates it manually)
        if (playerCamera == null)
        {
            GameObject camObj = new GameObject("PlayerCamera");
            camObj.tag = "MainCamera";
            camObj.transform.SetParent(transform);
            camObj.transform.localPosition = new Vector3(0, cameraHeight, 0);
            playerCamera = camObj.AddComponent<Camera>();

            // Add URP camera data component if available
            TryAddURPCameraData(camObj);

            Debug.Log("Camera created in Awake()");
        }
        else
        {
            Debug.Log("Camera already assigned");
        }
    }

    private void Start()
    {
        // Ensure maze is generated when entering play mode
        if (mazeBuilder != null && mazeBuilder.mapGenerator != null)
        {
            if (mazeBuilder.mapGenerator.grid == null)
            {
                Debug.Log("Generating maze on Start()...");
                mazeBuilder.mapGenerator.GenerateMaze();
                mazeBuilder.BuildMaze3D();
                MoveToStart(mazeBuilder);
            }
        }
    }

    private void TryAddURPCameraData(GameObject cameraObject)
    {
        // Try to add Universal Additional Camera Data component if URP is installed
        var cameraDataType = System.Type.GetType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
        if (cameraDataType != null)
        {
            cameraObject.AddComponent(cameraDataType);
        }
    }

    private void Update()
    {
        // Handle smooth movement
        if (isMoving)
        {
            UpdateMovement();
        }

        // Handle smooth rotation
        if (isRotating)
        {
            UpdateRotation();
        }

        // Accept input when not moving or rotating
        if (!isMoving && !isRotating)
        {
            HandleInput();
        }
    }

    private void HandleInput()
    {
        if (keyboard == null) return;

        // Check for key press (only trigger once per press)
        bool upNow = keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed;
        bool downNow = keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed;
        bool leftNow = keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed;
        bool rightNow = keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed;

        // Forward movement
        if (upNow && !upKeyPressed)
        {
            TryMoveForward();
        }
        upKeyPressed = upNow;

        // Backward movement
        if (downNow && !downKeyPressed)
        {
            TryMoveBackward();
        }
        downKeyPressed = downNow;

        // Turn left
        if (leftNow && !leftKeyPressed)
        {
            TurnLeft();
        }
        leftKeyPressed = leftNow;

        // Turn right
        if (rightNow && !rightKeyPressed)
        {
            TurnRight();
        }
        rightKeyPressed = rightNow;
    }

    private void TryMoveForward()
    {
        Vector2Int direction = GetFacingDirection();
        int targetX = gridX + direction.x;
        int targetZ = gridZ + direction.y;

        if (CanMoveTo(gridX, gridZ, targetX, targetZ))
        {
            StartMove(targetX, targetZ);
        }
    }

    private void TryMoveBackward()
    {
        Vector2Int direction = GetFacingDirection();
        int targetX = gridX - direction.x;
        int targetZ = gridZ - direction.y;

        if (CanMoveTo(gridX, gridZ, targetX, targetZ))
        {
            StartMove(targetX, targetZ);
        }
    }

    private void TurnLeft()
    {
        facing = (facing + 3) % 4; // Turn counter-clockwise
        StartRotation();
    }

    private void TurnRight()
    {
        facing = (facing + 1) % 4; // Turn clockwise
        StartRotation();
    }

    private void StartRotation()
    {
        rotationStart = transform.rotation;
        rotationTarget = Quaternion.Euler(0, facing * 90f, 0);
        rotationProgress = 0f;
        isRotating = true;
    }

    private void UpdateRotation()
    {
        rotationProgress += Time.deltaTime * rotationSpeed;

        if (rotationProgress >= 1f)
        {
            // Rotation complete
            transform.rotation = rotationTarget;
            isRotating = false;
            rotationProgress = 1f;
        }
        else
        {
            // Smooth slerp for rotation
            transform.rotation = Quaternion.Slerp(rotationStart, rotationTarget, rotationProgress);
        }
    }

    private Vector2Int GetFacingDirection()
    {
        switch (facing)
        {
            case 0: return new Vector2Int(0, 1);   // North
            case 1: return new Vector2Int(1, 0);   // East
            case 2: return new Vector2Int(0, -1);  // South
            case 3: return new Vector2Int(-1, 0);  // West
            default: return new Vector2Int(0, 1);
        }
    }

    private bool CanMoveTo(int fromX, int fromZ, int toX, int toZ)
    {
        if (mazeBuilder == null || mazeBuilder.mapGenerator == null) return false;

        // Check if grid exists
        if (mazeBuilder.mapGenerator.grid == null)
        {
            Debug.LogError("MapGenerator grid is null! Regenerating maze...");
            mazeBuilder.mapGenerator.GenerateMaze();
            if (mazeBuilder.mapGenerator.grid == null) return false;
        }

        // Check if target is in bounds
        int gridWidth = mazeBuilder.mapGenerator.grid.GetLength(0);
        int gridHeight = mazeBuilder.mapGenerator.grid.GetLength(1);

        if (toX < 0 || toX >= gridWidth || toZ < 0 || toZ >= gridHeight)
            return false;

        MapCell toCell = mazeBuilder.mapGenerator.grid[toX, toZ];

        // Target must be visited
        if (!toCell.visited)
            return false;

        // Target must NOT be a wall cell
        if (toCell.isWall)
            return false;

        // Target must not be a secret room (unless we have access)
        if (toCell.isSecretRoom)
            return false; // TODO: Check if player has opened this secret room

        return true;
    }

    private void StartMove(int targetX, int targetZ)
    {
        gridX = targetX;
        gridZ = targetZ;

        moveStartPos = transform.position;
        moveTargetPos = GetWorldPosition(targetX, targetZ);
        moveProgress = 0f;
        isMoving = true;
    }

    private void UpdateMovement()
    {
        moveProgress += Time.deltaTime * transitionSpeed;

        if (moveProgress >= 1f)
        {
            // Movement complete
            transform.position = moveTargetPos;
            isMoving = false;
            moveProgress = 1f;
        }
        else
        {
            // Smooth lerp
            transform.position = Vector3.Lerp(moveStartPos, moveTargetPos, moveProgress);
        }
    }

    private Vector3 GetWorldPosition(int x, int z)
    {
        if (mazeBuilder == null) return Vector3.zero;

        int gridWidth = mazeBuilder.mapGenerator.grid.GetLength(0);
        int gridHeight = mazeBuilder.mapGenerator.grid.GetLength(1);

        float offsetX = -(gridWidth / 2f) * mazeBuilder.cellSize;
        float offsetZ = -(gridHeight / 2f) * mazeBuilder.cellSize;

        return mazeBuilder.transform.position + new Vector3(
            x * mazeBuilder.cellSize + offsetX + mazeBuilder.cellSize / 2f,
            0,
            z * mazeBuilder.cellSize + offsetZ + mazeBuilder.cellSize / 2f
        );
    }

    // Position player at start position from map generator
    public void MoveToStart(Maze3DBuilder builder)
    {
        if (builder == null || builder.mapGenerator == null) return;

        this.mazeBuilder = builder;
        Vector2Int start = builder.mapGenerator.startPosition;

        // Set grid position
        gridX = start.x;
        gridZ = start.y;

        // Set world position
        transform.position = GetWorldPosition(gridX, gridZ);

        // Choose a facing direction based on open walls
        facing = ChooseBestFacingDirection(start.x, start.y);
        transform.rotation = Quaternion.Euler(0, facing * 90f, 0);
        isRotating = false; // Make sure we're not rotating

        Debug.Log($"Player positioned at grid ({start.x}, {start.y}), world position: {transform.position}, facing: {GetDirectionName(facing)}");
        Debug.Log($"Camera position: {playerCamera.transform.position}, Camera rotation: {playerCamera.transform.rotation.eulerAngles}");
    }

    private int ChooseBestFacingDirection(int x, int z)
    {
        if (mazeBuilder == null || mazeBuilder.mapGenerator == null || mazeBuilder.mapGenerator.grid == null)
            return 0; // Default to north if no grid

        // Check each direction by attempting to move in that direction
        // facing: 0 = North (+z), 1 = East (+x), 2 = South (-z), 3 = West (-x)

        // Try North (0, +1)
        if (CanMoveTo(x, z, x, z + 1))
        {
            Debug.Log($"Start cell ({x},{z}) can move NORTH - facing that way");
            return 0;
        }

        // Try East (+1, 0)
        if (CanMoveTo(x, z, x + 1, z))
        {
            Debug.Log($"Start cell ({x},{z}) can move EAST - facing that way");
            return 1;
        }

        // Try South (0, -1)
        if (CanMoveTo(x, z, x, z - 1))
        {
            Debug.Log($"Start cell ({x},{z}) can move SOUTH - facing that way");
            return 2;
        }

        // Try West (-1, 0)
        if (CanMoveTo(x, z, x - 1, z))
        {
            Debug.Log($"Start cell ({x},{z}) can move WEST - facing that way");
            return 3;
        }

        // If all directions are blocked (shouldn't happen), default to north
        Debug.LogWarning($"Start cell ({x},{z}) has no open directions! Defaulting to North");
        return 0;
    }

    private string GetDirectionName(int facing)
    {
        switch (facing)
        {
            case 0: return "North";
            case 1: return "East";
            case 2: return "South";
            case 3: return "West";
            default: return "Unknown";
        }
    }
}
