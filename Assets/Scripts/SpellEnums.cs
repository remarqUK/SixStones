using UnityEngine;

/// <summary>
/// D&D-style spell schools
/// </summary>
public enum SpellSchool
{
    Abjuration,      // Protection, wards, shields
    Conjuration,     // Summoning, teleportation
    Divination,      // Knowledge, detection, scrying
    Enchantment,     // Mind-affecting, charm
    Evocation,       // Energy, damage dealing
    Illusion,        // Deception, invisibility
    Necromancy,      // Life, death, undead
    Transmutation    // Transformation, enhancement
}

/// <summary>
/// D&D spell levels (0 = Cantrip, 1-9 = spell levels)
/// </summary>
public enum SpellLevel
{
    Cantrip = 0,
    Level1 = 1,
    Level2 = 2,
    Level3 = 3,
    Level4 = 4,
    Level5 = 5,
    Level6 = 6,
    Level7 = 7,
    Level8 = 8,
    Level9 = 9
}

/// <summary>
/// How long the spell lasts
/// </summary>
public enum SpellDuration
{
    Instantaneous,   // Immediate effect, no duration
    Rounds,          // Lasts X combat rounds/turns
    Minutes,         // Lasts X minutes
    Hours,           // Lasts X hours
    Concentration,   // Lasts while concentrating (up to X duration)
    UntilDispelled,  // Permanent until dispelled
    Special          // Custom duration logic
}

/// <summary>
/// How far the spell can reach
/// </summary>
public enum SpellRange
{
    Self,            // Caster only
    Touch,           // Must touch target
    Short,           // 30 feet / close range
    Medium,          // 60 feet / medium range
    Long,            // 120 feet / long range
    Sight,           // As far as caster can see
    Unlimited        // No range limit
}

/// <summary>
/// What the spell can target
/// </summary>
public enum SpellTarget
{
    Self,            // Only the caster
    SingleEnemy,     // One enemy
    SingleAlly,      // One ally
    SingleAny,       // Any single target
    AllEnemies,      // All enemies
    AllAllies,       // All allies
    Area,            // Area of effect
    Board            // Affects the game board
}

/// <summary>
/// Type of saving throw required
/// </summary>
public enum SavingThrowType
{
    None,            // No save required
    Strength,        // STR save
    Dexterity,       // DEX save
    Constitution,    // CON save
    Intelligence,    // INT save
    Wisdom,          // WIS save
    Charisma         // CHA save
}

/// <summary>
/// What kind of action is required to cast
/// </summary>
public enum CastingTime
{
    Action,          // Standard action
    BonusAction,     // Quick cast
    Reaction,        // Triggered response
    Ritual,          // 10 minutes extra (no spell slot)
    Special          // Custom casting time
}

/// <summary>
/// Spell components required
/// </summary>
[System.Flags]
public enum SpellComponents
{
    None = 0,
    Verbal = 1,      // Spoken words (V)
    Somatic = 2,     // Gestures (S)
    Material = 4     // Physical components (M)
}

/// <summary>
/// Type of spell effect
/// </summary>
public enum SpellEffectType
{
    Damage,          // Deals damage
    Healing,         // Restores health
    Buff,            // Positive status effect
    Debuff,          // Negative status effect
    Control,         // Disable or manipulate
    Summon,          // Create creatures/objects
    BoardManipulation, // Affects match-3 board
    Utility          // Other effects
}

/// <summary>
/// Damage types from D&D
/// </summary>
public enum DamageType
{
    None,
    Acid,
    Bludgeoning,
    Cold,
    Fire,
    Force,
    Lightning,
    Necrotic,
    Piercing,
    Poison,
    Psychic,
    Radiant,
    Slashing,
    Thunder
}
