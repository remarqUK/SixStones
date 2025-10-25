using UnityEngine;

/// <summary>
/// Manages game speed settings - affects gem falling speed and swap animations
/// </summary>
public class GameSpeedSettings : MonoBehaviour
{
    private static GameSpeedSettings instance;
    public static GameSpeedSettings Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<GameSpeedSettings>();
            }
            return instance;
        }
    }

    [Header("Game Speed")]
    [SerializeField] private GameSpeed currentSpeed = GameSpeed.Medium;

    // Speed multipliers - lower duration = faster animation
    private const float SLOW_MULTIPLIER = 1.5f;    // 1.5x slower (animations take 50% longer)
    private const float MEDIUM_MULTIPLIER = 1.0f;  // Normal speed
    private const float FAST_MULTIPLIER = 0.6f;    // 1.67x faster (animations take 40% less time)

    public enum GameSpeed
    {
        Slow = 0,
        Medium = 1,
        Fast = 2
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Get the animation speed multiplier based on current game speed
    /// This is applied to animation durations (lower = faster)
    /// </summary>
    public float GetSpeedMultiplier()
    {
        return currentSpeed switch
        {
            GameSpeed.Slow => SLOW_MULTIPLIER,
            GameSpeed.Medium => MEDIUM_MULTIPLIER,
            GameSpeed.Fast => FAST_MULTIPLIER,
            _ => MEDIUM_MULTIPLIER
        };
    }

    /// <summary>
    /// Set the game speed
    /// </summary>
    public void SetGameSpeed(GameSpeed speed)
    {
        currentSpeed = speed;
        Debug.Log($"Game speed set to: {speed} (multiplier: {GetSpeedMultiplier()})");
    }

    /// <summary>
    /// Set game speed from dropdown index (0=Slow, 1=Medium, 2=Fast)
    /// </summary>
    public void SetGameSpeedFromDropdown(int index)
    {
        if (index >= 0 && index <= 2)
        {
            SetGameSpeed((GameSpeed)index);
        }
    }

    /// <summary>
    /// Get the current game speed
    /// </summary>
    public GameSpeed GetCurrentSpeed()
    {
        return currentSpeed;
    }

    /// <summary>
    /// Apply speed multiplier to a base duration
    /// </summary>
    public float GetAdjustedDuration(float baseDuration)
    {
        return baseDuration * GetSpeedMultiplier();
    }
}
