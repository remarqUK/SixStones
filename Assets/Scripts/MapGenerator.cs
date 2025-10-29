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
    public MapCell[,] roomGrid; // Original room-based grid (before wall expansion)
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

        // STEP 2: Place secret rooms (random locations, not on start)
        PlaceSecretRooms();

        // STEP 3: Generate maze from start (skipping secret rooms)
        RecursiveBacktrack(startPosition.x, startPosition.y);

        // STEP 4: Place boss at furthest point from start (end of longest path)
        bossPosition = FindFurthestCellFromStart();
        grid[bossPosition.x, bossPosition.y].isBoss = true;

        // STEP 5: Connect secret rooms to adjacent maze cells with buttons
        ConnectSecretRooms();

        // Spawn enemies and treasure
        SpawnEnemiesAndTreasure();

        // STEP 6: Convert to wall-as-cell grid
        roomGrid = grid; // Store original room grid
        grid = ConvertToWallGrid(roomGrid);

        // Update positions to new grid coordinates (offset by 1 for border, multiply by 2 for spacing)
        startPosition = new Vector2Int(1 + startPosition.x * 2, 1 + startPosition.y * 2);
        bossPosition = new Vector2Int(1 + bossPosition.x * 2, 1 + bossPosition.y * 2);
        for (int i = 0; i < secretRoomPositions.Count; i++)
        {
            Vector2Int oldPos = secretRoomPositions[i];
            secretRoomPositions[i] = new Vector2Int(1 + oldPos.x * 2, 1 + oldPos.y * 2);
        }

        // Log generation details
        int openPassages = CountOpenPassages(bossPosition);
        Debug.Log($"Maze generated: {width}x{height} rooms → {grid.GetLength(0)}x{grid.GetLength(1)} grid, Start: {startPosition}, Boss: {bossPosition} ({openPassages} open passage(s)), Secret Rooms: {secretRoomPositions.Count}");
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

            // Don't place on start (boss not placed yet)
            if (cell.isStart)
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
            List<(Vector2Int pos, string direction)> adjacentMazeCells = new List<(Vector2Int, string)>();

            if (secretPos.y + 1 < height) CheckAdjacentForConnection(secretPos, new Vector2Int(0, 1), "South", adjacentMazeCells);
            if (secretPos.y - 1 >= 0) CheckAdjacentForConnection(secretPos, new Vector2Int(0, -1), "North", adjacentMazeCells);
            if (secretPos.x + 1 < width) CheckAdjacentForConnection(secretPos, new Vector2Int(1, 0), "West", adjacentMazeCells);
            if (secretPos.x - 1 >= 0) CheckAdjacentForConnection(secretPos, new Vector2Int(-1, 0), "East", adjacentMazeCells);

            // Connect to a random adjacent maze cell
            if (adjacentMazeCells.Count > 0)
            {
                var (mazePos, direction) = adjacentMazeCells[Random.Range(0, adjacentMazeCells.Count)];
                MapCell mazeCell = grid[mazePos.x, mazePos.y];

                // Add secret button to the MAZE CELL's wall
                switch (direction)
                {
                    case "North": mazeCell.hasSecretButtonNorth = true; break;
                    case "South": mazeCell.hasSecretButtonSouth = true; break;
                    case "East": mazeCell.hasSecretButtonEast = true; break;
                    case "West": mazeCell.hasSecretButtonWest = true; break;
                }

                Debug.Log($"Secret room at ({secretPos.x}, {secretPos.y}) connected from maze cell ({mazePos.x}, {mazePos.y}) via {direction} wall");
            }
        }
    }

    private void CheckAdjacentForConnection(Vector2Int secretPos, Vector2Int dir, string direction, List<(Vector2Int, string)> results)
    {
        int nx = secretPos.x + dir.x;
        int ny = secretPos.y + dir.y;

        if (!IsValidCell(nx, ny))
            return;

        MapCell adjacent = grid[nx, ny];

        // Must be part of maze and not a secret room
        if (adjacent.visited && !adjacent.isSecretRoom)
        {
            results.Add((new Vector2Int(nx, ny), direction));
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

    // Find furthest cell from start using BFS (for boss placement)
    private Vector2Int FindFurthestCellFromStart()
    {
        int[,] distances = new int[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                distances[x, y] = -1;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(startPosition);
        distances[startPosition.x, startPosition.y] = 0;

        Vector2Int furthest = startPosition;
        int maxDistance = 0;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            int currentDist = distances[current.x, current.y];

            if (currentDist > maxDistance)
            {
                maxDistance = currentDist;
                furthest = current;
            }

            // Check all four directions through open passages only
            TryEnqueueForBFS(current, new Vector2Int(0, 1), distances, queue);   // North
            TryEnqueueForBFS(current, new Vector2Int(1, 0), distances, queue);   // East
            TryEnqueueForBFS(current, new Vector2Int(0, -1), distances, queue);  // South
            TryEnqueueForBFS(current, new Vector2Int(-1, 0), distances, queue);  // West
        }

        return furthest;
    }

    private void TryEnqueueForBFS(Vector2Int current, Vector2Int dir, int[,] distances, Queue<Vector2Int> queue)
    {
        int nx = current.x + dir.x;
        int ny = current.y + dir.y;

        if (!IsValidCell(nx, ny) || distances[nx, ny] != -1)
            return;

        MapCell currentCell = grid[current.x, current.y];
        MapCell neighborCell = grid[nx, ny];

        // Don't traverse into secret rooms
        if (neighborCell.isSecretRoom)
            return;

        // Check if we can move through the wall (no wall = open passage)
        bool canMove = false;
        if (dir.y == 1) canMove = currentCell.CanMoveNorth();
        else if (dir.y == -1) canMove = currentCell.CanMoveSouth();
        else if (dir.x == 1) canMove = currentCell.CanMoveEast();
        else if (dir.x == -1) canMove = currentCell.CanMoveWest();

        if (canMove)
        {
            distances[nx, ny] = distances[current.x, current.y] + 1;
            queue.Enqueue(new Vector2Int(nx, ny));
        }
    }

    // Spawn enemies and treasure in random cells
    private void SpawnEnemiesAndTreasure()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                MapCell cell = grid[x, y];

                // Don't spawn on start, boss, or secret room cells
                if (cell.isStart || cell.isBoss || cell.isSecretRoom)
                    continue;

                // Only spawn in visited cells (part of the maze)
                if (!cell.visited)
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

    // Count how many open passages a cell has (for boss placement verification)
    private int CountOpenPassages(Vector2Int pos)
    {
        // In the wall-as-cell grid, check adjacent cells to see if they're passable
        int count = 0;
        int x = pos.x;
        int y = pos.y;
        int gridWidth = grid.GetLength(0);
        int gridHeight = grid.GetLength(1);

        // Check North
        if (y + 1 < gridHeight && !grid[x, y + 1].isWall) count++;

        // Check South
        if (y - 1 >= 0 && !grid[x, y - 1].isWall) count++;

        // Check East
        if (x + 1 < gridWidth && !grid[x + 1, y].isWall) count++;

        // Check West
        if (x - 1 >= 0 && !grid[x - 1, y].isWall) count++;

        return count;
    }

    // Convert room-based grid to wall-as-cell grid
    // Imagine entire map is solid stone, then carve out the maze
    private MapCell[,] ConvertToWallGrid(MapCell[,] roomGrid)
    {
        int roomWidth = roomGrid.GetLength(0);
        int roomHeight = roomGrid.GetLength(1);

        // New grid size: rooms + walls + outer border
        // Each room takes 1 cell, walls between take 1 cell, plus border
        int newWidth = roomWidth * 2 + 1;
        int newHeight = roomHeight * 2 + 1;

        MapCell[,] newGrid = new MapCell[newWidth, newHeight];

        // STEP 1: Fill entire grid with solid stone (walls)
        for (int x = 0; x < newWidth; x++)
        {
            for (int y = 0; y < newHeight; y++)
            {
                newGrid[x, y] = new MapCell(x, y);
                newGrid[x, y].isWall = true; // Everything is stone initially
                newGrid[x, y].visited = true; // All cells are "visited" so they render
            }
        }

        // STEP 2: Carve out rooms from the stone
        for (int rx = 0; rx < roomWidth; rx++)
        {
            for (int ry = 0; ry < roomHeight; ry++)
            {
                MapCell roomCell = roomGrid[rx, ry];

                // Skip unvisited rooms (they stay as stone)
                if (!roomCell.visited && !roomCell.isSecretRoom)
                    continue;

                // Calculate position in new grid (offset by 1 for border, then skip walls)
                int nx = 1 + rx * 2;
                int ny = 1 + ry * 2;

                // Carve out this room (make it NOT a wall)
                MapCell newCell = newGrid[nx, ny];
                newCell.isWall = false; // Carve out floor
                newCell.visited = true;
                newCell.isStart = roomCell.isStart;
                newCell.isBoss = roomCell.isBoss;
                newCell.hasEnemy = roomCell.hasEnemy;
                newCell.hasTreasure = roomCell.hasTreasure;
                newCell.isSecretRoom = roomCell.isSecretRoom;

                // STEP 3: Carve out passages (where walls were removed)

                // North passage (if no wall to the north)
                if (!roomCell.wallNorth && ry < roomHeight - 1)
                {
                    MapCell northRoom = roomGrid[rx, ry + 1];
                    if (northRoom.visited || northRoom.isSecretRoom)
                    {
                        newGrid[nx, ny + 1].isWall = false; // Carve passage
                    }
                }
                // If there's a secret button on the north wall, mark the wall cell
                else if (roomCell.hasSecretButtonNorth)
                {
                    newGrid[nx, ny + 1].isButtonWall = true;
                }

                // East passage (if no wall to the east)
                if (!roomCell.wallEast && rx < roomWidth - 1)
                {
                    MapCell eastRoom = roomGrid[rx + 1, ry];
                    if (eastRoom.visited || eastRoom.isSecretRoom)
                    {
                        newGrid[nx + 1, ny].isWall = false; // Carve passage
                    }
                }
                // If there's a secret button on the east wall, mark the wall cell
                else if (roomCell.hasSecretButtonEast)
                {
                    newGrid[nx + 1, ny].isButtonWall = true;
                }

                // South passage (if no wall to the south)
                if (!roomCell.wallSouth && ry > 0)
                {
                    MapCell southRoom = roomGrid[rx, ry - 1];
                    if (southRoom.visited || southRoom.isSecretRoom)
                    {
                        newGrid[nx, ny - 1].isWall = false; // Carve passage
                    }
                }
                // If there's a secret button on the south wall, mark the wall cell
                else if (roomCell.hasSecretButtonSouth)
                {
                    newGrid[nx, ny - 1].isButtonWall = true;
                }

                // West passage (if no wall to the west)
                if (!roomCell.wallWest && rx > 0)
                {
                    MapCell westRoom = roomGrid[rx - 1, ry];
                    if (westRoom.visited || westRoom.isSecretRoom)
                    {
                        newGrid[nx - 1, ny].isWall = false; // Carve passage
                    }
                }
                // If there's a secret button on the west wall, mark the wall cell
                else if (roomCell.hasSecretButtonWest)
                {
                    newGrid[nx - 1, ny].isButtonWall = true;
                }
            }
        }

        return newGrid;
    }
}
