using System.Collections.Generic;
using UnityEngine;

public class RandomItemGenerator : MonoBehaviour
{
    private static RandomItemGenerator instance;
    public static RandomItemGenerator Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("RandomItemGenerator");
                instance = go.AddComponent<RandomItemGenerator>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    // Level-based rarity chances (level 1-20)
    private static readonly Dictionary<int, Dictionary<ItemRarity, float>> rarityChances = new Dictionary<int, Dictionary<ItemRarity, float>>
    {
        // Level 1-5: Mostly common, some uncommon
        { 1, new Dictionary<ItemRarity, float> { { ItemRarity.Common, 0.9f }, { ItemRarity.Uncommon, 0.1f } } },
        { 2, new Dictionary<ItemRarity, float> { { ItemRarity.Common, 0.85f }, { ItemRarity.Uncommon, 0.15f } } },
        { 3, new Dictionary<ItemRarity, float> { { ItemRarity.Common, 0.80f }, { ItemRarity.Uncommon, 0.20f } } },
        { 4, new Dictionary<ItemRarity, float> { { ItemRarity.Common, 0.70f }, { ItemRarity.Uncommon, 0.30f } } },
        { 5, new Dictionary<ItemRarity, float> { { ItemRarity.Common, 0.60f }, { ItemRarity.Uncommon, 0.35f }, { ItemRarity.Rare, 0.05f } } },

        // Level 6-10: Common, uncommon, rare
        { 6, new Dictionary<ItemRarity, float> { { ItemRarity.Common, 0.50f }, { ItemRarity.Uncommon, 0.40f }, { ItemRarity.Rare, 0.10f } } },
        { 7, new Dictionary<ItemRarity, float> { { ItemRarity.Common, 0.40f }, { ItemRarity.Uncommon, 0.45f }, { ItemRarity.Rare, 0.15f } } },
        { 8, new Dictionary<ItemRarity, float> { { ItemRarity.Common, 0.30f }, { ItemRarity.Uncommon, 0.50f }, { ItemRarity.Rare, 0.20f } } },
        { 9, new Dictionary<ItemRarity, float> { { ItemRarity.Common, 0.25f }, { ItemRarity.Uncommon, 0.50f }, { ItemRarity.Rare, 0.25f } } },
        { 10, new Dictionary<ItemRarity, float> { { ItemRarity.Common, 0.20f }, { ItemRarity.Uncommon, 0.45f }, { ItemRarity.Rare, 0.30f }, { ItemRarity.VeryRare, 0.05f } } },

        // Level 11-15: Rare, very rare
        { 11, new Dictionary<ItemRarity, float> { { ItemRarity.Uncommon, 0.30f }, { ItemRarity.Rare, 0.50f }, { ItemRarity.VeryRare, 0.20f } } },
        { 12, new Dictionary<ItemRarity, float> { { ItemRarity.Uncommon, 0.25f }, { ItemRarity.Rare, 0.50f }, { ItemRarity.VeryRare, 0.25f } } },
        { 13, new Dictionary<ItemRarity, float> { { ItemRarity.Uncommon, 0.20f }, { ItemRarity.Rare, 0.50f }, { ItemRarity.VeryRare, 0.30f } } },
        { 14, new Dictionary<ItemRarity, float> { { ItemRarity.Rare, 0.50f }, { ItemRarity.VeryRare, 0.40f }, { ItemRarity.Legendary, 0.10f } } },
        { 15, new Dictionary<ItemRarity, float> { { ItemRarity.Rare, 0.40f }, { ItemRarity.VeryRare, 0.45f }, { ItemRarity.Legendary, 0.15f } } },

        // Level 16-20: Very rare, legendary, artifact
        { 16, new Dictionary<ItemRarity, float> { { ItemRarity.Rare, 0.30f }, { ItemRarity.VeryRare, 0.50f }, { ItemRarity.Legendary, 0.20f } } },
        { 17, new Dictionary<ItemRarity, float> { { ItemRarity.VeryRare, 0.50f }, { ItemRarity.Legendary, 0.45f }, { ItemRarity.Artifact, 0.05f } } },
        { 18, new Dictionary<ItemRarity, float> { { ItemRarity.VeryRare, 0.40f }, { ItemRarity.Legendary, 0.50f }, { ItemRarity.Artifact, 0.10f } } },
        { 19, new Dictionary<ItemRarity, float> { { ItemRarity.VeryRare, 0.30f }, { ItemRarity.Legendary, 0.60f }, { ItemRarity.Artifact, 0.10f } } },
        { 20, new Dictionary<ItemRarity, float> { { ItemRarity.Legendary, 0.80f }, { ItemRarity.Artifact, 0.20f } } },
    };

