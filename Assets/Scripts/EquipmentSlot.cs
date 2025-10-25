using UnityEngine;

public enum EquipmentSlot
{
    None,           // Not wearable
    Head,           // Helmets, hats, crowns
    Chest,          // Armor, robes, shirts
    Legs,           // Pants, greaves
    Feet,           // Boots, shoes
    Hands,          // Gloves, gauntlets
    MainHand,       // Weapons, shields (right hand)
    OffHand,        // Shields, weapons (left hand)
    TwoHand,        // Two-handed weapons
    Neck,           // Amulets, necklaces
    Ring,           // Rings
    Back,           // Cloaks, capes
    Waist,          // Belts
    Trinket         // Magical items, artifacts
}

public static class EquipmentSlotExtensions
{
    public static bool IsWearable(this EquipmentSlot slot)
    {
        return slot != EquipmentSlot.None;
    }

    public static string GetDisplayName(this EquipmentSlot slot)
    {
        return slot switch
        {
            EquipmentSlot.MainHand => "Main Hand",
            EquipmentSlot.OffHand => "Off Hand",
            EquipmentSlot.TwoHand => "Two-Handed",
            _ => slot.ToString()
        };
    }
}
