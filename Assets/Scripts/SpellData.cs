using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject defining a D&D-style spell
/// </summary>
[CreateAssetMenu(fileName = "New Spell", menuName = "Match3/Spell")]
public class SpellData : ScriptableObject
{
    [Header("Basic Information")]
    [Tooltip("The name of the spell")]
    public string spellName = "New Spell";

    [Tooltip("Flavor text describing the spell")]
    [TextArea(3, 6)]
    public string description = "";

    [Tooltip("The spell's school of magic")]
    public SpellSchool school = SpellSchool.Evocation;

    [Tooltip("Spell level (0 = Cantrip, 1-9 = spell levels)")]
    public SpellLevel level = SpellLevel.Cantrip;

    [Header("Casting Properties")]
    [Tooltip("What action type is required to cast")]
    public CastingTime castingTime = CastingTime.Action;

    [Tooltip("Can this spell be cast as a ritual?")]
    public bool isRitual = false;

    [Tooltip("Components required to cast (V/S/M)")]
    public SpellComponents components = SpellComponents.Verbal | SpellComponents.Somatic;

    [Tooltip("Description of material component if applicable")]
    public string materialComponent = "";

    [Tooltip("Is the material component consumed?")]
    public bool materialConsumed = false;

    [Header("Targeting")]
    [Tooltip("Maximum range of the spell")]
    public SpellRange range = SpellRange.Medium;

    [Tooltip("What can this spell target?")]
    public SpellTarget targetType = SpellTarget.SingleEnemy;

    [Tooltip("Area of effect radius (if applicable)")]
    public float areaRadius = 0f;

    [Header("Duration")]
    [Tooltip("How long the spell lasts")]
    public SpellDuration durationType = SpellDuration.Instantaneous;

    [Tooltip("Duration value (rounds, minutes, or hours)")]
    public int durationValue = 0;

    [Tooltip("Requires concentration to maintain?")]
    public bool requiresConcentration = false;

    [Header("Saving Throw / Attack")]
    [Tooltip("Does this spell require a saving throw?")]
    public SavingThrowType savingThrow = SavingThrowType.None;

    [Tooltip("Does this spell require an attack roll?")]
    public bool requiresAttackRoll = false;

    [Tooltip("Is this a melee or ranged spell attack?")]
    public bool isMeleeSpellAttack = false;

    [Header("Effects")]
    [Tooltip("Primary effect type")]
    public SpellEffectType primaryEffect = SpellEffectType.Damage;

    [Tooltip("Damage type (if applicable)")]
    public DamageType damageType = DamageType.Fire;

    [Tooltip("Base damage or healing dice (e.g., '3d6' = 3 dice of 6 sides)")]
    public DiceRoll baseDice = new DiceRoll(3, 6, 0);

    [Tooltip("Does damage/healing scale with spell slot level?")]
    public bool scalesWithLevel = false;

    [Tooltip("Additional dice per level above base")]
    public DiceRoll additionalDicePerLevel = new DiceRoll(1, 6, 0);

    [Header("Status Effects")]
    [Tooltip("Status effect applied to target (if any)")]
    public StatusEffectData appliedEffect;

    [Tooltip("Severity/stacks of the status effect")]
    public int effectSeverity = 1;

    [Tooltip("Duration of the status effect in turns")]
    public int effectDuration = 3;

    [Header("Requirements")]
    [Tooltip("Minimum player level required to learn this spell")]
    public int minimumPlayerLevel = 1;

    [Tooltip("Mana/resource cost to cast (if not using spell slots)")]
    public int manaCost = 0;

    [Tooltip("Cooldown in turns after casting (0 = no cooldown)")]
    public int cooldownTurns = 0;

    [Header("Match-3 Specific")]
    [Tooltip("Gem types that can charge/power this spell")]
    public List<GamePiece.PieceType> associatedGemTypes = new List<GamePiece.PieceType>();

    [Tooltip("Number of associated gems needed to unlock cast")]
    public int gemsRequiredToCast = 0;

    [Tooltip("Icon for the spell")]
    public Sprite spellIcon;

