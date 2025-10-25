using UnityEngine;

/// <summary>
/// ScriptableObject template defining a status effect type
/// Severity is NOT defined here - it's determined when the effect is applied
/// </summary>
[CreateAssetMenu(fileName = "New Status Effect", menuName = "Match3/Status Effect")]
public class StatusEffectData : ScriptableObject
{
    [Header("Identity")]
    public StatusEffectType effectType;
    public string effectName;

    [TextArea(2, 4)]
    public string descriptionTemplate = "Affected by {effectName}.";

    [Header("Behavior")]
    public EffectCategory category;
    public EffectStackingType stackingType;
    public EffectTriggerTiming triggerTiming;

    [Header("Damage/Healing")]
    [Tooltip("Does this effect deal damage or heal per turn?")]
    public bool affectsHealthPerTurn = false;

    [Tooltip("Damage type (if dealing damage)")]
    public DamageType damageType = DamageType.Poison;

    [Tooltip("Damage/heal per severity per turn (e.g., 1 = 1 dmg per stack)")]
    public int damagePerSeverity = 1;

    [Header("Stat Modification")]
    [Tooltip("Does this effect modify stats?")]
    public bool modifiesStats = false;

    [Tooltip("Stat modifier per severity (% per stack, e.g., -10 = -10% per stack)")]
    public float statModifierPerSeverity = -10f;

    [Tooltip("What stat is modified? (damage, defense, speed, etc.)")]
    public string modifiedStat = "damage";

    [Header("Control")]
    [Tooltip("Prevents gem swaps/actions")]
    public bool preventsActions = false;

    [Tooltip("Prevents spell casting")]
    public bool preventsSpellCasting = false;

    [Tooltip("Requires specific spell components to be blocked")]
    public SpellComponents blockedComponents = SpellComponents.None;

    [Header("Saving Throw")]
    [Tooltip("Can save at end of turn to remove effect?")]
    public bool allowsSaveToRemove = false;

    public SavingThrowType saveType = SavingThrowType.Constitution;

    [Tooltip("Base DC (can be modified when applied)")]
    public int baseSaveDC = 15;

    [Header("Stacking")]
    [Tooltip("Maximum severity/stacks")]
    public int maxSeverity = 10;

    [Header("Visual")]
    public Color effectColor = Color.green;
    public Sprite effectIcon;

    /// <summary>
    /// Get description with severity filled in
    /// </summary>
    public string GetDescription(int severity)
    {
        string desc = descriptionTemplate;
        desc = desc.Replace("{effectName}", effectName);
        desc = desc.Replace("{severity}", severity.ToString());

        if (affectsHealthPerTurn)
        {
            int damageAmount = damagePerSeverity * severity;
            if (damageAmount > 0)
            {
                desc += $" Takes {damageAmount} {damageType} damage per turn.";
            }
            else if (damageAmount < 0)
            {
                desc += $" Heals {Mathf.Abs(damageAmount)} HP per turn.";
            }
        }

        if (modifiesStats)
        {
            float totalModifier = statModifierPerSeverity * severity;
            string sign = totalModifier >= 0 ? "+" : "";
            desc += $" {sign}{totalModifier:F0}% {modifiedStat}.";
        }

        if (preventsActions)
        {
            desc += " Cannot take actions.";
        }

        if (preventsSpellCasting)
        {
            desc += " Cannot cast spells.";
        }

        return desc;
    }

    /// <summary>
    /// Calculate damage/healing for this effect
    /// </summary>
    public int CalculateDamagePerTurn(int severity)
    {
        if (!affectsHealthPerTurn) return 0;
        return damagePerSeverity * severity;
    }

    /// <summary>
    /// Calculate stat modifier
    /// </summary>
    public float CalculateStatModifier(int severity)
    {
        if (!modifiesStats) return 100f; // 100% = no change

        float totalModifier = 100f + (statModifierPerSeverity * severity);
        return Mathf.Max(0f, totalModifier); // Can't go below 0%
    }
}
