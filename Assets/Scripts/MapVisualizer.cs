using UnityEngine;

public class MapVisualizer : MonoBehaviour
{
    [Header("References")]
    public MapGenerator mapGenerator;

    [Header("Visualization Settings")]
    public float cellSize = 1f;
    public float wallThickness = 0.1f;

    [Header("Colors")]
    public Color wallColor = Color.white;
    public Color startColor = Color.green;
    public Color bossColor = Color.red;
    public Color enemyColor = new Color(1f, 0.5f, 0f); // Orange
    public Color treasureColor = Color.yellow;
    public Color currentPositionColor = Color.cyan;
    public Color pathColor = new Color(0.3f, 0.3f, 0.3f); // Dark gray
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

                // Draw cell background
                DrawCellBackground(cell, cellCenter);

                // Draw walls
                DrawCellWalls(cell, cellCenter);

                // Draw special markers
                DrawCellMarkers(cell, cellCenter);
            }
        }
    }

    private void DrawCellBackground(MapCell cell, Vector3 center)
    {
        // Draw floor/path - secret rooms are purple
        if (cell.isSecretRoom)
            Gizmos.color = secretRoomColor;
        else
            Gizmos.color = pathColor;

        Gizmos.DrawCube(center, new Vector3(cellSize * 0.9f, cellSize * 0.9f, 0.01f));
    }

    private void DrawCellWalls(MapCell cell, Vector3 center)
    {
        float halfSize = cellSize / 2f;
        float buttonSize = cellSize * 0.15f;

        // North wall
        if (cell.wallNorth)
        {
            Gizmos.color = wallColor;
            Vector3 wallCenter = center + new Vector3(0, halfSize, 0);
            Gizmos.DrawCube(wallCenter, new Vector3(cellSize, wallThickness, wallThickness));

            // Draw secret button if present
            if (cell.hasSecretButtonNorth)
            {
                Gizmos.color = secretButtonColor;
                Gizmos.DrawWireSphere(wallCenter, buttonSize);
            }
        }

        // South wall
        if (cell.wallSouth)
        {
            Gizmos.color = wallColor;
            Vector3 wallCenter = center + new Vector3(0, -halfSize, 0);
            Gizmos.DrawCube(wallCenter, new Vector3(cellSize, wallThickness, wallThickness));

            if (cell.hasSecretButtonSouth)
            {
                Gizmos.color = secretButtonColor;
                Gizmos.DrawWireSphere(wallCenter, buttonSize);
            }
        }

        // East wall
        if (cell.wallEast)
        {
            Gizmos.color = wallColor;
            Vector3 wallCenter = center + new Vector3(halfSize, 0, 0);
            Gizmos.DrawCube(wallCenter, new Vector3(wallThickness, cellSize, wallThickness));

            if (cell.hasSecretButtonEast)
            {
                Gizmos.color = secretButtonColor;
                Gizmos.DrawWireSphere(wallCenter, buttonSize);
            }
        }

        // West wall
        if (cell.wallWest)
        {
            Gizmos.color = wallColor;
            Vector3 wallCenter = center + new Vector3(-halfSize, 0, 0);
            Gizmos.DrawCube(wallCenter, new Vector3(wallThickness, cellSize, wallThickness));

            if (cell.hasSecretButtonWest)
            {
                Gizmos.color = secretButtonColor;
                Gizmos.DrawWireSphere(wallCenter, buttonSize);
            }
        }
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

        // Only visualize visited cells (main maze) or secret rooms
        if (!cell.visited && !cell.isSecretRoom)
            return;

        Vector3 pos = GetCellWorldPosition(x, y);

        // Create cell floor - purple for secret rooms
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Quad);
        floor.name = $"Cell_{x}_{y}";
        floor.transform.SetParent(parent);
        floor.transform.position = pos;
        floor.transform.localScale = new Vector3(cellSize * 0.9f, cellSize * 0.9f, 1f);
        floor.GetComponent<Renderer>().material.color = cell.isSecretRoom ? secretRoomColor : pathColor;

        // Create walls as needed
        if (cell.wallNorth) CreateWall(pos + Vector3.up * cellSize / 2f, new Vector3(cellSize, wallThickness, wallThickness), parent);
        if (cell.wallSouth) CreateWall(pos + Vector3.down * cellSize / 2f, new Vector3(cellSize, wallThickness, wallThickness), parent);
        if (cell.wallEast) CreateWall(pos + Vector3.right * cellSize / 2f, new Vector3(wallThickness, cellSize, wallThickness), parent);
        if (cell.wallWest) CreateWall(pos + Vector3.left * cellSize / 2f, new Vector3(wallThickness, cellSize, wallThickness), parent);

        // Create markers
        if (cell.isStart) CreateMarker(pos, startColor, "Start", parent);
        if (cell.isBoss) CreateMarker(pos, bossColor, "Boss", parent);
        if (cell.hasEnemy) CreateMarker(pos, enemyColor, "Enemy", parent, 0.6f);
        if (cell.hasTreasure) CreateMarker(pos, treasureColor, "Treasure", parent, 0.5f);
    }

    private void CreateWall(Vector3 position, Vector3 scale, Transform parent)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.SetParent(parent);
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.GetComponent<Renderer>().material.color = wallColor;
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
