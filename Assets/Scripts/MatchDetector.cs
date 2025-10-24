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
}
