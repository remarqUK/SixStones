using UnityEngine;
using UnityEditor;

public class InventorySetup : EditorWindow
{
    [MenuItem("Tools/Setup Player Inventory System")]
    public static void SetupPlayerInventorySystem()
    {
        // Find or create Player GameObject
        GameObject player = GameObject.Find("Player");

        if (player == null)
        {
            // Try to find GameManager instead
            GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                player = gameManager.gameObject;
                Debug.Log("Adding inventory system to GameManager");
            }
            else
            {
                player = new GameObject("Player");
                Debug.Log("Created new Player GameObject");
            }
        }

        // Add Inventory component if not present
        Inventory inventory = player.GetComponent<Inventory>();
        if (inventory == null)
        {
            inventory = player.AddComponent<Inventory>();
            Debug.Log("Added Inventory component");
        }
        else
        {
            Debug.Log("Inventory component already exists");
        }

        // Add PlayerEquipment component if not present
        PlayerEquipment equipment = player.GetComponent<PlayerEquipment>();
        if (equipment == null)
        {
            equipment = player.AddComponent<PlayerEquipment>();
            Debug.Log("Added PlayerEquipment component");
        }
        else
        {
            Debug.Log("PlayerEquipment component already exists");
        }

        // Add InventoryTester component if not present
        InventoryTester tester = player.GetComponent<InventoryTester>();
        if (tester == null)
        {
            tester = player.AddComponent<InventoryTester>();
            Debug.Log("Added InventoryTester component");
        }
        else
        {
            Debug.Log("InventoryTester component already exists");
        }

        // Add RandomItemTester component if not present
        RandomItemTester randomTester = player.GetComponent<RandomItemTester>();
        if (randomTester == null)
        {
            randomTester = player.AddComponent<RandomItemTester>();
            Debug.Log("Added RandomItemTester component");
        }
        else
        {
            Debug.Log("RandomItemTester component already exists");
        }

        // Mark scene as dirty
        EditorUtility.SetDirty(player);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );

        Debug.Log("<color=green>Player Inventory System setup complete!</color>");
        Debug.Log("\nNext steps:");
        Debug.Log("1. Run Tools -> Generate All Items from Library");
        Debug.Log("2. Enter Play mode");
        Debug.Log("3. Press keys to test inventory features");
        Debug.Log("\nHardcoded Item Testing (1-7):");
        Debug.Log("1 - Add hardcoded items to inventory");
        Debug.Log("2 - Print inventory");
        Debug.Log("3 - Equip items");
        Debug.Log("4 - Print equipment");
        Debug.Log("5 - Unequip all");
        Debug.Log("6 - Sort inventory by rarity");
        Debug.Log("7 - Remove all potions");
        Debug.Log("\nRandom Item Generation:");
        Debug.Log("G - Generate random item at current level");
        Debug.Log("1-9, 0 - Set player level (1-10)");
        Debug.Log("+/- - Increase/decrease level");
        Debug.Log("L - Generate 10 random items");
        Debug.Log("C - Clear inventory");

        Selection.activeGameObject = player;
    }
}
