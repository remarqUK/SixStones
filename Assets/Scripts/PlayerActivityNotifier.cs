using System;

/// <summary>
/// Centralized event system for player activity notifications
/// Allows decoupling between input sources and systems that react to player activity (e.g., HintSystem)
/// </summary>
public static class PlayerActivityNotifier
{
    /// <summary>
    /// Event fired whenever player performs any activity (movement, selection, swap, etc.)
    /// </summary>
    public static event Action OnPlayerActivity;

    /// <summary>
    /// Notify all subscribers that player activity has occurred
    /// Call this from any input handler when the player performs an action
    /// </summary>
    public static void NotifyActivity()
    {
        OnPlayerActivity?.Invoke();
    }

    /// <summary>
    /// Clear all event subscribers (useful for cleanup/testing)
    /// </summary>
    public static void ClearSubscribers()
    {
        OnPlayerActivity = null;
    }
}
