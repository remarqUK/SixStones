using UnityEngine;

public class MapVisualizer : MonoBehaviour
{
    [Header("References")]
    public MapGenerator mapGenerator;

    [Header("Visualization Settings")]
    public float cellSize = 1f;
    public float wallThickness = 0.1f;

    [Header("Colors")]
    public Color wallCellColor = new Color(0.533f, 0.533f, 0.533f); // #888 gray for wall cells
    public Color startColor = Color.green;
    public Color bossColor = Color.red;
    public Color enemyColor = new Color(1f, 0.5f, 0f); // Orange
    public Color treasureColor = Color.yellow;
    public Color currentPositionColor = Color.cyan;
    public Color pathColor = new Color(0.3f, 0.3f, 0.3f); // Dark gray for floor cells
    public Color secretRoomColor = new Color(0.5f, 0f, 0.5f); // Purple
    public Color secretButtonColor = new Color(1f, 0f, 1f); // Magenta

    private void OnDrawGizmos()
    {
        if (mapGenerator == null || mapGenerator.grid == null)
            return;

        DrawMaze();
    }

    private void DrawMaze()
    {
        // Draw the entire grid including border cells for secret rooms
        int gridWidth = mapGenerator.grid.GetLength(0);
        int gridHeight = mapGenerator.grid.GetLength(1);

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                MapCell cell = mapGenerator.grid[x, y];

                // Only draw visited cells (main maze) or secret rooms
                if (!cell.visited && !cell.isSecretRoom)
                    continue;

                Vector3 cellCenter = GetCellWorldPosition(x, y);

                // Draw cell background (wall or floor)
                DrawCellBackground(cell, cellCenter);

                // Draw special markers (only on non-wall cells)
                if (!cell.isWall)
                {
                    DrawCellMarkers(cell, cellCenter);
                }
            }
        }
    }

    private void DrawCellBackground(MapCell cell, Vector3 center)
    {
        // Determine cell color based on type
        if (cell.isWall)
        {
            // Wall cells are gray
            Gizmos.color = wallCellColor;
        }
        else if (cell.isSecretRoom)
        {
            // Secret room floors are purple
            Gizmos.color = secretRoomColor;
        }
        else
        {
            // Regular floor cells are dark gray
            Gizmos.color = pathColor;
        }

        // Draw cell as a square
        Gizmos.DrawCube(center, new Vector3(cellSize * 0.9f, cellSize * 0.9f, 0.01f));
    }

    private void DrawCellMarkers(MapCell cell, Vector3 center)
    {
        float markerSize = cellSize * 0.4f;

        // Current position (highest priority)
        if (cell.isCurrentPosition)
        {
            Gizmos.color = currentPositionColor;
            Gizmos.DrawSphere(center, markerSize);
            return; // Don't draw other markers if current position
        }

        // Start position
        if (cell.isStart)
        {
            Gizmos.color = startColor;
            Gizmos.DrawSphere(center, markerSize);
        }
        // Boss position
        else if (cell.isBoss)
        {
            Gizmos.color = bossColor;
            Gizmos.DrawCube(center, new Vector3(markerSize, markerSize, markerSize) * 1.5f);
        }
        // Enemy
        else if (cell.hasEnemy)
        {
            Gizmos.color = enemyColor;
            Gizmos.DrawWireSphere(center, markerSize * 0.8f);
        }

        // Treasure (can overlap with enemy)
        if (cell.hasTreasure && !cell.isStart && !cell.isBoss)
        {
            Gizmos.color = treasureColor;
            DrawStar(center, markerSize * 0.6f);
        }
    }

    private void DrawStar(Vector3 center, float size)
    {
        // Draw a simple 4-pointed star
        Gizmos.DrawLine(center + Vector3.up * size, center + Vector3.down * size);
        Gizmos.DrawLine(center + Vector3.left * size, center + Vector3.right * size);
    }

    private Vector3 GetCellWorldPosition(int x, int y)
    {
        // Center the maze around the object's position using actual grid size
        int gridWidth = mapGenerator.grid.GetLength(0);
        int gridHeight = mapGenerator.grid.GetLength(1);

        float offsetX = -(gridWidth / 2f) * cellSize;
        float offsetY = -(gridHeight / 2f) * cellSize;

        return transform.position + new Vector3(
            x * cellSize + offsetX + cellSize / 2f,
            y * cellSize + offsetY + cellSize / 2f,
            0
        );
    }

    // Runtime visualization method (optional - creates actual GameObjects)
    public void CreateRuntimeVisualization()
    {
        // Clear existing children
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        if (mapGenerator == null || mapGenerator.grid == null)
            return;

        GameObject mazeParent = new GameObject("MazeVisualization");
        mazeParent.transform.SetParent(transform);
        mazeParent.transform.localPosition = Vector3.zero;

        // Create visual elements for each cell including border
        int gridWidth = mapGenerator.grid.GetLength(0);
        int gridHeight = mapGenerator.grid.GetLength(1);

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                CreateCellVisualization(x, y, mazeParent.transform);
            }
        }
    }

    private void CreateCellVisualization(int x, int y, Transform parent)
    {
        MapCell cell = mapGenerator.grid[x, y];

        // Only visualize visited cells
        if (!cell.visited)
            return;

        Vector3 pos = GetCellWorldPosition(x, y);

        // Determine cell color based on type
        Color cellColor;
        string cellName;

        if (cell.isWall)
        {
            cellColor = wallCellColor;
            cellName = $"Wall_{x}_{y}";
        }
        else if (cell.isSecretRoom)
        {
            cellColor = secretRoomColor;
            cellName = $"SecretRoom_{x}_{y}";
        }
        else
        {
            cellColor = pathColor;
            cellName = $"Floor_{x}_{y}";
        }

        // Create cell quad
        GameObject cellQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        cellQuad.name = cellName;
        cellQuad.transform.SetParent(parent);
        cellQuad.transform.position = pos;
        cellQuad.transform.localScale = new Vector3(cellSize * 0.9f, cellSize * 0.9f, 1f);
        cellQuad.GetComponent<Renderer>().material.color = cellColor;

        // Create markers (only on non-wall cells)
        if (!cell.isWall)
        {
            if (cell.isStart) CreateMarker(pos, startColor, "Start", parent);
            if (cell.isBoss) CreateMarker(pos, bossColor, "Boss", parent);
            if (cell.hasEnemy) CreateMarker(pos, enemyColor, "Enemy", parent, 0.6f);
            if (cell.hasTreasure) CreateMarker(pos, treasureColor, "Treasure", parent, 0.5f);
        }
    }

    private void CreateMarker(Vector3 position, Color color, string label, Transform parent, float sizeMultiplier = 0.4f)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.name = label;
        marker.transform.SetParent(parent);
        marker.transform.position = position;
        marker.transform.localScale = Vector3.one * cellSize * sizeMultiplier;
        marker.GetComponent<Renderer>().material.color = color;
    }
}
