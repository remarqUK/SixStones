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

    [Header("Game Settings")]
    [SerializeField] private int pointsPerPiece = 10;
    [SerializeField] private int maxMoves = 30;

    private int player1Score = 0;
    private int player2Score = 0;
    private int player1RemainingMoves;
    private int player2RemainingMoves;
    private Dictionary<GamePiece.PieceType, int> player1ColorScores;
    private Dictionary<GamePiece.PieceType, int> player2ColorScores;

    private void Start()
    {
        player1RemainingMoves = maxMoves;
        player2RemainingMoves = maxMoves;
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

        // Drop mode: Score based on bottom row hits
        if (mode != null && mode.scoreOnBottomRowHit)
        {
            int totalPoints = matchedPieces.Count * mode.bottomRowHitPoints;

            // Add to current player's score (always player 1 in drop mode since it's single player)
            player1Score += totalPoints;

            Debug.Log($"Drop mode scoring: {matchedPieces.Count} gems × {mode.bottomRowHitPoints} = {totalPoints} points");
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
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("<b>Player 1 Colors:</b>");

            foreach (var kvp in player1ColorScores)
            {
                string colorName = kvp.Key.ToString();
                string colorHex = GetColorHex(kvp.Key);
                sb.AppendLine($"<color={colorHex}>{colorName}:</color> {kvp.Value}");
            }

            player1ColorScoresText.text = sb.ToString();
        }

        // Update Player 2 color scores
        if (player2ColorScoresText != null && player2ColorScores != null)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("<b>Player 2 Colors:</b>");

            foreach (var kvp in player2ColorScores)
            {
                string colorName = kvp.Key.ToString();
                string colorHex = GetColorHex(kvp.Key);
                sb.AppendLine($"<color={colorHex}>{colorName}:</color> {kvp.Value}");
            }

            player2ColorScoresText.text = sb.ToString();
        }

        // Log possible moves to console
        if (board != null)
        {
            int possibleMoves = board.GetPossibleMovesCount();
            Debug.Log($"Possible Moves: {possibleMoves}");
        }
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
            _ => "#FFFFFF"
        };
    }

    private void GameOver()
    {
        Debug.Log("===== GAME OVER CALLED =====");

        if (playerManager != null && playerManager.TwoPlayerMode)
        {
            Debug.Log($"Game Over! Player 1: {player1Score}, Player 2: {player2Score}");
            if (player1Score > player2Score)
                Debug.Log("Player 1 wins!");
            else if (player2Score > player1Score)
                Debug.Log("Player 2 wins!");
            else
                Debug.Log("It's a tie!");
        }
        else
        {
            Debug.Log($"Game Over! Final Score: {player1Score}");
        }
        // You can add game over UI here
    }

    public void ResetGame()
    {
        player1Score = 0;
        player2Score = 0;
        player1RemainingMoves = maxMoves;
        player2RemainingMoves = maxMoves;
        UpdateUI();
        // Reload the scene or reset the board
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}
