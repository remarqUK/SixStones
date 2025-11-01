using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor script to create example spells
/// </summary>
public class CreateExampleSpells
{
    [MenuItem("Tools/Create Example Spells")]
    public static void CreateSpells()
    {
        string spellFolder = "Assets/Spells";

        // Create folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder(spellFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Spells");
        }

        // Create example spells
        CreateFirebolt(spellFolder);
        CreateCureWounds(spellFolder);
        CreateMagicMissile(spellFolder);
        CreateShield(spellFolder);
        CreateLightning(spellFolder);
        CreateTransmuteGems(spellFolder);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Created example spells in Assets/Spells folder!");
    }

    private static void CreateFirebolt(string folder)
    {
        SpellData spell = ScriptableObject.CreateInstance<SpellData>();
        spell.spellName = "Firebolt";
        spell.description = "You hurl a mote of fire at a creature or object within range. Make a ranged spell attack against the target. On a hit, the target takes fire damage.";
        spell.school = SpellSchool.Evocation;
        spell.level = SpellLevel.Cantrip;
        spell.castingTime = CastingTime.Action;
        spell.components = SpellComponents.Verbal | SpellComponents.Somatic;
        spell.range = SpellRange.Long;
        spell.targetType = SpellTarget.SingleEnemy;
        spell.durationType = SpellDuration.Instantaneous;
        spell.primaryEffect = SpellEffectType.Damage;
        spell.damageType = DamageType.Fire;
        spell.baseDice = new DiceRoll(1, 10, 0);
        spell.requiresAttackRoll = true;
        spell.minimumPlayerLevel = 1;
        spell.associatedGemTypes.Add(GamePiece.PieceType.Red);
        spell.gemsRequiredToCast = 3;

        AssetDatabase.CreateAsset(spell, $"{folder}/Firebolt.asset");
    }

    private static void CreateCureWounds(string folder)
    {
        SpellData spell = ScriptableObject.CreateInstance<SpellData>();
        spell.spellName = "Cure Wounds";
        spell.description = "A creature you touch regains hit points. This spell has no effect on undead or constructs.";
        spell.school = SpellSchool.Evocation;
        spell.level = SpellLevel.Level1;
        spell.castingTime = CastingTime.Action;
        spell.components = SpellComponents.Verbal | SpellComponents.Somatic;
        spell.range = SpellRange.Touch;
        spell.targetType = SpellTarget.Self;
        spell.durationType = SpellDuration.Instantaneous;
        spell.primaryEffect = SpellEffectType.Healing;
        spell.baseDice = new DiceRoll(1, 8, 0);
        spell.scalesWithLevel = true;
        spell.additionalDicePerLevel = new DiceRoll(1, 8, 0);
        spell.minimumPlayerLevel = 1;
        spell.manaCost = 15;
        spell.associatedGemTypes.Add(GamePiece.PieceType.White);
        spell.gemsRequiredToCast = 5;

        AssetDatabase.CreateAsset(spell, $"{folder}/CureWounds.asset");
    }

    private static void CreateMagicMissile(string folder)
    {
        SpellData spell = ScriptableObject.CreateInstance<SpellData>();
        spell.spellName = "Magic Missile";
        spell.description = "You create three glowing darts of magical force. Each dart hits a creature of your choice that you can see within range. A dart deals 1d4+1 force damage.";
        spell.school = SpellSchool.Evocation;
        spell.level = SpellLevel.Level1;
        spell.castingTime = CastingTime.Action;
        spell.components = SpellComponents.Verbal | SpellComponents.Somatic;
        spell.range = SpellRange.Long;
        spell.targetType = SpellTarget.SingleEnemy;
        spell.durationType = SpellDuration.Instantaneous;
        spell.primaryEffect = SpellEffectType.Damage;
        spell.damageType = DamageType.Force;
        spell.baseDice = new DiceRoll(3, 4, 3); // 3d4+3 (three missiles of 1d4+1)
        spell.scalesWithLevel = true;
        spell.additionalDicePerLevel = new DiceRoll(1, 4, 1); // +1 missile per level
        spell.minimumPlayerLevel = 1;
        spell.manaCost = 15;
        spell.associatedGemTypes.Add(GamePiece.PieceType.Blue);
        spell.gemsRequiredToCast = 4;

        AssetDatabase.CreateAsset(spell, $"{folder}/MagicMissile.asset");
    }

    private static void CreateShield(string folder)
    {
        SpellData spell = ScriptableObject.CreateInstance<SpellData>();
        spell.spellName = "Shield";
        spell.description = "An invisible barrier of magical force appears and protects you. Until the start of your next turn, you have +5 to AC.";
        spell.school = SpellSchool.Abjuration;
        spell.level = SpellLevel.Level1;
        spell.castingTime = CastingTime.Reaction;
        spell.components = SpellComponents.Verbal | SpellComponents.Somatic;
        spell.range = SpellRange.Self;
        spell.targetType = SpellTarget.Self;
        spell.durationType = SpellDuration.Rounds;
        spell.durationValue = 1;
        spell.primaryEffect = SpellEffectType.Buff;
        spell.minimumPlayerLevel = 2;
        spell.manaCost = 15;
        spell.associatedGemTypes.Add(GamePiece.PieceType.White);
        spell.associatedGemTypes.Add(GamePiece.PieceType.Blue);
        spell.gemsRequiredToCast = 4;

        AssetDatabase.CreateAsset(spell, $"{folder}/Shield.asset");
    }

    private static void CreateLightning(string folder)
    {
        SpellData spell = ScriptableObject.CreateInstance<SpellData>();
        spell.spellName = "Lightning Bolt";
        spell.description = "A stroke of lightning forming a line 100 feet long and 5 feet wide blasts out from you. Each creature in the line must make a Dexterity saving throw, taking damage on a failed save, or half as much on a success.";
        spell.school = SpellSchool.Evocation;
        spell.level = SpellLevel.Level3;
        spell.castingTime = CastingTime.Action;
        spell.components = SpellComponents.Verbal | SpellComponents.Somatic | SpellComponents.Material;
        spell.materialComponent = "a bit of fur and a rod of amber, crystal, or glass";
        spell.range = SpellRange.Self;
        spell.targetType = SpellTarget.Area;
        spell.areaRadius = 100f;
        spell.durationType = SpellDuration.Instantaneous;
        spell.savingThrow = SavingThrowType.Dexterity;
        spell.primaryEffect = SpellEffectType.Damage;
        spell.damageType = DamageType.Lightning;
        spell.baseDice = new DiceRoll(8, 6, 0); // 8d6
        spell.scalesWithLevel = true;
        spell.additionalDicePerLevel = new DiceRoll(1, 6, 0);
        spell.minimumPlayerLevel = 5;
        spell.manaCost = 35;
        spell.cooldownTurns = 3;
        spell.associatedGemTypes.Add(GamePiece.PieceType.Blue);
        spell.associatedGemTypes.Add(GamePiece.PieceType.Yellow);
        spell.gemsRequiredToCast = 8;

        AssetDatabase.CreateAsset(spell, $"{folder}/LightningBolt.asset");
    }

    private static void CreateTransmuteGems(string folder)
    {
        SpellData spell = ScriptableObject.CreateInstance<SpellData>();
        spell.spellName = "Transmute Gems";
        spell.description = "You touch the game board and transform gems of one color into another. This spell allows strategic board manipulation.";
        spell.school = SpellSchool.Transmutation;
        spell.level = SpellLevel.Level2;
        spell.castingTime = CastingTime.Action;
        spell.components = SpellComponents.Verbal | SpellComponents.Somatic;
        spell.range = SpellRange.Self;
        spell.targetType = SpellTarget.Board;
        spell.durationType = SpellDuration.Instantaneous;
        spell.primaryEffect = SpellEffectType.BoardManipulation;
        spell.minimumPlayerLevel = 3;
        spell.manaCost = 25;
        spell.cooldownTurns = 2;
        spell.associatedGemTypes.Add(GamePiece.PieceType.Green);
        spell.associatedGemTypes.Add(GamePiece.PieceType.Purple);
        spell.gemsRequiredToCast = 6;

        AssetDatabase.CreateAsset(spell, $"{folder}/TransmuteGems.asset");
    }
}
