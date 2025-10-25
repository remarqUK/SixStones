using UnityEngine;

/// <summary>
/// Manages two-player turn-based gameplay
/// Players alternate turns unless they match 4+ pieces for a bonus turn
/// </summary>
public class PlayerManager : MonoBehaviour
{
    public enum Player
    {
        Player1,
        Player2
    }

    private bool twoPlayerMode = true;
    private Player currentPlayer = Player.Player1;
    private bool bonusTurnEarned = false;

    public Player CurrentPlayer => currentPlayer;
    public bool TwoPlayerMode => twoPlayerMode;
    public bool BonusTurnEarned => bonusTurnEarned;

    /// <summary>
    /// Initialize the player manager with the game mode settings
    /// </summary>
    public void Initialize(GameModeData mode)
    {
        if (mode != null)
        {
            twoPlayerMode = mode.playerCount >= 2;
            Debug.Log($"PlayerManager initialized - {(twoPlayerMode ? "Two Player Mode" : "Single Player Mode")} (playerCount: {mode.playerCount})");
        }
        else
        {
            twoPlayerMode = true; // Default to two player
            Debug.LogWarning("PlayerManager initialized without game mode - defaulting to Two Player Mode");
        }

        currentPlayer = Player.Player1;
        bonusTurnEarned = false;
    }

    /// <summary>
    /// Reset to Player 1's turn (e.g., when resetting the grid)
    /// </summary>
    public void ResetToPlayer1()
    {
        currentPlayer = Player.Player1;
        bonusTurnEarned = false;
        Debug.Log("Turn reset to Player 1");
    }

    /// <summary>
    /// Check if a match awards a bonus turn (4+ pieces matched)
    /// </summary>
    public void CheckForBonusTurn(int totalPiecesMatched)
    {
        if (!twoPlayerMode) return;

        if (totalPiecesMatched >= 4)
        {
            bonusTurnEarned = true;
            Debug.Log($"{currentPlayer} earned a bonus turn! (Matched {totalPiecesMatched} pieces)");
        }
        else
        {
            bonusTurnEarned = false;
        }
    }

    /// <summary>
    /// End the current turn and switch to the next player if no bonus turn earned
    /// </summary>
    public void EndTurn()
    {
        if (!twoPlayerMode) return;

        // Process status effects at end of turn
        if (StatusEffectManager.Instance != null)
        {
            StatusEffectManager.Instance.ProcessEndOfTurn(currentPlayer);
        }

        // Update spell system (cooldowns, concentration, etc.)
        if (SpellManager.Instance != null)
        {
            SpellManager.Instance.OnTurnEnd();
        }

        Player previousPlayer = currentPlayer;

        if (bonusTurnEarned)
        {
            Debug.Log($"{currentPlayer} gets a bonus turn!");
            bonusTurnEarned = false;
            // Keep the same player
        }
        else
        {
            // Switch player
            currentPlayer = currentPlayer == Player.Player1 ? Player.Player2 : Player.Player1;
            Debug.Log($"Turn switched to {currentPlayer}");
        }

        // Process status effects at start of new turn
        if (StatusEffectManager.Instance != null)
        {
            StatusEffectManager.Instance.ProcessStartOfTurn(currentPlayer);
        }
    }

    /// <summary>
    /// Get the display name for a player
    /// </summary>
    public string GetPlayerName(Player player)
    {
        return player == Player.Player1 ? "Player 1" : "Player 2";
    }

    /// <summary>
    /// Get the current player's display name
    /// </summary>
    public string GetCurrentPlayerName()
    {
        return GetPlayerName(currentPlayer);
    }
}
