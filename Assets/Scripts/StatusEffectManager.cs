using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

/// <summary>
/// Manages status effects on players
/// </summary>
public class StatusEffectManager : MonoBehaviour
{
    private static StatusEffectManager instance;
    public static StatusEffectManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<StatusEffectManager>();
            }
            return instance;
        }
    }

    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PlayerManager playerManager;

    [Header("Active Effects")]
    [SerializeField] private List<ActiveStatusEffect> player1Effects = new List<ActiveStatusEffect>();
    [SerializeField] private List<ActiveStatusEffect> player2Effects = new List<ActiveStatusEffect>();

    [Header("Events")]
    public UnityEvent<PlayerManager.Player, ActiveStatusEffect> onEffectApplied = new UnityEvent<PlayerManager.Player, ActiveStatusEffect>();
    public UnityEvent<PlayerManager.Player, ActiveStatusEffect> onEffectRemoved = new UnityEvent<PlayerManager.Player, ActiveStatusEffect>();
    public UnityEvent<PlayerManager.Player, ActiveStatusEffect, int> onEffectDamage = new UnityEvent<PlayerManager.Player, ActiveStatusEffect, int>();
    public UnityEvent<PlayerManager.Player, ActiveStatusEffect, int> onEffectHeal = new UnityEvent<PlayerManager.Player, ActiveStatusEffect, int>();

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
        // Auto-find references if not set
        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
        if (playerManager == null) playerManager = FindFirstObjectByType<PlayerManager>();
    }

    #region Apply/Remove Effects

    /// <summary>
    /// Apply a status effect to a player
    /// </summary>
    public ActiveStatusEffect ApplyEffect(PlayerManager.Player target, StatusEffectData effectData, int severity, int duration, PlayerManager.Player source = PlayerManager.Player.Player1, string sourceName = "")
    {
        if (effectData == null)
        {
            Debug.LogWarning("Cannot apply null effect!");
            return null;
        }

        List<ActiveStatusEffect> targetEffects = GetEffectsListForPlayer(target);

        // Check if effect already exists
        ActiveStatusEffect existing = targetEffects.Find(e => e.effectData == effectData);

        if (existing != null)
        {
            // Handle stacking based on effect type
            switch (effectData.stackingType)
            {
                case EffectStackingType.None:
                    Debug.Log($"{effectData.effectName} already active on {target}, cannot apply again");
                    return existing;

                case EffectStackingType.Refresh:
                    existing.RefreshDuration(duration);
                    if (severity > existing.severity)
                    {
                        existing.severity = Mathf.Min(severity, effectData.maxSeverity);
                    }
                    Debug.Log($"{effectData.effectName} on {target} refreshed to {existing.remainingDuration} turns");
                    return existing;

                case EffectStackingType.Stack:
                    existing.AddStack(severity);
                    Debug.Log($"{effectData.effectName} on {target} increased to +{existing.severity}");
                    return existing;

                case EffectStackingType.Extend:
                    existing.ExtendDuration(duration);
                    Debug.Log($"{effectData.effectName} on {target} extended by {duration} turns");
                    return existing;
            }
        }

        // Create new effect
        ActiveStatusEffect newEffect = new ActiveStatusEffect(effectData, severity, duration);
        newEffect.sourcePlayer = source;
        newEffect.sourceName = sourceName;

        targetEffects.Add(newEffect);

        Debug.Log($"Applied {effectData.effectName} +{severity} to {target} for {duration} turns");
        onEffectApplied.Invoke(target, newEffect);

        // Process "OnApply" trigger
        if (effectData.triggerTiming == EffectTriggerTiming.OnApply)
        {
            ProcessEffectTrigger(target, newEffect);
        }

        return newEffect;
    }

    /// <summary>
    /// Remove a specific effect from a player
    /// </summary>
    public bool RemoveEffect(PlayerManager.Player target, ActiveStatusEffect effect)
    {
        List<ActiveStatusEffect> targetEffects = GetEffectsListForPlayer(target);

        if (targetEffects.Remove(effect))
        {
            Debug.Log($"Removed {effect.effectData.effectName} from {target}");
            onEffectRemoved.Invoke(target, effect);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Remove all effects of a specific type from a player
    /// </summary>
    public int RemoveEffectsOfType(PlayerManager.Player target, StatusEffectType effectType)
    {
        List<ActiveStatusEffect> targetEffects = GetEffectsListForPlayer(target);
        List<ActiveStatusEffect> toRemove = targetEffects.Where(e => e.GetEffectType() == effectType).ToList();

        foreach (var effect in toRemove)
        {
            RemoveEffect(target, effect);
        }

        return toRemove.Count;
    }

    /// <summary>
    /// Remove all effects from a player
    /// </summary>
    public void ClearAllEffects(PlayerManager.Player target)
    {
        List<ActiveStatusEffect> targetEffects = GetEffectsListForPlayer(target);
        int count = targetEffects.Count;
        targetEffects.Clear();
        Debug.Log($"Cleared {count} effects from {target}");
    }

    /// <summary>
    /// Remove all debuffs from a player (for dispel/cleanse spells)
    /// </summary>
    public int DispelDebuffs(PlayerManager.Player target)
    {
        List<ActiveStatusEffect> targetEffects = GetEffectsListForPlayer(target);
        List<ActiveStatusEffect> debuffs = targetEffects.Where(e =>
            e.GetCategory() == EffectCategory.Debuff ||
            e.GetCategory() == EffectCategory.Control ||
            e.GetCategory() == EffectCategory.DamageOverTime
        ).ToList();

        foreach (var effect in debuffs)
        {
            RemoveEffect(target, effect);
        }

        return debuffs.Count;
    }

    #endregion

    #region Turn Processing

    /// <summary>
    /// Process effects at the start of a player's turn
    /// </summary>
    public void ProcessStartOfTurn(PlayerManager.Player player)
    {
        List<ActiveStatusEffect> effects = GetEffectsListForPlayer(player);

        // Process all "StartOfTurn" effects
        List<ActiveStatusEffect> startEffects = effects.Where(e =>
            e.GetTriggerTiming() == EffectTriggerTiming.StartOfTurn
        ).ToList();

        foreach (var effect in startEffects)
        {
            ProcessEffectTrigger(player, effect);
        }
    }

    /// <summary>
    /// Process effects at the end of a player's turn
    /// </summary>
    public void ProcessEndOfTurn(PlayerManager.Player player)
    {
        List<ActiveStatusEffect> effects = GetEffectsListForPlayer(player);

        // Process all "EndOfTurn" effects
        List<ActiveStatusEffect> endEffects = effects.Where(e =>
            e.GetTriggerTiming() == EffectTriggerTiming.EndOfTurn
        ).ToList();

        foreach (var effect in endEffects)
        {
            ProcessEffectTrigger(player, effect);
        }

        // Advance duration and check for saving throws
        List<ActiveStatusEffect> toRemove = new List<ActiveStatusEffect>();

        foreach (var effect in effects)
        {
            // Advance turn
            bool stillActive = effect.AdvanceTurn();

            // Check for save to remove
            if (stillActive && effect.effectData.allowsSaveToRemove)
            {
                if (AttemptSavingThrow(player, effect))
                {
                    Debug.Log($"{player} saved against {effect.effectData.effectName}!");
                    toRemove.Add(effect);
                    continue;
                }
            }

            // Remove if expired
            if (!stillActive)
            {
                toRemove.Add(effect);
            }
        }

        // Remove expired/saved effects
        foreach (var effect in toRemove)
        {
            RemoveEffect(player, effect);
        }
    }

    /// <summary>
    /// Process an effect trigger (damage, healing, etc.)
    /// </summary>
    private void ProcessEffectTrigger(PlayerManager.Player target, ActiveStatusEffect effect)
    {
        if (effect.effectData == null) return;

        // Damage or healing
        if (effect.effectData.affectsHealthPerTurn)
        {
            int amount = effect.GetDamagePerTurn();

            if (amount > 0)
            {
                // Damage
                Debug.Log($"{target} takes {amount} {effect.GetDamageType()} damage from {effect.effectData.effectName} +{effect.severity}");
                if (gameManager != null)
                {
                    gameManager.TakeDamage(target, amount);
                }
                onEffectDamage.Invoke(target, effect, amount);
            }
            else if (amount < 0)
            {
                // Healing
                int healing = Mathf.Abs(amount);
                Debug.Log($"{target} heals {healing} HP from {effect.effectData.effectName} +{effect.severity}");
                // TODO: Add healing method to GameManager
                onEffectHeal.Invoke(target, effect, healing);
            }
        }
    }

    /// <summary>
    /// Attempt a saving throw against an effect
    /// </summary>
    private bool AttemptSavingThrow(PlayerManager.Player player, ActiveStatusEffect effect)
    {
        if (effect.effectData == null) return false;

        // Roll d20
        int roll = Random.Range(1, 21);

        // Get save modifier (would come from character stats)
        int saveModifier = GetSaveModifier(player, effect.effectData.saveType);

        int total = roll + saveModifier;
        int dc = effect.GetSaveDC();

        bool success = total >= dc;

        Debug.Log($"{player} {effect.effectData.saveType} save: {roll} + {saveModifier} = {total} vs DC {dc} - {(success ? "SUCCESS" : "FAILED")}");

        return success;
    }

    /// <summary>
    /// Get save modifier for a player (placeholder - would use character stats)
    /// </summary>
    private int GetSaveModifier(PlayerManager.Player player, SavingThrowType saveType)
    {
        // Placeholder - would get from player stats
        int playerLevel = LevelSystem.Instance != null ? LevelSystem.Instance.GetCurrentLevel() : 1;
        int proficiencyBonus = 2 + (playerLevel - 1) / 4; // D&D 5e proficiency

        // Assume proficient in all saves for now
        return proficiencyBonus;
    }

    #endregion

    #region Queries

    /// <summary>
    /// Get all active effects on a player
    /// </summary>
    public List<ActiveStatusEffect> GetActiveEffects(PlayerManager.Player player)
    {
        return new List<ActiveStatusEffect>(GetEffectsListForPlayer(player));
    }

    /// <summary>
    /// Check if player has a specific effect type
    /// </summary>
    public bool HasEffect(PlayerManager.Player player, StatusEffectType effectType)
    {
        return GetEffectsListForPlayer(player).Any(e => e.GetEffectType() == effectType);
    }

    /// <summary>
    /// Get a specific effect if it exists
    /// </summary>
    public ActiveStatusEffect GetEffect(PlayerManager.Player player, StatusEffectType effectType)
    {
        return GetEffectsListForPlayer(player).Find(e => e.GetEffectType() == effectType);
    }

    /// <summary>
    /// Check if player can take actions (not stunned/paralyzed/etc)
    /// </summary>
    public bool CanTakeActions(PlayerManager.Player player)
    {
        return !GetEffectsListForPlayer(player).Any(e => e.PreventsActions());
    }

    /// <summary>
    /// Check if player can cast spells
    /// </summary>
    public bool CanCastSpells(PlayerManager.Player player)
    {
        return !GetEffectsListForPlayer(player).Any(e => e.PreventsSpellCasting());
    }

    /// <summary>
    /// Check if player can cast a spell with specific components
    /// </summary>
    public bool CanCastWithComponents(PlayerManager.Player player, SpellComponents components)
    {
        List<ActiveStatusEffect> effects = GetEffectsListForPlayer(player);

        foreach (var effect in effects)
        {
            if ((components & SpellComponents.Verbal) != 0 && effect.BlocksComponent(SpellComponents.Verbal))
                return false;
            if ((components & SpellComponents.Somatic) != 0 && effect.BlocksComponent(SpellComponents.Somatic))
                return false;
            if ((components & SpellComponents.Material) != 0 && effect.BlocksComponent(SpellComponents.Material))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Get total stat modifier from all effects
    /// </summary>
    public float GetStatModifier(PlayerManager.Player player, string statName)
    {
        List<ActiveStatusEffect> effects = GetEffectsListForPlayer(player);
        float totalModifier = 100f; // 100% = no change

        foreach (var effect in effects)
        {
            if (effect.effectData != null &&
                effect.effectData.modifiesStats &&
                effect.effectData.modifiedStat == statName)
            {
                float effectMod = effect.GetStatModifier();
                // Multiply modifiers (not add) - e.g., 80% * 120% = 96%
                totalModifier = (totalModifier / 100f) * effectMod;
            }
        }

        return totalModifier;
    }

    private List<ActiveStatusEffect> GetEffectsListForPlayer(PlayerManager.Player player)
    {
        return player == PlayerManager.Player.Player1 ? player1Effects : player2Effects;
    }

    #endregion
}
