using System.Collections.Generic;
using UnityEngine;

public class MatchDetector
{
    private Board board;
    private const int MIN_MATCH_LENGTH = 3;

    public MatchDetector(Board board)
    {
        this.board = board;
    }

    public List<GamePiece> FindAllMatches()
    {
        HashSet<GamePiece> allMatches = new HashSet<GamePiece>();

        // Check horizontal matches
        for (int y = 0; y < board.Height; y++)
        {
            for (int x = 0; x < board.Width; x++)
            {
                List<GamePiece> horizontalMatch = CheckHorizontalMatch(x, y);
                if (horizontalMatch.Count >= MIN_MATCH_LENGTH)
                {
                    foreach (GamePiece piece in horizontalMatch)
                    {
                        allMatches.Add(piece);
                    }
                }
            }
        }

        // Check vertical matches
        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                List<GamePiece> verticalMatch = CheckVerticalMatch(x, y);
                if (verticalMatch.Count >= MIN_MATCH_LENGTH)
                {
                    foreach (GamePiece piece in verticalMatch)
                    {
                        allMatches.Add(piece);
                    }
                }
            }
        }

        return new List<GamePiece>(allMatches);
    }

    private List<GamePiece> CheckHorizontalMatch(int startX, int startY)
    {
        List<GamePiece> matchedPieces = new List<GamePiece>();

        GamePiece startPiece = board.GetPieceAt(startX, startY);
        if (startPiece == null) return matchedPieces;

        matchedPieces.Add(startPiece);
        GamePiece.PieceType targetType = startPiece.Type;

        // Check to the right
        for (int x = startX + 1; x < board.Width; x++)
        {
            GamePiece piece = board.GetPieceAt(x, startY);
            if (piece == null || piece.Type != targetType)
            {
                break;
            }
            matchedPieces.Add(piece);
        }

        if (matchedPieces.Count < MIN_MATCH_LENGTH)
        {
            matchedPieces.Clear();
        }

        return matchedPieces;
    }

    private List<GamePiece> CheckVerticalMatch(int startX, int startY)
    {
        List<GamePiece> matchedPieces = new List<GamePiece>();

        GamePiece startPiece = board.GetPieceAt(startX, startY);
        if (startPiece == null) return matchedPieces;

        matchedPieces.Add(startPiece);
        GamePiece.PieceType targetType = startPiece.Type;

        // Check upwards
        for (int y = startY + 1; y < board.Height; y++)
        {
            GamePiece piece = board.GetPieceAt(startX, y);
            if (piece == null || piece.Type != targetType)
            {
                break;
            }
            matchedPieces.Add(piece);
        }

        if (matchedPieces.Count < MIN_MATCH_LENGTH)
        {
            matchedPieces.Clear();
        }

        return matchedPieces;
    }

    public bool HasMatchAt(int x, int y)
    {
        List<GamePiece> horizontalMatch = CheckHorizontalMatch(x, y);
        List<GamePiece> verticalMatch = CheckVerticalMatch(x, y);

        return horizontalMatch.Count >= MIN_MATCH_LENGTH || verticalMatch.Count >= MIN_MATCH_LENGTH;
    }

    /// <summary>
    /// Get all pieces that would match at a given position (checking all directions)
    /// Returns a HashSet to avoid duplicates
    /// </summary>
    public HashSet<Vector2Int> GetMatchPositionsAt(int x, int y)
    {
        HashSet<Vector2Int> matchPositions = new HashSet<Vector2Int>();
        GamePiece piece = board.GetPieceAt(x, y);

        if (piece == null) return matchPositions;

        GamePiece.PieceType targetType = piece.Type;

        // Check horizontal (left and right from this position)
        List<Vector2Int> horizontalPositions = new List<Vector2Int>();
        horizontalPositions.Add(new Vector2Int(x, y));

        // Check left
        for (int checkX = x - 1; checkX >= 0; checkX--)
        {
            GamePiece p = board.GetPieceAt(checkX, y);
            if (p == null || p.Type != targetType) break;
            horizontalPositions.Add(new Vector2Int(checkX, y));
        }

        // Check right
        for (int checkX = x + 1; checkX < board.Width; checkX++)
        {
            GamePiece p = board.GetPieceAt(checkX, y);
            if (p == null || p.Type != targetType) break;
            horizontalPositions.Add(new Vector2Int(checkX, y));
        }

        // Add horizontal matches if valid
        if (horizontalPositions.Count >= MIN_MATCH_LENGTH)
        {
            foreach (var pos in horizontalPositions)
            {
                matchPositions.Add(pos);
            }
        }

        // Check vertical (down and up from this position)
        List<Vector2Int> verticalPositions = new List<Vector2Int>();
        verticalPositions.Add(new Vector2Int(x, y));

        // Check down
        for (int checkY = y - 1; checkY >= 0; checkY--)
        {
            GamePiece p = board.GetPieceAt(x, checkY);
            if (p == null || p.Type != targetType) break;
            verticalPositions.Add(new Vector2Int(x, checkY));
        }

        // Check up
        for (int checkY = y + 1; checkY < board.Height; checkY++)
        {
            GamePiece p = board.GetPieceAt(x, checkY);
            if (p == null || p.Type != targetType) break;
            verticalPositions.Add(new Vector2Int(x, checkY));
        }

        // Add vertical matches if valid
        if (verticalPositions.Count >= MIN_MATCH_LENGTH)
        {
            foreach (var pos in verticalPositions)
            {
                matchPositions.Add(pos);
            }
        }

        return matchPositions;
    }

    /// <summary>
    /// Get the size of the largest match at a position
    /// </summary>
    public int GetLargestMatchSizeAt(int x, int y)
    {
        return GetMatchPositionsAt(x, y).Count;
    }
}
