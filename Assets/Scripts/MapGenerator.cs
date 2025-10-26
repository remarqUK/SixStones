using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Dimensions")]
    public int width = 10;
    public int height = 10;

    [Header("Generation Settings")]
    [Range(0f, 1f)]
    public float enemySpawnChance = 0.3f;
    [Range(0f, 1f)]
    public float treasureSpawnChance = 0.1f;
    [Range(0f, 1f)]
    public float secretRoomChance = 0.033f; // ~1 in 30
    public int randomSeed = 0; // 0 = random seed

    [Header("Generated Map")]
    public MapCell[,] grid;
    public Vector2Int startPosition;
    public Vector2Int bossPosition;
    public List<Vector2Int> secretRoomPositions = new List<Vector2Int>();

    // Generate a new maze
    public void GenerateMaze()
    {
        // Initialize random seed
        if (randomSeed != 0)
            Random.InitState(randomSeed);

        // Create grid
        grid = new MapCell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new MapCell(x, y);
            }
        }

        // STEP 1: Place start position (bottom middle)
        startPosition = new Vector2Int(width / 2, 0);
        grid[startPosition.x, startPosition.y].isStart = true;

        // STEP 2: Place boss position (far from start, top area)
        bossPosition = new Vector2Int(Random.Range(1, width - 1), height - 1);
        grid[bossPosition.x, bossPosition.y].isBoss = true;

        // STEP 3: Place secret rooms (random locations, not on start/boss)
        PlaceSecretRooms();

        // STEP 4: Generate maze around these fixed points
        RecursiveBacktrack(startPosition.x, startPosition.y);

        // STEP 5: Connect secret rooms to adjacent maze cells with buttons
        ConnectSecretRooms();

        // Spawn enemies and treasure
        SpawnEnemiesAndTreasure();

        Debug.Log($"Maze generated: {width}x{height}, Start: {startPosition}, Boss: {bossPosition}, Secret Rooms: {secretRoomPositions.Count}");
    }

    // Place secret rooms before generating maze
    private void PlaceSecretRooms()
    {
        secretRoomPositions.Clear();
        int totalCells = width * height;
        int targetSecretRooms = Mathf.FloorToInt(totalCells * secretRoomChance);

        int attempts = 0;
        int maxAttempts = totalCells * 2;

        while (secretRoomPositions.Count < targetSecretRooms && attempts < maxAttempts)
        {
            attempts++;
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            MapCell cell = grid[x, y];

            // Don't place on start or boss
            if (cell.isStart || cell.isBoss)
                continue;

            // Don't place duplicate
            Vector2Int pos = new Vector2Int(x, y);
            if (secretRoomPositions.Contains(pos))
                continue;

            // Mark as secret room
            cell.isSecretRoom = true;
            cell.hasTreasure = true;
            secretRoomPositions.Add(pos);
        }
    }

    // After maze generation, connect secret rooms to adjacent cells
    private void ConnectSecretRooms()
    {
        foreach (Vector2Int secretPos in secretRoomPositions)
        {
            MapCell secretCell = grid[secretPos.x, secretPos.y];

            // Ensure secret room has all 4 walls (it's fully enclosed)
            secretCell.wallNorth = true;
            secretCell.wallSouth = true;
            secretCell.wallEast = true;
            secretCell.wallWest = true;

            // Find adjacent maze cells (visited, not secret)
            List<(Vector2Int pos, string secretWallDirection)> adjacentMazeCells = new List<(Vector2Int, string)>();

            // Check North - secret room's NORTH wall connects to maze cell above
            if (secretPos.y + 1 < height) CheckAdjacentForConnection(secretPos, new Vector2Int(0, 1), "North", adjacentMazeCells);

            // Check South - secret room's SOUTH wall connects to maze cell below
            if (secretPos.y - 1 >= 0) CheckAdjacentForConnection(secretPos, new Vector2Int(0, -1), "South", adjacentMazeCells);

            // Check East - secret room's EAST wall connects to maze cell to the right
            if (secretPos.x + 1 < width) CheckAdjacentForConnection(secretPos, new Vector2Int(1, 0), "East", adjacentMazeCells);

            // Check West - secret room's WEST wall connects to maze cell to the left
            if (secretPos.x - 1 >= 0) CheckAdjacentForConnection(secretPos, new Vector2Int(-1, 0), "West", adjacentMazeCells);

            // Connect to a random adjacent maze cell
            if (adjacentMazeCells.Count > 0)
            {
                var (mazePos, secretWallDirection) = adjacentMazeCells[Random.Range(0, adjacentMazeCells.Count)];

                // Mark which wall of the SECRET ROOM has the button
                switch (secretWallDirection)
                {
                    case "North": secretCell.hasSecretButtonNorth = true; break;
                    case "South": secretCell.hasSecretButtonSouth = true; break;
                    case "East": secretCell.hasSecretButtonEast = true; break;
                    case "West": secretCell.hasSecretButtonWest = true; break;
                }

                Debug.Log($"Secret room at ({secretPos.x}, {secretPos.y}) has button on {secretWallDirection} wall (connects to maze at {mazePos.x}, {mazePos.y})");
            }
        }
    }

    private void CheckAdjacentForConnection(Vector2Int secretPos, Vector2Int dir, string secretWallDirection, List<(Vector2Int, string)> results)
    {
        int nx = secretPos.x + dir.x;
        int ny = secretPos.y + dir.y;

        if (!IsValidCell(nx, ny))
            return;

        MapCell adjacent = grid[nx, ny];

        // Must be part of maze and not a secret room
        if (adjacent.visited && !adjacent.isSecretRoom)
        {
            results.Add((new Vector2Int(nx, ny), secretWallDirection));
        }
    }

    // Recursive backtracking maze generation (skips secret rooms)
    private void RecursiveBacktrack(int x, int y)
    {
        grid[x, y].visited = true;

        // Get all unvisited neighbors in random order
        List<Vector2Int> directions = GetShuffledDirections();

        foreach (Vector2Int dir in directions)
        {
            int nx = x + dir.x;
            int ny = y + dir.y;

            // Check if neighbor is valid and unvisited
            if (IsValidCell(nx, ny) && !grid[nx, ny].visited)
            {
                MapCell neighborCell = grid[nx, ny];

                // Skip secret rooms - they stay isolated until connected manually
                if (neighborCell.isSecretRoom)
                    continue;

                // Remove wall between current cell and neighbor
                RemoveWall(x, y, nx, ny);

                // Recursively visit neighbor
                RecursiveBacktrack(nx, ny);
            }
        }
    }

    // Remove wall between two adjacent cells
    private void RemoveWall(int x1, int y1, int x2, int y2)
    {
        int dx = x2 - x1;
        int dy = y2 - y1;

        // Remove walls based on direction
        if (dx == 1) // Moving east
        {
            grid[x1, y1].wallEast = false;
            grid[x2, y2].wallWest = false;
        }
        else if (dx == -1) // Moving west
        {
            grid[x1, y1].wallWest = false;
            grid[x2, y2].wallEast = false;
        }
        else if (dy == 1) // Moving north
        {
            grid[x1, y1].wallNorth = false;
            grid[x2, y2].wallSouth = false;
        }
        else if (dy == -1) // Moving south
        {
            grid[x1, y1].wallSouth = false;
            grid[x2, y2].wallNorth = false;
        }
    }

    // Get shuffled directions for randomized maze
    private List<Vector2Int> GetShuffledDirections()
    {
        List<Vector2Int> directions = new List<Vector2Int>
        {
            new Vector2Int(0, 1),   // North
            new Vector2Int(1, 0),   // East
            new Vector2Int(0, -1),  // South
            new Vector2Int(-1, 0)   // West
        };

        // Fisher-Yates shuffle
        for (int i = directions.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Vector2Int temp = directions[i];
            directions[i] = directions[j];
            directions[j] = temp;
        }

        return directions;
    }

    // Check if cell coordinates are valid
    private bool IsValidCell(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    // Spawn enemies and treasure in random cells
    private void SpawnEnemiesAndTreasure()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                MapCell cell = grid[x, y];

                // Don't spawn on start or boss cells
                if (cell.isStart || cell.isBoss)
                    continue;

                // Spawn enemy
                if (Random.value < enemySpawnChance)
                {
                    cell.hasEnemy = true;
                }

                // Spawn treasure (can have both enemy and treasure)
                if (Random.value < treasureSpawnChance)
                {
                    cell.hasTreasure = true;
                }
            }
        }
    }

    // Get cell at position
    public MapCell GetCell(int x, int y)
    {
        if (IsValidCell(x, y))
            return grid[x, y];
        return null;
    }

    // Get cell at Vector2Int position
    public MapCell GetCell(Vector2Int pos)
    {
        return GetCell(pos.x, pos.y);
    }
}
