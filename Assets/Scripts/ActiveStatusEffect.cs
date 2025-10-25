using UnityEngine;
using System;

/// <summary>
/// An active instance of a status effect on a target
/// Severity is determined when applied, not by the effect template
/// </summary>
[System.Serializable]
public class ActiveStatusEffect
{
    [Header("Effect Template")]
    [Tooltip("The status effect definition")]
    public StatusEffectData effectData;

    [Header("Instance Data")]
    [Tooltip("Current severity/stacks (e.g., Poison +3 = severity 3)")]
    public int severity = 1;

    [Tooltip("Remaining duration in turns (-1 = permanent)")]
    public int remainingDuration = 3;

    [Tooltip("Original duration when applied")]
    public int originalDuration = 3;

    [Header("Custom Overrides")]
    [Tooltip("Override save DC (0 = use template default)")]
    public int customSaveDC = 0;

    [Tooltip("Override damage per turn (0 = use template calculation)")]
    public int customDamagePerTurn = 0;

    [Header("Source")]
    [Tooltip("Who applied this effect?")]
    public PlayerManager.Player sourcePlayer = PlayerManager.Player.Player1;

    [Tooltip("Spell/item that caused this (optional)")]
    public string sourceName = "";

    // Runtime tracking
    private DateTime appliedTime;
    private int tickCount = 0;

    /// <summary>
    /// Constructor
    /// </summary>
    public ActiveStatusEffect(StatusEffectData data, int severity, int duration)
    {
        this.effectData = data;
        this.severity = Mathf.Clamp(severity, 1, data != null ? data.maxSeverity : 10);
        this.remainingDuration = duration;
        this.originalDuration = duration;
        this.appliedTime = DateTime.Now;
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    public ActiveStatusEffect(ActiveStatusEffect other)
    {
        this.effectData = other.effectData;
        this.severity = other.severity;
        this.remainingDuration = other.remainingDuration;
        this.originalDuration = other.originalDuration;
        this.customSaveDC = other.customSaveDC;
        this.customDamagePerTurn = other.customDamagePerTurn;
        this.sourcePlayer = other.sourcePlayer;
        this.sourceName = other.sourceName;
        this.appliedTime = DateTime.Now;
        this.tickCount = 0;
    }

    /// <summary>
    /// Advance the effect by one turn
    /// Returns false if effect should be removed
    /// </summary>
    public bool AdvanceTurn()
    {
        if (remainingDuration > 0)
        {
            remainingDuration--;
        }
        tickCount++;
        return remainingDuration > 0 || remainingDuration == -1; // -1 = permanent
    }

    /// <summary>
    /// Try to add more stacks/severity
    /// </summary>
    public bool AddStack(int amount = 1)
    {
        if (effectData == null) return false;

        if (effectData.stackingType == EffectStackingType.Stack)
        {
            int oldSeverity = severity;
            severity = Mathf.Min(severity + amount, effectData.maxSeverity);
            return severity != oldSeverity;
        }
        return false;
    }

    /// <summary>
    /// Refresh the duration
    /// </summary>
    public void RefreshDuration(int newDuration = -1)
    {
        if (newDuration > 0)
        {
            remainingDuration = newDuration;
            originalDuration = newDuration;
        }
        else
        {
            remainingDuration = originalDuration;
        }
    }

    /// <summary>
    /// Extend the duration
    /// </summary>
    public void ExtendDuration(int additionalTurns)
    {
        if (remainingDuration > 0)
        {
            remainingDuration += additionalTurns;
        }
        else if (remainingDuration == -1)
        {
            // Permanent effects can't be extended
            return;
        }
        else
        {
            // Expired effect, restart it
            remainingDuration = additionalTurns;
        }
    }

    /// <summary>
    /// Calculate damage/healing this effect does per turn
    /// </summary>
    public int GetDamagePerTurn()
    {
        if (customDamagePerTurn != 0)
        {
            return customDamagePerTurn;
        }

        if (effectData != null)
        {
            return effectData.CalculateDamagePerTurn(severity);
        }

        return 0;
    }

    /// <summary>
    /// Get the stat modifier this effect provides
    /// </summary>
    public float GetStatModifier()
    {
        if (effectData != null)
        {
            return effectData.CalculateStatModifier(severity);
        }
        return 100f; // No modification
    }

    /// <summary>
    /// Get the save DC for this effect
    /// </summary>
    public int GetSaveDC()
    {
        if (customSaveDC > 0) return customSaveDC;
        if (effectData != null) return effectData.baseSaveDC;
        return 15; // Default
    }

    /// <summary>
    /// Get display text for UI
    /// </summary>
    public string GetDisplayText()
    {
        if (effectData == null) return "Unknown Effect";

        Color color = effectData.effectColor;
        string text = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{effectData.effectName}";

        if (severity > 1)
        {
            text += $" +{severity}";
        }
        text += "</color>";

        if (remainingDuration > 0)
        {
            text += $" ({remainingDuration}t)";
        }
        else if (remainingDuration == -1)
        {
            text += " (∞)";
        }

        return text;
    }

    /// <summary>
    /// Get tooltip text
    /// </summary>
    public string GetTooltip()
    {
        if (effectData == null) return "Unknown Effect";

        string tooltip = $"<b>{effectData.effectName}</b>";
        if (severity > 1)
        {
            tooltip += $" <color=#{ColorUtility.ToHtmlStringRGB(effectData.effectColor)}>+{severity}</color>";
        }
        tooltip += $"\n{effectData.GetDescription(severity)}";

        if (remainingDuration > 0)
        {
            tooltip += $"\n<i>Duration: {remainingDuration} turns</i>";
        }
        else if (remainingDuration == -1)
        {
            tooltip += "\n<i>Duration: Permanent</i>";
        }

        if (effectData.allowsSaveToRemove)
        {
            tooltip += $"\n<i>{effectData.saveType} save DC {GetSaveDC()} to remove</i>";
        }

        if (!string.IsNullOrEmpty(sourceName))
        {
            tooltip += $"\n<i>Source: {sourceName}</i>";
        }

        return tooltip;
    }

    /// <summary>
    /// Check if this effect prevents actions
    /// </summary>
    public bool PreventsActions()
    {
        return effectData != null && effectData.preventsActions;
    }

    /// <summary>
    /// Check if this effect prevents spell casting
    /// </summary>
    public bool PreventsSpellCasting()
    {
        return effectData != null && effectData.preventsSpellCasting;
    }

    /// <summary>
    /// Check if this effect blocks specific spell components
    /// </summary>
    public bool BlocksComponent(SpellComponents component)
    {
        if (effectData == null) return false;
        return (effectData.blockedComponents & component) != 0;
    }

    public int GetTickCount() => tickCount;
    public DateTime GetAppliedTime() => appliedTime;
    public StatusEffectType GetEffectType() => effectData != null ? effectData.effectType : StatusEffectType.Poisoned;
    public EffectCategory GetCategory() => effectData != null ? effectData.category : EffectCategory.Debuff;
    public EffectTriggerTiming GetTriggerTiming() => effectData != null ? effectData.triggerTiming : EffectTriggerTiming.Passive;
    public DamageType GetDamageType() => effectData != null ? effectData.damageType : DamageType.Poison;
}
