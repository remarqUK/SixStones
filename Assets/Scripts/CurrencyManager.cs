using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages player currency (gold) for purchasing items
/// </summary>
public class CurrencyManager : MonoBehaviour
{
    private static CurrencyManager instance;
    public static CurrencyManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<CurrencyManager>();
            }
            return instance;
        }
    }

    [Header("Gold")]
    [SerializeField] private int currentGold = 0;
    [SerializeField] private int startingGold = 100;

    [Header("Events")]
    public UnityEvent<int> onGoldChanged = new UnityEvent<int>(); // Passes current gold
    public UnityEvent<int> onGoldGained = new UnityEvent<int>(); // Passes amount gained
    public UnityEvent<int> onGoldSpent = new UnityEvent<int>(); // Passes amount spent
    public UnityEvent onInsufficientFunds = new UnityEvent(); // Fired when trying to buy something too expensive

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
        // Set starting gold
        if (currentGold == 0)
        {
            currentGold = startingGold;
        }

        // Notify UI of initial state
        NotifyGoldChanged();
    }

    #region Gold Management

    /// <summary>
    /// Add gold to the player's total
    /// </summary>
    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        currentGold += amount;
        Debug.Log($"Gained {amount} gold! Total: {currentGold}");

        onGoldGained.Invoke(amount);
        NotifyGoldChanged();
    }

    /// <summary>
    /// Try to spend gold. Returns true if successful, false if insufficient funds
    /// </summary>
    public bool SpendGold(int amount)
    {
        if (amount <= 0) return false;

        if (currentGold >= amount)
        {
            currentGold -= amount;
            Debug.Log($"Spent {amount} gold. Remaining: {currentGold}");

            onGoldSpent.Invoke(amount);
            NotifyGoldChanged();
            return true;
        }
        else
        {
            Debug.Log($"Insufficient funds! Need {amount}, have {currentGold}");
            onInsufficientFunds.Invoke();
            return false;
        }
    }

    /// <summary>
    /// Check if player can afford something
    /// </summary>
    public bool CanAfford(int cost)
    {
        return currentGold >= cost;
    }

    /// <summary>
    /// Set gold to a specific amount (for debugging/cheats)
    /// </summary>
    public void SetGold(int amount)
    {
        currentGold = Mathf.Max(0, amount);
        NotifyGoldChanged();
        Debug.Log($"Gold set to {currentGold}");
    }

    /// <summary>
    /// Get current gold amount
    /// </summary>
    public int GetCurrentGold()
    {
        return currentGold;
    }

    /// <summary>
    /// Reset gold to starting amount
    /// </summary>
    public void ResetGold()
    {
        currentGold = startingGold;
        NotifyGoldChanged();
        Debug.Log($"Gold reset to {startingGold}");
    }

    private void NotifyGoldChanged()
    {
        onGoldChanged.Invoke(currentGold);
    }

    #endregion

    #region Gold Rewards

    /// <summary>
    /// Calculate gold reward based on match size
    /// Matches give gold in addition to XP
    /// </summary>
    public static int CalculateGoldReward(int matchCount)
    {
        // Gold rewards scale with match size
        // 3 gems = 5 gold
        // 4 gems = 12 gold
        // 5 gems = 25 gold
        // 6+ gems = 50 gold
        return matchCount switch
        {
            3 => 5,
            4 => 12,
            5 => 25,
            _ => matchCount >= 6 ? 50 : 0
        };
    }

    /// <summary>
    /// Calculate gold reward from level up
    /// </summary>
    public static int CalculateLevelUpBonus(int newLevel)
    {
        // Level up bonus = level * 50 gold
        return newLevel * 50;
    }

    /// <summary>
    /// Award gold based on player level (daily bonus, quest reward, etc.)
    /// </summary>
    public void AwardLevelScaledGold(int baseAmount, float levelMultiplier = 0.1f)
    {
        int playerLevel = LevelSystem.Instance != null ? LevelSystem.Instance.GetCurrentLevel() : 1;
        int bonus = Mathf.RoundToInt(baseAmount * levelMultiplier * playerLevel);
        int total = baseAmount + bonus;

        AddGold(total);
        Debug.Log($"Awarded {total} gold ({baseAmount} base + {bonus} level bonus)");
    }

    #endregion

    #region Save/Load Support (Placeholder)

    /// <summary>
    /// Get save data for persistence
    /// </summary>
    public CurrencySaveData GetSaveData()
    {
        return new CurrencySaveData
        {
            gold = currentGold
        };
    }

    /// <summary>
    /// Load from save data
    /// </summary>
    public void LoadSaveData(CurrencySaveData data)
    {
        if (data != null)
        {
            currentGold = data.gold;
            NotifyGoldChanged();
            Debug.Log($"Loaded gold: {currentGold}");
        }
    }

    #endregion
}

/// <summary>
/// Serializable data for saving/loading currency
/// </summary>
[System.Serializable]
public class CurrencySaveData
{
    public int gold;
}
