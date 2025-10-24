using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CPU player that automatically makes moves for Player 2
/// Prioritizes: 5-gem matches > 4-gem matches > 3-gem matches
/// </summary>
public class CPUPlayer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float thinkDelay = 1.0f; // Time before CPU makes a move
    [SerializeField] private Board board;
    [SerializeField] private PlayerManager playerManager;

    private bool isThinking = false;

    public bool IsThinking => isThinking;

    /// <summary>
    /// Check if it's CPU's turn and make a move
    /// </summary>
    public void CheckAndMakeMove()
    {
        Debug.Log($"CheckAndMakeMove called - isThinking: {isThinking}, board: {board != null}, playerManager: {playerManager != null}");

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
        CheckAndMakeMove();
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
            Debug.Log($"CPU makes move: ({bestMove.x1},{bestMove.y1}) -> ({bestMove.x2},{bestMove.y2}) for {bestMove.matchSize}-gem match");
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
        int bestMatchSize = 0;

        // Try all possible swaps
        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                // Try swapping with right neighbor
                if (x < board.Width - 1)
                {
                    int matchSize = GetMatchSizeForSwap(x, y, x + 1, y);
                    if (matchSize > bestMatchSize)
                    {
                        bestMatchSize = matchSize;
                        bestMove = new MoveData(x, y, x + 1, y, matchSize);
                    }
                }

                // Try swapping with top neighbor
                if (y < board.Height - 1)
                {
                    int matchSize = GetMatchSizeForSwap(x, y, x, y + 1);
                    if (matchSize > bestMatchSize)
                    {
                        bestMatchSize = matchSize;
                        bestMove = new MoveData(x, y, x, y + 1, matchSize);
                    }
                }
            }
        }

        return bestMove;
    }

    private int GetMatchSizeForSwap(int x1, int y1, int x2, int y2)
    {
        // Simulate the swap
        GamePiece piece1 = board.GetPieceAt(x1, y1);
        GamePiece piece2 = board.GetPieceAt(x2, y2);

        if (piece1 == null || piece2 == null) return 0;

        // Save original types
        GamePiece.PieceType originalType1 = piece1.Type;
        GamePiece.PieceType originalType2 = piece2.Type;

        // Temporarily swap types
        piece1.SetTypeForSimulation(originalType2);
        piece2.SetTypeForSimulation(originalType1);

        // Check for matches at both positions
        int maxMatch = 0;

        // Check horizontal and vertical matches at position 1
        int horizontal1 = CountHorizontalMatch(x1, y1);
        int vertical1 = CountVerticalMatch(x1, y1);
        int match1 = Mathf.Max(horizontal1, vertical1);

        // Check horizontal and vertical matches at position 2
        int horizontal2 = CountHorizontalMatch(x2, y2);
        int vertical2 = CountVerticalMatch(x2, y2);
        int match2 = Mathf.Max(horizontal2, vertical2);

        maxMatch = Mathf.Max(match1, match2);

        // Restore original types
        piece1.SetTypeForSimulation(originalType1);
        piece2.SetTypeForSimulation(originalType2);

        return maxMatch >= 3 ? maxMatch : 0;
    }

    private int CountHorizontalMatch(int x, int y)
    {
        GamePiece piece = board.GetPieceAt(x, y);
        if (piece == null) return 0;

        GamePiece.PieceType type = piece.Type;
        int count = 1;

        // Count left
        for (int i = x - 1; i >= 0; i--)
        {
            GamePiece p = board.GetPieceAt(i, y);
            if (p == null || p.Type != type) break;
            count++;
        }

        // Count right
        for (int i = x + 1; i < board.Width; i++)
        {
            GamePiece p = board.GetPieceAt(i, y);
            if (p == null || p.Type != type) break;
            count++;
        }

        return count;
    }

    private int CountVerticalMatch(int x, int y)
    {
        GamePiece piece = board.GetPieceAt(x, y);
        if (piece == null) return 0;

        GamePiece.PieceType type = piece.Type;
        int count = 1;

        // Count down
        for (int i = y - 1; i >= 0; i--)
        {
            GamePiece p = board.GetPieceAt(x, i);
            if (p == null || p.Type != type) break;
            count++;
        }

        // Count up
        for (int i = y + 1; i < board.Height; i++)
        {
            GamePiece p = board.GetPieceAt(x, i);
            if (p == null || p.Type != type) break;
            count++;
        }

        return count;
    }

    private class MoveData
    {
        public int x1, y1, x2, y2;
        public int matchSize;

        public MoveData(int x1, int y1, int x2, int y2, int matchSize)
        {
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
            this.matchSize = matchSize;
        }
    }
}
