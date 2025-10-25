using System;
using UnityEngine;

/// <summary>
/// Event arguments for health changes
/// </summary>
public class HealthChangedEventArgs : EventArgs
{
    public PlayerManager.Player Player { get; }
    public int OldHealth { get; }
    public int NewHealth { get; }
    public int ChangeAmount { get; }
    public HealthChangeReason Reason { get; }

    public HealthChangedEventArgs(PlayerManager.Player player, int oldHealth, int newHealth, HealthChangeReason reason)
    {
        Player = player;
        OldHealth = oldHealth;
        NewHealth = newHealth;
        ChangeAmount = newHealth - oldHealth;
        Reason = reason;
    }
}

/// <summary>
/// Event arguments for when a player's health is depleted
/// </summary>
public class HealthDepletedEventArgs : EventArgs
{
    public PlayerManager.Player Player { get; }
    public PlayerManager.Player Winner { get; }

    public HealthDepletedEventArgs(PlayerManager.Player player, PlayerManager.Player winner)
    {
        Player = player;
        Winner = winner;
    }
}

/// <summary>
/// Reason for health change - useful for triggering different effects
/// </summary>
public enum HealthChangeReason
{
    Damage,        // Damage from matching damage gems
    Healing,       // Healing from items/abilities
    Poison,        // Poison damage over time
    Reset          // Health reset (game restart)
}

/// <summary>
/// Static event system for health changes
/// Allows multiple systems to react to HP changes (UI, sound, achievements, etc.)
/// </summary>
public static class HealthEventSystem
{
    /// <summary>
    /// Fired whenever any player's health changes
    /// Subscribe to this for: health bar animations, damage numbers, screen shake
    /// </summary>
    public static event EventHandler<HealthChangedEventArgs> OnHealthChanged;

    /// <summary>
    /// Fired when a player's health reaches zero
    /// Subscribe to this for: game over logic, victory animations, achievements
    /// </summary>
    public static event EventHandler<HealthDepletedEventArgs> OnHealthDepleted;

    /// <summary>
    /// Notify all subscribers that health has changed
    /// </summary>
    public static void NotifyHealthChanged(PlayerManager.Player player, int oldHealth, int newHealth, HealthChangeReason reason)
    {
        OnHealthChanged?.Invoke(null, new HealthChangedEventArgs(player, oldHealth, newHealth, reason));

        // Log for debugging
        string changeText = newHealth > oldHealth ? "gained" : "lost";
        int amount = Mathf.Abs(newHealth - oldHealth);
        Debug.Log($"HealthEvent: {player} {changeText} {amount} HP ({oldHealth} → {newHealth}) - Reason: {reason}");
    }

    /// <summary>
    /// Notify all subscribers that a player's health has been depleted
    /// </summary>
    public static void NotifyHealthDepleted(PlayerManager.Player player, PlayerManager.Player winner)
    {
        OnHealthDepleted?.Invoke(null, new HealthDepletedEventArgs(player, winner));
        Debug.Log($"HealthEvent: {player} HP depleted! {winner} wins!");
    }

    /// <summary>
    /// Clear all subscribers (useful for cleanup or testing)
    /// </summary>
    public static void ClearAllSubscribers()
    {
        OnHealthChanged = null;
        OnHealthDepleted = null;
    }
}
