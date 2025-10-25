using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [Header("Equipment Slots")]
    [SerializeField] private Dictionary<EquipmentSlot, Item> equippedItems = new Dictionary<EquipmentSlot, Item>();

    [Header("References")]
    [SerializeField] private Inventory inventory;

    // Events
    public event Action<EquipmentSlot, Item> OnItemEquipped;
    public event Action<EquipmentSlot, Item> OnItemUnequipped;
    public event Action<PlayerEquipment> OnEquipmentChanged;

    public Dictionary<EquipmentSlot, Item> EquippedItems => equippedItems;

    private void Awake()
    {
        if (inventory == null)
        {
            inventory = GetComponent<Inventory>();
        }

        InitializeEquipmentSlots();
    }

    private void InitializeEquipmentSlots()
    {
        // Initialize all equipment slots as empty
        equippedItems.Clear();
        foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
        {
            if (slot != EquipmentSlot.None)
            {
                equippedItems[slot] = null;
            }
        }
    }

    // Equip item from inventory
    public bool EquipItem(Item item)
    {
        if (item == null || !item.IsWearable)
        {
            Debug.LogWarning("Cannot equip item: item is null or not wearable");
            return false;
        }

        // Check if player has this item in inventory
        if (inventory != null && !inventory.HasItem(item))
        {
            Debug.LogWarning($"Cannot equip {item.ItemName}: not in inventory");
            return false;
        }

        EquipmentSlot slot = item.EquipmentSlot;

        // Handle two-handed weapons
        if (slot == EquipmentSlot.TwoHand)
        {
            // Unequip main hand and off hand
            UnequipSlot(EquipmentSlot.MainHand);
            UnequipSlot(EquipmentSlot.OffHand);
        }
        else if (slot == EquipmentSlot.MainHand || slot == EquipmentSlot.OffHand)
        {
            // Unequip two-handed weapon if equipped
            UnequipSlot(EquipmentSlot.TwoHand);
        }

        // Handle rings - allow multiple rings
        if (slot == EquipmentSlot.Ring)
        {
            // Find first empty ring slot or replace existing
            // For simplicity, just use the Ring slot
        }

        // Unequip current item in slot
        Item previousItem = GetEquippedItem(slot);
        if (previousItem != null)
        {
            UnequipSlot(slot);
        }

        // Remove from inventory
        if (inventory != null)
        {
            inventory.RemoveItem(item, 1);
        }

        // Equip the item
        equippedItems[slot] = item;

        OnItemEquipped?.Invoke(slot, item);
        OnEquipmentChanged?.Invoke(this);

        Debug.Log($"Equipped {item.ItemName} to {slot.GetDisplayName()}");
        return true;
    }

    // Unequip item from slot
    public bool UnequipSlot(EquipmentSlot slot)
    {
        if (slot == EquipmentSlot.None)
            return false;

        Item equippedItem = GetEquippedItem(slot);
        if (equippedItem == null)
            return false;

        // Check if inventory has space
        if (inventory != null && !inventory.HasSpaceFor(equippedItem, 1))
        {
            Debug.LogWarning($"Cannot unequip {equippedItem.ItemName}: inventory full");
            return false;
        }

        // Remove from equipment
        equippedItems[slot] = null;

        // Add back to inventory
        if (inventory != null)
        {
            inventory.AddItem(equippedItem, 1);
        }

        OnItemUnequipped?.Invoke(slot, equippedItem);
        OnEquipmentChanged?.Invoke(this);

        Debug.Log($"Unequipped {equippedItem.ItemName} from {slot.GetDisplayName()}");
        return true;
    }

    // Unequip specific item
    public bool UnequipItem(Item item)
    {
        if (item == null)
            return false;

        foreach (var kvp in equippedItems)
        {
            if (kvp.Value == item)
            {
                return UnequipSlot(kvp.Key);
            }
        }

        return false;
    }

    // Get equipped item in specific slot
    public Item GetEquippedItem(EquipmentSlot slot)
    {
        if (slot == EquipmentSlot.None)
            return null;

        return equippedItems.ContainsKey(slot) ? equippedItems[slot] : null;
    }

    // Check if item is equipped
    public bool IsItemEquipped(Item item)
    {
        if (item == null)
            return false;

        return equippedItems.Values.Contains(item);
    }

    // Check if slot has item equipped
    public bool IsSlotEquipped(EquipmentSlot slot)
    {
        return GetEquippedItem(slot) != null;
    }

    // Get total armor class from all equipped items
    public int GetTotalArmorClass()
    {
        int totalAC = 10; // Base AC

        foreach (var item in equippedItems.Values)
        {
            if (item != null)
            {
                totalAC += item.ArmorClass;
            }
        }

        return totalAC;
    }

    // Get total attribute bonus
    public int GetAttributeBonus(string attribute)
    {
        int bonus = 0;

        foreach (var item in equippedItems.Values)
        {
            if (item == null)
                continue;

            bonus += attribute.ToLower() switch
            {
                "strength" or "str" => item.BonusStrength,
                "dexterity" or "dex" => item.BonusDexterity,
                "constitution" or "con" => item.BonusConstitution,
                "intelligence" or "int" => item.BonusIntelligence,
                "wisdom" or "wis" => item.BonusWisdom,
                "charisma" or "cha" => item.BonusCharisma,
                _ => 0
            };
        }

        return bonus;
    }

    // Get all equipped items
    public List<Item> GetAllEquippedItems()
    {
        return equippedItems.Values.Where(item => item != null).ToList();
    }

    // Get total weight of equipped items
    public float GetTotalEquippedWeight()
    {
        float totalWeight = 0f;

        foreach (var item in equippedItems.Values)
        {
            if (item != null)
            {
                totalWeight += item.Weight;
            }
        }

        return totalWeight;
    }

    // Unequip all items
    public void UnequipAll()
    {
        List<EquipmentSlot> slotsToUnequip = new List<EquipmentSlot>();

        foreach (var kvp in equippedItems)
        {
            if (kvp.Value != null)
            {
                slotsToUnequip.Add(kvp.Key);
            }
        }

        foreach (var slot in slotsToUnequip)
        {
            UnequipSlot(slot);
        }
    }

    // Print equipped items
    public void PrintEquipment()
    {
        Debug.Log("=== EQUIPPED ITEMS ===");
        Debug.Log($"Total AC: {GetTotalArmorClass()}");
        Debug.Log($"Total Weight: {GetTotalEquippedWeight()} lbs");
        Debug.Log("\nEquipment:");

        foreach (var kvp in equippedItems)
        {
            if (kvp.Value != null)
            {
                Debug.Log($"{kvp.Key.GetDisplayName()}: {kvp.Value.ItemName}");
            }
        }

        Debug.Log("\nAttribute Bonuses:");
        Debug.Log($"Strength: +{GetAttributeBonus("str")}");
        Debug.Log($"Dexterity: +{GetAttributeBonus("dex")}");
        Debug.Log($"Constitution: +{GetAttributeBonus("con")}");
        Debug.Log($"Intelligence: +{GetAttributeBonus("int")}");
        Debug.Log($"Wisdom: +{GetAttributeBonus("wis")}");
        Debug.Log($"Charisma: +{GetAttributeBonus("cha")}");
    }
}
