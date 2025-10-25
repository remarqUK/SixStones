using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CPU player that automatically makes moves for Player 2
/// Prioritizes: Moves with 4+ cascades > Immediate match size (5 > 4 > 3)
/// </summary>
public class CPUPlayer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float thinkDelay = 1.0f; // Time before CPU makes a move
    [SerializeField] private Board board;
    [SerializeField] private PlayerManager playerManager;

    [Header("Auto-Play")]
    [SerializeField] private bool autoPlay = true; // If false, CPU won't move automatically

    private bool isThinking = false;

    public bool IsThinking => isThinking;
    public bool AutoPlay => autoPlay;

    private void OnDestroy()
    {
        // Stop all coroutines to prevent memory leaks
        StopAllCoroutines();
        isThinking = false;
    }

    /// <summary>
    /// Toggle auto-play on/off
    /// </summary>
    public void ToggleAutoPlay()
    {
        autoPlay = !autoPlay;
        Debug.Log($"CPU Auto-Play: {(autoPlay ? "ON" : "OFF")}");
    }

    /// <summary>
    /// Manually trigger CPU move (useful when auto-play is off)
    /// </summary>
    public void MakeMoveNow()
    {
        if (isThinking)
        {
            Debug.Log("CPU is already thinking");
            return;
        }

        Debug.Log("Manual CPU move triggered");
        InternalMakeMove();
    }

    /// <summary>
    /// Check if it's CPU's turn and make a move (respects auto-play setting)
    /// </summary>
    public void CheckAndMakeMove()
    {
        Debug.Log($"CheckAndMakeMove called - isThinking: {isThinking}, autoPlay: {autoPlay}, board: {board != null}, playerManager: {playerManager != null}");

        // Don't auto-play if disabled
        if (!autoPlay)
        {
            Debug.Log("Auto-play is OFF - CPU waiting for manual trigger");
            return;
        }

        InternalMakeMove();
    }

    /// <summary>
    /// Internal method to actually make the move (bypasses auto-play check)
    /// </summary>
    private void InternalMakeMove()
    {
        if (isThinking)
        {
            Debug.Log("CPU is already thinking, skipping");
            return;
        }

        if (board == null || playerManager == null)
        {
            Debug.LogWarning("Board or PlayerManager is null!");
            return;
        }

        if (board.IsProcessing)
        {
            Debug.Log("Board is still processing, waiting...");
            StartCoroutine(WaitAndCheckAgain());
            return;
        }

        // Check if it's Player 2's turn and CPU is enabled
        GameModeData mode = board.CurrentMode;
        Debug.Log($"Current mode: {mode?.modeName}, player2IsCPU: {mode?.player2IsCPU}, currentPlayer: {playerManager.CurrentPlayer}");

        if (mode == null || !mode.player2IsCPU)
        {
            Debug.Log("CPU not enabled for this mode");
            return;
        }

        if (playerManager.CurrentPlayer != PlayerManager.Player.Player2)
        {
            Debug.Log("Not Player 2's turn");
            return;
        }

        // Make a move
        Debug.Log("CPU should make a move now!");
        StartCoroutine(ThinkAndMove());
    }

    private IEnumerator WaitAndCheckAgain()
    {
        yield return new WaitForSeconds(0.5f);
        InternalMakeMove();
    }

    private IEnumerator ThinkAndMove()
    {
        isThinking = true;
        Debug.Log("CPU is thinking...");

        // Wait before making move (simulate thinking)
        yield return new WaitForSeconds(thinkDelay);

        // Find best move
        MoveData bestMove = FindBestMove();

        if (bestMove != null)
        {
            string cascadeInfo = bestMove.cascadeMatchSize > 0 ? $" + {bestMove.cascadeMatchSize}-gem cascade" : "";
            Debug.Log($"CPU makes move: ({bestMove.x1},{bestMove.y1}) -> ({bestMove.x2},{bestMove.y2}) - {bestMove.immediateMatchSize}-gem match{cascadeInfo} (score: {bestMove.score})");
            board.SwapPieces(new Vector2Int(bestMove.x1, bestMove.y1), new Vector2Int(bestMove.x2, bestMove.y2));
        }
        else
        {
            Debug.LogWarning("CPU couldn't find a valid move!");
        }

        isThinking = false;
    }

    private MoveData FindBestMove()
    {
        MoveData bestMove = null;
        int bestScore = 0;
        List<MoveData> allValidMoves = new List<MoveData>();

        // Try all possible swaps
        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                // Try swapping with right neighbor
                if (x < board.Width - 1)
                {
                    MoveData move = EvaluateSwap(x, y, x + 1, y);
                    if (move != null)
                    {
                        allValidMoves.Add(move);
                        if (move.score > bestScore)
                        {
                            bestScore = move.score;
                            bestMove = move;
                        }
                    }
                }

                // Try swapping with top neighbor
                if (y < board.Height - 1)
                {
                    MoveData move = EvaluateSwap(x, y, x, y + 1);
                    if (move != null)
                    {
                        allValidMoves.Add(move);
                        if (move.score > bestScore)
                        {
                            bestScore = move.score;
                            bestMove = move;
                        }
                    }
                }
            }
        }

        // Debug log all moves
        Debug.Log($"=== CPU Move Evaluation ({allValidMoves.Count} possible moves) ===");

        // Sort moves by score (descending)
        allValidMoves.Sort((a, b) => b.score.CompareTo(a.score));

        // Log top 10 moves (or all if less than 10)
        int movesToShow = Mathf.Min(10, allValidMoves.Count);
        for (int i = 0; i < movesToShow; i++)
        {
            MoveData move = allValidMoves[i];
            string cascadeInfo = move.cascadeMatchSize > 0 ? $" + {move.cascadeMatchSize}c" : "";
            string bestMarker = move == bestMove ? " ← BEST" : "";
            Debug.Log($"  {i + 1}. ({move.x1},{move.y1})→({move.x2},{move.y2}): {move.immediateMatchSize}m{cascadeInfo} = {move.score}{bestMarker}");
        }

        if (allValidMoves.Count > 10)
        {
            Debug.Log($"  ... and {allValidMoves.Count - 10} more moves");
        }

        return bestMove;
    }

    /// <summary>
    /// Evaluate a swap by checking immediate matches and one-step cascade
    /// Returns MoveData with score, or null if no valid match
    /// </summary>
    private MoveData EvaluateSwap(int x1, int y1, int x2, int y2)
    {
        GamePiece piece1 = board.GetPieceAt(x1, y1);
        GamePiece piece2 = board.GetPieceAt(x2, y2);

        if (piece1 == null || piece2 == null) return null;

        // Save original types
        GamePiece.PieceType originalType1 = piece1.Type;
        GamePiece.PieceType originalType2 = piece2.Type;

        // Temporarily swap types
        piece1.SetTypeForSimulation(originalType2);
        piece2.SetTypeForSimulation(originalType1);

        // Find immediate matches
        HashSet<Vector2Int> matchedPositions = FindAllMatchesAtPositions(x1, y1, x2, y2);
        int immediateMatchSize = matchedPositions.Count;

        int cascadeMatchSize = 0;

        // If we have a valid immediate match, simulate cascade
        if (immediateMatchSize >= 3)
        {
            cascadeMatchSize = SimulateCascade(matchedPositions);
        }

        // Restore original types
        piece1.SetTypeForSimulation(originalType1);
        piece2.SetTypeForSimulation(originalType2);

        // No valid match
        if (immediateMatchSize < 3) return null;

        // Calculate score: prioritize moves with 4+ cascades
        int cascadeBonus = cascadeMatchSize >= 4 ? 1000 : 0;
        int score = cascadeBonus + immediateMatchSize;

        return new MoveData(x1, y1, x2, y2, immediateMatchSize, cascadeMatchSize, score);
    }

    /// <summary>
    /// Find all matched pieces at the two swap positions using MatchDetector
    /// </summary>
    private HashSet<Vector2Int> FindAllMatchesAtPositions(int x1, int y1, int x2, int y2)
    {
        HashSet<Vector2Int> allMatches = new HashSet<Vector2Int>();
        MatchDetector matchDetector = board.MatchDetector;

        if (matchDetector == null) return allMatches;

        // Get matches at position 1
        HashSet<Vector2Int> matches1 = matchDetector.GetMatchPositionsAt(x1, y1);
        foreach (var pos in matches1)
        {
            allMatches.Add(pos);
        }

        // Get matches at position 2
        HashSet<Vector2Int> matches2 = matchDetector.GetMatchPositionsAt(x2, y2);
        foreach (var pos in matches2)
        {
            allMatches.Add(pos);
        }

        return allMatches;
    }

    /// <summary>
    /// Simulate one cascade after removing matched pieces
    /// Returns the size of the cascade match (0 if no cascade)
    /// </summary>
    private int SimulateCascade(HashSet<Vector2Int> removedPositions)
    {
        // Create a virtual board state by copying current types
        GamePiece.PieceType[,] virtualBoard = new GamePiece.PieceType[board.Width, board.Height];

        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                GamePiece piece = board.GetPieceAt(x, y);
                virtualBoard[x, y] = piece != null ? piece.Type : GamePiece.PieceType.Red; // Default for null
            }
        }

        // Remove matched pieces from virtual board
        foreach (var pos in removedPositions)
        {
            virtualBoard[pos.x, pos.y] = GamePiece.PieceType.Red; // Mark as empty (we'll use Red as placeholder)
        }

        // Simulate pieces falling in each column
        for (int x = 0; x < board.Width; x++)
        {
            // Collect non-empty pieces from bottom to top
            List<GamePiece.PieceType> column = new List<GamePiece.PieceType>();
            for (int y = 0; y < board.Height; y++)
            {
                bool wasRemoved = removedPositions.Contains(new Vector2Int(x, y));
                if (!wasRemoved)
                {
                    column.Add(virtualBoard[x, y]);
                }
            }

            // Fill column from bottom with existing pieces, rest stays as Red (empty)
            for (int y = 0; y < board.Height; y++)
            {
                if (y < column.Count)
                {
                    virtualBoard[x, y] = column[y];
                }
                // Spaces above are new pieces - we can't predict their type, so skip checking them
            }
        }

        // Check for new matches in the virtual board (only in fallen pieces area)
        int maxCascadeMatch = 0;
        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                // Only check positions that could have pieces fall into them
                int horizontalMatch = CountHorizontalMatchInVirtualBoard(virtualBoard, x, y);
                int verticalMatch = CountVerticalMatchInVirtualBoard(virtualBoard, x, y);
                int match = Mathf.Max(horizontalMatch, verticalMatch);
                if (match > maxCascadeMatch)
                {
                    maxCascadeMatch = match;
                }
            }
        }

        return maxCascadeMatch >= 3 ? maxCascadeMatch : 0;
    }

    /// <summary>
    /// Count horizontal match in virtual board simulation
    /// </summary>
    private int CountHorizontalMatchInVirtualBoard(GamePiece.PieceType[,] virtualBoard, int x, int y)
    {
        GamePiece.PieceType type = virtualBoard[x, y];
        int count = 1;

        // Count left
        for (int i = x - 1; i >= 0; i--)
        {
            if (virtualBoard[i, y] != type) break;
            count++;
        }

        // Count right
        for (int i = x + 1; i < board.Width; i++)
        {
            if (virtualBoard[i, y] != type) break;
            count++;
        }

        return count;
    }

    /// <summary>
    /// Count vertical match in virtual board simulation
    /// </summary>
    private int CountVerticalMatchInVirtualBoard(GamePiece.PieceType[,] virtualBoard, int x, int y)
    {
        GamePiece.PieceType type = virtualBoard[x, y];
        int count = 1;

        // Count down
        for (int i = y - 1; i >= 0; i--)
        {
            if (virtualBoard[x, i] != type) break;
            count++;
        }

        // Count up
        for (int i = y + 1; i < board.Height; i++)
        {
            if (virtualBoard[x, i] != type) break;
            count++;
        }

        return count;
    }

    private class MoveData
    {
        public int x1, y1, x2, y2;
        public int immediateMatchSize;
        public int cascadeMatchSize;
        public int score;

        public MoveData(int x1, int y1, int x2, int y2, int immediateMatchSize, int cascadeMatchSize, int score)
        {
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
            this.immediateMatchSize = immediateMatchSize;
            this.cascadeMatchSize = cascadeMatchSize;
            this.score = score;
        }
    }
}
