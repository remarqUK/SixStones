using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Match3/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Board Configuration")]
    [Range(4, 12)] public int boardWidth = 8;
    [Range(4, 12)] public int boardHeight = 8;
    [Range(0.5f, 2f)] public float cellSize = 1f;

    [Header("Animation Settings")]
    [Range(0.1f, 1f)] public float swapDuration = 0.2f;
    [Range(0.1f, 1f)] public float fallDuration = 0.3f;
    [Range(0.1f, 1f)] public float destroyDuration = 0.3f;

    [Header("Gameplay Settings")]
    [Range(1, 100)] public int pointsPerPiece = 1;
    [Range(10, 100)] public int maxMoves = 30;
    [Range(3, 5)] public int minMatchLength = 3;

    [Header("Input Settings")]
    [Range(0.1f, 2f)] public float dragThreshold = 0.5f;

    [Header("Piece Colors")]
    public Color redColor = new Color(0.9f, 0.2f, 0.2f);
    public Color blueColor = new Color(0.2f, 0.5f, 0.9f);
    public Color greenColor = new Color(0.2f, 0.8f, 0.3f);
    public Color yellowColor = new Color(0.95f, 0.85f, 0.2f);
    public Color purpleColor = new Color(0.7f, 0.2f, 0.8f);
    public Color orangeColor = new Color(0.95f, 0.5f, 0.1f);

    public Color GetColorForType(GamePiece.PieceType type)
    {
        return type switch
        {
            GamePiece.PieceType.Red => redColor,
            GamePiece.PieceType.Blue => blueColor,
            GamePiece.PieceType.Green => greenColor,
            GamePiece.PieceType.Yellow => yellowColor,
            GamePiece.PieceType.Purple => purpleColor,
            GamePiece.PieceType.Orange => orangeColor,
            _ => Color.white
        };
    }
}
