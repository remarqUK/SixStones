using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<Item> items = new List<Item>();

    public List<Item> Items => items;

    // Find item by name
    public Item GetItemByName(string itemName)
    {
        return items.FirstOrDefault(item => item.ItemName.Equals(itemName, System.StringComparison.OrdinalIgnoreCase));
    }

    // Get items by rarity
    public List<Item> GetItemsByRarity(ItemRarity rarity)
    {
        return items.Where(item => item.Rarity == rarity).ToList();
    }

    // Get items by equipment slot
    public List<Item> GetItemsBySlot(EquipmentSlot slot)
    {
        return items.Where(item => item.EquipmentSlot == slot).ToList();
    }

    // Get wearable items only
    public List<Item> GetWearableItems()
    {
        return items.Where(item => item.IsWearable).ToList();
    }

    // Get items within a cost range
    public List<Item> GetItemsByCostRange(int minCost, int maxCost)
    {
        return items.Where(item => item.Cost >= minCost && item.Cost <= maxCost).ToList();
    }

    // Get random item
    public Item GetRandomItem()
    {
        if (items.Count == 0) return null;
        return items[Random.Range(0, items.Count)];
    }

    // Get random item by rarity
    public Item GetRandomItemByRarity(ItemRarity rarity)
    {
        var itemsOfRarity = GetItemsByRarity(rarity);
        if (itemsOfRarity.Count == 0) return null;
        return itemsOfRarity[Random.Range(0, itemsOfRarity.Count)];
    }

    // Add item to database
    public void AddItem(Item item)
    {
        if (item != null && !items.Contains(item))
        {
            items.Add(item);
        }
    }

    // Remove item from database
    public void RemoveItem(Item item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
        }
    }

    // Check if item exists
    public bool ContainsItem(Item item)
    {
        return items.Contains(item);
    }

    // Get total item count
    public int GetItemCount()
    {
        return items.Count;
    }
}
