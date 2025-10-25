using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField] private int width = 8;
    [SerializeField] private int height = 8;
    [SerializeField] private float cellSize = 1f;

    [Header("Prefabs")]
    [SerializeField] private GameObject gamePiecePrefab;
    [SerializeField] private GameObject cellBackgroundPrefab;

    [Header("Gameplay Settings")]
    [SerializeField] private float swapDuration = 0.2f;
    [SerializeField] private float fallDuration = 0.3f;

    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private CPUPlayer cpuPlayer;
    [SerializeField] private GameModeData currentMode;

    private GamePiece[,] pieces;
    private bool isProcessing = false;
    private MatchDetector matchDetector;
    private GameObject backgroundParent; // Store reference for cleanup
    private int largestMatchThisTurn = 0; // Track largest match across entire turn (initial + cascades)

    public int Width => width;
    public int Height => height;
    public bool IsProcessing => isProcessing;
    public GameModeData CurrentMode => currentMode;
    public MatchDetector MatchDetector => matchDetector;

    private void Start()
    {
        Debug.Log("===== BOARD START() CALLED =====");
        Debug.Log($"Board Instance ID: {GetInstanceID()}");
        Debug.Log($"Frame: {Time.frameCount}, Time: {Time.time}");
        Debug.Log($"Total Board objects in scene: {FindObjectsByType<Board>(FindObjectsSortMode.None).Length}");

        // Apply game mode settings if available
        if (currentMode != null)
        {
            Debug.Log($"Current mode: {currentMode.modeName}, configured size: {currentMode.boardWidth}x{currentMode.boardHeight}");

            if (currentMode.boardWidth > 0)
                width = currentMode.boardWidth;

            if (currentMode.boardHeight > 0)
                height = currentMode.boardHeight;

            Debug.Log($"Board size set to: {width}x{height}");

            // Initialize player manager with game mode settings
            if (playerManager != null)
            {
                playerManager.Initialize(currentMode);
            }
        }
        else
        {
            Debug.LogWarning("No game mode assigned to Board! Using default size.");
        }

        matchDetector = new MatchDetector(this);
        InitializeBoard();
    }

    private void OnDestroy()
    {
        // Stop all running coroutines to prevent orphaned coroutines
        StopAllCoroutines();

        // Destroy all pieces
        if (pieces != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (pieces[x, y] != null)
                    {
                        Destroy(pieces[x, y].gameObject);
                        pieces[x, y] = null;
                    }
                }
            }
        }

        // Destroy background
        if (backgroundParent != null)
        {
            Destroy(backgroundParent);
            backgroundParent = null;
        }
    }

    private void InitializeBoard()
    {
        pieces = new GamePiece[width, height];

        // Log grid measurements
        float gridWidth = width * cellSize;
        float gridHeight = height * cellSize;
        Debug.Log($"=== GRID INITIALIZED ===");
        Debug.Log($"Grid Height: {gridHeight} world units");
        Debug.Log($"Number of gems in height: {height}");
        Debug.Log($"Height of each gem cell: {cellSize} world units");
        Debug.Log($"Expected gem sprite height: {cellSize * 0.75f} world units (75% of cell)");

        // Create background
        CreateBackground();

        // Fill board with pieces (invisible initially to prevent flicker)
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CreatePiece(x, y, animate: false, visible: false);
            }
        }

        // If scoring on bottom row hits, ensure no target gems on bottom row initially
        if (currentMode != null && currentMode.scoreOnBottomRowHit)
        {
            EnsureNoDropGemsOnBottomRow();
        }

        // Ensure no initial matches (pieces will be made visible after clearing)
        StartCoroutine(ClearInitialMatches());
    }

    /// <summary>
    /// Public method to reset the grid - clears all pieces and regenerates the board
    /// </summary>
    public void ResetGrid()
    {
        Debug.Log("===== RESET GRID CALLED =====");

        // Clear all existing pieces
        if (pieces != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (pieces[x, y] != null)
                    {
                        Destroy(pieces[x, y].gameObject);
                        pieces[x, y] = null;
                    }
                }
            }
        }

        // Regenerate the board
        pieces = new GamePiece[width, height];

        // Fill board with new pieces (invisible initially to prevent flicker)
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CreatePiece(x, y, animate: false, visible: false);
            }
        }

        // If scoring on bottom row hits, ensure no target gems on bottom row initially
        if (currentMode != null && currentMode.scoreOnBottomRowHit)
        {
            EnsureNoDropGemsOnBottomRow();
        }

        // Reset to Player 1's turn
        if (playerManager != null)
        {
            playerManager.ResetToPlayer1();
        }

        // Reset health
        if (gameManager != null)
        {
            gameManager.ResetHealth();
        }

        // Ensure no initial matches (pieces will be made visible after clearing)
        StartCoroutine(ClearInitialMatches());
    }

    private void CreateBackground()
    {
        // Cell backgrounds disabled - using camera background color instead
        // This provides a cleaner look with the dark side panels

        // Clean up any existing background
        if (backgroundParent != null)
        {
            Destroy(backgroundParent);
            backgroundParent = null;
        }

        return;
    }

    private void CreatePiece(int x, int y, bool animate = false, bool visible = true)
    {
        // Safety check: don't create if a piece already exists at this position
        if (pieces[x, y] != null)
        {
            Debug.LogWarning($"CreatePiece: Piece already exists at ({x},{y}), skipping creation");
            return;
        }

        GamePiece.PieceType randomType = GetRandomPieceType();
        Vector3 spawnPosition = animate ? GridToWorld(x, height + 2) : GridToWorld(x, y);

        GameObject pieceObj = Instantiate(gamePiecePrefab, spawnPosition, Quaternion.identity, transform);
        pieceObj.name = $"Piece_{x}_{y}";

        GamePiece piece = pieceObj.GetComponent<GamePiece>();
        piece.Initialize(randomType, new Vector2Int(x, y), this);
        pieces[x, y] = piece;

        // Set visibility
        piece.SetVisible(visible);

        if (animate)
        {
            float adjustedDuration = GetAdjustedDuration(fallDuration);
            piece.MoveTo(GridToWorld(x, y), adjustedDuration);
        }
    }

    private GamePiece.PieceType GetRandomPieceType()
    {
        int typeCount = System.Enum.GetValues(typeof(GamePiece.PieceType)).Length;
        return (GamePiece.PieceType)Random.Range(0, typeCount);
    }

    public Vector3 GridToWorld(int x, int y)
    {
        float offsetX = -(width - 1) * cellSize / 2f;
        float offsetY = -(height - 1) * cellSize / 2f;
        return new Vector3(x * cellSize + offsetX, y * cellSize + offsetY, 0);
    }

    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        float offsetX = -(width - 1) * cellSize / 2f;
        float offsetY = -(height - 1) * cellSize / 2f;

        int x = Mathf.RoundToInt((worldPosition.x - offsetX) / cellSize);
        int y = Mathf.RoundToInt((worldPosition.y - offsetY) / cellSize);

        return new Vector2Int(x, y);
    }

    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    /// <summary>
    /// Get duration adjusted by current game speed setting
    /// </summary>
    private float GetAdjustedDuration(float baseDuration)
    {
        if (GameSpeedSettings.Instance != null)
        {
            return GameSpeedSettings.Instance.GetAdjustedDuration(baseDuration);
        }
        return baseDuration; // Fallback if no speed settings found
    }

    public bool IsValidPosition(Vector2Int pos)
    {
        return IsValidPosition(pos.x, pos.y);
    }

    public GamePiece GetPieceAt(int x, int y)
    {
        if (pieces == null) return null; // Board not initialized yet
        if (!IsValidPosition(x, y)) return null;
        return pieces[x, y];
    }

    public GamePiece GetPieceAt(Vector2Int pos)
    {
        return GetPieceAt(pos.x, pos.y);
    }

    public void SwapPieces(Vector2Int pos1, Vector2Int pos2)
    {
        if (!IsValidPosition(pos1) || !IsValidPosition(pos2)) return;
        if (isProcessing) return;

        // Block swaps if game is over
        if (gameManager != null && gameManager.IsGameOver)
        {
            Debug.Log("Cannot swap - game is over");
            return;
        }

        StartCoroutine(SwapPiecesCoroutine(pos1, pos2));
    }

    private IEnumerator SwapPiecesCoroutine(Vector2Int pos1, Vector2Int pos2)
    {
        isProcessing = true;

        // Reset turn tracker - this is a new player action
        largestMatchThisTurn = 0;

        GamePiece piece1 = pieces[pos1.x, pos1.y];
        GamePiece piece2 = pieces[pos2.x, pos2.y];

        // Swap in grid
        pieces[pos1.x, pos1.y] = piece2;
        pieces[pos2.x, pos2.y] = piece1;

        // Update grid positions
        piece1.SetGridPosition(pos2);
        piece2.SetGridPosition(pos1);

        // Animate swap
        float adjustedSwapDuration = GetAdjustedDuration(swapDuration);
        piece1.MoveTo(GridToWorld(pos2.x, pos2.y), adjustedSwapDuration);
        piece2.MoveTo(GridToWorld(pos1.x, pos1.y), adjustedSwapDuration);

        yield return new WaitForSeconds(adjustedSwapDuration);

        // Check for matches
        List<GamePiece> matches = matchDetector.FindAllMatches();

        if (matches.Count == 0)
        {
            // No matches, swap back
            pieces[pos1.x, pos1.y] = piece1;
            pieces[pos2.x, pos2.y] = piece2;
            piece1.SetGridPosition(pos1);
            piece2.SetGridPosition(pos2);
            float adjustedSwapBackDuration = GetAdjustedDuration(swapDuration);
            piece1.MoveTo(GridToWorld(pos1.x, pos1.y), adjustedSwapBackDuration);
            piece2.MoveTo(GridToWorld(pos2.x, pos2.y), adjustedSwapBackDuration);
            yield return new WaitForSeconds(adjustedSwapBackDuration);

            Debug.Log("Invalid move - no matches created. Turn continues.");
            // Invalid move - DON'T end turn, let player try again
        }
        else
        {
            // Process matches (tracks largest match across entire turn)
            yield return StartCoroutine(ProcessMatches(matches));

            // After all matches and cascades are done, check for bonus turn
            if (playerManager != null && playerManager.TwoPlayerMode)
            {
                // Award bonus turn if ANY match in the sequence (initial or cascade) was 4+
                playerManager.CheckForBonusTurn(largestMatchThisTurn);
            }

            // Valid move completed - end turn
            EndTurn();
        }

        isProcessing = false;
    }

    private IEnumerator ProcessMatches(List<GamePiece> matchedPieces)
    {
        Debug.Log($"ProcessMatches: Processing {matchedPieces.Count} matched pieces");

        // Track the largest match in this turn (for bonus turn calculation)
        if (matchedPieces.Count > largestMatchThisTurn)
        {
            largestMatchThisTurn = matchedPieces.Count;
            Debug.Log($"New largest match this turn: {largestMatchThisTurn} pieces");
        }

        // Report scores to GameManager (only if using standard match-based scoring)
        if (gameManager != null && currentMode != null && !currentMode.scoreOnBottomRowHit)
        {
            gameManager.AddScore(matchedPieces);
        }

        // Clear matched pieces
        foreach (GamePiece piece in matchedPieces)
        {
            Vector2Int pos = piece.GridPosition;
            pieces[pos.x, pos.y] = null;
            piece.DestroyPiece();
        }

        yield return new WaitForSeconds(GetAdjustedDuration(0.3f));

        // Make pieces fall
        yield return StartCoroutine(MakePiecesFall());

        // CRITICAL: Wait for all falling animations to complete
        yield return StartCoroutine(WaitForAllAnimations());

        // Check for bottom row hits (if mode scores on bottom row)
        yield return StartCoroutine(CheckBottomRowHits());

        // Fill empty spaces
        Debug.Log("ProcessMatches: Filling empty spaces");
        FillEmptySpaces();

        // CRITICAL: Wait for all new piece animations to complete before checking matches
        yield return StartCoroutine(WaitForAllAnimations());

        // Final validation before checking for new matches
        ValidateBoardIntegrity("After all animations completed");

        // Check for new matches (only if using standard match-based scoring)
        if (currentMode == null || !currentMode.scoreOnBottomRowHit)
        {
            List<GamePiece> newMatches = matchDetector.FindAllMatches();
            if (newMatches.Count > 0)
            {
                Debug.Log($"ProcessMatches: Found {newMatches.Count} cascade matches");
                // Process cascade matches (bonus turn check happens after all cascades)
                yield return StartCoroutine(ProcessMatches(newMatches));
            }
            else
            {
                Debug.Log("ProcessMatches: No more cascades, checking for possible moves");
                // No more cascades - check if we need to regenerate due to no possible moves
                yield return StartCoroutine(CheckAndRegenerateIfNeeded());
            }
        }
        else
        {
            // If scoring on bottom row, check again after pieces spawn
            yield return StartCoroutine(CheckBottomRowHits());
        }
    }

    /// <summary>
    /// Wait for all piece animations to complete before proceeding
    /// CRITICAL: Ensures we never check for matches while pieces are moving
    /// </summary>
    private IEnumerator WaitForAllAnimations()
    {
        Debug.Log("WaitForAllAnimations: Starting wait");

        // CRITICAL: Wait one frame to allow coroutines to start executing
        // Without this, MoveTo() coroutines haven't started yet and IsMoving is still false
        yield return null;

        // Keep checking until no pieces are moving
        bool anyMoving = true;
        int checkCount = 0;

        while (anyMoving)
        {
            anyMoving = false;
            checkCount++;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (pieces[x, y] != null && pieces[x, y].IsMoving)
                    {
                        anyMoving = true;
                        break;
                    }
                }
                if (anyMoving) break;
            }

            if (anyMoving)
            {
                yield return null; // Wait one frame
            }
        }

        Debug.Log($"WaitForAllAnimations: All animations complete (checked {checkCount} times)");
    }

    /// <summary>
    /// Check if there are no possible moves and regenerate the board if the mode allows it
    /// </summary>
    private IEnumerator CheckAndRegenerateIfNeeded()
    {
        // Only check if the mode has this feature enabled
        if (currentMode == null || !currentMode.regenerateOnNoMoves)
        {
            yield break;
        }

        int possibleMoves = GetPossibleMovesCount();
        if (possibleMoves == 0)
        {
            Debug.Log($"===== NO POSSIBLE MOVES - Regenerating board (mode: {currentMode.modeName}) =====");

            // Clear entire board
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (pieces[x, y] != null)
                    {
                        Destroy(pieces[x, y].gameObject);
                        pieces[x, y] = null;
                    }
                }
            }

            yield return new WaitForSeconds(GetAdjustedDuration(0.3f));

            // Recreate board (invisible until clearing is complete)
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    CreatePiece(x, y, animate: false, visible: false);
                }
            }

            // Apply bottom-row scoring restrictions if needed
            if (currentMode.scoreOnBottomRowHit)
            {
                EnsureNoDropGemsOnBottomRow();
            }

            // Clear any initial matches (will make pieces visible when done)
            yield return StartCoroutine(ClearInitialMatches());
        }
    }

    private IEnumerator MakePiecesFall()
    {
        int piecesMoved = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (pieces[x, y] == null)
                {
                    // Find piece above
                    for (int above = y + 1; above < height; above++)
                    {
                        if (pieces[x, above] != null)
                        {
                            // Move piece down
                            GamePiece fallingPiece = pieces[x, above];
                            pieces[x, above] = null;
                            pieces[x, y] = fallingPiece;
                            fallingPiece.SetGridPosition(new Vector2Int(x, y));
                            float adjustedFallDuration = GetAdjustedDuration(fallDuration);
                            fallingPiece.MoveTo(GridToWorld(x, y), adjustedFallDuration);
                            piecesMoved++;
                            break;
                        }
                    }
                }
            }
        }

        // Don't wait here - let WaitForAllAnimations() handle it properly
        yield break;
    }

    private void FillEmptySpaces()
    {
        // Check if current mode allows spawning new pieces
        if (currentMode != null && !currentMode.spawnNewPieces)
        {
            Debug.Log($"FillEmptySpaces: Skipping - mode '{currentMode.modeName}' has spawnNewPieces=false");
            return;
        }

        int piecesCreated = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (pieces[x, y] == null)
                {
                    CreatePiece(x, y, true);
                    piecesCreated++;
                }
            }
        }

        if (piecesCreated > 0)
        {
            Debug.Log($"FillEmptySpaces: Created {piecesCreated} new pieces");
        }

        // Validate board has no null positions
        ValidateBoardIntegrity("After FillEmptySpaces");
    }

    /// <summary>
    /// Debug helper to validate board has no null positions
    /// </summary>
    private void ValidateBoardIntegrity(string context)
    {
        int nullCount = 0;
        System.Text.StringBuilder nullPositions = new System.Text.StringBuilder();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (pieces[x, y] == null)
                {
                    nullCount++;
                    nullPositions.Append($"({x},{y}) ");
                }
            }
        }

        if (nullCount > 0)
        {
            Debug.LogError($"BOARD INTEGRITY ERROR {context}: Found {nullCount} null positions: {nullPositions}");
        }
    }

    private IEnumerator ClearInitialMatches()
    {
        yield return new WaitForSeconds(GetAdjustedDuration(0.1f));

        List<GamePiece> matches = matchDetector.FindAllMatches();
        while (matches.Count > 0)
        {
            // Replace matched pieces with random different types (keep invisible)
            foreach (GamePiece piece in matches)
            {
                Vector2Int pos = piece.GridPosition;
                // CRITICAL: Clear the reference BEFORE creating new piece
                pieces[pos.x, pos.y] = null;
                Destroy(piece.gameObject);
                CreatePiece(pos.x, pos.y, animate: false, visible: false);
            }

            yield return new WaitForSeconds(GetAdjustedDuration(0.1f));
            matches = matchDetector.FindAllMatches();
        }

        // Check if there are any possible moves
        int possibleMoves = GetPossibleMovesCount();
        Debug.Log($"ClearInitialMatches: Found {possibleMoves} possible moves");
        if (possibleMoves == 0)
        {
            Debug.Log("===== BOARD REGENERATION TRIGGERED - No possible moves =====");

            // Clear entire board
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (pieces[x, y] != null)
                    {
                        Destroy(pieces[x, y].gameObject);
                        pieces[x, y] = null;
                    }
                }
            }

            // Recreate board (keep invisible until clearing is complete)
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    CreatePiece(x, y, animate: false, visible: false);
                }
            }

            // Recursively check again
            yield return StartCoroutine(ClearInitialMatches());
            yield break;
        }

        // All clearing complete - animate pieces dropping from above
        yield return StartCoroutine(AnimateInitialDrop());

        Debug.Log("Board initialization complete - all pieces dropped");

        // Update UI now that board is initialized
        if (gameManager != null)
        {
            gameManager.UpdateUI();
        }
    }

    /// <summary>
    /// Animate all pieces dropping from above the board (used during initial setup)
    /// Drops row by row from bottom to top with staggered timing
    /// </summary>
    private IEnumerator AnimateInitialDrop()
    {
        // Position all pieces above the board
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (pieces[x, y] != null)
                {
                    GamePiece piece = pieces[x, y];

                    // All pieces start 2 cells above the top of the board
                    Vector3 startPosition = GridToWorld(x, height + 2);

                    // Move piece to start position instantly
                    piece.transform.position = startPosition;
                }
            }
        }

        // Drop rows one at a time from bottom (y=0) to top (y=height-1)
        for (int y = 0; y < height; y++)
        {
            // Make this row visible and start dropping
            for (int x = 0; x < width; x++)
            {
                if (pieces[x, y] != null)
                {
                    GamePiece piece = pieces[x, y];

                    // Make visible
                    piece.SetVisible(true);

                    // Start falling animation
                    Vector3 targetPosition = GridToWorld(x, y);
                    float adjustedDuration = GetAdjustedDuration(fallDuration);
                    piece.MoveTo(targetPosition, adjustedDuration);
                }
            }

            // Wait before dropping next row
            yield return new WaitForSeconds(GetAdjustedDuration(0.1f));
        }

        // Wait for all animations to complete
        yield return StartCoroutine(WaitForAllAnimations());
    }

    /// <summary>
    /// Calculate the number of possible moves on the board
    /// </summary>
    public int GetPossibleMovesCount()
    {
        // Return 0 if board not initialized yet
        if (pieces == null) return 0;

        int possibleMoves = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GamePiece piece = pieces[x, y];
                if (piece == null) continue;

                // Check swap with right neighbor
                if (x < width - 1)
                {
                    if (WouldCreateMatch(x, y, x + 1, y))
                    {
                        possibleMoves++;
                    }
                }

                // Check swap with top neighbor
                if (y < height - 1)
                {
                    if (WouldCreateMatch(x, y, x, y + 1))
                    {
                        possibleMoves++;
                    }
                }
            }
        }

        return possibleMoves;
    }

    /// <summary>
    /// Get a random possible move for hint system
    /// Returns positions of two pieces that can be swapped to create a match
    /// Returns (-1,-1) and (-1,-1) if no moves available
    /// </summary>
    public (Vector2Int, Vector2Int) GetRandomPossibleMove()
    {
        // Return invalid positions if board not initialized
        if (pieces == null)
        {
            return (new Vector2Int(-1, -1), new Vector2Int(-1, -1));
        }

        // Collect all possible moves
        List<(Vector2Int, Vector2Int)> possibleMoves = new List<(Vector2Int, Vector2Int)>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GamePiece piece = pieces[x, y];
                if (piece == null) continue;

                // Check swap with right neighbor
                if (x < width - 1)
                {
                    if (WouldCreateMatch(x, y, x + 1, y))
                    {
                        possibleMoves.Add((new Vector2Int(x, y), new Vector2Int(x + 1, y)));
                    }
                }

                // Check swap with top neighbor
                if (y < height - 1)
                {
                    if (WouldCreateMatch(x, y, x, y + 1))
                    {
                        possibleMoves.Add((new Vector2Int(x, y), new Vector2Int(x, y + 1)));
                    }
                }
            }
        }

        // Return random move if any exist
        if (possibleMoves.Count > 0)
        {
            int randomIndex = Random.Range(0, possibleMoves.Count);
            return possibleMoves[randomIndex];
        }

        // No moves available
        return (new Vector2Int(-1, -1), new Vector2Int(-1, -1));
    }

    /// <summary>
    /// Ensure no target gems on bottom row (for modes that score on bottom row hits)
    /// </summary>
    private void EnsureNoDropGemsOnBottomRow()
    {
        if (currentMode == null || !currentMode.scoreOnBottomRowHit) return;

        GamePiece.PieceType dropGem = currentMode.bottomRowScoreGem;
        int replacementCount = 0;

        // Check bottom row (y = 0)
        for (int x = 0; x < width; x++)
        {
            GamePiece piece = pieces[x, 0];
            if (piece != null && piece.Type == dropGem)
            {
                // CRITICAL: Clear reference before destroying and creating
                pieces[x, 0] = null;
                Destroy(piece.gameObject);

                // Create a new piece that's NOT the drop gem type (invisible during initialization)
                GamePiece newPiece = CreatePieceAvoidingType(x, 0, dropGem, visible: false);
                pieces[x, 0] = newPiece;
                replacementCount++;
            }
        }

        if (replacementCount > 0)
        {
            Debug.Log($"Replaced {replacementCount} {dropGem} gems on bottom row during initialization");
        }
    }

    /// <summary>
    /// Create a piece avoiding a specific type
    /// </summary>
    private GamePiece CreatePieceAvoidingType(int x, int y, GamePiece.PieceType avoidType, bool visible = true)
    {
        GamePiece.PieceType[] allTypes = (GamePiece.PieceType[])System.Enum.GetValues(typeof(GamePiece.PieceType));
        GamePiece.PieceType selectedType;

        // Keep randomizing until we get a type that's not the avoided type
        do
        {
            selectedType = allTypes[Random.Range(0, allTypes.Length)];
        }
        while (selectedType == avoidType);

        Vector3 position = GridToWorld(x, y);
        GameObject pieceObj = Instantiate(gamePiecePrefab, position, Quaternion.identity, transform);
        pieceObj.name = $"Piece_{x}_{y}";

        GamePiece piece = pieceObj.GetComponent<GamePiece>();
        piece.Initialize(selectedType, new Vector2Int(x, y), this);
        piece.SetVisible(visible);

        return piece;
    }

    /// <summary>
    /// Check bottom row for target gems (for modes that score on bottom row hits)
    /// </summary>
    private IEnumerator CheckBottomRowHits()
    {
        // Only process if mode scores on bottom row hits
        if (currentMode == null || !currentMode.scoreOnBottomRowHit)
        {
            yield break;
        }

        List<GamePiece> bottomRowHits = new List<GamePiece>();

        // Scan bottom row (y = 0) for target gem type
        for (int x = 0; x < width; x++)
        {
            GamePiece piece = pieces[x, 0];
            if (piece != null && piece.Type == currentMode.bottomRowScoreGem)
            {
                bottomRowHits.Add(piece);
            }
        }

        if (bottomRowHits.Count > 0)
        {
            Debug.Log($"Bottom row hits: {bottomRowHits.Count} {currentMode.bottomRowScoreGem} gems reached the bottom!");

            // Award points for each gem
            if (gameManager != null)
            {
                int totalPoints = bottomRowHits.Count * currentMode.bottomRowHitPoints;
                Debug.Log($"Awarding {totalPoints} points for bottom row hits");

                // Create a temporary list for scoring (AddScore expects a list)
                gameManager.AddScore(bottomRowHits);
            }

            // Remove the gems from the bottom row
            foreach (GamePiece piece in bottomRowHits)
            {
                Vector2Int pos = piece.GridPosition;
                pieces[pos.x, pos.y] = null;
                piece.DestroyPiece();
            }

            yield return new WaitForSeconds(GetAdjustedDuration(0.3f));

            // Make pieces fall to fill the gaps
            yield return StartCoroutine(MakePiecesFall());

            // Wait for falling animations to complete
            yield return StartCoroutine(WaitForAllAnimations());

            // Fill empty spaces
            FillEmptySpaces();

            // Wait for new piece animations to complete
            yield return StartCoroutine(WaitForAllAnimations());

            // Recursively check again
            yield return StartCoroutine(CheckBottomRowHits());
        }
    }

    /// <summary>
    /// End the current turn and switch players if needed
    /// </summary>
    private void EndTurn()
    {
        if (playerManager != null)
        {
            playerManager.EndTurn();

            // Update UI to show current player
            if (gameManager != null)
            {
                gameManager.UpdateUI();
            }

            // Check for possible moves before the next turn starts
            StartCoroutine(CheckAndRegenerateBeforeTurn());
        }
    }

    /// <summary>
    /// Check if there are possible moves before a turn starts, regenerate if needed
    /// </summary>
    private IEnumerator CheckAndRegenerateBeforeTurn()
    {
        // Check if there are any possible moves
        int possibleMoves = GetPossibleMovesCount();

        if (possibleMoves == 0)
        {
            Debug.Log("===== NO POSSIBLE MOVES - Regenerating board before turn =====");
            yield return StartCoroutine(RegenerateBoard());
        }

        // After ensuring moves exist, check if CPU should make a move
        if (cpuPlayer != null)
        {
            cpuPlayer.CheckAndMakeMove();
        }
    }

    /// <summary>
    /// Regenerate the entire board
    /// </summary>
    private IEnumerator RegenerateBoard()
    {
        // Clear entire board
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (pieces[x, y] != null)
                {
                    Destroy(pieces[x, y].gameObject);
                    pieces[x, y] = null;
                }
            }
        }

        yield return new WaitForSeconds(GetAdjustedDuration(0.3f));

        // Recreate board (invisible until clearing is complete)
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CreatePiece(x, y, animate: false, visible: false);
            }
        }

        // Clear any initial matches and ensure no drop gems on bottom
        yield return StartCoroutine(ClearInitialMatches());
    }

    /// <summary>
    /// Check if swapping two pieces would create a match
    /// </summary>
    private bool WouldCreateMatch(int x1, int y1, int x2, int y2)
    {
        // Temporarily swap
        GamePiece temp = pieces[x1, y1];
        pieces[x1, y1] = pieces[x2, y2];
        pieces[x2, y2] = temp;

        // Check for matches at both positions
        bool hasMatch = matchDetector.HasMatchAt(x1, y1) || matchDetector.HasMatchAt(x2, y2);

        // Swap back
        temp = pieces[x1, y1];
        pieces[x1, y1] = pieces[x2, y2];
        pieces[x2, y2] = temp;

        return hasMatch;
    }
}
