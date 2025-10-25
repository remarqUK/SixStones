using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages hint display after player inactivity
/// Shows a pulsating gem that can be matched for the delay duration,
/// then hides it for twice the delay before showing the same hint again
/// Subscribes to PlayerActivityNotifier events to reset timer
/// </summary>
public class HintSystem : MonoBehaviour
{
    [SerializeField] private Board board;

    private float timeSinceLastActivity;
    private GamePiece currentHintPiece;
    private Vector2Int hintPosition;
    private Coroutine hintCycleCoroutine;
    private bool hasCheckedForMoves = false; // Track if we've already checked this cycle

    private void OnEnable()
    {
        // Subscribe to player activity events
        PlayerActivityNotifier.OnPlayerActivity += ResetTimer;
    }

    private void OnDisable()
    {
        // Unsubscribe from player activity events
        PlayerActivityNotifier.OnPlayerActivity -= ResetTimer;
    }

    private void OnDestroy()
    {
        // Ensure we unsubscribe from events
        PlayerActivityNotifier.OnPlayerActivity -= ResetTimer;

        // Stop hint cycle coroutine if running
        StopHintCycle();
    }

    private void Update()
    {
        // Check if hints are enabled for current mode
        GameModeData mode = board != null ? board.CurrentMode : null;
        if (mode == null || !mode.showHints)
        {
            if (hintCycleCoroutine != null)
            {
                StopHintCycle();
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

        // Start hint cycle after initial delay (only check once per activity cycle)
        if (hintCycleCoroutine == null && !hasCheckedForMoves && timeSinceLastActivity >= mode.hintDelay)
        {
            StartHintCycle();
        }
    }

    /// <summary>
    /// Reset the idle timer (call when player makes a move)
    /// </summary>
    public void ResetTimer()
    {
        timeSinceLastActivity = 0f;
        hasCheckedForMoves = false; // Allow checking again on next cycle
        StopHintCycle();
    }

    /// <summary>
    /// Start the hint cycle: show hint, hide, wait, repeat
    /// </summary>
    private void StartHintCycle()
    {
        if (board == null) return;

        // Mark that we've checked for moves this cycle (prevents spamming checks every frame)
        hasCheckedForMoves = true;

        // Get a random possible move and save it
        (Vector2Int pos1, Vector2Int pos2) = board.GetRandomPossibleMove();

        if (pos1.x == -1) // No moves available
        {
            Debug.Log("No possible moves for hint (board may need regeneration)");
            return;
        }

        // Pick one of the two pieces to pulsate and save the position
        hintPosition = Random.value > 0.5f ? pos1 : pos2;

        GameModeData mode = board.CurrentMode;

        // Calculate display duration as a multiple of pulse cycle duration
        // This ensures the animation completes fully before hiding
        int numberOfPulseCycles = Mathf.CeilToInt(mode.hintDelay / GamePiece.PULSE_CYCLE_DURATION);
        float displayDuration = numberOfPulseCycles * GamePiece.PULSE_CYCLE_DURATION;

        hintCycleCoroutine = StartCoroutine(HintCycleCoroutine(displayDuration));

        Debug.Log($"Starting hint cycle at ({hintPosition.x}, {hintPosition.y}) - Display: {displayDuration}s ({numberOfPulseCycles} cycles), Sleep: {displayDuration * 2f}s");
    }

    /// <summary>
    /// Coroutine that manages the hint show/hide cycle
    /// displayDuration is calculated to be a multiple of pulse cycle duration
    /// </summary>
    private IEnumerator HintCycleCoroutine(float displayDuration)
    {
        while (true)
        {
            // Show hint for the calculated duration (multiple of pulse cycle)
            ShowHint();
            yield return new WaitForSeconds(displayDuration);

            // Hide hint and wait for twice the display duration
            HideHint();
            yield return new WaitForSeconds(displayDuration * 2f);
        }
    }

    /// <summary>
    /// Show the hint at the saved position
    /// </summary>
    private void ShowHint()
    {
        if (board == null) return;

        currentHintPiece = board.GetPieceAt(hintPosition.x, hintPosition.y);

        if (currentHintPiece != null)
        {
            currentHintPiece.StartPulsate();
            Debug.Log($"Showing hint at ({hintPosition.x}, {hintPosition.y})");
        }
    }

    /// <summary>
    /// Hide the current hint without stopping the cycle
    /// </summary>
    private void HideHint()
    {
        if (currentHintPiece != null)
        {
            currentHintPiece.StopPulsate();
            Debug.Log("Hiding hint");
        }
    }

    /// <summary>
    /// Stop the hint cycle completely
    /// </summary>
    private void StopHintCycle()
    {
        if (hintCycleCoroutine != null)
        {
            StopCoroutine(hintCycleCoroutine);
            hintCycleCoroutine = null;
        }

        if (currentHintPiece != null)
        {
            currentHintPiece.StopPulsate();
            currentHintPiece = null;
        }
    }
}
