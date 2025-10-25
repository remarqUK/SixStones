using UnityEngine;

public class RandomItemTester : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int playerLevel = 1;
    [SerializeField] private Inventory inventory;

    private void Awake()
    {
        if (inventory == null)
        {
            inventory = GetComponent<Inventory>();
        }
    }

    private void Start()
    {
        Debug.Log("=== RANDOM ITEM GENERATOR TESTER ===");
        Debug.Log("Press G - Generate random item at current level");
        Debug.Log("Press 1-9 - Set player level to 1-9");
        Debug.Log("Press 0 - Set player level to 10");
        Debug.Log("Press + - Increase level by 1");
        Debug.Log("Press - - Decrease level by 1");
        Debug.Log("Press L - Generate 10 items at current level");
        Debug.Log("Press C - Clear inventory");
        Debug.Log($"\nCurrent Level: {playerLevel}");
    }

    private void Update()
    {
        // Generate single item
        if (Input.GetKeyDown(KeyCode.G))
        {
            GenerateAndAddRandomItem();
        }

        // Level shortcuts
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetLevel(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetLevel(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetLevel(3);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SetLevel(4);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SetLevel(5);
        if (Input.GetKeyDown(KeyCode.Alpha6)) SetLevel(6);
        if (Input.GetKeyDown(KeyCode.Alpha7)) SetLevel(7);
        if (Input.GetKeyDown(KeyCode.Alpha8)) SetLevel(8);
        if (Input.GetKeyDown(KeyCode.Alpha9)) SetLevel(9);
        if (Input.GetKeyDown(KeyCode.Alpha0)) SetLevel(10);

        // Level adjustment
        if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.Plus))
        {
            SetLevel(playerLevel + 1);
        }
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.Underscore))
        {
            SetLevel(playerLevel - 1);
        }

        // Generate multiple items
        if (Input.GetKeyDown(KeyCode.L))
        {
            GenerateMultipleItems(10);
        }

        // Clear inventory
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearInventory();
        }
    }

    private void GenerateAndAddRandomItem()
    {
        Item randomItem = RandomItemGenerator.Instance.GenerateRandomItem(playerLevel);

        if (randomItem != null)
        {
            Debug.Log($"\n<color=cyan>=== GENERATED ITEM (Level {playerLevel}) ===</color>");
            Debug.Log(randomItem.GetFullDescription());

            if (inventory != null)
            {
                bool success = inventory.AddItem(randomItem, 1);
                if (success)
                {
                    Debug.Log($"<color=green>Added to inventory!</color>");
                }
                else
                {
                    Debug.Log($"<color=red>Inventory full!</color>");
                }
            }
        }
        else
        {
            Debug.LogError("Failed to generate random item!");
        }
    }

    private void GenerateMultipleItems(int count)
    {
        Debug.Log($"\n<color=yellow>=== GENERATING {count} ITEMS AT LEVEL {playerLevel} ===</color>");

        int added = 0;
        for (int i = 0; i < count; i++)
        {
            Item randomItem = RandomItemGenerator.Instance.GenerateRandomItem(playerLevel);
            if (randomItem != null)
            {
                if (inventory != null)
                {
                    bool success = inventory.AddItem(randomItem, 1);
                    if (success)
                    {
                        added++;
                        Debug.Log($"{i + 1}. {randomItem.GetColoredName()} ({randomItem.EquipmentSlot})");
                    }
                }
            }
        }

        Debug.Log($"<color=green>Successfully added {added}/{count} items to inventory</color>");

        if (inventory != null)
        {
            inventory.PrintInventory();
        }
    }

    private void SetLevel(int level)
    {
        playerLevel = Mathf.Clamp(level, 1, 20);
        Debug.Log($"<color=yellow>Player level set to: {playerLevel}</color>");
    }

    private void ClearInventory()
    {
        if (inventory != null)
        {
            inventory.ClearInventory();
            Debug.Log("<color=red>Inventory cleared!</color>");
        }
    }

    // Public method to generate item (can be called from other scripts)
    public Item GenerateItemForLevel(int level)
    {
        return RandomItemGenerator.Instance.GenerateRandomItem(level);
    }

    // Generate item with specific rarity
    public Item GenerateItemWithRarity(int level, ItemRarity rarity)
    {
        Item item = null;
        int attempts = 0;
        int maxAttempts = 100;

        // Keep generating until we get the desired rarity
        while (attempts < maxAttempts)
        {
            item = RandomItemGenerator.Instance.GenerateRandomItem(level);
            if (item != null && item.Rarity == rarity)
            {
                break;
            }
            attempts++;
        }

        if (attempts >= maxAttempts)
        {
            Debug.LogWarning($"Could not generate {rarity} item at level {level} after {maxAttempts} attempts");
        }

        return item;
    }

    // Generate loot for defeating enemies
    public void GenerateLootDrop(int enemyLevel, int itemCount)
    {
        Debug.Log($"\n<color=orange>=== LOOT DROP (Enemy Level {enemyLevel}) ===</color>");

        for (int i = 0; i < itemCount; i++)
        {
            Item loot = RandomItemGenerator.Instance.GenerateRandomItem(enemyLevel);
            if (loot != null && inventory != null)
            {
                inventory.AddItem(loot, 1);
                Debug.Log($"Dropped: {loot.GetColoredName()}");
            }
        }
    }
}
