using UnityEngine;
using System.Collections.Generic;

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
    public Material buttonWallMaterial;
    public Material secretRoomFloorMaterial;
    public Material startMarkerMaterial;
    public Material bossMarkerMaterial;
    public Material treasureMarkerMaterial;
    public Material enemyMarkerMaterial;

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

        // Lists to store vertex data for floors and walls
        List<Vector3> floorVertices = new List<Vector3>();
        List<int> floorTriangles = new List<int>();
        List<Vector2> floorUVs = new List<Vector2>();

        List<Vector3> secretFloorVertices = new List<Vector3>();
        List<int> secretFloorTriangles = new List<int>();
        List<Vector2> secretFloorUVs = new List<Vector2>();

        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();
        List<Vector2> wallUVs = new List<Vector2>();

        List<Vector3> buttonWallVertices = new List<Vector3>();
        List<int> buttonWallTriangles = new List<int>();
        List<Vector2> buttonWallUVs = new List<Vector2>();

        int cellsBuilt = 0;
        int wallsBuilt = 0;
        int buttonWallsBuilt = 0;
        int floorsBuilt = 0;

        // Build each cell by adding vertices to lists
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                MapCell cell = mapGenerator.grid[x, z];

                // Skip unvisited cells
                if (!cell.visited)
                    continue;

                cellsBuilt++;
                Vector3 cellPos = GetCell3DPosition(x, z);

                // Build based on cell type
                if (cell.isWall)
                {
                    // Check if this is a button wall
                    if (cell.isButtonWall)
                    {
                        AddWallToMesh(cellPos, cell, buttonWallVertices, buttonWallTriangles, buttonWallUVs);
                        buttonWallsBuilt++;
                    }
                    else
                    {
                        AddWallToMesh(cellPos, cell, wallVertices, wallTriangles, wallUVs);
                        wallsBuilt++;
                    }
                }
                else
                {
                    // Add floor vertices to appropriate mesh
                    if (cell.isSecretRoom)
                        AddFloorToMesh(cellPos, cell, secretFloorVertices, secretFloorTriangles, secretFloorUVs);
                    else
                        AddFloorToMesh(cellPos, cell, floorVertices, floorTriangles, floorUVs);
                    floorsBuilt++;

                    // Create markers (kept as separate objects for clarity)
                    if (cell.isStart) CreateMarker(cellPos, startMarkerMaterial, "Start", Color.green);
                    if (cell.isBoss) CreateMarker(cellPos, bossMarkerMaterial, "Boss", Color.red);
                    if (cell.hasTreasure) CreateMarker(cellPos, treasureMarkerMaterial, "Treasure", Color.yellow);
                    if (cell.hasEnemy) CreateMarker(cellPos, enemyMarkerMaterial, "Enemy", new Color(1f, 0.5f, 0f)); // Orange
                }
            }
        }

        // Create mesh objects
        CreateMeshObject("Floors", floorVertices, floorTriangles, floorUVs, floorMaterial, true);
        if (secretFloorVertices.Count > 0)
            CreateMeshObject("SecretFloors", secretFloorVertices, secretFloorTriangles, secretFloorUVs, secretRoomFloorMaterial, true);
        CreateMeshObject("Walls", wallVertices, wallTriangles, wallUVs, wallMaterial, true);
        if (buttonWallVertices.Count > 0)
        {
            GameObject buttonWallsObj = CreateMeshObject("ButtonWalls", buttonWallVertices, buttonWallTriangles, buttonWallUVs, buttonWallMaterial, true);
            // Add interaction script to button walls
            if (buttonWallsObj != null)
            {
                SecretDoorInteraction interaction = buttonWallsObj.AddComponent<SecretDoorInteraction>();
                interaction.mapGenerator = mapGenerator;
                interaction.maze3DBuilder = this;
            }
        }

        Debug.Log($"3D Maze built: {gridWidth}x{gridHeight}, Cells: {cellsBuilt}, Floors: {floorsBuilt}, Walls: {wallsBuilt}, Button Walls: {buttonWallsBuilt}");
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

    // Public method to convert world position to grid coordinates
    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        if (mapGenerator == null || mapGenerator.grid == null)
            return new Vector2Int(-1, -1);

        int gridWidth = mapGenerator.grid.GetLength(0);
        int gridHeight = mapGenerator.grid.GetLength(1);

        float offsetX = -(gridWidth / 2f) * cellSize;
        float offsetZ = -(gridHeight / 2f) * cellSize;

        // Convert world position back to grid coordinates
        int x = Mathf.RoundToInt((worldPosition.x - offsetX - cellSize / 2f) / cellSize);
        int z = Mathf.RoundToInt((worldPosition.z - offsetZ - cellSize / 2f) / cellSize);

        // Clamp to grid bounds
        x = Mathf.Clamp(x, 0, gridWidth - 1);
        z = Mathf.Clamp(z, 0, gridHeight - 1);

        return new Vector2Int(x, z);
    }

    private void AddFloorToMesh(Vector3 position, MapCell cell, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        // Use full cell size for floors
        float halfSize = cellSize / 2f;
        float floorY = -0.5f; // Floor height

        // Calculate world-space vertex positions
        Vector3 center = position + Vector3.up * floorY;
        int startIndex = vertices.Count;

        // Add 4 vertices for the floor quad (top face of a thin cube)
        vertices.Add(center + new Vector3(-halfSize, 0, -halfSize)); // Bottom-left
        vertices.Add(center + new Vector3(halfSize, 0, -halfSize));  // Bottom-right
        vertices.Add(center + new Vector3(halfSize, 0, halfSize));   // Top-right
        vertices.Add(center + new Vector3(-halfSize, 0, halfSize));  // Top-left

        // Add triangles (two triangles make a quad)
        triangles.Add(startIndex + 0);
        triangles.Add(startIndex + 2);
        triangles.Add(startIndex + 1);

        triangles.Add(startIndex + 0);
        triangles.Add(startIndex + 3);
        triangles.Add(startIndex + 2);

        // Add UVs for texture mapping
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(0, 1));
    }

    private void AddWallToMesh(Vector3 position, MapCell cell, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        // Use full cell size for walls
        float halfSize = cellSize / 2f;
        float halfHeight = wallHeight / 2f;
        float wallY = halfHeight - 0.5f; // Center the wall vertically

        // Calculate world-space center position
        Vector3 center = position + Vector3.up * wallY;
        int startIndex = vertices.Count;

        // Add 8 vertices for a cube (world-space positions)
        // Bottom vertices
        vertices.Add(center + new Vector3(-halfSize, -halfHeight, -halfSize)); // 0
        vertices.Add(center + new Vector3(halfSize, -halfHeight, -halfSize));  // 1
        vertices.Add(center + new Vector3(halfSize, -halfHeight, halfSize));   // 2
        vertices.Add(center + new Vector3(-halfSize, -halfHeight, halfSize));  // 3

        // Top vertices
        vertices.Add(center + new Vector3(-halfSize, halfHeight, -halfSize)); // 4
        vertices.Add(center + new Vector3(halfSize, halfHeight, -halfSize));  // 5
        vertices.Add(center + new Vector3(halfSize, halfHeight, halfSize));   // 6
        vertices.Add(center + new Vector3(-halfSize, halfHeight, halfSize));  // 7

        // Add triangles for all 6 faces
        // Front face
        AddQuad(triangles, startIndex + 0, startIndex + 1, startIndex + 5, startIndex + 4);
        // Back face
        AddQuad(triangles, startIndex + 2, startIndex + 3, startIndex + 7, startIndex + 6);
        // Left face
        AddQuad(triangles, startIndex + 3, startIndex + 0, startIndex + 4, startIndex + 7);
        // Right face
        AddQuad(triangles, startIndex + 1, startIndex + 2, startIndex + 6, startIndex + 5);
        // Top face
        AddQuad(triangles, startIndex + 4, startIndex + 5, startIndex + 6, startIndex + 7);
        // Bottom face
        AddQuad(triangles, startIndex + 3, startIndex + 2, startIndex + 1, startIndex + 0);

        // Add UVs for all 8 vertices (simple mapping)
        for (int i = 0; i < 8; i++)
        {
            uvs.Add(new Vector2(i % 2, i / 4));
        }
    }

    private void AddQuad(List<int> triangles, int v0, int v1, int v2, int v3)
    {
        // First triangle
        triangles.Add(v0);
        triangles.Add(v2);
        triangles.Add(v1);

        // Second triangle
        triangles.Add(v0);
        triangles.Add(v3);
        triangles.Add(v2);
    }

    private GameObject CreateMeshObject(string name, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, Material material, bool addCollider = true)
    {
        if (vertices.Count == 0)
            return null;

        GameObject meshObject = new GameObject(name);
        meshObject.transform.SetParent(mazeContainer.transform);
        meshObject.transform.localPosition = Vector3.zero;

        Mesh mesh = new Mesh();
        mesh.name = name + "Mesh";
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
        if (material != null)
            meshRenderer.material = material;
        else
            meshRenderer.material = new Material(Shader.Find("Standard"));

        // Add mesh collider for physics (optional)
        if (addCollider)
        {
            MeshCollider meshCollider = meshObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
        }

        return meshObject;
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
