using UnityEngine;

public static class ItemNameGenerator
{
    // Name prefixes based on rarity
    private static readonly string[] commonPrefixes = { "Worn", "Simple", "Basic", "Old", "Sturdy", "Plain" };
    private static readonly string[] uncommonPrefixes = { "Fine", "Quality", "Reinforced", "Polished", "Sharpened", "Hardened" };
    private static readonly string[] rarePrefixes = { "Masterwork", "Enchanted", "Superior", "Exquisite", "Gleaming", "Runed" };
    private static readonly string[] veryRarePrefixes = { "Ancient", "Mystical", "Legendary", "Powerful", "Arcane", "Sacred" };
    private static readonly string[] legendaryPrefixes = { "Divine", "Godforged", "Eternal", "Celestial", "Mythical", "Immortal" };
    private static readonly string[] artifactPrefixes = { "Primordial", "World-Ending", "Reality-Bending", "Cosmos-Forged", "Infinite" };

    // Weapon base names
    private static readonly string[] oneHandedWeapons = { "Sword", "Axe", "Mace", "Hammer", "Blade", "Saber", "Scimitar", "Rapier", "Falchion" };
    private static readonly string[] twoHandedWeapons = { "Greatsword", "Greataxe", "Maul", "Warhammer", "Claymore", "Halberd", "Glaive", "Pike" };

    // Armor base names by slot
    private static readonly string[] chestArmor = { "Breastplate", "Cuirass", "Hauberk", "Chestplate", "Mail", "Armor", "Coat" };
    private static readonly string[] headArmor = { "Helmet", "Helm", "Crown", "Circlet", "Coif", "Cap", "Hood" };
    private static readonly string[] legArmor = { "Greaves", "Leggings", "Pants", "Cuisses", "Chausses" };
    private static readonly string[] feetArmor = { "Boots", "Shoes", "Sabatons", "Greaves", "Sandals" };
    private static readonly string[] handArmor = { "Gauntlets", "Gloves", "Bracers", "Vambraces", "Mitts" };
    private static readonly string[] shields = { "Shield", "Buckler", "Tower Shield", "Kite Shield", "Aegis" };

    // Accessory base names
    private static readonly string[] rings = { "Ring", "Band", "Loop", "Circlet", "Signet" };
    private static readonly string[] amulets = { "Amulet", "Necklace", "Pendant", "Talisman", "Medallion" };
    private static readonly string[] cloaks = { "Cloak", "Cape", "Mantle", "Robe", "Shroud" };
    private static readonly string[] belts = { "Belt", "Girdle", "Sash", "Cincture", "Waistband" };

    // Consumable base names
    private static readonly string[] potions = { "Potion", "Elixir", "Tonic", "Brew", "Draught", "Philter" };

    // Magical suffixes
    private static readonly string[] magicalSuffixes = {
        "of Power", "of Might", "of Protection", "of Fury", "of Speed", "of Strength",
        "of Intelligence", "of Wisdom", "of Charisma", "of the Bear", "of the Eagle",
        "of the Tiger", "of the Dragon", "of the Phoenix", "of Flames", "of Frost",
        "of Lightning", "of Thunder", "of the Storm", "of the Sun", "of the Moon",
        "of the Stars", "of the Void", "of Shadows", "of Light", "of Darkness",
        "of the Ancients", "of Heroes", "of Champions", "of Kings", "of Gods"
    };

    // Elemental modifiers
    private static readonly string[] elementalModifiers = {
        "Flaming", "Freezing", "Shocking", "Thundering", "Acidic", "Venomous",
        "Radiant", "Necrotic", "Spectral", "Ethereal", "Prismatic"
    };

