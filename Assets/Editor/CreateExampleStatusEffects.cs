using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor script to create example status effects
/// </summary>
public class CreateExampleStatusEffects
{
    [MenuItem("Tools/Create Example Status Effects")]
    public static void CreateEffects()
    {
        string effectFolder = "Assets/StatusEffects";

        // Create folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder(effectFolder))
        {
            AssetDatabase.CreateFolder("Assets", "StatusEffects");
        }

        // Create example status effects
        CreatePoisoned(effectFolder);
        CreateBurning(effectFolder);
        CreateStunned(effectFolder);
        CreateWeakened(effectFolder);
        CreateEmpowered(effectFolder);
        CreateSilenced(effectFolder);
        CreateRegenerating(effectFolder);
        CreateFrightened(effectFolder);
        CreateShielded(effectFolder);
        CreateBlessed(effectFolder);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Created example status effects in Assets/StatusEffects folder!");
    }

    private static void CreatePoisoned(string folder)
    {
        StatusEffectData effect = ScriptableObject.CreateInstance<StatusEffectData>();
        effect.effectType = StatusEffectType.Poisoned;
        effect.effectName = "Poisoned";
        effect.descriptionTemplate = "Poisoned +{severity}. Takes poison damage each turn.";
        effect.category = EffectCategory.DamageOverTime;
        effect.stackingType = EffectStackingType.Stack;
        effect.triggerTiming = EffectTriggerTiming.StartOfTurn;
        effect.affectsHealthPerTurn = true;
        effect.damageType = DamageType.Poison;
        effect.damagePerSeverity = 1; // 1 damage per stack per turn
        effect.allowsSaveToRemove = true;
        effect.saveType = SavingThrowType.Constitution;
        effect.baseSaveDC = 12;
        effect.maxSeverity = 10;
        effect.effectColor = new Color(0.5f, 0f, 0.8f); // Purple

        AssetDatabase.CreateAsset(effect, $"{folder}/Poisoned.asset");
    }

    private static void CreateBurning(string folder)
    {
        StatusEffectData effect = ScriptableObject.CreateInstance<StatusEffectData>();
        effect.effectType = StatusEffectType.Burning;
        effect.effectName = "Burning";
        effect.descriptionTemplate = "On fire! Takes fire damage each turn.";
        effect.category = EffectCategory.DamageOverTime;
        effect.stackingType = EffectStackingType.Stack;
        effect.triggerTiming = EffectTriggerTiming.StartOfTurn;
        effect.affectsHealthPerTurn = true;
        effect.damageType = DamageType.Fire;
        effect.damagePerSeverity = 2; // 2 damage per stack per turn (fire hurts!)
        effect.allowsSaveToRemove = true;
        effect.saveType = SavingThrowType.Dexterity;
        effect.baseSaveDC = 13;
        effect.maxSeverity = 5;
        effect.effectColor = new Color(1f, 0.5f, 0f); // Orange

        AssetDatabase.CreateAsset(effect, $"{folder}/Burning.asset");
    }

    private static void CreateStunned(string folder)
    {
        StatusEffectData effect = ScriptableObject.CreateInstance<StatusEffectData>();
        effect.effectType = StatusEffectType.Stunned;
        effect.effectName = "Stunned";
        effect.descriptionTemplate = "Cannot take actions or swap gems.";
        effect.category = EffectCategory.Control;
        effect.stackingType = EffectStackingType.Refresh; // Can't stack stun
        effect.triggerTiming = EffectTriggerTiming.Passive;
        effect.preventsActions = true;
        effect.allowsSaveToRemove = true;
        effect.saveType = SavingThrowType.Constitution;
        effect.baseSaveDC = 15;
        effect.maxSeverity = 1;
        effect.effectColor = new Color(0.7f, 0.7f, 0f); // Yellow

        AssetDatabase.CreateAsset(effect, $"{folder}/Stunned.asset");
    }

    private static void CreateWeakened(string folder)
    {
        StatusEffectData effect = ScriptableObject.CreateInstance<StatusEffectData>();
        effect.effectType = StatusEffectType.Weakened;
        effect.effectName = "Weakened";
        effect.descriptionTemplate = "Damage output reduced.";
        effect.category = EffectCategory.StatModifier;
        effect.stackingType = EffectStackingType.Stack;
        effect.triggerTiming = EffectTriggerTiming.Passive;
        effect.modifiesStats = true;
        effect.modifiedStat = "damage";
        effect.statModifierPerSeverity = -10f; // -10% damage per stack
        effect.maxSeverity = 5; // Max -50% damage
        effect.effectColor = new Color(0.6f, 0.3f, 0.3f); // Dull red

        AssetDatabase.CreateAsset(effect, $"{folder}/Weakened.asset");
    }

