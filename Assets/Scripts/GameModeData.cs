using UnityEngine;

/// <summary>
/// Defines a game mode with its specific rules and behaviors
/// Unity 6 best practice: Data-driven mode system for easy expansion
/// </summary>
[CreateAssetMenu(fileName = "NewGameMode", menuName = "Match3/Game Mode")]
public class GameModeData : ScriptableObject
{
    [Header("Mode Identity")]
    public string modeName = "Standard";
    [TextArea(2, 4)]
    public string description = "Classic match-3 gameplay";

    [Header("Player Configuration")]
    [Tooltip("Number of players for this mode")]
    public int playerCount = 2;

    [Tooltip("Is Player 2 controlled by CPU?")]
    public bool player2IsCPU = false;

    [Header("Board Configuration")]
    [Tooltip("Board width (0 = use default)")]
    [Range(0, 20)]
    public int boardWidth = 0;

    [Tooltip("Board height (0 = use default)")]
    [Range(0, 20)]
    public int boardHeight = 0;

    [Header("Piece Spawning")]
    [Tooltip("Should new pieces spawn from the top to fill empty spaces?")]
    public bool spawnNewPieces = true;

    [Tooltip("Should pieces fall down to fill gaps?")]
    public bool piecesCanFall = true;

    [Header("Gameplay Rules")]
    [Tooltip("Number of moves allowed (0 = unlimited)")]
    public int moveLimit = 30;

    [Tooltip("Minimum pieces required for a match")]
    [Range(3, 5)]
    public int minMatchLength = 3;

    [Tooltip("Are cascade/combo matches allowed?")]
    public bool allowCascades = true;

    [Header("Scoring")]
    [Tooltip("Score based on matches (standard) or bottom row hits (drop mode)")]
    public bool scoreOnBottomRowHit = false;

    [Tooltip("Gem type that scores when hitting bottom row (drop mode only)")]
    public GamePiece.PieceType bottomRowScoreGem = GamePiece.PieceType.Red;

    [Tooltip("Points awarded when target gem hits bottom row")]
    public int bottomRowHitPoints = 10;

    [Tooltip("Points for matching exactly 3 pieces")]
    public int match3Points = 3;

    [Tooltip("Points for matching exactly 4 pieces")]
    public int match4Points = 5;

    [Tooltip("Points for matching 5 or more pieces")]
    public int match5PlusPoints = 10;

    [Tooltip("Points multiplier for this mode")]
    [Range(0.5f, 5f)]
    public float scoreMultiplier = 1f;

    [Tooltip("Bonus points for clearing the entire board (Solve mode)")]
    public int boardClearBonus = 1000;

    [Header("Hint System")]
    [Tooltip("Show hint after player is idle for a while")]
    public bool showHints = true;

    [Tooltip("Seconds of inactivity before showing hint")]
    [Range(3f, 10f)]
    public float hintDelay = 5f;

    [Header("Win/Loss Conditions")]
    [Tooltip("Win condition: Clear all pieces")]
    public bool winOnBoardClear = false;

    [Tooltip("Lose condition: No possible moves remaining")]
    public bool loseOnNoMoves = false;

    [Tooltip("Target score to win (0 = no score target)")]
    public int targetScore = 0;

    [Header("Visual")]
    public Color modeColor = Color.white;
    public Sprite modeIcon;

    /// <summary>
    /// Calculate points for a match based on number of pieces matched
    /// </summary>
    public int GetPointsForMatch(int pieceCount)
    {
        int basePoints;

        if (pieceCount == 3)
            basePoints = match3Points;
        else if (pieceCount == 4)
            basePoints = match4Points;
        else // 5 or more
            basePoints = match5PlusPoints;

        return Mathf.RoundToInt(basePoints * scoreMultiplier);
    }

    /// <summary>
    /// Get a description of the mode's key features
    /// </summary>
    public string GetModeInfo()
    {
        string info = $"<b>{modeName}</b>\n{description}\n\n";

        if (!spawnNewPieces)
            info += "• No new pieces spawn\n";

        if (moveLimit > 0)
            info += $"• {moveLimit} moves allowed\n";
        else
            info += "• Unlimited moves\n";

        if (winOnBoardClear)
            info += "• Clear all pieces to win\n";

        if (loseOnNoMoves)
            info += "• Game over if no moves remain\n";

        if (targetScore > 0)
            info += $"• Reach {targetScore} points to win\n";

        return info;
    }
}