    public static string GenerateWeaponName(ItemRarity rarity, bool twoHanded)
    {
        string prefix = GetPrefix(rarity);
        string baseName = twoHanded
            ? twoHandedWeapons[Random.Range(0, twoHandedWeapons.Length)]
            : oneHandedWeapons[Random.Range(0, oneHandedWeapons.Length)];

        // Higher rarity items get additional modifiers
        if (rarity >= ItemRarity.Rare && Random.value < 0.5f)
        {
            string elemental = elementalModifiers[Random.Range(0, elementalModifiers.Length)];
            return $"{prefix} {elemental} {baseName}";
        }

        if (rarity >= ItemRarity.Uncommon && Random.value < 0.6f)
        {
            string suffix = magicalSuffixes[Random.Range(0, magicalSuffixes.Length)];
            return $"{prefix} {baseName} {suffix}";
        }

        return $"{prefix} {baseName}";
    }

    public static string GenerateArmorName(ItemRarity rarity, EquipmentSlot slot)
    {
        string prefix = GetPrefix(rarity);
        string baseName = slot switch
        {
            EquipmentSlot.Chest => chestArmor[Random.Range(0, chestArmor.Length)],
            EquipmentSlot.Head => headArmor[Random.Range(0, headArmor.Length)],
            EquipmentSlot.Legs => legArmor[Random.Range(0, legArmor.Length)],
            EquipmentSlot.Feet => feetArmor[Random.Range(0, feetArmor.Length)],
            EquipmentSlot.Hands => handArmor[Random.Range(0, handArmor.Length)],
            EquipmentSlot.OffHand => shields[Random.Range(0, shields.Length)],
            _ => "Armor"
        };

        if (rarity >= ItemRarity.Rare && Random.value < 0.5f)
        {
            string suffix = magicalSuffixes[Random.Range(0, magicalSuffixes.Length)];
            return $"{prefix} {baseName} {suffix}";
        }

        return $"{prefix} {baseName}";
    }

    public static string GenerateAccessoryName(ItemRarity rarity, EquipmentSlot slot)
    {
        string prefix = GetPrefix(rarity);
        string baseName = slot switch
        {
            EquipmentSlot.Ring => rings[Random.Range(0, rings.Length)],
            EquipmentSlot.Neck => amulets[Random.Range(0, amulets.Length)],
            EquipmentSlot.Back => cloaks[Random.Range(0, cloaks.Length)],
            EquipmentSlot.Waist => belts[Random.Range(0, belts.Length)],
            _ => "Accessory"
        };

        // Accessories almost always get a magical suffix
        if (rarity >= ItemRarity.Uncommon || Random.value < 0.7f)
        {
            string suffix = magicalSuffixes[Random.Range(0, magicalSuffixes.Length)];
            return $"{prefix} {baseName} {suffix}";
        }

        return $"{prefix} {baseName}";
    }

    public static string GenerateConsumableName(ItemRarity rarity)
    {
        string prefix = GetPrefix(rarity);
        string baseName = potions[Random.Range(0, potions.Length)];

        string[] consumableTypes = {
            "Healing", "Mana", "Strength", "Dexterity", "Intelligence",
            "Fortitude", "Speed", "Invisibility", "Fire Resistance", "Heroism"
        };

        string type = consumableTypes[Random.Range(0, consumableTypes.Length)];

        return rarity >= ItemRarity.Uncommon
            ? $"{prefix} {baseName} of {type}"
            : $"{baseName} of {type}";
    }

    private static string GetPrefix(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => commonPrefixes[Random.Range(0, commonPrefixes.Length)],
            ItemRarity.Uncommon => uncommonPrefixes[Random.Range(0, uncommonPrefixes.Length)],
            ItemRarity.Rare => rarePrefixes[Random.Range(0, rarePrefixes.Length)],
            ItemRarity.VeryRare => veryRarePrefixes[Random.Range(0, veryRarePrefixes.Length)],
            ItemRarity.Legendary => legendaryPrefixes[Random.Range(0, legendaryPrefixes.Length)],
            ItemRarity.Artifact => artifactPrefixes[Random.Range(0, artifactPrefixes.Length)],
            _ => commonPrefixes[Random.Range(0, commonPrefixes.Length)]
        };
    }
}
