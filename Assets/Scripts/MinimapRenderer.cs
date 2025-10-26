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
    public Color unvisitedColor = Color.black;

    [Header("UI References")]
    public RawImage minimapImage;
    public RectTransform minimapContainer;

    private Texture2D minimapTexture;
    private HashSet<Vector2Int> visitedCells = new HashSet<Vector2Int>();
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

        // Draw visited cells
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                // Only draw if visited
                if (!visitedCells.Contains(pos))
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
                else if (cell.isWall)
                {
                    cellColor = wallCellColor; // Wall cells are gray
                }
                else
                {
                    cellColor = visitedCellColor; // Floor cells are dark gray
                }

                DrawCell(x, y, cellColor, pixels);

                // Draw player direction indicator
                if (pos == currentPlayerPos)
                {
                    DrawPlayerDirection(x, y, pixels);
                }
            }
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
}
