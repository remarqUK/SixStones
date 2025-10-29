using UnityEngine;
using UnityEngine.InputSystem;

public class SecretDoorInteraction : MonoBehaviour
{
    public MapGenerator mapGenerator;
    public Maze3DBuilder maze3DBuilder;

    private void Start()
    {
        Debug.Log($"SecretDoorInteraction initialized on GameObject {gameObject.name}");

        // Verify collider exists
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"No collider found on {gameObject.name}!");
        }
        else
        {
            Debug.Log($"Collider found: {col.GetType().Name}, enabled: {col.enabled}");
        }
    }

    private void Update()
    {
        // Handle mouse click with new Input System
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hit;

            Debug.Log($"Mouse clicked at screen position {mousePosition}, casting ray from camera");

            if (Physics.Raycast(ray, out hit, 1000f))
            {
                Debug.Log($"Raycast hit: {hit.collider.gameObject.name} at {hit.point}");

                if (hit.collider.gameObject == gameObject)
                {
                    Debug.Log($"Clicked button wall at world position: {hit.point}");
                    ActivateSecretDoorAtPosition(hit.point);
                }
            }
            else
            {
                Debug.Log("Raycast hit nothing");
            }
        }

        // Handle touch input for mobile
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            if (Touchscreen.current.primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
            {
                Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                Ray ray = Camera.main.ScreenPointToRay(touchPosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 1000f))
                {
                    if (hit.collider.gameObject == gameObject)
                    {
                        Debug.Log($"Touched button wall at world position: {hit.point}");
                        ActivateSecretDoorAtPosition(hit.point);
                    }
                }
            }
        }
    }

    private void ActivateSecretDoorAtPosition(Vector3 worldPosition)
    {
        if (mapGenerator == null || maze3DBuilder == null)
        {
            Debug.LogError("SecretDoorInteraction: MapGenerator or Maze3DBuilder reference is missing!");
            return;
        }

        // Convert world position to grid coordinates
        Vector2Int cellPosition = maze3DBuilder.WorldToGrid(worldPosition);

        Debug.Log($"Converted world position {worldPosition} to grid position {cellPosition}");

        // Check if cell position is valid
        int gridWidth = mapGenerator.grid.GetLength(0);
        int gridHeight = mapGenerator.grid.GetLength(1);

        if (cellPosition.x < 0 || cellPosition.x >= gridWidth ||
            cellPosition.y < 0 || cellPosition.y >= gridHeight)
        {
            Debug.LogError($"Invalid cell position: {cellPosition}");
            return;
        }

        // Get the cell
        MapCell cell = mapGenerator.grid[cellPosition.x, cellPosition.y];

        // Convert button wall to floor
        if (cell.isButtonWall)
        {
            Debug.Log($"Activating secret door at {cellPosition}");

            cell.isButtonWall = false;
            cell.isWall = false;
            cell.visited = true; // Mark as visited so it appears on minimap

            // Rebuild the 3D maze to reflect the changes
            maze3DBuilder.BuildMaze3D();

            Debug.Log($"Secret door opened at {cellPosition}!");
        }
        else
        {
            Debug.LogWarning($"Clicked cell at {cellPosition} is not a button wall (isButtonWall: {cell.isButtonWall}, isWall: {cell.isWall})");
        }
    }
}