    // Generate a random item based on player level
    public Item GenerateRandomItem(int playerLevel)
    {
        playerLevel = Mathf.Clamp(playerLevel, 1, 20);

        // Determine rarity based on level
        ItemRarity rarity = GetRandomRarity(playerLevel);

        // Determine item type (weapon, armor, accessory, consumable)
        ItemType itemType = GetRandomItemType();

        // Generate item based on type
        ItemDefinition itemDef = itemType switch
        {
            ItemType.Weapon => GenerateRandomWeapon(playerLevel, rarity),
            ItemType.Armor => GenerateRandomArmor(playerLevel, rarity),
            ItemType.Accessory => GenerateRandomAccessory(playerLevel, rarity),
            ItemType.Consumable => GenerateRandomConsumable(playerLevel, rarity),
            _ => GenerateRandomWeapon(playerLevel, rarity)
        };

        // Create and return the Item ScriptableObject
        return CreateItemFromDefinition(itemDef);
    }

    private ItemRarity GetRandomRarity(int level)
    {
        if (!rarityChances.ContainsKey(level))
            level = Mathf.Clamp(level, 1, 20);

        float roll = Random.value;
        float cumulative = 0f;

        foreach (var kvp in rarityChances[level])
        {
            cumulative += kvp.Value;
            if (roll <= cumulative)
                return kvp.Key;
        }

        return ItemRarity.Common;
    }

    private ItemType GetRandomItemType()
    {
        float roll = Random.value;

        if (roll < 0.35f) return ItemType.Weapon;
        if (roll < 0.60f) return ItemType.Armor;
        if (roll < 0.85f) return ItemType.Accessory;
        return ItemType.Consumable;
    }

    private ItemDefinition GenerateRandomWeapon(int level, ItemRarity rarity)
    {
        // Select weapon type
        EquipmentSlot slot = Random.value < 0.7f ? EquipmentSlot.MainHand : EquipmentSlot.TwoHand;

        // Generate name
        string name = ItemNameGenerator.GenerateWeaponName(rarity, slot == EquipmentSlot.TwoHand);

        // Generate description
        string desc = $"A {rarity.ToString().ToLower()} weapon found at level {level}.";

        // Calculate damage based on level and rarity
        string damage = CalculateWeaponDamage(level, rarity, slot == EquipmentSlot.TwoHand);
        DamageType damageType = GetRandomDamageType(rarity);

        // Calculate stats
        int bonusStr = CalculateAttributeBonus(level, rarity);
        int bonusDex = rarity >= ItemRarity.Rare && Random.value < 0.3f ? CalculateAttributeBonus(level, rarity) / 2 : 0;

        // Calculate cost and weight
        int cost = CalculateItemCost(level, rarity, ItemType.Weapon);
        float weight = slot == EquipmentSlot.TwoHand ? Random.Range(5f, 8f) : Random.Range(2f, 4f);

        return new ItemDefinition(
            name: name,
            desc: desc,
            rarity: rarity,
            slot: slot,
            weight: weight,
            cost: cost,
            dmg: damage,
            dmgType: damageType,
            str: bonusStr,
            dex: bonusDex
        );
    }

    private ItemDefinition GenerateRandomArmor(int level, ItemRarity rarity)
    {
        // Select armor slot
        EquipmentSlot[] armorSlots = { EquipmentSlot.Chest, EquipmentSlot.Head, EquipmentSlot.Legs, EquipmentSlot.Feet, EquipmentSlot.Hands, EquipmentSlot.OffHand };
        EquipmentSlot slot = armorSlots[Random.Range(0, armorSlots.Length)];

        // Generate name
        string name = ItemNameGenerator.GenerateArmorName(rarity, slot);

        // Generate description
        string desc = $"A {rarity.ToString().ToLower()} piece of armor found at level {level}.";

        // Calculate AC based on level, rarity, and slot
        int ac = CalculateArmorClass(level, rarity, slot);

        // Calculate stats
        int bonusCon = CalculateAttributeBonus(level, rarity);
        int bonusDex = slot == EquipmentSlot.Chest && Random.value < 0.2f ? CalculateAttributeBonus(level, rarity) / 2 : 0;

        // Calculate cost and weight
        int cost = CalculateItemCost(level, rarity, ItemType.Armor);
        float weight = CalculateArmorWeight(slot);

        return new ItemDefinition(
            name: name,
            desc: desc,
            rarity: rarity,
            slot: slot,
            weight: weight,
            cost: cost,
            ac: ac,
            con: bonusCon,
            dex: bonusDex
        );
    }

