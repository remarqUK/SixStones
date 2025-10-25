using UnityEngine;

public class InventoryTester : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private PlayerEquipment equipment;

    [Header("Test Items (Assign from Resources/Items)")]
    [SerializeField] private Item longsword;
    [SerializeField] private Item leatherArmor;
    [SerializeField] private Item healthPotion;
    [SerializeField] private Item ringOfProtection;
    [SerializeField] private Item flametongue;

    private void Awake()
    {
        if (inventory == null)
            inventory = GetComponent<Inventory>();

        if (equipment == null)
            equipment = GetComponent<PlayerEquipment>();

        // Load items from Resources if not assigned
        LoadItemsFromResources();
    }

    private void LoadItemsFromResources()
    {
        if (longsword == null)
            longsword = Resources.Load<Item>("Items/Longsword");

        if (leatherArmor == null)
            leatherArmor = Resources.Load<Item>("Items/LeatherArmor");

        if (healthPotion == null)
            healthPotion = Resources.Load<Item>("Items/HealthPotion");

        if (ringOfProtection == null)
            ringOfProtection = Resources.Load<Item>("Items/RingOfProtection");

        if (flametongue == null)
            flametongue = Resources.Load<Item>("Items/Flametongue");
    }

    private void Start()
    {
        // Subscribe to inventory events
        if (inventory != null)
        {
            inventory.OnItemAdded += HandleItemAdded;
            inventory.OnItemRemoved += HandleItemRemoved;
        }

        // Subscribe to equipment events
        if (equipment != null)
        {
            equipment.OnItemEquipped += HandleItemEquipped;
            equipment.OnItemUnequipped += HandleItemUnequipped;
        }

        Debug.Log("Inventory Tester initialized. Press keys to test:");
        Debug.Log("1 - Add items to inventory");
        Debug.Log("2 - Print inventory");
        Debug.Log("3 - Equip items");
        Debug.Log("4 - Print equipment");
        Debug.Log("5 - Unequip all");
        Debug.Log("6 - Sort inventory by rarity");
        Debug.Log("7 - Remove all potions");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TestAddItems();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TestPrintInventory();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TestEquipItems();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            TestPrintEquipment();
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            TestUnequipAll();
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            TestSortInventory();
        }

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            TestRemovePotions();
        }
    }

    private void TestAddItems()
    {
        Debug.Log("\n=== TESTING: Add Items ===");

        if (inventory == null)
        {
            Debug.LogError("Inventory not found!");
            return;
        }

        // Add weapons
        if (longsword != null)
        {
            inventory.AddItem(longsword, 1);
            inventory.AddItem(longsword, 1); // Add second one
        }

        if (flametongue != null)
        {
            inventory.AddItem(flametongue, 1);
        }

        // Add armor
        if (leatherArmor != null)
        {
            inventory.AddItem(leatherArmor, 1);
        }

        // Add accessories
        if (ringOfProtection != null)
        {
            inventory.AddItem(ringOfProtection, 1);
        }

        // Add stackable potions
        if (healthPotion != null)
        {
            inventory.AddItem(healthPotion, 5);
            inventory.AddItem(healthPotion, 3); // Should stack
        }

        Debug.Log("Items added to inventory");
    }

    private void TestPrintInventory()
    {
        Debug.Log("\n=== TESTING: Print Inventory ===");

        if (inventory == null)
        {
            Debug.LogError("Inventory not found!");
            return;
        }

        inventory.PrintInventory();
    }

    private void TestEquipItems()
    {
        Debug.Log("\n=== TESTING: Equip Items ===");

        if (equipment == null)
        {
            Debug.LogError("Equipment not found!");
            return;
        }

        if (inventory == null || inventory.UsedSlots == 0)
        {
            Debug.LogWarning("Inventory is empty. Add items first (press 1)");
            return;
        }

        // Equip items
        if (inventory.HasItem(flametongue))
        {
            equipment.EquipItem(flametongue);
        }
        else if (inventory.HasItem(longsword))
        {
            equipment.EquipItem(longsword);
        }

        if (inventory.HasItem(leatherArmor))
        {
            equipment.EquipItem(leatherArmor);
        }

        if (inventory.HasItem(ringOfProtection))
        {
            equipment.EquipItem(ringOfProtection);
        }

        Debug.Log("Items equipped");
    }

    private void TestPrintEquipment()
    {
        Debug.Log("\n=== TESTING: Print Equipment ===");

        if (equipment == null)
        {
            Debug.LogError("Equipment not found!");
            return;
        }

        equipment.PrintEquipment();
    }

    private void TestUnequipAll()
    {
        Debug.Log("\n=== TESTING: Unequip All ===");

        if (equipment == null)
        {
            Debug.LogError("Equipment not found!");
            return;
        }

        equipment.UnequipAll();
        Debug.Log("All items unequipped");
    }

    private void TestSortInventory()
    {
        Debug.Log("\n=== TESTING: Sort Inventory by Rarity ===");

        if (inventory == null)
        {
            Debug.LogError("Inventory not found!");
            return;
        }

        inventory.SortByRarity();
        inventory.PrintInventory();
    }

    private void TestRemovePotions()
    {
        Debug.Log("\n=== TESTING: Remove Potions ===");

        if (inventory == null)
        {
            Debug.LogError("Inventory not found!");
            return;
        }

        if (healthPotion != null)
        {
            int removed = inventory.RemoveItem(healthPotion, 999);
            Debug.Log($"Removed {removed} health potions");
        }

        inventory.PrintInventory();
    }

    // Event handlers
    private void HandleItemAdded(Item item, int quantity)
    {
        Debug.Log($"<color=green>[INVENTORY] Added: {item.ItemName} x{quantity}</color>");
    }

    private void HandleItemRemoved(Item item, int quantity)
    {
        Debug.Log($"<color=red>[INVENTORY] Removed: {item.ItemName} x{quantity}</color>");
    }

    private void HandleItemEquipped(EquipmentSlot slot, Item item)
    {
        Debug.Log($"<color=yellow>[EQUIPMENT] Equipped {item.ItemName} to {slot.GetDisplayName()}</color>");
    }

    private void HandleItemUnequipped(EquipmentSlot slot, Item item)
    {
        Debug.Log($"<color=orange>[EQUIPMENT] Unequipped {item.ItemName} from {slot.GetDisplayName()}</color>");
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (inventory != null)
        {
            inventory.OnItemAdded -= HandleItemAdded;
            inventory.OnItemRemoved -= HandleItemRemoved;
        }

        if (equipment != null)
        {
            equipment.OnItemEquipped -= HandleItemEquipped;
            equipment.OnItemUnequipped -= HandleItemUnequipped;
        }
    }
}
