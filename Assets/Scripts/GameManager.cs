using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Board board;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI movesText;
    [SerializeField] private TextMeshProUGUI player1ColorScoresText;
    [SerializeField] private TextMeshProUGUI player2ColorScoresText;
    [SerializeField] private TextMeshProUGUI possibleMovesText;
    [SerializeField] private TextMeshProUGUI currentPlayerText;
    [SerializeField] private TextMeshProUGUI player1ScoreText;
    [SerializeField] private TextMeshProUGUI player2ScoreText;
    [SerializeField] private TextMeshProUGUI player1HealthText;
    [SerializeField] private TextMeshProUGUI player2HealthText;

    [Header("Game Settings")]
    [SerializeField] private int pointsPerPiece = 10;
    [SerializeField] private int maxMoves = 30;
    [SerializeField] private int startingHealth = 100;

    [Header("Player Strength")]
    [SerializeField] [Range(0.1f, 5f)] private float player1Strength = 1.0f;
    [SerializeField] [Range(0.1f, 5f)] private float player2Strength = 1.0f;

    private int player1Score = 0;
    private int player2Score = 0;
    private int player1Health;
    private int player2Health;
    private int player1RemainingMoves;
    private int player2RemainingMoves;
    private Dictionary<GamePiece.PieceType, int> player1ColorScores;
    private Dictionary<GamePiece.PieceType, int> player2ColorScores;
    private bool isGameOver = false;

    public bool IsGameOver => isGameOver;

    private void Start()
    {
        player1RemainingMoves = maxMoves;
        player2RemainingMoves = maxMoves;
        player1Health = startingHealth;
        player2Health = startingHealth;
        InitializeColorScores();
        UpdateUI();
    }

    private void InitializeColorScores()
    {
        player1ColorScores = new Dictionary<GamePiece.PieceType, int>();
        player2ColorScores = new Dictionary<GamePiece.PieceType, int>();

        foreach (GamePiece.PieceType type in System.Enum.GetValues(typeof(GamePiece.PieceType)))
        {
            player1ColorScores[type] = 0;
            player2ColorScores[type] = 0;
        }
    }

    public void AddScore(List<GamePiece> matchedPieces, bool checkBonusTurn = false)
    {
        if (matchedPieces == null || matchedPieces.Count == 0) return;

        // Get current game mode for scoring rules
        GameModeData mode = board != null ? board.CurrentMode : null;

        // Bottom row scoring: Score based on gems hitting bottom row
        if (mode != null && mode.scoreOnBottomRowHit)
        {
            int totalPoints = matchedPieces.Count * mode.bottomRowHitPoints;

            // Add to current player's score
            player1Score += totalPoints;

            Debug.Log($"Bottom row scoring: {matchedPieces.Count} gems × {mode.bottomRowHitPoints} = {totalPoints} points");
            UpdateUI();
            return;
        }

        // Group matches by color and calculate points
        Dictionary<GamePiece.PieceType, List<GamePiece>> colorGroups = new Dictionary<GamePiece.PieceType, List<GamePiece>>();

        foreach (GamePiece piece in matchedPieces)
        {
            if (!colorGroups.ContainsKey(piece.Type))
            {
                colorGroups[piece.Type] = new List<GamePiece>();
            }
            colorGroups[piece.Type].Add(piece);
        }

        // Calculate points based on match size using game mode scoring
        int totalMatchPoints = 0;
        int totalPiecesMatched = matchedPieces.Count;

        // Determine which player's color scores to update
        Dictionary<GamePiece.PieceType, int> currentPlayerColorScores = player1ColorScores;
        if (playerManager != null && playerManager.TwoPlayerMode && playerManager.CurrentPlayer == PlayerManager.Player.Player2)
        {
            currentPlayerColorScores = player2ColorScores;
        }

        foreach (var kvp in colorGroups)
        {
            int matchCount = kvp.Value.Count;
            int matchPoints;

            if (mode != null)
            {
                // Use game mode's scoring rules
                matchPoints = mode.GetPointsForMatch(matchCount);
            }
            else
            {
                // Fallback to old system
                matchPoints = matchCount * pointsPerPiece;
            }

            // Add to current player's color scores
            currentPlayerColorScores[kvp.Key] += matchPoints;
            totalMatchPoints += matchPoints;
        }

        // Add points to current player's total score
        if (playerManager != null && playerManager.TwoPlayerMode)
        {
            if (playerManager.CurrentPlayer == PlayerManager.Player.Player1)
                player1Score += totalMatchPoints;
            else
                player2Score += totalMatchPoints;

            // Damage calculation: Check all matched gem types for damage-dealing gems
            int totalDamage = 0;
            foreach (var kvp in colorGroups)
            {
                GamePiece.PieceType gemType = kvp.Key;
                int matchCount = kvp.Value.Count;

                // Check if this gem type can deal damage
                if (mode != null && mode.CanGemDealDamage(gemType))
                {
                    // Get base damage for this match size
                    int baseDamage = mode.GetDamageForMatch(matchCount);

                    // Apply current player's strength multiplier
                    float playerStrength = playerManager.CurrentPlayer == PlayerManager.Player.Player1
                        ? player1Strength
                        : player2Strength;

                    int damage = Mathf.RoundToInt(baseDamage * playerStrength);
                    totalDamage += damage;

                    Debug.Log($"{playerManager.GetCurrentPlayerName()} matched {matchCount} {gemType} gems! Base damage: {baseDamage}, Strength: {playerStrength}x, Total: {damage}");
                }
            }

            // Apply total damage to opponent
            if (totalDamage > 0)
            {
                if (playerManager.CurrentPlayer == PlayerManager.Player.Player1)
                {
                    // Player 1 deals damage to Player 2
                    player2Health -= totalDamage;
                    Debug.Log($"Player 1 deals {totalDamage} damage! Player 2 HP: {player2Health}");
                }
                else
                {
                    // Player 2 deals damage to Player 1
                    player1Health -= totalDamage;
                    Debug.Log($"Player 2 deals {totalDamage} damage! Player 1 HP: {player1Health}");
                }

                // Check for game over
                if (player1Health <= 0)
                {
                    Debug.Log("===== PLAYER 2 WINS - Player 1 HP reached 0! =====");
                    GameOver();
                    return;
                }
                else if (player2Health <= 0)
                {
                    Debug.Log("===== PLAYER 1 WINS - Player 2 HP reached 0! =====");
                    GameOver();
                    return;
                }
            }

            // Check if player earned a bonus turn (only for initial match, not cascades)
            if (checkBonusTurn)
            {
                playerManager.CheckForBonusTurn(totalPiecesMatched);
            }
        }
        else
        {
            // Single player mode - add to player 1 score
            player1Score += totalMatchPoints;
        }

        UpdateUI();
    }

    public void UseMove()
    {
        // Decrement current player's moves
        if (playerManager != null && playerManager.TwoPlayerMode)
        {
            if (playerManager.CurrentPlayer == PlayerManager.Player.Player1)
            {
                player1RemainingMoves--;
                if (player1RemainingMoves <= 0)
                {
                    Debug.Log("Player 1 is out of moves!");
                    GameOver();
                }
            }
            else
            {
                player2RemainingMoves--;
                if (player2RemainingMoves <= 0)
                {
                    Debug.Log("Player 2 is out of moves!");
                    GameOver();
                }
            }
        }
        else
        {
            // Single player mode
            player1RemainingMoves--;
            if (player1RemainingMoves <= 0)
            {
                GameOver();
            }
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        // Update legacy score text for backward compatibility
        if (scoreText != null)
        {
            if (playerManager != null && playerManager.TwoPlayerMode)
                scoreText.text = $"Total: {player1Score + player2Score}";
            else
                scoreText.text = $"Score: {player1Score}";
        }

        // Update player-specific scores
        if (player1ScoreText != null)
        {
            player1ScoreText.text = $"Player 1: {player1Score}";
        }

        if (player2ScoreText != null)
        {
            player2ScoreText.text = $"Player 2: {player2Score}";
        }

        // Update current player indicator
        if (currentPlayerText != null && playerManager != null && playerManager.TwoPlayerMode)
        {
            string bonusText = playerManager.BonusTurnEarned ? " (BONUS TURN!)" : "";
            currentPlayerText.text = $"Current: {playerManager.GetCurrentPlayerName()}{bonusText}";
        }

        // Log moves to console
        if (playerManager != null && playerManager.TwoPlayerMode)
        {
            Debug.Log($"Player 1 Moves: {player1RemainingMoves}, Player 2 Moves: {player2RemainingMoves}");
        }
        else
        {
            Debug.Log($"Moves Remaining: {player1RemainingMoves}");
        }

        // Update Player 1 color scores
        if (player1ColorScoresText != null && player1ColorScores != null)
        {
            player1ColorScoresText.text = FormatColorScores("Player 1", player1ColorScores);
        }

        // Update Player 2 color scores
        if (player2ColorScoresText != null && player2ColorScores != null)
        {
            player2ColorScoresText.text = FormatColorScores("Player 2", player2ColorScores);
        }

        // Update Player 1 health (clamp to 0)
        if (player1HealthText != null)
        {
            int displayHealth = Mathf.Max(0, player1Health);
            player1HealthText.text = $"HP: {displayHealth}";
        }

        // Update Player 2 health (clamp to 0)
        if (player2HealthText != null)
        {
            int displayHealth = Mathf.Max(0, player2Health);
            player2HealthText.text = $"HP: {displayHealth}";
        }

        // Log possible moves to console
        if (board != null)
        {
            int possibleMoves = board.GetPossibleMovesCount();
            Debug.Log($"Possible Moves: {possibleMoves}");
        }
    }

    /// <summary>
    /// Format color scores for display (DRY helper method)
    /// </summary>
    private string FormatColorScores(string playerName, Dictionary<GamePiece.PieceType, int> colorScores)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"<b>{playerName} Colors:</b>");

        foreach (var kvp in colorScores)
        {
            string colorName = kvp.Key.ToString();
            string colorHex = GetColorHex(kvp.Key);
            sb.AppendLine($"<color={colorHex}>{colorName}:</color> {kvp.Value}");
        }

        return sb.ToString();
    }

    private string GetColorHex(GamePiece.PieceType type)
    {
        return type switch
        {
            GamePiece.PieceType.Red => "#E63333",
            GamePiece.PieceType.Blue => "#3380E6",
            GamePiece.PieceType.Green => "#33CC4D",
            GamePiece.PieceType.Yellow => "#F2D933",
            GamePiece.PieceType.Purple => "#B333CC",
            GamePiece.PieceType.Orange => "#F28019",
            GamePiece.PieceType.White => "#F2F2F2",
            _ => "#FFFFFF"
        };
    }

    private void GameOver()
    {
        Debug.Log("===== GAME OVER CALLED =====");
        isGameOver = true;

        string gameOverMessage = "";

        if (playerManager != null && playerManager.TwoPlayerMode)
        {
            Debug.Log($"Game Over! Player 1: {player1Score}, Player 2: {player2Score}");

            // Determine winner by health (HP reached 0) or score
            if (player1Health <= 0)
            {
                gameOverMessage = "GAME OVER - Player 2 Wins!";
                Debug.Log("Player 2 wins by knockout!");
            }
            else if (player2Health <= 0)
            {
                gameOverMessage = "GAME OVER - Player 1 Wins!";
                Debug.Log("Player 1 wins by knockout!");
            }
            else if (player1Score > player2Score)
            {
                gameOverMessage = "GAME OVER - Player 1 Wins!";
                Debug.Log("Player 1 wins by score!");
            }
            else if (player2Score > player1Score)
            {
                gameOverMessage = "GAME OVER - Player 2 Wins!";
                Debug.Log("Player 2 wins by score!");
            }
            else
            {
                gameOverMessage = "GAME OVER - It's a Tie!";
                Debug.Log("It's a tie!");
            }
        }
        else
        {
            gameOverMessage = $"GAME OVER - Score: {player1Score}";
            Debug.Log($"Game Over! Final Score: {player1Score}");
        }

        // Display game over message in current player text
        if (currentPlayerText != null)
        {
            currentPlayerText.text = gameOverMessage;
        }

        UpdateUI();
    }

    public void ResetGame()
    {
        player1Score = 0;
        player2Score = 0;
        player1Health = startingHealth;
        player2Health = startingHealth;
        player1RemainingMoves = maxMoves;
        player2RemainingMoves = maxMoves;
        isGameOver = false;
        UpdateUI();
        // Reload the scene or reset the board
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    /// <summary>
    /// Reset health (called when grid is reset)
    /// </summary>
    public void ResetHealth()
    {
        player1Health = startingHealth;
        player2Health = startingHealth;
        Debug.Log($"Health reset to {startingHealth} for both players");
        UpdateUI();
    }
}
