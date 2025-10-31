using UnityEngine;
using UnityEditor;

public static class AssignGemSprites
{
    [MenuItem("Tools/Assign Gem Sprites")]
    public static void AssignSprites()
    {
        Debug.Log("=== Assigning Gem Sprites ===");

        // Map of gem asset names to sprite GUIDs
        var spriteAssignments = new System.Collections.Generic.Dictionary<string, string>
        {
            { "RedGem", "4279063375ca549449ac820bee4f4e10" },      // GreenGemAI.png (Red gem sprite)
            { "BlueGem", "41cdee8228d8bea4a8be32ac7a9bff70" },     // BlueGemAI.png
            { "GreenGem", "4862a0d574b00fd4c894b03bd7574f94" },    // YelloGemAI.png (Green gem sprite)
            { "YellowGem", "bede8ee61bd88ff4f903b226462311d0" },   // RedGemAI.png (Yellow gem sprite - actual file)
            { "PurpleGem", "aa4468d0dbc35db4490a36b5a5de8155" },   // PurpleGemAI.png
            { "OrangeGem", "cb60afa967462ff4393c9a387a9ad842" },   // OrangeGemAI.png
            { "WhiteGem", "f49099d14103fe34983d96a06f57ea0e" }     // WhiteGemAI.png
        };

        int assignedCount = 0;

        foreach (var kvp in spriteAssignments)
        {
            string gemAssetName = kvp.Key;
            string spriteGuid = kvp.Value;

            // Load the gem asset
            string gemPath = $"Assets/GemTypes/{gemAssetName}.asset";
            GemTypeData gemData = AssetDatabase.LoadAssetAtPath<GemTypeData>(gemPath);

            if (gemData == null)
            {
                Debug.LogWarning($"Could not find gem asset: {gemPath}");
                continue;
            }

            // Load the sprite using GUID
            string spritePath = AssetDatabase.GUIDToAssetPath(spriteGuid);
            if (string.IsNullOrEmpty(spritePath))
            {
                Debug.LogWarning($"Could not find sprite with GUID: {spriteGuid}");
                continue;
            }

            // Load the sprite (handle sub-assets)
            Sprite sprite = null;
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(spritePath);
            foreach (var asset in allAssets)
            {
                if (asset is Sprite s)
                {
                    sprite = s;
                    break;
                }
            }

            if (sprite == null)
            {
                Debug.LogWarning($"No sprite found at path: {spritePath}");
                continue;
            }

            // Assign the sprite
            gemData.gemSprite = sprite;
            EditorUtility.SetDirty(gemData);

            Debug.Log($"Assigned sprite '{sprite.name}' to {gemAssetName}");
            assignedCount++;
        }

        // Save all changes
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"=== Finished: Assigned {assignedCount} sprites ===");
    }
}
