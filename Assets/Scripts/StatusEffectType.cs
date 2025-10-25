using UnityEngine;

/// <summary>
/// D&D-style status effects that can be applied to players
/// </summary>
public enum StatusEffectType
{
    // Damage Over Time
    Poisoned,           // Takes poison damage each turn
    Burning,            // Takes fire damage each turn
    Bleeding,           // Takes physical damage each turn
    Cursed,             // Takes necrotic damage each turn

    // Control Effects
    Stunned,            // Cannot take actions
    Paralyzed,          // Cannot move or take actions, auto-fail DEX saves
    Petrified,          // Turned to stone, cannot act, resistance to all damage
    Incapacitated,      // Cannot take actions or reactions
    Restrained,         // Speed = 0, disadvantage on DEX saves
    Prone,              // Disadvantage on attacks
    Grappled,           // Speed = 0

    // Mental Effects
    Charmed,            // Cannot attack charmer, charmer has advantage on social checks
    Frightened,         // Disadvantage on ability checks and attacks while source is in sight
    Confused,           // Random actions each turn

    // Sensory Effects
    Blinded,            // Cannot see, auto-fail sight checks, disadvantage on attacks
    Deafened,           // Cannot hear, auto-fail hearing checks
    Silenced,           // Cannot cast spells with verbal components

    // Stat Modifications
    Weakened,           // Reduced damage output
    Slowed,             // Reduced movement/actions
    Exhausted,          // Multiple levels of exhaustion (D&D style)
    Empowered,          // Increased damage output
    Hastened,           // Extra actions/movement

    // Defensive Effects
    Shielded,           // Damage reduction
    Invisible,          // Cannot be seen, advantage on attacks
    Blessed,            // Bonus to attack rolls and saving throws
    Warded,             // Resistance to specific damage type

    // Debilitating Effects
    Diseased,           // Reduces maximum HP
    Fatigued,           // Disadvantage on ability checks
    Dazed,              // Can take only one action (action OR movement, not both)

    // Special Effects
    Regenerating,       // Heals each turn
    Concentrating,      // Maintaining a concentration spell
    Hexed,              // Takes extra damage from specific source
    Marked,             // Attacker has advantage against this target

    // Immunity Effects
    ImmuneToPoison,     // Cannot be poisoned
    ImmuneToFire,       // Cannot take fire damage
    ImmuneToIce,        // Cannot take cold damage
    ImmuneToControl     // Immune to control effects
}

/// <summary>
/// How the effect's severity behaves
/// </summary>
public enum EffectStackingType
{
    None,               // Cannot stack at all - only one instance
    Refresh,            // New application refreshes duration, keeps highest severity
    Stack,              // Multiple applications stack severity (poison +1, +2, +3...)
    Extend              // New applications extend the duration
}

/// <summary>
/// When the effect triggers
/// </summary>
public enum EffectTriggerTiming
{
    OnApply,            // Triggers immediately when applied
    StartOfTurn,        // Triggers at the start of affected creature's turn
    EndOfTurn,          // Triggers at the end of affected creature's turn
    OnDamage,           // Triggers when target takes damage
    OnCast,             // Triggers when target casts a spell
    OnMove,             // Triggers when target moves/swaps gems
    Passive             // Always active (no trigger)
}

/// <summary>
/// Categories for grouping effects
/// </summary>
public enum EffectCategory
{
    Buff,               // Positive effect
    Debuff,             // Negative effect
    Control,            // Prevents actions
    DamageOverTime,     // Deals damage over time
    HealOverTime,       // Heals over time
    StatModifier,       // Modifies stats
    Immunity            // Grants immunity
}