    private ItemDefinition GenerateRandomAccessory(int level, ItemRarity rarity)
    {
        // Select accessory slot
        EquipmentSlot[] accessorySlots = { EquipmentSlot.Ring, EquipmentSlot.Neck, EquipmentSlot.Back, EquipmentSlot.Waist };
        EquipmentSlot slot = accessorySlots[Random.Range(0, accessorySlots.Length)];

        // Generate name
        string name = ItemNameGenerator.GenerateAccessoryName(rarity, slot);

        // Generate description
        string desc = $"A magical {slot.ToString().ToLower()} item found at level {level}.";

        // Accessories get random attribute bonuses
        int bonusStr = Random.value < 0.3f ? CalculateAttributeBonus(level, rarity) : 0;
        int bonusDex = Random.value < 0.3f ? CalculateAttributeBonus(level, rarity) : 0;
        int bonusCon = Random.value < 0.3f ? CalculateAttributeBonus(level, rarity) : 0;
        int bonusInt = Random.value < 0.3f ? CalculateAttributeBonus(level, rarity) : 0;
        int bonusWis = Random.value < 0.3f ? CalculateAttributeBonus(level, rarity) : 0;
        int bonusCha = Random.value < 0.3f ? CalculateAttributeBonus(level, rarity) : 0;

        // Ensure at least one bonus
        if (bonusStr + bonusDex + bonusCon + bonusInt + bonusWis + bonusCha == 0)
        {
            int randomStat = Random.Range(0, 6);
            int bonus = CalculateAttributeBonus(level, rarity);
            switch (randomStat)
            {
                case 0: bonusStr = bonus; break;
                case 1: bonusDex = bonus; break;
                case 2: bonusCon = bonus; break;
                case 3: bonusInt = bonus; break;
                case 4: bonusWis = bonus; break;
                case 5: bonusCha = bonus; break;
            }
        }

        int ac = rarity >= ItemRarity.Rare ? Random.Range(1, 3) : 0;
        int cost = CalculateItemCost(level, rarity, ItemType.Accessory);
        float weight = Random.Range(0.1f, 1f);

        return new ItemDefinition(
            name: name,
            desc: desc,
            rarity: rarity,
            slot: slot,
            weight: weight,
            cost: cost,
            ac: ac,
            str: bonusStr,
            dex: bonusDex,
            con: bonusCon,
            intel: bonusInt,
            wis: bonusWis,
            cha: bonusCha
        );
    }

    private ItemDefinition GenerateRandomConsumable(int level, ItemRarity rarity)
    {
        // Generate name
        string name = ItemNameGenerator.GenerateConsumableName(rarity);

        // Generate description
        string desc = $"A consumable item that can be used in combat or exploration.";

        int cost = CalculateItemCost(level, rarity, ItemType.Consumable);
        int maxStack = rarity >= ItemRarity.Rare ? 5 : 10;

        return new ItemDefinition(
            name: name,
            desc: desc,
            rarity: rarity,
            slot: EquipmentSlot.None,
            weight: 0.5f,
            cost: cost,
            stackable: true,
            maxStack: maxStack
        );
    }

    // Helper methods for calculations
    private string CalculateWeaponDamage(int level, ItemRarity rarity, bool twoHanded)
    {
        int baseDice = twoHanded ? 2 : 1;
        int diceSize = twoHanded ? 6 : 8;

        // Add bonus dice for higher rarity
        int bonusDice = rarity switch
        {
            ItemRarity.Uncommon => 1,
            ItemRarity.Rare => 2,
            ItemRarity.VeryRare => 3,
            ItemRarity.Legendary => 4,
            ItemRarity.Artifact => 5,
            _ => 0
        };

        int bonusFlat = (level / 5) + ((int)rarity);

        if (bonusDice > 0)
        {
            return $"{baseDice}d{diceSize}+{bonusDice}d{diceSize / 2}+{bonusFlat}";
        }
        else if (bonusFlat > 0)
        {
            return $"{baseDice}d{diceSize}+{bonusFlat}";
        }
        else
        {
            return $"{baseDice}d{diceSize}";
        }
    }

    private int CalculateArmorClass(int level, ItemRarity rarity, EquipmentSlot slot)
    {
        int baseAC = slot switch
        {
            EquipmentSlot.Chest => 11 + (level / 3),
            EquipmentSlot.OffHand => 2 + (level / 5),
            EquipmentSlot.Head => 1 + (level / 4),
            EquipmentSlot.Legs => 1 + (level / 5),
            EquipmentSlot.Hands => 1 + (level / 6),
            EquipmentSlot.Feet => 1 + (level / 6),
            _ => 0
        };

        int rarityBonus = (int)rarity;
        return baseAC + rarityBonus;
    }

