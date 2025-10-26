using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateZoneConfiguration : MonoBehaviour
{
    [MenuItem("Tools/Create Zone Configuration")]
    static void CreateZoneConfig()
    {
        // Create the ZoneConfiguration ScriptableObject
        ZoneConfiguration config = ScriptableObject.CreateInstance<ZoneConfiguration>();

        // Define the 6 main zones
        string[] zoneNames = { "Cellar", "Forest", "Temple", "Dungeon", "Castle", "Tower" };

        foreach (string zoneName in zoneNames)
        {
            ZoneConfiguration.Zone zone = new ZoneConfiguration.Zone();
            zone.zoneName = zoneName;

            // Create 6 subzones for each zone
            for (int subZoneIndex = 1; subZoneIndex <= 6; subZoneIndex++)
            {
                ZoneConfiguration.SubZone subZone = new ZoneConfiguration.SubZone();
                subZone.subZoneName = $"{zoneName} Floor {subZoneIndex}";

                // Create 6 maps for each subzone
                for (int mapIndex = 1; mapIndex <= 6; mapIndex++)
                {
                    ZoneConfiguration.ZoneMap map = new ZoneConfiguration.ZoneMap();
                    map.mapName = $"{zoneName} F{subZoneIndex} Level {mapIndex}";
                    map.mapWidth = 10;
                    map.mapHeight = 10;

                    subZone.maps.Add(map);
                }

                zone.subZones.Add(subZone);
            }

            config.zones.Add(zone);
        }

        // Ensure Resources directory exists
        string resourcesPath = "Assets/Resources";
        if (!Directory.Exists(resourcesPath))
        {
            Directory.CreateDirectory(resourcesPath);
        }

        // Save the ScriptableObject
        string assetPath = "Assets/Resources/ZoneConfiguration.asset";
        AssetDatabase.CreateAsset(config, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Zone Configuration created at {assetPath}");
        Debug.Log($"Total structure: 6 zones x 6 floors x 6 levels = 216 total maps");

        // Select the created asset
        Selection.activeObject = config;
        EditorGUIUtility.PingObject(config);
    }

    [MenuItem("Tools/Create Player Progress Manager")]
    static void CreatePlayerProgressManager()
    {
        // Check if PlayerProgress already exists in the scene
        PlayerProgress existing = FindFirstObjectByType<PlayerProgress>();
        if (existing != null)
        {
            Debug.LogWarning("PlayerProgress already exists in the scene!");
            Selection.activeGameObject = existing.gameObject;
            return;
        }

        // Create a new GameObject with PlayerProgress component
        GameObject progressObj = new GameObject("PlayerProgressManager");
        PlayerProgress progress = progressObj.AddComponent<PlayerProgress>();

        // Load the ZoneConfiguration asset
        ZoneConfiguration config = Resources.Load<ZoneConfiguration>("ZoneConfiguration");
        if (config != null)
        {
            progress.zoneConfig = config;
            Debug.Log("PlayerProgress manager created and linked to ZoneConfiguration");
        }
        else
        {
            Debug.LogWarning("PlayerProgress created, but ZoneConfiguration not found. Run 'Tools > Create Zone Configuration' first.");
        }

        // Select the created object
        Selection.activeGameObject = progressObj;
        EditorGUIUtility.PingObject(progressObj);
    }
}
