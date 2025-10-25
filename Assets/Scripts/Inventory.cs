using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int maxSlots = 20;
    [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();

    public int MaxSlots => maxSlots;
    public List<InventorySlot> Slots => slots;
    public int UsedSlots => slots.Count(slot => !slot.IsEmpty);
    public int FreeSlots => maxSlots - UsedSlots;

    // Events
    public event Action<Item, int> OnItemAdded;
    public event Action<Item, int> OnItemRemoved;
    public event Action<Inventory> OnInventoryChanged;

    private void Awake()
    {
        InitializeInventory();
    }

    private void InitializeInventory()
    {
        // Ensure we have exactly maxSlots
        slots.Clear();
        for (int i = 0; i < maxSlots; i++)
        {
            InventorySlot slot = new InventorySlot();
            slot.OnSlotChanged += HandleSlotChanged;
            slots.Add(slot);
        }
    }

    private void HandleSlotChanged(InventorySlot slot)
    {
        OnInventoryChanged?.Invoke(this);
    }

    // Add item to inventory (returns true if all items were added)
    public bool AddItem(Item item, int quantity = 1)
    {
        if (item == null || quantity <= 0)
            return false;

        int remainingQuantity = quantity;

        // First, try to add to existing stacks
        if (item.IsStackable)
        {
            foreach (var slot in slots)
            {
                if (slot.Item == item && !slot.IsFull)
                {
                    remainingQuantity = slot.AddItem(item, remainingQuantity);
                    if (remainingQuantity <= 0)
                    {
                        OnItemAdded?.Invoke(item, quantity);
                        OnInventoryChanged?.Invoke(this);
                        return true;
                    }
                }
            }
        }

        // Then, try to add to empty slots
        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
            {
                remainingQuantity = slot.AddItem(item, remainingQuantity);
                if (remainingQuantity <= 0)
                {
                    OnItemAdded?.Invoke(item, quantity);
                    OnInventoryChanged?.Invoke(this);
                    return true;
                }
            }
        }

        // If we still have items remaining, inventory is full
        if (remainingQuantity < quantity)
        {
            OnItemAdded?.Invoke(item, quantity - remainingQuantity);
            OnInventoryChanged?.Invoke(this);
        }

        return remainingQuantity == 0;
    }

    // Remove item from inventory (returns actual amount removed)
    public int RemoveItem(Item item, int quantity = 1)
    {
        if (item == null || quantity <= 0)
            return 0;

        int remainingToRemove = quantity;

        // Remove from slots that contain this item
        for (int i = slots.Count - 1; i >= 0; i--)
        {
            if (slots[i].Item == item)
            {
                int removed = slots[i].RemoveItem(remainingToRemove);
                remainingToRemove -= removed;

                if (remainingToRemove <= 0)
                    break;
            }
        }

        int totalRemoved = quantity - remainingToRemove;
        if (totalRemoved > 0)
        {
            OnItemRemoved?.Invoke(item, totalRemoved);
            OnInventoryChanged?.Invoke(this);
        }

        return totalRemoved;
    }

    // Remove item from specific slot
    public int RemoveItemFromSlot(int slotIndex, int quantity = 1)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count)
            return 0;

        var slot = slots[slotIndex];
        if (slot.IsEmpty)
            return 0;

        Item item = slot.Item;
        int removed = slot.RemoveItem(quantity);

        if (removed > 0)
        {
            OnItemRemoved?.Invoke(item, removed);
            OnInventoryChanged?.Invoke(this);
        }

        return removed;
    }

    // Check if inventory contains item
    public bool HasItem(Item item, int quantity = 1)
    {
        if (item == null)
            return false;

        return GetItemCount(item) >= quantity;
    }

    // Get total count of specific item
    public int GetItemCount(Item item)
    {
        if (item == null)
            return 0;

        int count = 0;
        foreach (var slot in slots)
        {
            if (slot.Item == item)
            {
                count += slot.Quantity;
            }
        }
        return count;
    }

    // Find first slot containing item
    public int FindItemSlot(Item item)
    {
        if (item == null)
            return -1;

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].Item == item)
                return i;
        }
        return -1;
    }

    // Find all slots containing item
    public List<int> FindAllItemSlots(Item item)
    {
        List<int> slotIndices = new List<int>();

        if (item == null)
            return slotIndices;

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].Item == item)
            {
                slotIndices.Add(i);
            }
        }

        return slotIndices;
    }

    // Get all unique items in inventory
    public List<Item> GetAllItems()
    {
        return slots
            .Where(slot => !slot.IsEmpty)
            .Select(slot => slot.Item)
            .Distinct()
            .ToList();
    }

    // Check if inventory has space for item
    public bool HasSpaceFor(Item item, int quantity = 1)
    {
        if (item == null || quantity <= 0)
            return false;

        int remainingQuantity = quantity;

        // Check existing stacks
        if (item.IsStackable)
        {
            foreach (var slot in slots)
            {
                if (slot.Item == item)
                {
                    int availableSpace = slot.GetAvailableSpace(item);
                    remainingQuantity -= availableSpace;
                    if (remainingQuantity <= 0)
                        return true;
                }
            }
        }

        // Check empty slots
        int emptySlots = slots.Count(slot => slot.IsEmpty);
        if (item.IsStackable)
        {
            int slotsNeeded = Mathf.CeilToInt((float)remainingQuantity / item.MaxStackSize);
            return emptySlots >= slotsNeeded;
        }
        else
        {
            return emptySlots >= remainingQuantity;
        }
    }

    // Get inventory slot by index
    public InventorySlot GetSlot(int index)
    {
        if (index < 0 || index >= slots.Count)
            return null;

        return slots[index];
    }

    // Swap two slots
    public void SwapSlots(int slotIndex1, int slotIndex2)
    {
        if (slotIndex1 < 0 || slotIndex1 >= slots.Count ||
            slotIndex2 < 0 || slotIndex2 >= slots.Count)
            return;

        var slot1 = slots[slotIndex1];
        var slot2 = slots[slotIndex2];

        // Store slot1 data
        Item tempItem = slot1.Item;
        int tempQuantity = slot1.Quantity;

        // Set slot1 to slot2 data
        slot1.SetSlot(slot2.Item, slot2.Quantity);

        // Set slot2 to stored slot1 data
        slot2.SetSlot(tempItem, tempQuantity);

        OnInventoryChanged?.Invoke(this);
    }

    // Clear entire inventory
    public void ClearInventory()
    {
        foreach (var slot in slots)
        {
            slot.Clear();
        }
        OnInventoryChanged?.Invoke(this);
    }

    // Get total weight of all items
    public float GetTotalWeight()
    {
        float totalWeight = 0f;
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty)
            {
                totalWeight += slot.Item.Weight * slot.Quantity;
            }
        }
        return totalWeight;
    }

    // Get total value of all items
    public int GetTotalValue()
    {
        int totalValue = 0;
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty)
            {
                totalValue += slot.Item.Cost * slot.Quantity;
            }
        }
        return totalValue;
    }

    // Sort inventory by item name
    public void SortByName()
    {
        var sortedItems = slots
            .Where(slot => !slot.IsEmpty)
            .OrderBy(slot => slot.Item.ItemName)
            .ToList();

        ClearInventory();

        foreach (var slot in sortedItems)
        {
            AddItem(slot.Item, slot.Quantity);
        }
    }

    // Sort inventory by rarity
    public void SortByRarity()
    {
        var sortedItems = slots
            .Where(slot => !slot.IsEmpty)
            .OrderByDescending(slot => slot.Item.Rarity)
            .ToList();

        ClearInventory();

        foreach (var slot in sortedItems)
        {
            AddItem(slot.Item, slot.Quantity);
        }
    }

    // Sort inventory by value
    public void SortByValue()
    {
        var sortedItems = slots
            .Where(slot => !slot.IsEmpty)
            .OrderByDescending(slot => slot.Item.Cost)
            .ToList();

        ClearInventory();

        foreach (var slot in sortedItems)
        {
            AddItem(slot.Item, slot.Quantity);
        }
    }

    // Debug method to print inventory contents
    public void PrintInventory()
    {
        Debug.Log("=== INVENTORY ===");
        Debug.Log($"Used Slots: {UsedSlots}/{maxSlots}");
        Debug.Log($"Total Weight: {GetTotalWeight()} lbs");
        Debug.Log($"Total Value: {GetTotalValue()} gp");
        Debug.Log("\nContents:");

        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i].IsEmpty)
            {
                Debug.Log($"Slot {i}: {slots[i]}");
            }
        }
    }
}