    private int CalculateAttributeBonus(int level, ItemRarity rarity)
    {
        int base_bonus = 1 + (level / 4);
        int rarityBonus = (int)rarity;
        return base_bonus + rarityBonus;
    }

    private int CalculateItemCost(int level, ItemRarity rarity, ItemType type)
    {
        int baseCost = type switch
        {
            ItemType.Weapon => 50,
            ItemType.Armor => 75,
            ItemType.Accessory => 100,
            ItemType.Consumable => 25,
            _ => 50
        };

        int levelMultiplier = 1 + level;
        int rarityMultiplier = rarity switch
        {
            ItemRarity.Common => 1,
            ItemRarity.Uncommon => 5,
            ItemRarity.Rare => 20,
            ItemRarity.VeryRare => 50,
            ItemRarity.Legendary => 200,
            ItemRarity.Artifact => 1000,
            _ => 1
        };

        return baseCost * levelMultiplier * rarityMultiplier;
    }

    private float CalculateArmorWeight(EquipmentSlot slot)
    {
        return slot switch
        {
            EquipmentSlot.Chest => Random.Range(10f, 25f),
            EquipmentSlot.OffHand => Random.Range(5f, 10f),
            EquipmentSlot.Head => Random.Range(2f, 5f),
            EquipmentSlot.Legs => Random.Range(5f, 10f),
            EquipmentSlot.Hands => Random.Range(1f, 3f),
            EquipmentSlot.Feet => Random.Range(2f, 4f),
            _ => 1f
        };
    }

    private DamageType GetRandomDamageType(ItemRarity rarity)
    {
        // Common items use physical damage
        if (rarity < ItemRarity.Rare)
        {
            DamageType[] physicalTypes = { DamageType.Slashing, DamageType.Piercing, DamageType.Bludgeoning };
            return physicalTypes[Random.Range(0, physicalTypes.Length)];
        }

        // Rare+ items can use elemental damage
        DamageType[] allTypes = {
            DamageType.Slashing, DamageType.Piercing, DamageType.Bludgeoning,
            DamageType.Fire, DamageType.Cold, DamageType.Lightning, DamageType.Thunder,
            DamageType.Acid, DamageType.Necrotic, DamageType.Radiant, DamageType.Force
        };
        return allTypes[Random.Range(0, allTypes.Length)];
    }

    private Item CreateItemFromDefinition(ItemDefinition def)
    {
        Item item = ScriptableObject.CreateInstance<Item>();

        var itemType = typeof(Item);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

        itemType.GetField("itemName", flags)?.SetValue(item, def.itemName);
        itemType.GetField("description", flags)?.SetValue(item, def.description);
        itemType.GetField("rarity", flags)?.SetValue(item, def.rarity);
        itemType.GetField("equipmentSlot", flags)?.SetValue(item, def.equipmentSlot);
        itemType.GetField("weight", flags)?.SetValue(item, def.weight);
        itemType.GetField("cost", flags)?.SetValue(item, def.cost);
        itemType.GetField("armorClass", flags)?.SetValue(item, def.armorClass);
        itemType.GetField("damage", flags)?.SetValue(item, def.damage);
        itemType.GetField("damageType", flags)?.SetValue(item, def.damageType);
        itemType.GetField("bonusStrength", flags)?.SetValue(item, def.bonusStrength);
        itemType.GetField("bonusDexterity", flags)?.SetValue(item, def.bonusDexterity);
        itemType.GetField("bonusConstitution", flags)?.SetValue(item, def.bonusConstitution);
        itemType.GetField("bonusIntelligence", flags)?.SetValue(item, def.bonusIntelligence);
        itemType.GetField("bonusWisdom", flags)?.SetValue(item, def.bonusWisdom);
        itemType.GetField("bonusCharisma", flags)?.SetValue(item, def.bonusCharisma);
        itemType.GetField("isStackable", flags)?.SetValue(item, def.isStackable);
        itemType.GetField("maxStackSize", flags)?.SetValue(item, def.maxStackSize);
        itemType.GetField("isQuestItem", flags)?.SetValue(item, def.isQuestItem);
        itemType.GetField("canBeSold", flags)?.SetValue(item, def.canBeSold);
        itemType.GetField("canBeDropped", flags)?.SetValue(item, def.canBeDropped);

        return item;
    }
}

public enum ItemType
{
    Weapon,
    Armor,
    Accessory,
    Consumable
}