    /// <summary>
    /// Calculates total damage/healing for this spell at a given spell slot level
    /// </summary>
    public int CalculateEffect(int spellSlotLevel)
    {
        int total = baseDice.Roll();

        if (scalesWithLevel && spellSlotLevel > (int)level)
        {
            int levelsAbove = spellSlotLevel - (int)level;
            for (int i = 0; i < levelsAbove; i++)
            {
                total += additionalDicePerLevel.Roll();
            }
        }

        return total;
    }

    /// <summary>
    /// Gets the spell description with all properties
    /// </summary>
    public string GetFullDescription()
    {
        string desc = $"<b>{spellName}</b>\n";
        desc += $"<i>{GetLevelText()} {school}</i>\n\n";
        desc += $"{description}\n\n";

        desc += $"<b>Casting Time:</b> {castingTime}";
        if (isRitual) desc += " (ritual)";
        desc += "\n";

        desc += $"<b>Range:</b> {range}";
        if (areaRadius > 0) desc += $" ({areaRadius}ft radius)";
        desc += "\n";

        desc += $"<b>Components:</b> {GetComponentsText()}\n";
        desc += $"<b>Duration:</b> {GetDurationText()}\n\n";

        if (savingThrow != SavingThrowType.None)
        {
            desc += $"<b>Save:</b> {savingThrow}\n";
        }

        if (baseDice.count > 0)
        {
            desc += $"<b>Effect:</b> {baseDice} {damageType}\n";
            if (scalesWithLevel)
            {
                desc += $"<i>+{additionalDicePerLevel} per level above {(int)level}</i>\n";
            }
        }

        return desc;
    }

    private string GetLevelText()
    {
        return level == SpellLevel.Cantrip ? "Cantrip" : $"Level {(int)level}";
    }

    private string GetComponentsText()
    {
        List<string> comps = new List<string>();
        if ((components & SpellComponents.Verbal) != 0) comps.Add("V");
        if ((components & SpellComponents.Somatic) != 0) comps.Add("S");
        if ((components & SpellComponents.Material) != 0)
        {
            string matText = "M";
            if (!string.IsNullOrEmpty(materialComponent))
            {
                matText += $" ({materialComponent}";
                if (materialConsumed) matText += ", consumed";
                matText += ")";
            }
            comps.Add(matText);
        }
        return string.Join(", ", comps);
    }

    private string GetDurationText()
    {
        string dur = durationType.ToString();
        if (durationValue > 0)
        {
            dur = $"{durationValue} {durationType}";
        }
        if (requiresConcentration)
        {
            dur = $"Concentration, up to {dur}";
        }
        return dur;
    }
}

/// <summary>
/// Represents dice rolls in D&D format (e.g., 3d6+2)
/// </summary>
[System.Serializable]
public class DiceRoll
{
    [Tooltip("Number of dice to roll")]
    public int count = 1;

    [Tooltip("Number of sides on each die")]
    public int sides = 6;

    [Tooltip("Modifier to add to the total")]
    public int modifier = 0;

    public DiceRoll(int count, int sides, int modifier)
    {
        this.count = count;
        this.sides = sides;
        this.modifier = modifier;
    }

    /// <summary>
    /// Rolls the dice and returns the total
    /// </summary>
    public int Roll()
    {
        int total = modifier;
        for (int i = 0; i < count; i++)
        {
            total += Random.Range(1, sides + 1);
        }
        return total;
    }

    /// <summary>
    /// Returns the average value of this dice roll
    /// </summary>
    public float Average()
    {
        return count * ((sides + 1) / 2f) + modifier;
    }

    /// <summary>
    /// Returns the maximum possible value
    /// </summary>
    public int Maximum()
    {
        return count * sides + modifier;
    }

    /// <summary>
    /// Returns the minimum possible value
    /// </summary>
    public int Minimum()
    {
        return count + modifier;
    }

    public override string ToString()
    {
        string result = $"{count}d{sides}";
        if (modifier > 0) result += $"+{modifier}";
        else if (modifier < 0) result += modifier;
        return result;
    }
}
