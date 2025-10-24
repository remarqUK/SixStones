using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Object pool for GamePiece objects using Unity 6 best practices
/// Reduces garbage collection by reusing objects instead of Instantiate/Destroy
/// </summary>
public class PiecePool : MonoBehaviour
{
    [SerializeField] private GameObject piecePrefab;
    [SerializeField] private int initialPoolSize = 100;

    private Queue<GamePieceModern> availablePieces = new Queue<GamePieceModern>();
    private HashSet<GamePieceModern> activePieces = new HashSet<GamePieceModern>();

    private void Start()
    {
        // Pre-warm the pool
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewPiece();
        }
    }

    private GamePieceModern CreateNewPiece()
    {
        GameObject obj = Instantiate(piecePrefab, transform);
        GamePieceModern piece = obj.GetComponent<GamePieceModern>();

        if (piece == null)
        {
            Debug.LogError("Prefab must have GamePieceModern component!");
            Destroy(obj);
            return null;
        }

        obj.SetActive(false);
        availablePieces.Enqueue(piece);
        return piece;
    }

    public GamePieceModern Get()
    {
        GamePieceModern piece;

        if (availablePieces.Count > 0)
        {
            piece = availablePieces.Dequeue();
        }
        else
        {
            // Pool exhausted, create new piece
            piece = CreateNewPiece();
            if (piece == null) return null;
        }

        piece.gameObject.SetActive(true);
        activePieces.Add(piece);
        return piece;
    }

    public void Return(GamePieceModern piece)
    {
        if (piece == null) return;

        if (activePieces.Remove(piece))
        {
            // Reset piece state before returning to pool
            piece.ResetForPool();

            piece.gameObject.SetActive(false);
            piece.transform.SetParent(transform);
            piece.transform.localPosition = Vector3.zero;
            piece.transform.localScale = Vector3.one;

            availablePieces.Enqueue(piece);
        }
    }

    public void ReturnAll()
    {
        // Convert to array to avoid modification during iteration
        GamePieceModern[] pieces = new GamePieceModern[activePieces.Count];
        activePieces.CopyTo(pieces);

        foreach (GamePieceModern piece in pieces)
        {
            Return(piece);
        }
    }

    public int ActiveCount => activePieces.Count;
    public int AvailableCount => availablePieces.Count;
    public int TotalCount => ActiveCount + AvailableCount;
}
