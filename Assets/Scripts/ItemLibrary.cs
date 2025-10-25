using System.Collections.Generic;
using UnityEngine;

// Item definition structure for hardcoded items
[System.Serializable]
public struct ItemDefinition
{
    public string itemName;
    public string description;
    public ItemRarity rarity;
    public EquipmentSlot equipmentSlot;
    public float weight;
    public int cost;
    public int armorClass;
    public string damage;
    public DamageType damageType;
    public int bonusStrength;
    public int bonusDexterity;
    public int bonusConstitution;
    public int bonusIntelligence;
    public int bonusWisdom;
    public int bonusCharisma;
    public bool isStackable;
    public int maxStackSize;
    public bool isQuestItem;
    public bool canBeSold;
    public bool canBeDropped;

    public ItemDefinition(
        string name,
        string desc,
        ItemRarity rarity = ItemRarity.Common,
        EquipmentSlot slot = EquipmentSlot.None,
        float weight = 1f,
        int cost = 10,
        int ac = 0,
        string dmg = "0",
        DamageType dmgType = DamageType.None,
        int str = 0, int dex = 0, int con = 0, int intel = 0, int wis = 0, int cha = 0,
        bool stackable = false,
        int maxStack = 1,
        bool questItem = false,
        bool canSell = true,
        bool canDrop = true)
    {
        itemName = name;
        description = desc;
        this.rarity = rarity;
        equipmentSlot = slot;
        this.weight = weight;
        this.cost = cost;
        armorClass = ac;
        damage = dmg;
        damageType = dmgType;
        bonusStrength = str;
        bonusDexterity = dex;
        bonusConstitution = con;
        bonusIntelligence = intel;
        bonusWisdom = wis;
        bonusCharisma = cha;
        isStackable = stackable;
        maxStackSize = maxStack;
        isQuestItem = questItem;
        canBeSold = canSell;
        canBeDropped = canDrop;
    }
}

