using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// Centralized events system for Match3 game using Unity 6 best practices
/// </summary>
public class GameEvents : MonoBehaviour
{
    public static GameEvents Instance { get; private set; }

    [System.Serializable]
    public class PiecesMatchedEvent : UnityEvent<List<GamePiece>> { }

    [System.Serializable]
    public class ScoreChangedEvent : UnityEvent<int> { }

    [System.Serializable]
    public class MovesChangedEvent : UnityEvent<int> { }

    [System.Serializable]
    public class GameOverEvent : UnityEvent { }

    [Header("Game Events")]
    public PiecesMatchedEvent OnPiecesMatched = new PiecesMatchedEvent();
    public ScoreChangedEvent OnScoreChanged = new ScoreChangedEvent();
    public MovesChangedEvent OnMovesChanged = new MovesChangedEvent();
    public GameOverEvent OnGameOver = new GameOverEvent();

    private void Awake()
    {
        // Singleton pattern for easy access
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
