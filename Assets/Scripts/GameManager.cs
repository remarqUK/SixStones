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
    [SerializeField] private TextMeshProUGUI player1ScoreText;
    [SerializeField] private TextMeshProUGUI player2ScoreText;
    [SerializeField] private TextMeshProUGUI player1HealthText;
    [SerializeField] private TextMeshProUGUI player2HealthText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI xpText;
    [SerializeField] private TextMeshProUGUI goldText;

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
    private int player1MaxHealth;
    private int player2MaxHealth;
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
        player1MaxHealth = startingHealth;
        player2MaxHealth = startingHealth;
        InitializeColorScores();
        UpdateUI();

        // Subscribe to health depletion events
        HealthEventSystem.OnHealthDepleted += OnHealthDepletedHandler;

        // Subscribe to level system events
        if (LevelSystem.Instance != null)
        {
            LevelSystem.Instance.onXPChanged.AddListener(UpdateLevelUI);
            LevelSystem.Instance.onLevelUp.AddListener(OnLevelUp);
        }

        // Subscribe to currency system events
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.onGoldChanged.AddListener(UpdateGoldUI);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        HealthEventSystem.OnHealthDepleted -= OnHealthDepletedHandler;

        // Unsubscribe from level system events
        if (LevelSystem.Instance != null)
        {
            LevelSystem.Instance.onXPChanged.RemoveListener(UpdateLevelUI);
            LevelSystem.Instance.onLevelUp.RemoveListener(OnLevelUp);
        }

        // Unsubscribe from currency system events
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.onGoldChanged.RemoveListener(UpdateGoldUI);
        }
    }

    /// <summary>
    /// Event handler for when a player's health is depleted
    /// </summary>
    private void OnHealthDepletedHandler(object sender, HealthDepletedEventArgs e)
    {
        GameOver();
    }

    /// <summary>
    /// Updates the level and XP UI display
    /// </summary>
    private void UpdateLevelUI(int currentXP, int requiredXP, int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Level: {level}";
        }

        if (xpText != null)
        {
            xpText.text = $"XP: {currentXP}/{requiredXP}";
        }
    }

    /// <summary>
    /// Updates the gold UI display
    /// </summary>
    private void UpdateGoldUI(int currentGold)
    {
        if (goldText != null)
        {
            goldText.text = $"Gold: {currentGold}";
        }
    }

    /// <summary>
    /// Event handler for when player levels up
    /// </summary>
    private void OnLevelUp(int newLevel)
    {
        Debug.Log($"Congratulations! You reached level {newLevel}!");

        // Award gold bonus for leveling up
        if (CurrencyManager.Instance != null)
        {
            int goldBonus = CurrencyManager.CalculateLevelUpBonus(newLevel);
            CurrencyManager.Instance.AddGold(goldBonus);
            Debug.Log($"Level up bonus: {goldBonus} gold!");
        }

        // Could add visual/audio effects here later
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

    public void AddScore(List<GamePiece> matchedPieces)
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

            // Apply total damage to opponent using event-driven system
            if (totalDamage > 0)
            {
                // Determine opponent
                PlayerManager.Player opponent = playerManager.CurrentPlayer == PlayerManager.Player.Player1
                    ? PlayerManager.Player.Player2
                    : PlayerManager.Player.Player1;

                // Apply damage through centralized system (fires events automatically)
                TakeDamage(opponent, totalDamage);

                // If game over was triggered by health depletion, stop processing
                if (isGameOver)
                {
                    return;
                }
            }

            // NOTE: Bonus turn checking moved to Board.cs after all cascades complete
            // This allows rewarding strategic play that creates cascade matches
        }
        else
        {
            // Single player mode - add to player 1 score
            player1Score += totalMatchPoints;
        }

        // Award XP based on matches (Player 1 only)
        if (LevelSystem.Instance != null)
        {
            // Only award XP to Player 1 in two-player mode
            bool shouldAwardXP = true;
            if (playerManager != null && playerManager.TwoPlayerMode)
            {
                shouldAwardXP = (playerManager.CurrentPlayer == PlayerManager.Player.Player1);
            }

            if (shouldAwardXP)
            {
                int totalXP = 0;
                foreach (var kvp in colorGroups)
                {
                    int matchCount = kvp.Value.Count;
                    // XP reward based on match size
                    int xpReward = CalculateXPReward(matchCount);
                    totalXP += xpReward;
                }

                if (totalXP > 0)
                {
                    LevelSystem.Instance.AddXP(totalXP);
                }
            }
        }

        // Award gold based on gem types (Player 1 only)
        if (CurrencyManager.Instance != null && GemTypeManager.Instance != null)
        {
            // Only award gold to Player 1 in two-player mode
            bool shouldAwardGold = true;
            if (playerManager != null && playerManager.TwoPlayerMode)
            {
                shouldAwardGold = (playerManager.CurrentPlayer == PlayerManager.Player.Player1);
            }

            if (shouldAwardGold)
            {
                int totalGold = GemTypeManager.Instance.CalculateTotalGold(colorGroups);

                if (totalGold > 0)
                {
                    CurrencyManager.Instance.AddGold(totalGold);
                }
            }
        }

        // Charge spell gems based on matches (Player 1 only)
        if (SpellManager.Instance != null)
        {
            // Only charge gems for Player 1 in two-player mode
            bool shouldChargeGems = true;
            if (playerManager != null && playerManager.TwoPlayerMode)
            {
                shouldChargeGems = (playerManager.CurrentPlayer == PlayerManager.Player.Player1);
            }

            if (shouldChargeGems)
            {
                foreach (var kvp in colorGroups)
                {
                    GamePiece.PieceType gemType = kvp.Key;
                    int matchCount = kvp.Value.Count;
                    SpellManager.Instance.AddGemCharge(gemType, matchCount);
                }
            }
        }

        UpdateUI();
    }

    private int CalculateXPReward(int matchCount)
    {
        // XP rewards scale with match size
        // 3 gems = 10 XP
        // 4 gems = 25 XP
        // 5 gems = 50 XP
        // 6+ gems = 100 XP
        return matchCount switch
        {
            3 => 10,
            4 => 25,
            5 => 50,
            _ => matchCount >= 6 ? 100 : 0
        };
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

    /// <summary>
    /// Centralized method to modify a player's health
    /// ALWAYS use this method instead of directly modifying health fields
    /// Fires health change events for other systems to react to
    /// </summary>
    public void ModifyHealth(PlayerManager.Player player, int amount, HealthChangeReason reason)
    {
        if (isGameOver) return; // Don't modify health after game over

        int oldHealth = player == PlayerManager.Player.Player1 ? player1Health : player2Health;
        int newHealth = oldHealth + amount;

        // Update the health value
        if (player == PlayerManager.Player.Player1)
        {
            player1Health = newHealth;
        }
        else
        {
            player2Health = newHealth;
        }

        // Fire health changed event
        HealthEventSystem.NotifyHealthChanged(player, oldHealth, newHealth, reason);

        // Check if health depleted
        if (newHealth <= 0)
        {
            // Determine winner
            PlayerManager.Player winner = player == PlayerManager.Player.Player1
                ? PlayerManager.Player.Player2
                : PlayerManager.Player.Player1;

            // Fire health depleted event
            HealthEventSystem.NotifyHealthDepleted(player, winner);
        }

        UpdateUI();
    }

    /// <summary>
    /// Apply damage to a player (convenience method)
    /// </summary>
    public void TakeDamage(PlayerManager.Player player, int damage)
    {
        ModifyHealth(player, -damage, HealthChangeReason.Damage);
    }

    /// <summary>
    /// Heal a player (convenience method)
    /// </summary>
    public void Heal(PlayerManager.Player player, int healAmount)
    {
        ModifyHealth(player, healAmount, HealthChangeReason.Healing);
    }

    /// <summary>
    /// Apply poison damage (convenience method)
    /// </summary>
    public void ApplyPoisonDamage(PlayerManager.Player player, int damage)
    {
        ModifyHealth(player, -damage, HealthChangeReason.Poison);
    }

    /// <summary>
    /// Increase a player's maximum health
    /// </summary>
    public void IncreaseMaxHealth(PlayerManager.Player player, int amount)
    {
        if (amount <= 0) return;

        if (player == PlayerManager.Player.Player1)
        {
            player1MaxHealth += amount;
            Debug.Log($"Player 1 max health increased by {amount}! New max: {player1MaxHealth}");
        }
        else
        {
            player2MaxHealth += amount;
            Debug.Log($"Player 2 max health increased by {amount}! New max: {player2MaxHealth}");
        }

        UpdateUI();
    }

    /// <summary>
    /// Set a player's maximum health to a specific value
    /// </summary>
    public void SetMaxHealth(PlayerManager.Player player, int newMaxHealth)
    {
        if (newMaxHealth <= 0) return;

        if (player == PlayerManager.Player.Player1)
        {
            player1MaxHealth = newMaxHealth;
            // Clamp current health to new max
            player1Health = Mathf.Min(player1Health, player1MaxHealth);
            Debug.Log($"Player 1 max health set to {player1MaxHealth}");
        }
        else
        {
            player2MaxHealth = newMaxHealth;
            // Clamp current health to new max
            player2Health = Mathf.Min(player2Health, player2MaxHealth);
            Debug.Log($"Player 2 max health set to {player2MaxHealth}");
        }

        UpdateUI();
    }

    /// <summary>
    /// Get a player's current max health
    /// </summary>
    public int GetMaxHealth(PlayerManager.Player player)
    {
        return player == PlayerManager.Player.Player1 ? player1MaxHealth : player2MaxHealth;
    }

    /// <summary>
    /// Get a player's current health
    /// </summary>
    public int GetCurrentHealth(PlayerManager.Player player)
    {
        return player == PlayerManager.Player.Player1 ? player1Health : player2Health;
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

        // Debug logging removed for performance (was called on every UI update)

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
            player1HealthText.text = $"HP: {displayHealth}/{player1MaxHealth}";
        }

        // Update Player 2 health (clamp to 0)
        if (player2HealthText != null)
        {
            int displayHealth = Mathf.Max(0, player2Health);
            player2HealthText.text = $"HP: {displayHealth}/{player2MaxHealth}";
        }

        // Removed expensive GetPossibleMovesCount() debug logging for performance
        // (was scanning entire board on every UI update)
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

        Debug.Log(gameOverMessage);
        UpdateUI();
    }

    public void ResetGame()
    {
        player1Score = 0;
        player2Score = 0;
        player1Health = startingHealth;
        player2Health = startingHealth;
        player1MaxHealth = startingHealth;
        player2MaxHealth = startingHealth;
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
        int oldP1Health = player1Health;
        int oldP2Health = player2Health;

        player1Health = startingHealth;
        player2Health = startingHealth;

        Debug.Log($"Health reset to {startingHealth} for both players");

        // Fire reset events (won't trigger game over since health is being restored)
        HealthEventSystem.NotifyHealthChanged(PlayerManager.Player.Player1, oldP1Health, startingHealth, HealthChangeReason.Reset);
        HealthEventSystem.NotifyHealthChanged(PlayerManager.Player.Player2, oldP2Health, startingHealth, HealthChangeReason.Reset);

        UpdateUI();
    }
}
