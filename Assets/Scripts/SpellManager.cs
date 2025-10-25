using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

/// <summary>
/// Manages player spells, spell slots, and casting mechanics
/// </summary>
public class SpellManager : MonoBehaviour
{
    private static SpellManager instance;
    public static SpellManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<SpellManager>();
            }
            return instance;
        }
    }

    [Header("Spell Management Mode")]
    [Tooltip("Use D&D-style spell slots or mana system?")]
    public bool useSpellSlots = true;

    [Header("Mana System (if not using spell slots)")]
    [SerializeField] private int currentMana = 100;
    [SerializeField] private int maxMana = 100;
    [SerializeField] private int manaRegenPerTurn = 10;

    [Header("Spell Slots (D&D Style)")]
    [Tooltip("Available spell slots per level [Level 1-9]")]
    [SerializeField] private int[] maxSpellSlots = new int[9] { 2, 0, 0, 0, 0, 0, 0, 0, 0 };
    [SerializeField] private int[] currentSpellSlots = new int[9];

    [Header("Known Spells")]
    [Tooltip("All spells the player has learned")]
    [SerializeField] private List<SpellData> knownSpells = new List<SpellData>();

    [Tooltip("Spells currently prepared (ready to cast)")]
    [SerializeField] private List<SpellData> preparedSpells = new List<SpellData>();

    [Tooltip("Maximum number of spells that can be prepared")]
    [SerializeField] private int maxPreparedSpells = 5;

    [Header("Concentration")]
    [Tooltip("Currently active concentration spell")]
    [SerializeField] private SpellData concentrationSpell = null;
    [SerializeField] private int concentrationTurnsRemaining = 0;

    [Header("Cooldowns")]
    private Dictionary<SpellData, int> spellCooldowns = new Dictionary<SpellData, int>();

    [Header("Gem Charge System")]
    [Tooltip("Track gem matches for spell charging")]
    private Dictionary<GamePiece.PieceType, int> gemCharges = new Dictionary<GamePiece.PieceType, int>();

    [Header("Events")]
    public UnityEvent<SpellData, int> onSpellCast = new UnityEvent<SpellData, int>(); // Spell, slot level
    public UnityEvent<int, int> onManaChanged = new UnityEvent<int, int>(); // current, max
    public UnityEvent onSpellSlotsChanged = new UnityEvent();
    public UnityEvent<SpellData> onConcentrationBroken = new UnityEvent<SpellData>();

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

        // Initialize spell slots
        currentSpellSlots = new int[9];
        for (int i = 0; i < 9; i++)
        {
            currentSpellSlots[i] = maxSpellSlots[i];
        }

        InitializeGemCharges();
    }

    private void InitializeGemCharges()
    {
        foreach (GamePiece.PieceType type in System.Enum.GetValues(typeof(GamePiece.PieceType)))
        {
            gemCharges[type] = 0;
        }
    }

    #region Spell Learning and Preparation

    /// <summary>
    /// Learn a new spell (add to known spells)
    /// </summary>
    public bool LearnSpell(SpellData spell)
    {
        if (spell == null) return false;

        if (knownSpells.Contains(spell))
        {
            Debug.Log($"Already know {spell.spellName}");
            return false;
        }

        // Check level requirement
        if (LevelSystem.Instance != null && LevelSystem.Instance.GetCurrentLevel() < spell.minimumPlayerLevel)
        {
            Debug.Log($"Must be level {spell.minimumPlayerLevel} to learn {spell.spellName}");
            return false;
        }

        knownSpells.Add(spell);
        Debug.Log($"Learned new spell: {spell.spellName}!");
        return true;
    }

    /// <summary>
    /// Prepare a spell for casting (D&D mechanic)
    /// </summary>
    public bool PrepareSpell(SpellData spell)
    {
        if (!knownSpells.Contains(spell))
        {
            Debug.Log($"Don't know {spell.spellName}");
            return false;
        }

        if (preparedSpells.Contains(spell))
        {
            Debug.Log($"{spell.spellName} is already prepared");
            return false;
        }

        if (preparedSpells.Count >= maxPreparedSpells)
        {
            Debug.Log($"Cannot prepare more than {maxPreparedSpells} spells");
            return false;
        }

        preparedSpells.Add(spell);
        Debug.Log($"Prepared {spell.spellName}");
        return true;
    }

    /// <summary>
    /// Unprepare a spell
    /// </summary>
    public void UnprepareSpell(SpellData spell)
    {
        preparedSpells.Remove(spell);
        Debug.Log($"Unprepared {spell.spellName}");
    }

    /// <summary>
    /// Clear all prepared spells (for long rest)
    /// </summary>
    public void ClearPreparedSpells()
    {
        preparedSpells.Clear();
    }

    #endregion

    #region Spell Casting

    /// <summary>
    /// Attempts to cast a spell at a specific slot level
    /// </summary>
    public bool CastSpell(SpellData spell, int slotLevel = -1)
    {
        if (spell == null) return false;

        // Use spell's base level if no slot level specified
        if (slotLevel == -1)
        {
            slotLevel = (int)spell.level;
        }

        // Cantrips don't cost resources
        bool isCantrip = spell.level == SpellLevel.Cantrip;

        // Check if spell is prepared (or if it's a cantrip)
        if (!isCantrip && !preparedSpells.Contains(spell))
        {
            Debug.Log($"{spell.spellName} is not prepared!");
            return false;
        }

        // Check cooldown
        if (spellCooldowns.ContainsKey(spell) && spellCooldowns[spell] > 0)
        {
            Debug.Log($"{spell.spellName} is on cooldown for {spellCooldowns[spell]} more turns");
            return false;
        }

        // Check gem requirements
        if (spell.gemsRequiredToCast > 0)
        {
            int totalGemCharge = 0;
            foreach (var gemType in spell.associatedGemTypes)
            {
                if (gemCharges.ContainsKey(gemType))
                {
                    totalGemCharge += gemCharges[gemType];
                }
            }

            if (totalGemCharge < spell.gemsRequiredToCast)
            {
                Debug.Log($"{spell.spellName} requires {spell.gemsRequiredToCast} matching gems, only have {totalGemCharge}");
                return false;
            }
        }

        // Check resources (mana or spell slots)
        if (!isCantrip)
        {
            if (useSpellSlots)
            {
                if (!HasSpellSlot(slotLevel))
                {
                    Debug.Log($"No level {slotLevel} spell slots available!");
                    return false;
                }
            }
            else
            {
                if (currentMana < spell.manaCost)
                {
                    Debug.Log($"Not enough mana! Need {spell.manaCost}, have {currentMana}");
                    return false;
                }
            }
        }

        // Check concentration (can only concentrate on one spell at a time)
        if (spell.requiresConcentration && concentrationSpell != null)
        {
            Debug.Log($"Breaking concentration on {concentrationSpell.spellName}");
            BreakConcentration();
        }

        // Consume resources
        if (!isCantrip)
        {
            if (useSpellSlots)
            {
                UseSpellSlot(slotLevel);
            }
            else
            {
                SpendMana(spell.manaCost);
            }
        }

        // Consume gem charges
        if (spell.gemsRequiredToCast > 0)
        {
            ConsumeGemCharges(spell);
        }

        // Apply cooldown
        if (spell.cooldownTurns > 0)
        {
            spellCooldowns[spell] = spell.cooldownTurns;
        }

        // Set concentration
        if (spell.requiresConcentration)
        {
            concentrationSpell = spell;
            concentrationTurnsRemaining = spell.durationValue;
        }

        // Fire event
        onSpellCast.Invoke(spell, slotLevel);

        Debug.Log($"Cast {spell.spellName} at level {slotLevel}!");
        return true;
    }

    private void ConsumeGemCharges(SpellData spell)
    {
        int remaining = spell.gemsRequiredToCast;
        foreach (var gemType in spell.associatedGemTypes)
        {
            if (gemCharges.ContainsKey(gemType) && remaining > 0)
            {
                int consumed = Mathf.Min(gemCharges[gemType], remaining);
                gemCharges[gemType] -= consumed;
                remaining -= consumed;
            }
        }
    }

    #endregion

    #region Spell Slots

    public bool HasSpellSlot(int level)
    {
        if (level < 1 || level > 9) return false;
        return currentSpellSlots[level - 1] > 0;
    }

    public void UseSpellSlot(int level)
    {
        if (level < 1 || level > 9) return;
        currentSpellSlots[level - 1] = Mathf.Max(0, currentSpellSlots[level - 1] - 1);
        onSpellSlotsChanged.Invoke();
        Debug.Log($"Used level {level} spell slot. Remaining: {currentSpellSlots[level - 1]}/{maxSpellSlots[level - 1]}");
    }

    public void RestoreSpellSlot(int level, int count = 1)
    {
        if (level < 1 || level > 9) return;
        currentSpellSlots[level - 1] = Mathf.Min(maxSpellSlots[level - 1], currentSpellSlots[level - 1] + count);
        onSpellSlotsChanged.Invoke();
    }

    public void SetMaxSpellSlots(int level, int count)
    {
        if (level < 1 || level > 9) return;
        maxSpellSlots[level - 1] = count;
        currentSpellSlots[level - 1] = Mathf.Min(currentSpellSlots[level - 1], count);
        onSpellSlotsChanged.Invoke();
    }

    public int GetCurrentSpellSlots(int level)
    {
        if (level < 1 || level > 9) return 0;
        return currentSpellSlots[level - 1];
    }

    public int GetMaxSpellSlots(int level)
    {
        if (level < 1 || level > 9) return 0;
        return maxSpellSlots[level - 1];
    }

    /// <summary>
    /// Long Rest - restore all spell slots
    /// </summary>
    public void LongRest()
    {
        for (int i = 0; i < 9; i++)
        {
            currentSpellSlots[i] = maxSpellSlots[i];
        }
        currentMana = maxMana;
        BreakConcentration();
        spellCooldowns.Clear();
        onSpellSlotsChanged.Invoke();
        onManaChanged.Invoke(currentMana, maxMana);
        Debug.Log("Long rest completed - all resources restored!");
    }

    /// <summary>
    /// Short Rest - restore some lower level spell slots (D&D 5e style)
    /// </summary>
    public void ShortRest()
    {
        // Restore one spell slot of each level 1-3
        for (int i = 0; i < 3; i++)
        {
            if (currentSpellSlots[i] < maxSpellSlots[i])
            {
                currentSpellSlots[i]++;
            }
        }
        currentMana = Mathf.Min(maxMana, currentMana + (maxMana / 2));
        onSpellSlotsChanged.Invoke();
        onManaChanged.Invoke(currentMana, maxMana);
        Debug.Log("Short rest completed!");
    }

    #endregion

    #region Mana System

    public void SpendMana(int amount)
    {
        currentMana = Mathf.Max(0, currentMana - amount);
        onManaChanged.Invoke(currentMana, maxMana);
    }

    public void RestoreMana(int amount)
    {
        currentMana = Mathf.Min(maxMana, currentMana + amount);
        onManaChanged.Invoke(currentMana, maxMana);
    }

    public void SetMaxMana(int amount)
    {
        maxMana = amount;
        currentMana = Mathf.Min(currentMana, maxMana);
        onManaChanged.Invoke(currentMana, maxMana);
    }

    public int GetCurrentMana() => currentMana;
    public int GetMaxMana() => maxMana;

    #endregion

    #region Concentration

    public void BreakConcentration()
    {
        if (concentrationSpell != null)
        {
            onConcentrationBroken.Invoke(concentrationSpell);
            Debug.Log($"Concentration on {concentrationSpell.spellName} broken!");
            concentrationSpell = null;
            concentrationTurnsRemaining = 0;
        }
    }

    public SpellData GetConcentrationSpell() => concentrationSpell;
    public bool IsConcentrating() => concentrationSpell != null;

    #endregion

    #region Turn Management

    /// <summary>
    /// Call this at the end of each turn to update cooldowns, concentration, etc.
    /// </summary>
    public void OnTurnEnd()
    {
        // Reduce cooldowns
        List<SpellData> spellsToUpdate = new List<SpellData>(spellCooldowns.Keys);
        foreach (var spell in spellsToUpdate)
        {
            spellCooldowns[spell] = Mathf.Max(0, spellCooldowns[spell] - 1);
            if (spellCooldowns[spell] == 0)
            {
                spellCooldowns.Remove(spell);
                Debug.Log($"{spell.spellName} cooldown finished!");
            }
        }

        // Update concentration
        if (concentrationSpell != null)
        {
            concentrationTurnsRemaining--;
            if (concentrationTurnsRemaining <= 0)
            {
                BreakConcentration();
            }
        }

        // Regenerate mana
        if (!useSpellSlots)
        {
            RestoreMana(manaRegenPerTurn);
        }
    }

    #endregion

    #region Gem Charge System

    /// <summary>
    /// Add gem charges when gems are matched
    /// </summary>
    public void AddGemCharge(GamePiece.PieceType gemType, int count = 1)
    {
        if (!gemCharges.ContainsKey(gemType))
        {
            gemCharges[gemType] = 0;
        }
        gemCharges[gemType] += count;
    }

    public int GetGemCharge(GamePiece.PieceType gemType)
    {
        return gemCharges.ContainsKey(gemType) ? gemCharges[gemType] : 0;
    }

    public void ClearGemCharges()
    {
        InitializeGemCharges();
    }

    #endregion

    #region Queries

    public List<SpellData> GetKnownSpells() => new List<SpellData>(knownSpells);
    public List<SpellData> GetPreparedSpells() => new List<SpellData>(preparedSpells);
    public int GetMaxPreparedSpells() => maxPreparedSpells;

    /// <summary>
    /// Gets all spells that can currently be cast
    /// </summary>
    public List<SpellData> GetCastableSpells()
    {
        return preparedSpells.Where(spell => CanCast(spell)).ToList();
    }

    /// <summary>
    /// Checks if a spell can be cast right now
    /// </summary>
    public bool CanCast(SpellData spell)
    {
        if (spell == null) return false;

        bool isCantrip = spell.level == SpellLevel.Cantrip;

        // Check if prepared (cantrips don't need to be prepared)
        if (!isCantrip && !preparedSpells.Contains(spell)) return false;

        // Check cooldown
        if (spellCooldowns.ContainsKey(spell) && spellCooldowns[spell] > 0) return false;

        // Check resources
        if (!isCantrip)
        {
            if (useSpellSlots)
            {
                // Check if any spell slot level can cast this
                bool hasSlot = false;
                for (int level = (int)spell.level; level <= 9; level++)
                {
                    if (HasSpellSlot(level))
                    {
                        hasSlot = true;
                        break;
                    }
                }
                if (!hasSlot) return false;
            }
            else
            {
                if (currentMana < spell.manaCost) return false;
            }
        }

        // Check gem requirements
        if (spell.gemsRequiredToCast > 0)
        {
            int totalGemCharge = 0;
            foreach (var gemType in spell.associatedGemTypes)
            {
                if (gemCharges.ContainsKey(gemType))
                {
                    totalGemCharge += gemCharges[gemType];
                }
            }
            if (totalGemCharge < spell.gemsRequiredToCast) return false;
        }

        return true;
    }

    #endregion
}
