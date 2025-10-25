using UnityEngine;
using UnityEngine.Events;

public class LevelSystem : MonoBehaviour
{
    private static LevelSystem instance;
    public static LevelSystem Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<LevelSystem>();
            }
            return instance;
        }
    }

    [Header("Level Configuration")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentXP = 0;
    [SerializeField] private int baseXPRequired = 100;
    [SerializeField] private float xpMultiplier = 1.5f;

    [Header("Events")]
    public UnityEvent<int> onLevelUp = new UnityEvent<int>(); // Passes new level
    public UnityEvent<int, int, int> onXPChanged = new UnityEvent<int, int, int>(); // Passes currentXP, requiredXP, level

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Notify UI of initial state
        NotifyXPChanged();
    }

    /// <summary>
    /// Calculates the XP required to reach the next level using exponential growth
    /// Formula: baseXP * (multiplier ^ (level - 1))
    /// </summary>
    public int GetXPRequiredForLevel(int level)
    {
        if (level <= 1) return baseXPRequired;
        return Mathf.RoundToInt(baseXPRequired * Mathf.Pow(xpMultiplier, level - 1));
    }

    /// <summary>
    /// Gets the XP required to reach the next level from current level
    /// </summary>
    public int GetXPRequiredForNextLevel()
    {
        return GetXPRequiredForLevel(currentLevel);
    }

    /// <summary>
    /// Adds experience points and handles level ups
    /// </summary>
    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        currentXP += amount;
        Debug.Log($"Gained {amount} XP! Total: {currentXP}/{GetXPRequiredForNextLevel()}");

        // Check for level up(s)
        while (currentXP >= GetXPRequiredForNextLevel())
        {
            LevelUp();
        }

        NotifyXPChanged();
    }

    private void LevelUp()
    {
        int xpRequired = GetXPRequiredForNextLevel();
        currentXP -= xpRequired;
        currentLevel++;

        Debug.Log($"LEVEL UP! Now level {currentLevel}");
        onLevelUp.Invoke(currentLevel);
    }

    private void NotifyXPChanged()
    {
        onXPChanged.Invoke(currentXP, GetXPRequiredForNextLevel(), currentLevel);
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public int GetCurrentXP()
    {
        return currentXP;
    }

    public float GetXPProgress()
    {
        int required = GetXPRequiredForNextLevel();
        return required > 0 ? (float)currentXP / required : 0f;
    }

    /// <summary>
    /// Resets level and XP to starting values
    /// </summary>
    public void ResetProgress()
    {
        currentLevel = 1;
        currentXP = 0;
        NotifyXPChanged();
        Debug.Log("Level progress reset");
    }
}