public static class ItemLibrary
{
    // HARDCODED ITEM LIST - EDIT THIS TO ADD/MODIFY ITEMS
    public static readonly List<ItemDefinition> AllItems = new List<ItemDefinition>
    {
        // ==================== WEAPONS ====================

        // Common Weapons
        new ItemDefinition(
            name: "Longsword",
            desc: "A versatile blade favored by warriors across the realm. Well-balanced and deadly in skilled hands.",
            rarity: ItemRarity.Common,
            slot: EquipmentSlot.MainHand,
            weight: 3f,
            cost: 15,
            dmg: "1d8",
            dmgType: DamageType.Slashing
        ),

        new ItemDefinition(
            name: "Shortsword",
            desc: "A lightweight blade perfect for quick strikes. Popular among rogues and scouts.",
            rarity: ItemRarity.Common,
            slot: EquipmentSlot.MainHand,
            weight: 2f,
            cost: 10,
            dmg: "1d6",
            dmgType: DamageType.Piercing
        ),

        new ItemDefinition(
            name: "Dagger",
            desc: "A small, easily concealed blade. Can be thrown or used in melee combat.",
            rarity: ItemRarity.Common,
            slot: EquipmentSlot.MainHand,
            weight: 1f,
            cost: 2,
            dmg: "1d4",
            dmgType: DamageType.Piercing
        ),

        new ItemDefinition(
            name: "Battleaxe",
            desc: "A heavy axe designed for cleaving through armor and bone.",
            rarity: ItemRarity.Common,
            slot: EquipmentSlot.MainHand,
            weight: 4f,
            cost: 10,
            dmg: "1d8",
            dmgType: DamageType.Slashing
        ),

        new ItemDefinition(
            name: "Warhammer",
            desc: "A weighty hammer that can crush armor and shatter shields.",
            rarity: ItemRarity.Common,
            slot: EquipmentSlot.MainHand,
            weight: 5f,
            cost: 12,
            dmg: "1d8",
            dmgType: DamageType.Bludgeoning
        ),

        new ItemDefinition(
            name: "Greataxe",
            desc: "A massive two-handed axe that requires great strength to wield. Devastating when it connects.",
            rarity: ItemRarity.Common,
            slot: EquipmentSlot.TwoHand,
            weight: 7f,
            cost: 30,
            dmg: "1d12",
            dmgType: DamageType.Slashing
        ),

        // Uncommon Weapons
        new ItemDefinition(
            name: "Silver Longsword",
            desc: "A finely crafted longsword with a silver-plated blade. Effective against supernatural creatures.",
            rarity: ItemRarity.Uncommon,
            slot: EquipmentSlot.MainHand,
            weight: 3f,
            cost: 100,
            dmg: "1d8+1",
            dmgType: DamageType.Slashing,
            str: 1
        ),

        // Rare Weapons
        new ItemDefinition(
            name: "Vorpal Blade",
            desc: "An incredibly sharp sword that can sever limbs and heads with ease. The blade hums with magical energy.",
            rarity: ItemRarity.Rare,
            slot: EquipmentSlot.MainHand,
            weight: 3f,
            cost: 800,
            dmg: "1d8+2",
            dmgType: DamageType.Slashing,
            str: 2,
            dex: 1
        ),

        // Legendary Weapons
        new ItemDefinition(
            name: "Flametongue",
            desc: "This magical longsword bursts into flame when drawn. The blade burns with intense heat, dealing devastating fire damage to enemies.",
            rarity: ItemRarity.Legendary,
            slot: EquipmentSlot.MainHand,
            weight: 3f,
            cost: 5000,
            dmg: "1d8+2d6",
            dmgType: DamageType.Fire,
            str: 2
        ),

        new ItemDefinition(
            name: "Frostbrand",
            desc: "An ancient greatsword that radiates cold. When swung, it leaves trails of frost in the air.",
            rarity: ItemRarity.Legendary,
            slot: EquipmentSlot.TwoHand,
            weight: 6f,
            cost: 6000,
            dmg: "2d6+1d8",
            dmgType: DamageType.Cold,
            str: 3,
            con: 2
        ),

        // ==================== ARMOR ====================

        // Common Armor
        new ItemDefinition(
            name: "Leather Armor",
            desc: "Light armor crafted from supple leather. Offers basic protection without restricting movement.",
            rarity: ItemRarity.Common,
            slot: EquipmentSlot.Chest,
            weight: 10f,
            cost: 10,
            ac: 11
        ),

        new ItemDefinition(
            name: "Chainmail",
            desc: "Interlocking metal rings provide solid protection. Makes some noise when moving.",
            rarity: ItemRarity.Common,
            slot: EquipmentSlot.Chest,
            weight: 55f,
            cost: 75,
            ac: 16
        ),

        new ItemDefinition(
            name: "Plate Armor",
            desc: "Heavy plates of steel offering maximum protection. Extremely heavy but nearly impenetrable.",
            rarity: ItemRarity.Common,
            slot: EquipmentSlot.Chest,
            weight: 65f,
            cost: 1500,
            ac: 18
        ),

        // Uncommon Armor
        new ItemDefinition(
            name: "Studded Leather Armor",
            desc: "Leather armor reinforced with metal studs. Provides better protection while maintaining mobility.",
            rarity: ItemRarity.Uncommon,
            slot: EquipmentSlot.Chest,
            weight: 13f,
            cost: 45,
            ac: 12,
            dex: 1
        ),

        // Rare Armor
        new ItemDefinition(
            name: "Elven Chain",
            desc: "Magical chainmail crafted by elven smiths. Light as leather but strong as steel.",
            rarity: ItemRarity.Rare,
            slot: EquipmentSlot.Chest,
            weight: 20f,
            cost: 5000,
            ac: 16,
            dex: 2,
            con: 1
        ),

        // ==================== SHIELDS ====================

        new ItemDefinition(
            name: "Wooden Shield",
            desc: "A simple shield made of reinforced wood and iron bands.",
            rarity: ItemRarity.Common,
            slot: EquipmentSlot.OffHand,
            weight: 6f,
            cost: 10,
            ac: 2
        ),

        new ItemDefinition(
            name: "Steel Shield",
            desc: "A sturdy shield made entirely of steel. Provides excellent protection.",
            rarity: ItemRarity.Uncommon,
            slot: EquipmentSlot.OffHand,
            weight: 8f,
            cost: 25,
            ac: 2
        ),

        // ==================== ACCESSORIES ====================

        // Rings
        new ItemDefinition(
            name: "Ring of Protection",
            desc: "A magical ring that shimmers with protective energy. Grants the wearer enhanced defenses.",
            rarity: ItemRarity.Rare,
            slot: EquipmentSlot.Ring,
            weight: 0.1f,
            cost: 500,
            ac: 1,
            con: 1
        ),

        new ItemDefinition(
            name: "Ring of Strength",
            desc: "This heavy gold ring pulses with power, greatly enhancing the wearer's physical might.",
            rarity: ItemRarity.VeryRare,
            slot: EquipmentSlot.Ring,
            weight: 0.1f,
            cost: 2000,
            str: 3
        ),

        new ItemDefinition(
            name: "Ring of Wizardry",
            desc: "An ornate ring set with arcane runes. Enhances the wearer's magical abilities.",
            rarity: ItemRarity.Rare,
            slot: EquipmentSlot.Ring,
            weight: 0.1f,
            cost: 800,
            intel: 2,
            wis: 1
        ),

        // Amulets
        new ItemDefinition(
            name: "Amulet of Health",
            desc: "A jade amulet that radiates vitality. The wearer feels noticeably healthier.",
            rarity: ItemRarity.VeryRare,
            slot: EquipmentSlot.Neck,
            weight: 0.5f,
            cost: 3000,
            con: 4
        ),

        new ItemDefinition(
            name: "Amulet of Wisdom",
            desc: "A silver amulet bearing the symbol of an open eye. Grants enhanced perception and insight.",
            rarity: ItemRarity.Rare,
            slot: EquipmentSlot.Neck,
            weight: 0.5f,
            cost: 1200,
            wis: 3
        ),

        // Cloaks
        new ItemDefinition(
            name: "Cloak of Elvenkind",
            desc: "A forest-green cloak that helps its wearer blend into surroundings. Extremely well-crafted.",
            rarity: ItemRarity.Uncommon,
            slot: EquipmentSlot.Back,
            weight: 1f,
            cost: 300,
            dex: 2
        ),

        new ItemDefinition(
            name: "Cloak of Protection",
            desc: "A heavy cloak imbued with protective magic. Provides defense against both physical and magical attacks.",
            rarity: ItemRarity.Rare,
            slot: EquipmentSlot.Back,
            weight: 2f,
            cost: 600,
            ac: 1,
            con: 1,
            wis: 1
        ),

        // Belts
        new ItemDefinition(
            name: "Belt of Giant Strength",
            desc: "A thick leather belt adorned with a massive buckle. Grants the strength of a hill giant to its wearer.",
            rarity: ItemRarity.Legendary,
            slot: EquipmentSlot.Waist,
            weight: 2f,
            cost: 8000,
            str: 5
        ),

        // Head
        new ItemDefinition(
            name: "Helmet of Comprehension",
            desc: "A bronze helmet etched with mystical symbols. Enhances the wearer's mental faculties.",
            rarity: ItemRarity.Uncommon,
            slot: EquipmentSlot.Head,
            weight: 3f,
            cost: 400,
            ac: 1,
            intel: 2
        ),

        // Gloves
        new ItemDefinition(
            name: "Gauntlets of Ogre Power",
            desc: "These heavy gauntlets are sized for an ogre but magically resize to fit any wearer. Grant tremendous strength.",
            rarity: ItemRarity.Rare,
            slot: EquipmentSlot.Hands,
            weight: 2f,
            cost: 1500,
            str: 3,
            con: 1
        ),

        // Boots
        new ItemDefinition(
            name: "Boots of Speed",
            desc: "Lightweight boots that allow the wearer to move with supernatural quickness.",
            rarity: ItemRarity.Rare,
            slot: EquipmentSlot.Feet,
            weight: 1f,
            cost: 800,
            dex: 3
        ),

        // ==================== CONSUMABLES ====================

        new ItemDefinition(
            name: "Health Potion",
            desc: "A vial of red liquid that restores health when consumed. Heals 2d4+2 hit points.",
            rarity: ItemRarity.Common,
            slot: EquipmentSlot.None,
            weight: 0.5f,
            cost: 50,
            stackable: true,
            maxStack: 10
        ),

        new ItemDefinition(
            name: "Greater Health Potion",
            desc: "A larger vial of crimson liquid with golden swirls. Heals 4d4+4 hit points.",
            rarity: ItemRarity.Uncommon,
            slot: EquipmentSlot.None,
            weight: 0.5f,
            cost: 150,
            stackable: true,
            maxStack: 10
        ),

        new ItemDefinition(
            name: "Mana Potion",
            desc: "A glowing blue potion that restores magical energy. Restores 2d4+2 mana points.",
            rarity: ItemRarity.Common,
            slot: EquipmentSlot.None,
            weight: 0.5f,
            cost: 50,
            stackable: true,
            maxStack: 10
        ),

        new ItemDefinition(
            name: "Elixir of Strength",
            desc: "A thick, orange potion. Temporarily increases Strength by 4 for 1 hour.",
            rarity: ItemRarity.Uncommon,
            slot: EquipmentSlot.None,
            weight: 0.5f,
            cost: 100,
            stackable: true,
            maxStack: 5
        ),

        // ==================== ARTIFACTS ====================

        new ItemDefinition(
            name: "Crown of Kings",
            desc: "An ancient artifact worn by the rulers of old. Its power grants the wearer exceptional charisma and wisdom, inspiring allies and commanding respect from all who gaze upon it.",
            rarity: ItemRarity.Artifact,
            slot: EquipmentSlot.Head,
            weight: 2f,
            cost: 50000,
            ac: 2,
            cha: 4,
            wis: 3,
            questItem: true,
            canSell: false
        ),

        new ItemDefinition(
            name: "Staff of the Archmage",
            desc: "A legendary staff that has passed through the hands of the greatest mages in history. Radiates immense magical power.",
            rarity: ItemRarity.Artifact,
            slot: EquipmentSlot.TwoHand,
            weight: 4f,
            cost: 75000,
            dmg: "1d6+3d10",
            dmgType: DamageType.Force,
            intel: 5,
            wis: 3,
            questItem: true,
            canSell: false,
            canDrop: false
        ),

        // ==================== QUEST ITEMS ====================

        new ItemDefinition(
            name: "Ancient Amulet Fragment",
            desc: "A broken piece of an ancient amulet. Strange runes cover its surface. Perhaps it can be restored?",
            rarity: ItemRarity.Rare,
            slot: EquipmentSlot.None,
            weight: 0.2f,
            cost: 0,
            questItem: true,
            canSell: false
        ),

        new ItemDefinition(
            name: "Mysterious Key",
            desc: "An ornate key made of an unknown metal. It doesn't seem to fit any normal lock.",
            rarity: ItemRarity.Uncommon,
            slot: EquipmentSlot.None,
            weight: 0.1f,
            cost: 0,
            questItem: true,
            canSell: false,
            canDrop: false
        ),
    };

    // Helper method to find item by name
    public static ItemDefinition? FindItemByName(string name)
    {
        foreach (var item in AllItems)
        {
            if (item.itemName.Equals(name, System.StringComparison.OrdinalIgnoreCase))
            {
                return item;
            }
        }
        return null;
    }

    // Get items by rarity
    public static List<ItemDefinition> GetItemsByRarity(ItemRarity rarity)
    {
        List<ItemDefinition> result = new List<ItemDefinition>();
        foreach (var item in AllItems)
        {
            if (item.rarity == rarity)
            {
                result.Add(item);
            }
        }
        return result;
    }

    // Get items by slot
    public static List<ItemDefinition> GetItemsBySlot(EquipmentSlot slot)
    {
        List<ItemDefinition> result = new List<ItemDefinition>();
        foreach (var item in AllItems)
        {
            if (item.equipmentSlot == slot)
            {
                result.Add(item);
            }
        }
        return result;
    }
}
