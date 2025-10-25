using System;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    [SerializeField] private Item item;
    [SerializeField] private int quantity;

    public Item Item => item;
    public int Quantity => quantity;
    public bool IsEmpty => item == null || quantity <= 0;
    public bool IsFull => item != null && quantity >= item.MaxStackSize;

    // Events
    public event Action<InventorySlot> OnSlotChanged;

    public InventorySlot()
    {
        item = null;
        quantity = 0;
    }

    public InventorySlot(Item item, int quantity)
    {
        this.item = item;
        this.quantity = Mathf.Max(0, quantity);
    }

    // Add items to this slot (returns overflow quantity)
    public int AddItem(Item itemToAdd, int quantityToAdd)
    {
        if (itemToAdd == null || quantityToAdd <= 0)
            return quantityToAdd;

        // If slot is empty, add the item
        if (IsEmpty)
        {
            item = itemToAdd;
            int maxToAdd = itemToAdd.IsStackable ? Mathf.Min(quantityToAdd, itemToAdd.MaxStackSize) : 1;
            quantity = maxToAdd;
            OnSlotChanged?.Invoke(this);
            return quantityToAdd - maxToAdd;
        }

        // If different item, can't add to this slot
        if (item != itemToAdd)
            return quantityToAdd;

        // If not stackable, can't add more
        if (!item.IsStackable)
            return quantityToAdd;

        // Add as many as possible to stack
        int spaceAvailable = item.MaxStackSize - quantity;
        int amountToAdd = Mathf.Min(spaceAvailable, quantityToAdd);
        quantity += amountToAdd;

        OnSlotChanged?.Invoke(this);
        return quantityToAdd - amountToAdd;
    }

    // Remove items from this slot (returns actual amount removed)
    public int RemoveItem(int quantityToRemove)
    {
        if (IsEmpty || quantityToRemove <= 0)
            return 0;

        int amountRemoved = Mathf.Min(quantity, quantityToRemove);
        quantity -= amountRemoved;

        if (quantity <= 0)
        {
            Clear();
        }
        else
        {
            OnSlotChanged?.Invoke(this);
        }

        return amountRemoved;
    }

    // Set the slot contents directly
    public void SetSlot(Item newItem, int newQuantity)
    {
        item = newItem;
        quantity = Mathf.Max(0, newQuantity);

        if (item != null && quantity > item.MaxStackSize)
        {
            quantity = item.MaxStackSize;
        }

        OnSlotChanged?.Invoke(this);
    }

    // Clear the slot
    public void Clear()
    {
        item = null;
        quantity = 0;
        OnSlotChanged?.Invoke(this);
    }

    // Check if this slot can accept the item
    public bool CanAcceptItem(Item itemToAdd)
    {
        if (itemToAdd == null)
            return false;

        if (IsEmpty)
            return true;

        if (item != itemToAdd)
            return false;

        if (!item.IsStackable)
            return false;

        return quantity < item.MaxStackSize;
    }

    // Get the amount of space available for this item
    public int GetAvailableSpace(Item itemToCheck)
    {
        if (itemToCheck == null)
            return 0;

        if (IsEmpty)
            return itemToCheck.IsStackable ? itemToCheck.MaxStackSize : 1;

        if (item != itemToCheck)
            return 0;

        if (!item.IsStackable)
            return 0;

        return item.MaxStackSize - quantity;
    }

    public override string ToString()
    {
        if (IsEmpty)
            return "Empty Slot";

        return $"{item.ItemName} x{quantity}";
    }
}
