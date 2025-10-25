using UnityEngine;
using UnityEditor;
using System.Reflection;

public class ItemCreator : EditorWindow
{
    [MenuItem("Tools/Generate All Items from Library")]
    public static void GenerateAllItems()
    {
        string itemsPath = "Assets/Resources/Items";

        // Create directories if they don't exist
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        if (!AssetDatabase.IsValidFolder(itemsPath))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "Items");
        }

        int created = 0;
        int updated = 0;

        // Generate all items from ItemLibrary
        foreach (var itemDef in ItemLibrary.AllItems)
        {
            string fileName = itemDef.itemName.Replace(" ", "");
            string assetPath = $"{itemsPath}/{fileName}.asset";

            // Check if item already exists
            Item existingItem = AssetDatabase.LoadAssetAtPath<Item>(assetPath);

            if (existingItem != null)
            {
                // Update existing item
                ApplyItemDefinition(existingItem, itemDef);
                EditorUtility.SetDirty(existingItem);
                updated++;
                Debug.Log($"Updated: {itemDef.itemName}");
            }
            else
            {
                // Create new item
                Item newItem = ScriptableObject.CreateInstance<Item>();
                ApplyItemDefinition(newItem, itemDef);
                AssetDatabase.CreateAsset(newItem, assetPath);
                created++;
                Debug.Log($"Created: {itemDef.itemName}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"<color=green>Item generation complete!</color>");
        Debug.Log($"Created: {created} | Updated: {updated} | Total: {ItemLibrary.AllItems.Count}");
        Debug.Log($"Items saved to: {itemsPath}");
    }

    private static void ApplyItemDefinition(Item item, ItemDefinition def)
    {
        var itemType = typeof(Item);
        BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

        // Set all fields using reflection
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
    }

    [MenuItem("Tools/List All Items in Library")]
    public static void ListAllItems()
    {
        Debug.Log($"=== ITEM LIBRARY ({ItemLibrary.AllItems.Count} items) ===\n");

        // Group by rarity
        foreach (ItemRarity rarity in System.Enum.GetValues(typeof(ItemRarity)))
        {
            var itemsOfRarity = ItemLibrary.GetItemsByRarity(rarity);
            if (itemsOfRarity.Count > 0)
            {
                Debug.Log($"<color={(rarity.GetColor().r > 0.5f ? "orange" : "cyan")}>{rarity} ({itemsOfRarity.Count})</color>");
                foreach (var item in itemsOfRarity)
                {
                    string slotInfo = item.equipmentSlot != EquipmentSlot.None ? $" [{item.equipmentSlot}]" : "";
                    Debug.Log($"  - {item.itemName}{slotInfo} ({item.cost}gp, {item.weight}lbs)");
                }
                Debug.Log("");
            }
        }
    }
}
