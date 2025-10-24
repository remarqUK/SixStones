using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages hint display after player inactivity
/// Shows a pulsating gem that can be matched
/// </summary>
public class HintSystem : MonoBehaviour
{
    [SerializeField] private Board board;

    private float timeSinceLastActivity;
    private GamePiece currentHintPiece;
    private bool hintActive = false;

    private void Update()
    {
        // Check if hints are enabled for current mode
        GameModeData mode = board != null ? board.CurrentMode : null;
        if (mode == null || !mode.showHints)
        {
            if (hintActive)
            {
                ClearHint();
            }
            return;
        }

        // Don't show hints while board is processing
        if (board.IsProcessing)
        {
            ResetTimer();
            return;
        }

        // Increment idle timer
        timeSinceLastActivity += Time.deltaTime;

        // Show hint after delay
        if (!hintActive && timeSinceLastActivity >= mode.hintDelay)
        {
            ShowHint();
        }
    }

    /// <summary>
    /// Reset the idle timer (call when player makes a move)
    /// </summary>
    public void ResetTimer()
    {
        timeSinceLastActivity = 0f;
        ClearHint();
    }

    /// <summary>
    /// Show a hint by pulsating a gem that can be matched
    /// </summary>
    private void ShowHint()
    {
        if (board == null) return;

        // Get a random possible move
        (Vector2Int pos1, Vector2Int pos2) = board.GetRandomPossibleMove();

        if (pos1.x == -1) // No moves available
        {
            Debug.Log("No possible moves for hint");
            return;
        }

        // Pick one of the two pieces to pulsate
        Vector2Int hintPos = Random.value > 0.5f ? pos1 : pos2;
        currentHintPiece = board.GetPieceAt(hintPos.x, hintPos.y);

        if (currentHintPiece != null)
        {
            currentHintPiece.StartPulsate();
            hintActive = true;
            Debug.Log($"Showing hint at ({hintPos.x}, {hintPos.y})");
        }
    }

    /// <summary>
    /// Clear the current hint
    /// </summary>
    private void ClearHint()
    {
        if (currentHintPiece != null)
        {
            currentHintPiece.StopPulsate();
            currentHintPiece = null;
        }
        hintActive = false;
    }
}
