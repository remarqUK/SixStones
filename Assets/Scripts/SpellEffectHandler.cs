using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles applying spell effects to targets
/// </summary>
public class SpellEffectHandler : MonoBehaviour
{
    private static SpellEffectHandler instance;
    public static SpellEffectHandler Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<SpellEffectHandler>();
            }
            return instance;
        }
    }

    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Board board;
    [SerializeField] private PlayerManager playerManager;

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
        if (board == null) board = FindFirstObjectByType<Board>();
        if (playerManager == null) playerManager = FindFirstObjectByType<PlayerManager>();

        // Subscribe to spell cast events
        if (SpellManager.Instance != null)
        {
            SpellManager.Instance.onSpellCast.AddListener(OnSpellCast);
        }
    }

    private void OnDestroy()
    {
        if (SpellManager.Instance != null)
        {
            SpellManager.Instance.onSpellCast.RemoveListener(OnSpellCast);
        }
    }

    /// <summary>
    /// Called when a spell is cast
    /// </summary>
    private void OnSpellCast(SpellData spell, int slotLevel)
    {
        Debug.Log($"Processing spell effect: {spell.spellName} at level {slotLevel}");

        // Apply the spell effect based on its type
        switch (spell.primaryEffect)
        {
            case SpellEffectType.Damage:
                ApplyDamageEffect(spell, slotLevel);
                break;

            case SpellEffectType.Healing:
                ApplyHealingEffect(spell, slotLevel);
                break;

            case SpellEffectType.Buff:
                ApplyBuffEffect(spell, slotLevel);
                break;

            case SpellEffectType.Debuff:
                ApplyDebuffEffect(spell, slotLevel);
                break;

            case SpellEffectType.Control:
                ApplyControlEffect(spell, slotLevel);
                break;

            case SpellEffectType.BoardManipulation:
                ApplyBoardManipulationEffect(spell, slotLevel);
                break;

            case SpellEffectType.Summon:
                ApplySummonEffect(spell, slotLevel);
                break;

            case SpellEffectType.Utility:
                ApplyUtilityEffect(spell, slotLevel);
                break;
        }

        // Apply status effect if spell has one
        if (spell.appliedEffect != null && StatusEffectManager.Instance != null)
        {
            PlayerManager.Player target = GetSpellTarget(spell);
            PlayerManager.Player caster = GetCurrentPlayer();

            StatusEffectManager.Instance.ApplyEffect(
                target,
                spell.appliedEffect,
                spell.effectSeverity,
                spell.effectDuration,
                caster,
                spell.spellName
            );
        }
    }

    #region Effect Implementations

    private void ApplyDamageEffect(SpellData spell, int slotLevel)
    {
        int damage = spell.CalculateEffect(slotLevel);

        Debug.Log($"{spell.spellName} deals {damage} {spell.damageType} damage!");

        // Apply damage based on target type
        switch (spell.targetType)
        {
            case SpellTarget.Self:
                // Damage caster (rare, but possible)
                DamageCurrentPlayer(damage);
                break;

            case SpellTarget.SingleEnemy:
                DamageOpponent(damage);
                break;

            case SpellTarget.AllEnemies:
                // In a match-3 game, usually just one opponent
                DamageOpponent(damage);
                break;

            case SpellTarget.Board:
                // Damage based on board state (custom logic)
                Debug.Log("Board-targeted damage spell");
                break;
        }

        // Apply saving throw if applicable
        if (spell.savingThrow != SavingThrowType.None)
        {
            bool savedSuccessfully = RollSavingThrow(spell.savingThrow);
            if (savedSuccessfully)
            {
                Debug.Log($"Target saved against {spell.spellName}! Damage halved.");
                // Typically would halve damage here
            }
        }
    }

    private void ApplyHealingEffect(SpellData spell, int slotLevel)
    {
        int healing = spell.CalculateEffect(slotLevel);

        Debug.Log($"{spell.spellName} heals for {healing} HP!");

        switch (spell.targetType)
        {
            case SpellTarget.Self:
            case SpellTarget.SingleAlly:
                HealCurrentPlayer(healing);
                break;

            case SpellTarget.AllAllies:
                // Heal all allies (in 2P mode, might heal both)
                HealCurrentPlayer(healing);
                break;
        }
    }

    private void ApplyBuffEffect(SpellData spell, int slotLevel)
    {
        Debug.Log($"{spell.spellName} applies buff effect!");

        // Example buffs could be:
        // - Increase gem match damage
        // - Grant extra moves
        // - Increase gem drop rate
        // - Double XP gain
        // - Shield (damage reduction)

        // This is a placeholder - specific buff mechanics would go here
    }

    private void ApplyDebuffEffect(SpellData spell, int slotLevel)
    {
        Debug.Log($"{spell.spellName} applies debuff effect!");

        // Example debuffs could be:
        // - Reduce opponent's gem match damage
        // - Freeze opponent's turn
        // - Poison (damage over time)
        // - Weakness (reduce stats)

        // This is a placeholder - specific debuff mechanics would go here
    }

    private void ApplyControlEffect(SpellData spell, int slotLevel)
    {
        Debug.Log($"{spell.spellName} applies control effect!");

        // Example control effects:
        // - Skip opponent's turn
        // - Prevent opponent from using spells
        // - Force opponent to swap random gems
        // - Lock opponent's gem selection

        // This is a placeholder - specific control mechanics would go here
    }

    private void ApplyBoardManipulationEffect(SpellData spell, int slotLevel)
    {
        Debug.Log($"{spell.spellName} manipulates the board!");

        // Example board manipulations:
        // - Destroy random gems
        // - Transform gems to specific types
        // - Shuffle the board
        // - Create special gems
        // - Clear entire rows/columns

        if (board != null)
        {
            // Example: Destroy random gems
            // int gemsToDestroy = spell.CalculateEffect(slotLevel);
            // DestroyRandomGems(gemsToDestroy);
        }
    }

    private void ApplySummonEffect(SpellData spell, int slotLevel)
    {
        Debug.Log($"{spell.spellName} summons something!");

        // Example summon effects:
        // - Summon special gems
        // - Summon helper creatures (for visual flavor)
        // - Create temporary bonus pieces

        // This is a placeholder - specific summon mechanics would go here
    }

    private void ApplyUtilityEffect(SpellData spell, int slotLevel)
    {
        Debug.Log($"{spell.spellName} provides utility!");

        // Example utility effects:
        // - Reveal possible matches
        // - Show future gem drops
        // - Increase hint duration
        // - Reset cooldowns
        // - Draw additional spell options

        // This is a placeholder - specific utility mechanics would go here
    }

    #endregion

    #region Helper Methods

    private PlayerManager.Player GetCurrentPlayer()
    {
        if (playerManager != null && playerManager.TwoPlayerMode)
        {
            return playerManager.CurrentPlayer;
        }
        return PlayerManager.Player.Player1;
    }

    private PlayerManager.Player GetSpellTarget(SpellData spell)
    {
        if (playerManager == null) return PlayerManager.Player.Player1;

        PlayerManager.Player caster = GetCurrentPlayer();

        switch (spell.targetType)
        {
            case SpellTarget.Self:
            case SpellTarget.SingleAlly:
                return caster;

            case SpellTarget.SingleEnemy:
            case SpellTarget.AllEnemies:
                // Return opponent
                if (playerManager.TwoPlayerMode)
                {
                    return caster == PlayerManager.Player.Player1
                        ? PlayerManager.Player.Player2
                        : PlayerManager.Player.Player1;
                }
                return PlayerManager.Player.Player1;

            default:
                return caster;
        }
    }

    private void DamageOpponent(int damage)
    {
        if (gameManager == null || playerManager == null) return;

        if (playerManager.TwoPlayerMode)
        {
            PlayerManager.Player opponent = playerManager.CurrentPlayer == PlayerManager.Player.Player1
                ? PlayerManager.Player.Player2
                : PlayerManager.Player.Player1;

            gameManager.TakeDamage(opponent, damage);
        }
        else
        {
            // In single player, might damage an AI opponent or boss
            Debug.Log($"Dealt {damage} damage to opponent");
        }
    }

    private void DamageCurrentPlayer(int damage)
    {
        if (gameManager == null || playerManager == null) return;

        if (playerManager.TwoPlayerMode)
        {
            gameManager.TakeDamage(playerManager.CurrentPlayer, damage);
        }
        else
        {
            gameManager.TakeDamage(PlayerManager.Player.Player1, damage);
        }
    }

    private void HealCurrentPlayer(int healing)
    {
        if (gameManager == null) return;

        // GameManager doesn't have a Heal method yet, so we'd need to add one
        // For now, log the healing
        Debug.Log($"Healed current player for {healing} HP");

        // TODO: Add healing method to GameManager
        // gameManager.HealPlayer(playerManager.CurrentPlayer, healing);
    }

    /// <summary>
    /// Simulates a D&D saving throw
    /// </summary>
    private bool RollSavingThrow(SavingThrowType saveType, int dc = 15)
    {
        // Roll d20
        int roll = Random.Range(1, 21);

        // Get modifier based on save type (would come from character stats)
        int modifier = GetSaveModifier(saveType);

        int total = roll + modifier;

        bool success = total >= dc;
        Debug.Log($"{saveType} save: rolled {roll} + {modifier} = {total} vs DC {dc} - {(success ? "SUCCESS" : "FAILED")}");

        return success;
    }

    private int GetSaveModifier(SavingThrowType saveType)
    {
        // This would come from character stats
        // For now, return a placeholder value based on player level
        int playerLevel = LevelSystem.Instance != null ? LevelSystem.Instance.GetCurrentLevel() : 1;
        int proficiencyBonus = 2 + (playerLevel - 1) / 4; // D&D 5e proficiency scaling

        // Assume proficient in all saves for now
        return proficiencyBonus;
    }

    /// <summary>
    /// Example board manipulation: destroy random gems
    /// </summary>
    private void DestroyRandomGems(int count)
    {
        if (board == null) return;

        // This would need to interact with the Board to destroy gems
        // For now, just a placeholder
        Debug.Log($"Destroying {count} random gems");
    }

    #endregion

    #region Public API

    /// <summary>
    /// Manually apply a spell effect (for testing or special cases)
    /// </summary>
    public void ApplySpellEffect(SpellData spell, int slotLevel = -1)
    {
        if (slotLevel == -1)
        {
            slotLevel = (int)spell.level;
        }
        OnSpellCast(spell, slotLevel);
    }

    #endregion
}
