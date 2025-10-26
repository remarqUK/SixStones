using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MinimapRenderer : MonoBehaviour
{
    [Header("References")]
    public MapGenerator mapGenerator;
    public FirstPersonMazeController player;

    [Header("Minimap Settings")]
    public int cellPixelSize = 10; // Size of each cell in pixels
    public Color visitedCellColor = new Color(0.3f, 0.3f, 0.3f); // Dark gray
    public Color currentCellColor = Color.cyan;
    public Color wallColor = Color.white;
    public Color unvisitedColor = Color.black;

    [Header("UI References")]
    public RawImage minimapImage;
    public RectTransform minimapContainer;

    private Texture2D minimapTexture;
    private HashSet<Vector2Int> visitedCells = new HashSet<Vector2Int>();
    private Vector2Int lastPlayerPosition;
    private int lastPlayerFacing;

    private void Start()
    {
        if (mapGenerator == null || player == null)
        {
            Debug.LogError("MinimapRenderer requires MapGenerator and Player references!");
            return;
        }

        InitializeMinimap();
        UpdateMinimap();
    }

    private void Update()
    {
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
        int width = mapGenerator.width;
        int height = mapGenerator.height;

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
        if (mapGenerator.grid != null)
        {
            visitedCells.Add(mapGenerator.startPosition);
        }

        Debug.Log($"Minimap initialized: {width}x{height} grid, {width * cellPixelSize}x{height * cellPixelSize} pixels");
    }

    private void UpdateMinimap()
    {
        if (mapGenerator == null || mapGenerator.grid == null || minimapTexture == null)
            return;

        int width = mapGenerator.width;
        int height = mapGenerator.height;

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

                // Draw cell background
                Vector2Int currentPlayerPos = new Vector2Int(player.GridX, player.GridZ);
                Color cellColor = (pos == currentPlayerPos) ? currentCellColor : visitedCellColor;

                DrawCell(x, y, cellColor, pixels);

                // Draw walls
                DrawWalls(x, y, cell, pixels);

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

    private void DrawWalls(int gridX, int gridY, MapCell cell, Color[] pixels)
    {
        int startX = gridX * cellPixelSize;
        int startY = gridY * cellPixelSize;

        // Draw north wall (top)
        if (cell.wallNorth)
        {
            for (int x = 0; x < cellPixelSize; x++)
            {
                int index = (startY + cellPixelSize - 1) * minimapTexture.width + (startX + x);
                if (index >= 0 && index < pixels.Length)
                    pixels[index] = wallColor;
            }
        }

        // Draw south wall (bottom)
        if (cell.wallSouth)
        {
            for (int x = 0; x < cellPixelSize; x++)
            {
                int index = startY * minimapTexture.width + (startX + x);
                if (index >= 0 && index < pixels.Length)
                    pixels[index] = wallColor;
            }
        }

        // Draw east wall (right)
        if (cell.wallEast)
        {
            for (int y = 0; y < cellPixelSize; y++)
            {
                int index = (startY + y) * minimapTexture.width + (startX + cellPixelSize - 1);
                if (index >= 0 && index < pixels.Length)
                    pixels[index] = wallColor;
            }
        }

        // Draw west wall (left)
        if (cell.wallWest)
        {
            for (int y = 0; y < cellPixelSize; y++)
            {
                int index = (startY + y) * minimapTexture.width + startX;
                if (index >= 0 && index < pixels.Length)
                    pixels[index] = wallColor;
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