    private static void CreateEmpowered(string folder)
    {
        StatusEffectData effect = ScriptableObject.CreateInstance<StatusEffectData>();
        effect.effectType = StatusEffectType.Empowered;
        effect.effectName = "Empowered";
        effect.descriptionTemplate = "Damage output increased!";
        effect.category = EffectCategory.Buff;
        effect.stackingType = EffectStackingType.Stack;
        effect.triggerTiming = EffectTriggerTiming.Passive;
        effect.modifiesStats = true;
        effect.modifiedStat = "damage";
        effect.statModifierPerSeverity = 10f; // +10% damage per stack
        effect.maxSeverity = 5; // Max +50% damage
        effect.effectColor = new Color(1f, 0.9f, 0.3f); // Gold

        AssetDatabase.CreateAsset(effect, $"{folder}/Empowered.asset");
    }

    private static void CreateSilenced(string folder)
    {
        StatusEffectData effect = ScriptableObject.CreateInstance<StatusEffectData>();
        effect.effectType = StatusEffectType.Silenced;
        effect.effectName = "Silenced";
        effect.descriptionTemplate = "Cannot cast spells with verbal components.";
        effect.category = EffectCategory.Control;
        effect.stackingType = EffectStackingType.Refresh;
        effect.triggerTiming = EffectTriggerTiming.Passive;
        effect.preventsSpellCasting = false; // Only blocks verbal components
        effect.blockedComponents = SpellComponents.Verbal;
        effect.allowsSaveToRemove = false;
        effect.maxSeverity = 1;
        effect.effectColor = new Color(0.3f, 0.3f, 0.6f); // Dark blue

        AssetDatabase.CreateAsset(effect, $"{folder}/Silenced.asset");
    }

    private static void CreateRegenerating(string folder)
    {
        StatusEffectData effect = ScriptableObject.CreateInstance<StatusEffectData>();
        effect.effectType = StatusEffectType.Regenerating;
        effect.effectName = "Regenerating";
        effect.descriptionTemplate = "Heals each turn.";
        effect.category = EffectCategory.HealOverTime;
        effect.stackingType = EffectStackingType.Stack;
        effect.triggerTiming = EffectTriggerTiming.StartOfTurn;
        effect.affectsHealthPerTurn = true;
        effect.damagePerSeverity = -1; // Negative = healing (1 HP per stack per turn)
        effect.maxSeverity = 10;
        effect.effectColor = new Color(0.3f, 1f, 0.3f); // Green

        AssetDatabase.CreateAsset(effect, $"{folder}/Regenerating.asset");
    }

    private static void CreateFrightened(string folder)
    {
        StatusEffectData effect = ScriptableObject.CreateInstance<StatusEffectData>();
        effect.effectType = StatusEffectType.Frightened;
        effect.effectName = "Frightened";
        effect.descriptionTemplate = "Scared! Disadvantage on attacks and checks.";
        effect.category = EffectCategory.Debuff;
        effect.stackingType = EffectStackingType.Refresh;
        effect.triggerTiming = EffectTriggerTiming.Passive;
        effect.modifiesStats = true;
        effect.modifiedStat = "damage";
        effect.statModifierPerSeverity = -25f; // -25% effectiveness
        effect.allowsSaveToRemove = true;
        effect.saveType = SavingThrowType.Wisdom;
        effect.baseSaveDC = 14;
        effect.maxSeverity = 1;
        effect.effectColor = new Color(0.4f, 0f, 0.4f); // Dark purple

        AssetDatabase.CreateAsset(effect, $"{folder}/Frightened.asset");
    }

    private static void CreateShielded(string folder)
    {
        StatusEffectData effect = ScriptableObject.CreateInstance<StatusEffectData>();
        effect.effectType = StatusEffectType.Shielded;
        effect.effectName = "Shielded";
        effect.descriptionTemplate = "Protected! Reduces incoming damage.";
        effect.category = EffectCategory.Buff;
        effect.stackingType = EffectStackingType.Stack;
        effect.triggerTiming = EffectTriggerTiming.Passive;
        effect.modifiesStats = true;
        effect.modifiedStat = "defense";
        effect.statModifierPerSeverity = 5f; // +5 damage reduction per stack
        effect.maxSeverity = 10; // Max 50 damage reduction
        effect.effectColor = new Color(0.5f, 0.8f, 1f); // Light blue

        AssetDatabase.CreateAsset(effect, $"{folder}/Shielded.asset");
    }

    private static void CreateBlessed(string folder)
    {
        StatusEffectData effect = ScriptableObject.CreateInstance<StatusEffectData>();
        effect.effectType = StatusEffectType.Blessed;
        effect.effectName = "Blessed";
        effect.descriptionTemplate = "Blessed by divine power. Bonus to attacks and saves.";
        effect.category = EffectCategory.Buff;
        effect.stackingType = EffectStackingType.Refresh;
        effect.triggerTiming = EffectTriggerTiming.Passive;
        effect.modifiesStats = true;
        effect.modifiedStat = "damage";
        effect.statModifierPerSeverity = 10f; // +10% bonus
        effect.maxSeverity = 1;
        effect.effectColor = new Color(1f, 1f, 0.7f); // Light yellow

        AssetDatabase.CreateAsset(effect, $"{folder}/Blessed.asset");
    }
}
