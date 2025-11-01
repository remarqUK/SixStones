using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using System.Collections.Generic;

public class MinimapRenderer : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    [FormerlySerializedAs("mapGenerator")]
    [FormerlySerializedAs("mapGeneratorField")]
    private MapGenerator _mapGenerator;

    [SerializeField]
    [FormerlySerializedAs("player")]
    [FormerlySerializedAs("playerField")]
    private FirstPersonMazeController _player;

    [Header("Minimap Settings")]
    [SerializeField] private int cellPixelSize = 5; // Size of each cell in pixels (smaller since grid is larger now)
    [SerializeField] private Color visitedCellColor = new Color(0.3f, 0.3f, 0.3f); // Dark gray
    [SerializeField] private Color currentCellColor = Color.cyan;
    [SerializeField] private Color wallCellColor = new Color(0.533f, 0.533f, 0.533f); // #888 gray for wall cells
    [SerializeField] private Color buttonWallColor = new Color(1f, 0.5f, 0f); // Orange for button walls
    [SerializeField] private Color unvisitedColor = Color.black;
    [SerializeField] private Color wallBorderColor = Color.yellow; // Yellow border for walls adjacent to visited path
    [SerializeField] private Color lookAheadColor = new Color(0.5f, 0.5f, 0.7f); // Light blue-gray for cells ahead

    [Header("UI References")]
    [SerializeField]
    [FormerlySerializedAs("minimapImage")]
    [FormerlySerializedAs("minimapImageField")]
    private RawImage _minimapImage;

    // Public properties for editor scripts and encapsulation
    public MapGenerator mapGenerator
    {
        get { return _mapGenerator; }
        set { _mapGenerator = value; }
    }

    public FirstPersonMazeController player
    {
        get { return _player; }
        set { _player = value; }
    }

    public RawImage minimapImage
    {
        get { return _minimapImage; }
        set { _minimapImage = value; }
    }

    public RectTransform minimapContainer { get; set; } // Legacy property for editor script compatibility (unused)

    private Texture2D minimapTexture;
    private readonly HashSet<Vector2Int> visitedCells = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> revealedCells = new HashSet<Vector2Int>(); // Cells revealed by looking ahead
    private Vector2Int currentPlayerPosition;
    private int currentPlayerFacing;
    private bool initialized = false;
    private bool isDirty = false; // Dirty flag to track when redraw is needed
    private Color[] pixelBuffer; // Reusable pixel array to avoid allocations

    private void Start()
    {
        if (_mapGenerator == null || _player == null)
        {
            Debug.LogError("MinimapRenderer: MapGenerator or Player references are NULL! Please reassign them in the Inspector.");
            Debug.LogError($"_mapGenerator is null: {_mapGenerator == null}, _player is null: {_player == null}");
            return;
        }

        Debug.Log("MinimapRenderer: References are valid, waiting for grid data...");
        // Initialize will be called when we have grid data
    }

    private void OnEnable()
    {
        // Subscribe to player events
        if (_player != null)
        {
            _player.OnPositionChanged += HandlePositionChanged;
            _player.OnFacingChanged += HandleFacingChanged;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from player events
        if (_player != null)
        {
            _player.OnPositionChanged -= HandlePositionChanged;
            _player.OnFacingChanged -= HandleFacingChanged;
        }
    }

    private void LateUpdate()
    {
        // Initialize on first frame when grid is ready
        if (!initialized && _mapGenerator != null && _mapGenerator.grid != null)
        {
            InitializeMinimap();
            isDirty = true; // Force initial draw
            initialized = true;
            Debug.Log("Minimap initialized in LateUpdate()");
        }

        // Only update if something changed
        if (isDirty && initialized)
        {
            UpdateMinimap();
            isDirty = false;
        }
    }

    private void HandlePositionChanged(Vector2Int newPosition)
    {
        currentPlayerPosition = newPosition;
        visitedCells.Add(newPosition);
        isDirty = true; // Mark for redraw
    }

    private void HandleFacingChanged(int newFacing)
    {
        currentPlayerFacing = newFacing;
        isDirty = true; // Mark for redraw
    }

    private void InitializeMinimap()
    {
        if (_mapGenerator.grid == null)
        {
            Debug.LogError("MapGenerator grid is null!");
            return;
        }

        // Use actual grid dimensions (not room dimensions)
        int width = _mapGenerator.grid.GetLength(0);
        int height = _mapGenerator.grid.GetLength(1);

        // Create texture
        minimapTexture = new Texture2D(width * cellPixelSize, height * cellPixelSize);
        minimapTexture.filterMode = FilterMode.Point; // Crisp pixels
        minimapTexture.wrapMode = TextureWrapMode.Clamp;

        // Assign to RawImage
        if (_minimapImage != null)
        {
            _minimapImage.texture = minimapTexture;
        }

        // Initialize pixel buffer (reusable)
        pixelBuffer = new Color[minimapTexture.width * minimapTexture.height];

        // Initialize current player state
        currentPlayerPosition = new Vector2Int(_player.GridX, _player.GridZ);
        currentPlayerFacing = _player.Facing;

        // Mark starting position as visited ONLY if visitedCells is empty
        // (if it's not empty, it means we're reinitializing after a load and the data is already restored)
        if (visitedCells.Count == 0)
        {
            visitedCells.Add(_mapGenerator.startPosition);
            visitedCells.Add(currentPlayerPosition); // Also add current position in case they differ
            Debug.Log($"Minimap initialized: {width}x{height} grid (expected 21x21), texture: {width * cellPixelSize}x{height * cellPixelSize} pixels, start position: {_mapGenerator.startPosition}, player position: {currentPlayerPosition}");
        }
        else
        {
            Debug.Log($"Minimap reinitialized after load: {width}x{height} grid, texture: {width * cellPixelSize}x{height * cellPixelSize} pixels, {visitedCells.Count} visited cells already restored");
        }
    }

    private void UpdateMinimap()
    {
        if (_mapGenerator == null || _mapGenerator.grid == null || minimapTexture == null || pixelBuffer == null)
            return;

        // Use actual grid dimensions
        int width = _mapGenerator.grid.GetLength(0);
        int height = _mapGenerator.grid.GetLength(1);

        // Clear texture using reusable buffer
        for (int i = 0; i < pixelBuffer.Length; i++)
        {
            pixelBuffer[i] = unvisitedColor;
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

                MapCell cell = _mapGenerator.grid[x, y];

                // Skip unvisited cells in the actual maze
                if (!cell.visited && !cell.isSecretRoom)
                    continue;

                // Determine cell color
                Color cellColor;

                if (pos == currentPlayerPosition)
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

                DrawCell(x, y, cellColor, pixelBuffer);

                // Draw player direction indicator
                if (pos == currentPlayerPosition)
                {
                    DrawPlayerDirection(x, y, pixelBuffer);
                }
            }
        }

        // Draw look-ahead path (cells ahead until wall)
        DrawLookAheadPath(pixelBuffer);

        // Draw wall borders for visited cells
        foreach (Vector2Int pos in visitedCells)
        {
            DrawWallBorders(pos.x, pos.y, pixelBuffer);
        }

        minimapTexture.SetPixels(pixelBuffer);
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

            switch (currentPlayerFacing)
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
        MapCell cell = _mapGenerator.grid[gridX, gridY];
        if (cell.isWall)
            return;

        int width = _mapGenerator.grid.GetLength(0);
        int height = _mapGenerator.grid.GetLength(1);

        int startX = gridX * cellPixelSize;
        int startY = gridY * cellPixelSize;

        // Check and draw borders for each direction using helper method
        // North (y+1) - top edge
        if (HasWallInDirection(gridX, gridY + 1, width, height))
            DrawBorderEdge(startX, startY + cellPixelSize - 1, true, pixels);

        // South (y-1) - bottom edge
        if (HasWallInDirection(gridX, gridY - 1, width, height))
            DrawBorderEdge(startX, startY, true, pixels);

        // East (x+1) - right edge
        if (HasWallInDirection(gridX + 1, gridY, width, height))
            DrawBorderEdge(startX + cellPixelSize - 1, startY, false, pixels);

        // West (x-1) - left edge
        if (HasWallInDirection(gridX - 1, gridY, width, height))
            DrawBorderEdge(startX, startY, false, pixels);
    }

    private void DrawBorderEdge(int startPixelX, int startPixelY, bool isHorizontal, Color[] pixels)
    {
        // Draw a line of pixels along either horizontal or vertical edge
        int count = cellPixelSize;

        for (int i = 0; i < count; i++)
        {
            int pixelX = isHorizontal ? startPixelX + i : startPixelX;
            int pixelY = isHorizontal ? startPixelY : startPixelY + i;
            int index = pixelY * minimapTexture.width + pixelX;

            if (index >= 0 && index < pixels.Length)
                pixels[index] = wallBorderColor;
        }
    }

    private bool HasWallInDirection(int x, int y, int width, int height)
    {
        // Out of bounds = wall
        if (x < 0 || x >= width || y < 0 || y >= height)
            return true;

        // Check if neighbor is a wall cell (including button walls)
        MapCell neighbor = _mapGenerator.grid[x, y];
        return neighbor.isWall || neighbor.isButtonWall;
    }

    private void DrawLookAheadPath(Color[] pixels)
    {
        int width = _mapGenerator.grid.GetLength(0);
        int height = _mapGenerator.grid.GetLength(1);

        // Start from player's current position
        int currentX = currentPlayerPosition.x;
        int currentY = currentPlayerPosition.y;

        // Get direction vector based on facing
        int deltaX = 0;
        int deltaY = 0;

        switch (currentPlayerFacing)
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

            // Draw this cell in look-ahead color only if not already visited
            if (!visitedCells.Contains(cellPos))
            {
                DrawCell(stepX, stepY, lookAheadColor, pixels);
            }

            // Move to next cell
            stepX += deltaX;
            stepY += deltaY;
        }
    }

    #region Save/Load Support

    /// <summary>
    /// Get all visited cells for save system
    /// </summary>
    public List<Vector2Int> GetVisitedCells()
    {
        return new List<Vector2Int>(visitedCells);
    }

    /// <summary>
    /// Get all revealed cells for save system
    /// </summary>
    public List<Vector2Int> GetRevealedCells()
    {
        return new List<Vector2Int>(revealedCells);
    }

    /// <summary>
    /// Reset minimap state for loading (clears initialization flag and all data)
    /// Call this before restoring maze and minimap data
    /// </summary>
    public void ResetForLoad()
    {
        // Reset initialization flag so minimap will reinitialize with the restored maze
        initialized = false;

        // Clear all exploration data
        visitedCells.Clear();
        revealedCells.Clear();

        // Destroy old texture if it exists
        if (minimapTexture != null)
        {
            Destroy(minimapTexture);
            minimapTexture = null;
            pixelBuffer = null;
        }

        Debug.Log("MinimapRenderer: Reset for loading - will reinitialize on next LateUpdate");
    }

    /// <summary>
    /// Restore visited and revealed cells (used when loading saved game)
    /// </summary>
    public void RestoreExploredState(List<Vector2Int> visited, List<Vector2Int> revealed)
    {
        // Temporarily unsubscribe from player events to prevent interference during restore
        bool wasSubscribed = false;
        if (_player != null)
        {
            _player.OnPositionChanged -= HandlePositionChanged;
            _player.OnFacingChanged -= HandleFacingChanged;
            wasSubscribed = true;
        }

        visitedCells.Clear();
        revealedCells.Clear();

        if (visited != null)
        {
            foreach (var cell in visited)
            {
                visitedCells.Add(cell);
            }
        }

        if (revealed != null)
        {
            foreach (var cell in revealed)
            {
                revealedCells.Add(cell);
            }
        }

        // Mark for redraw
        isDirty = true;

        // Resubscribe to player events
        if (wasSubscribed && _player != null)
        {
            _player.OnPositionChanged += HandlePositionChanged;
            _player.OnFacingChanged += HandleFacingChanged;
        }

        Debug.Log($"MinimapRenderer: Restored {visitedCells.Count} visited cells and {revealedCells.Count} revealed cells");
    }

    #endregion
}
