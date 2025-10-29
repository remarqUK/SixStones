using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MinimapRenderer : MonoBehaviour
{
    [Header("References")]
    public MapGenerator mapGenerator;
    public FirstPersonMazeController player;

    [Header("Minimap Settings")]
    public int cellPixelSize = 5; // Size of each cell in pixels (smaller since grid is larger now)
    public Color visitedCellColor = new Color(0.3f, 0.3f, 0.3f); // Dark gray
    public Color currentCellColor = Color.cyan;
    public Color wallCellColor = new Color(0.533f, 0.533f, 0.533f); // #888 gray for wall cells
    public Color buttonWallColor = new Color(1f, 0.5f, 0f); // Orange for button walls
    public Color unvisitedColor = Color.black;
    public Color wallBorderColor = Color.yellow; // Yellow border for walls adjacent to visited path
    public Color lookAheadColor = new Color(0.5f, 0.5f, 0.7f); // Light blue-gray for cells ahead

    [Header("UI References")]
    public RawImage minimapImage;
    public RectTransform minimapContainer;

    private Texture2D minimapTexture;
    private HashSet<Vector2Int> visitedCells = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> revealedCells = new HashSet<Vector2Int>(); // Cells revealed by looking ahead
    private Vector2Int lastPlayerPosition;
    private int lastPlayerFacing;
    private bool initialized = false;

    private void Start()
    {
        if (mapGenerator == null || player == null)
        {
            Debug.LogError("MinimapRenderer requires MapGenerator and Player references!");
            return;
        }

        // Don't initialize yet - wait for grid to be ready
    }

    private void Update()
    {
        // Initialize on first frame when grid is ready
        if (!initialized && mapGenerator != null && mapGenerator.grid != null)
        {
            InitializeMinimap();
            UpdateMinimap();
            initialized = true;
            Debug.Log("Minimap initialized in Update()");
        }

        if (!initialized)
            return;

        // Check if player position or facing changed
        Vector2Int currentPos = new Vector2Int(player.GridX, player.GridZ);
        int currentFacing = player.Facing;

        if (currentPos != lastPlayerPosition || currentFacing != lastPlayerFacing)
        {
            lastPlayerPosition = currentPos;
            lastPlayerFacing = currentFacing;

            // Mark current cell as visited
            visitedCells.Add(currentPos);

            UpdateMinimap();
        }
    }

    private void InitializeMinimap()
    {
        if (mapGenerator.grid == null)
        {
            Debug.LogError("MapGenerator grid is null!");
            return;
        }

        // Use actual grid dimensions (not room dimensions)
        int width = mapGenerator.grid.GetLength(0);
        int height = mapGenerator.grid.GetLength(1);

        // Create texture
        minimapTexture = new Texture2D(width * cellPixelSize, height * cellPixelSize);
        minimapTexture.filterMode = FilterMode.Point; // Crisp pixels
        minimapTexture.wrapMode = TextureWrapMode.Clamp;

        // Assign to RawImage
        if (minimapImage != null)
        {
            minimapImage.texture = minimapTexture;
        }

        // Mark starting position as visited
        visitedCells.Add(mapGenerator.startPosition);

        Debug.Log($"Minimap initialized: {width}x{height} grid (expected 21x21), texture: {width * cellPixelSize}x{height * cellPixelSize} pixels, start position: {mapGenerator.startPosition}");
    }

    private void UpdateMinimap()
    {
        if (mapGenerator == null || mapGenerator.grid == null || minimapTexture == null)
            return;

        // Use actual grid dimensions
        int width = mapGenerator.grid.GetLength(0);
        int height = mapGenerator.grid.GetLength(1);

        // Clear texture
        Color[] pixels = new Color[minimapTexture.width * minimapTexture.height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = unvisitedColor;
        }

        // Draw visited cells and revealed cells
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                // Draw if visited OR revealed by looking
                bool isVisited = visitedCells.Contains(pos);
                bool isRevealed = revealedCells.Contains(pos);

                if (!isVisited && !isRevealed)
                    continue;

                MapCell cell = mapGenerator.grid[x, y];

                // Skip unvisited cells in the actual maze
                if (!cell.visited && !cell.isSecretRoom)
                    continue;

                // Determine cell color
                Vector2Int currentPlayerPos = new Vector2Int(player.GridX, player.GridZ);
                Color cellColor;

                if (pos == currentPlayerPos)
                {
                    cellColor = currentCellColor; // Player position is cyan
                }
                else if (cell.isButtonWall)
                {
                    cellColor = buttonWallColor; // Button walls are orange
                }
                else if (cell.isWall)
                {
                    cellColor = wallCellColor; // Wall cells are gray
                }
                else if (isVisited)
                {
                    cellColor = visitedCellColor; // Visited floor cells are dark gray
                }
                else
                {
                    cellColor = lookAheadColor; // Revealed but not visited cells are light blue-gray
                }

                DrawCell(x, y, cellColor, pixels);

                // Draw player direction indicator
                if (pos == currentPlayerPos)
                {
                    DrawPlayerDirection(x, y, pixels);
                }
            }
        }

        // Draw look-ahead path (cells ahead until wall)
        DrawLookAheadPath(pixels);

        // Draw wall borders for visited cells
        foreach (Vector2Int pos in visitedCells)
        {
            DrawWallBorders(pos.x, pos.y, pixels);
        }

        minimapTexture.SetPixels(pixels);
        minimapTexture.Apply();
    }

    private void DrawCell(int gridX, int gridY, Color color, Color[] pixels)
    {
        int startX = gridX * cellPixelSize;
        int startY = gridY * cellPixelSize;

        for (int x = 0; x < cellPixelSize; x++)
        {
            for (int y = 0; y < cellPixelSize; y++)
            {
                int pixelX = startX + x;
                int pixelY = startY + y;
                int index = pixelY * minimapTexture.width + pixelX;

                if (index >= 0 && index < pixels.Length)
                {
                    pixels[index] = color;
                }
            }
        }
    }


    private void DrawPlayerDirection(int gridX, int gridY, Color[] pixels)
    {
        // Draw a small arrow or line showing facing direction
        int centerX = gridX * cellPixelSize + cellPixelSize / 2;
        int centerY = gridY * cellPixelSize + cellPixelSize / 2;

        // Draw a 3-pixel line in facing direction
        int length = cellPixelSize / 3;

        for (int i = 0; i < length; i++)
        {
            int pixelX = centerX;
            int pixelY = centerY;

            switch (player.Facing)
            {
                case 0: // North
                    pixelY = centerY + i;
                    break;
                case 1: // East
                    pixelX = centerX + i;
                    break;
                case 2: // South
                    pixelY = centerY - i;
                    break;
                case 3: // West
                    pixelX = centerX - i;
                    break;
            }

            int index = pixelY * minimapTexture.width + pixelX;
            if (index >= 0 && index < pixels.Length)
            {
                pixels[index] = Color.yellow;
            }
        }
    }

    private void DrawWallBorders(int gridX, int gridY, Color[] pixels)
    {
        // Only draw borders for non-wall cells
        MapCell cell = mapGenerator.grid[gridX, gridY];
        if (cell.isWall)
            return;

        int width = mapGenerator.grid.GetLength(0);
        int height = mapGenerator.grid.GetLength(1);

        int startX = gridX * cellPixelSize;
        int startY = gridY * cellPixelSize;

        // Check North (y+1)
        if (HasWallInDirection(gridX, gridY + 1, width, height))
        {
            // Draw top edge
            for (int x = 0; x < cellPixelSize; x++)
            {
                int pixelX = startX + x;
                int pixelY = startY + cellPixelSize - 1;
                int index = pixelY * minimapTexture.width + pixelX;
                if (index >= 0 && index < pixels.Length)
                    pixels[index] = wallBorderColor;
            }
        }

        // Check South (y-1)
        if (HasWallInDirection(gridX, gridY - 1, width, height))
        {
            // Draw bottom edge
            for (int x = 0; x < cellPixelSize; x++)
            {
                int pixelX = startX + x;
                int pixelY = startY;
                int index = pixelY * minimapTexture.width + pixelX;
                if (index >= 0 && index < pixels.Length)
                    pixels[index] = wallBorderColor;
            }
        }

        // Check East (x+1)
        if (HasWallInDirection(gridX + 1, gridY, width, height))
        {
            // Draw right edge
            for (int y = 0; y < cellPixelSize; y++)
            {
                int pixelX = startX + cellPixelSize - 1;
                int pixelY = startY + y;
                int index = pixelY * minimapTexture.width + pixelX;
                if (index >= 0 && index < pixels.Length)
                    pixels[index] = wallBorderColor;
            }
        }

        // Check West (x-1)
        if (HasWallInDirection(gridX - 1, gridY, width, height))
        {
            // Draw left edge
            for (int y = 0; y < cellPixelSize; y++)
            {
                int pixelX = startX;
                int pixelY = startY + y;
                int index = pixelY * minimapTexture.width + pixelX;
                if (index >= 0 && index < pixels.Length)
                    pixels[index] = wallBorderColor;
            }
        }
    }

    private bool HasWallInDirection(int x, int y, int width, int height)
    {
        // Out of bounds = wall
        if (x < 0 || x >= width || y < 0 || y >= height)
            return true;

        // Check if neighbor is a wall cell (including button walls)
        MapCell neighbor = mapGenerator.grid[x, y];
        return neighbor.isWall || neighbor.isButtonWall;
    }

    private void DrawLookAheadPath(Color[] pixels)
    {
        int width = mapGenerator.grid.GetLength(0);
        int height = mapGenerator.grid.GetLength(1);

        // Start from player's current position
        int currentX = player.GridX;
        int currentY = player.GridZ;

        // Get direction vector based on facing
        int deltaX = 0;
        int deltaY = 0;

        switch (player.Facing)
        {
            case 0: // North
                deltaY = 1;
                break;
            case 1: // East
                deltaX = 1;
                break;
            case 2: // South
                deltaY = -1;
                break;
            case 3: // West
                deltaX = -1;
                break;
        }

        // Trace forward until we hit a wall
        int stepX = currentX + deltaX;
        int stepY = currentY + deltaY;

        while (stepX >= 0 && stepX < width && stepY >= 0 && stepY < height)
        {
            MapCell cell = mapGenerator.grid[stepX, stepY];

            // Stop if we hit a wall (including button walls)
            if (cell.isWall || cell.isButtonWall)
                break;

            // Add to revealed cells (permanent)
            Vector2Int cellPos = new Vector2Int(stepX, stepY);
            revealedCells.Add(cellPos);

            // Draw this cell in look-ahead color (will be drawn brighter in the main loop)
            DrawCell(stepX, stepY, lookAheadColor, pixels);

            // Move to next cell
            stepX += deltaX;
            stepY += deltaY;
        }
    }
}
