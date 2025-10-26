using UnityEngine;

public class Maze3DBuilder : MonoBehaviour
{
    [Header("Map Generator")]
    public MapGenerator mapGenerator;

    [Header("3D Settings")]
    public float cellSize = 4f;
    public float wallHeight = 3f;
    public float wallThickness = 0.5f;

    [Header("Materials")]
    public Material floorMaterial;
    public Material wallMaterial;
    public Material secretRoomFloorMaterial;
    public Material startMarkerMaterial;
    public Material bossMarkerMaterial;

    [Header("Generated Objects")]
    public GameObject mazeContainer;

    public void BuildMaze3D()
    {
        if (mapGenerator == null || mapGenerator.grid == null)
        {
            Debug.LogError("MapGenerator or grid is null!");
            return;
        }

        // Clear existing maze
        if (mazeContainer != null)
            DestroyImmediate(mazeContainer);

        mazeContainer = new GameObject("Maze3D");
        mazeContainer.transform.SetParent(transform);
        mazeContainer.transform.localPosition = Vector3.zero;

        int gridWidth = mapGenerator.grid.GetLength(0);
        int gridHeight = mapGenerator.grid.GetLength(1);

        int cellsBuilt = 0;
        int wallsBuilt = 0;
        int floorsBuilt = 0;

        // Build each cell
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                MapCell cell = mapGenerator.grid[x, z];

                // Only build visited cells (maze) or secret rooms
                if (!cell.visited && !cell.isSecretRoom)
                    continue;

                cellsBuilt++;
                Vector3 cellPos = GetCell3DPosition(x, z);

                // Create floor
                CreateFloor(cellPos, cell);
                floorsBuilt++;

                // Create walls
                if (cell.wallNorth) { CreateWall(cellPos, Vector3.forward, "North"); wallsBuilt++; }
                if (cell.wallSouth) { CreateWall(cellPos, Vector3.back, "South"); wallsBuilt++; }
                if (cell.wallEast) { CreateWall(cellPos, Vector3.right, "East"); wallsBuilt++; }
                if (cell.wallWest) { CreateWall(cellPos, Vector3.left, "West"); wallsBuilt++; }

                // Create markers
                if (cell.isStart) CreateMarker(cellPos, startMarkerMaterial, "Start", Color.green);
                if (cell.isBoss) CreateMarker(cellPos, bossMarkerMaterial, "Boss", Color.red);
            }
        }

        Debug.Log($"3D Maze built: {gridWidth}x{gridHeight}, Cells: {cellsBuilt}, Floors: {floorsBuilt}, Walls: {wallsBuilt}");
        Debug.Log($"Maze center position: {transform.position}");
    }

    private Vector3 GetCell3DPosition(int x, int z)
    {
        // Center the maze
        int gridWidth = mapGenerator.grid.GetLength(0);
        int gridHeight = mapGenerator.grid.GetLength(1);

        float offsetX = -(gridWidth / 2f) * cellSize;
        float offsetZ = -(gridHeight / 2f) * cellSize;

        return new Vector3(
            x * cellSize + offsetX + cellSize / 2f,
            0,
            z * cellSize + offsetZ + cellSize / 2f
        );
    }

    private void CreateFloor(Vector3 position, MapCell cell)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = $"Floor_{cell.x}_{cell.y}";
        floor.transform.SetParent(mazeContainer.transform);
        floor.transform.position = position + Vector3.down * 0.5f;

        // Shrink floor slightly to create black border between cells
        float borderSize = 0.1f; // Size of black border
        floor.transform.localScale = new Vector3(cellSize - borderSize, 0.1f, cellSize - borderSize);

        // Use different material for secret rooms
        Material mat = cell.isSecretRoom ? secretRoomFloorMaterial : floorMaterial;
        if (mat != null)
            floor.GetComponent<Renderer>().material = mat;
        else
            floor.GetComponent<Renderer>().material.color = cell.isSecretRoom ? Color.magenta : Color.gray;

        // Add collider
        floor.GetComponent<BoxCollider>().enabled = true;
    }

    private void CreateWall(Vector3 cellCenter, Vector3 direction, string wallName)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = $"Wall_{wallName}";
        wall.transform.SetParent(mazeContainer.transform);

        // Position wall at edge of cell
        Vector3 wallPos = cellCenter + direction * (cellSize / 2f);
        wall.transform.position = wallPos + Vector3.up * (wallHeight / 2f - 0.5f);

        // Shrink walls slightly to create border gaps
        float borderSize = 0.1f;

        // Scale wall (thin in movement direction, wide perpendicular)
        if (direction == Vector3.forward || direction == Vector3.back)
        {
            wall.transform.localScale = new Vector3(cellSize - borderSize, wallHeight, wallThickness);
        }
        else
        {
            wall.transform.localScale = new Vector3(wallThickness, wallHeight, cellSize - borderSize);
        }

        // Apply material
        if (wallMaterial != null)
            wall.GetComponent<Renderer>().material = wallMaterial;
        else
            wall.GetComponent<Renderer>().material.color = Color.white;

        // Add collider
        wall.GetComponent<BoxCollider>().enabled = true;
    }

    private void CreateMarker(Vector3 position, Material material, string label, Color defaultColor)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.name = label;
        marker.transform.SetParent(mazeContainer.transform);
        marker.transform.position = position + Vector3.up * 0.5f;
        marker.transform.localScale = new Vector3(1f, 0.1f, 1f);

        if (material != null)
            marker.GetComponent<Renderer>().material = material;
        else
            marker.GetComponent<Renderer>().material.color = defaultColor;

        // Remove collider so player can walk through
        Destroy(marker.GetComponent<Collider>());
    }
}
