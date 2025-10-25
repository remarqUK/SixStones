using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    [Header("Basic Information")]
    [SerializeField] private string itemName;
    [SerializeField] [TextArea(3, 6)] private string description;
    [SerializeField] private Sprite icon;

    [Header("Item Properties")]
    [SerializeField] private ItemRarity rarity = ItemRarity.Common;
    [SerializeField] private EquipmentSlot equipmentSlot = EquipmentSlot.None;
    [SerializeField] private float weight = 1f;
    [SerializeField] private int cost = 10;

    [Header("Combat Stats")]
    [SerializeField] private int armorClass = 0;
    [SerializeField] private string damage = "0";  // e.g., "1d6", "2d8+2"
    [SerializeField] private DamageType damageType = DamageType.None;

    [Header("Magical Properties (Optional)")]
    [SerializeField] private int bonusStrength = 0;
    [SerializeField] private int bonusDexterity = 0;
    [SerializeField] private int bonusConstitution = 0;
    [SerializeField] private int bonusIntelligence = 0;
    [SerializeField] private int bonusWisdom = 0;
    [SerializeField] private int bonusCharisma = 0;

    [Header("Special Properties")]
    [SerializeField] private bool isStackable = false;
    [SerializeField] private int maxStackSize = 1;
    [SerializeField] private bool isQuestItem = false;
    [SerializeField] private bool canBeSold = true;
    [SerializeField] private bool canBeDropped = true;

    // Public accessors
    public string ItemName => itemName;
    public string Description => description;
    public Sprite Icon => icon;
    public ItemRarity Rarity => rarity;
    public EquipmentSlot EquipmentSlot => equipmentSlot;
    public float Weight => weight;
    public int Cost => cost;
    public int ArmorClass => armorClass;
    public string Damage => damage;
    public DamageType DamageType => damageType;
    public int BonusStrength => bonusStrength;
    public int BonusDexterity => bonusDexterity;
    public int BonusConstitution => bonusConstitution;
    public int BonusIntelligence => bonusIntelligence;
    public int BonusWisdom => bonusWisdom;
    public int BonusCharisma => bonusCharisma;
    public bool IsStackable => isStackable;
    public int MaxStackSize => maxStackSize;
    public bool IsQuestItem => isQuestItem;
    public bool CanBeSold => canBeSold;
    public bool CanBeDropped => canBeDropped;

    public bool IsWearable => equipmentSlot.IsWearable();

    // Utility methods
    public string GetColoredName()
    {
        Color rarityColor = rarity.GetColor();
        string hexColor = ColorUtility.ToHtmlStringRGB(rarityColor);
        return $"<color=#{hexColor}>{itemName}</color>";
    }

    public string GetFullDescription()
    {
        string fullDesc = $"<b>{GetColoredName()}</b>\n";
        fullDesc += $"<i>{rarity.GetDisplayName()}</i>\n\n";
        fullDesc += $"{description}\n\n";

        // Combat stats
        if (armorClass > 0)
            fullDesc += $"Armor Class: {armorClass}\n";

        if (!string.IsNullOrEmpty(damage) && damage != "0")
            fullDesc += $"Damage: {damage} ({damageType})\n";

        // Equipment slot
        if (IsWearable)
            fullDesc += $"Slot: {equipmentSlot.GetDisplayName()}\n";

        // Attribute bonuses
        if (bonusStrength != 0)
            fullDesc += $"Strength: {(bonusStrength > 0 ? "+" : "")}{bonusStrength}\n";
        if (bonusDexterity != 0)
            fullDesc += $"Dexterity: {(bonusDexterity > 0 ? "+" : "")}{bonusDexterity}\n";
        if (bonusConstitution != 0)
            fullDesc += $"Constitution: {(bonusConstitution > 0 ? "+" : "")}{bonusConstitution}\n";
        if (bonusIntelligence != 0)
            fullDesc += $"Intelligence: {(bonusIntelligence > 0 ? "+" : "")}{bonusIntelligence}\n";
        if (bonusWisdom != 0)
            fullDesc += $"Wisdom: {(bonusWisdom > 0 ? "+" : "")}{bonusWisdom}\n";
        if (bonusCharisma != 0)
            fullDesc += $"Charisma: {(bonusCharisma > 0 ? "+" : "")}{bonusCharisma}\n";

        // Item properties
        fullDesc += $"\nWeight: {weight} lbs\n";
        fullDesc += $"Value: {cost} gp\n";

        if (isQuestItem)
            fullDesc += "<color=yellow>Quest Item</color>\n";

        return fullDesc;
    }

    public override string ToString()
    {
        return $"{itemName} ({rarity})";
    }
}
