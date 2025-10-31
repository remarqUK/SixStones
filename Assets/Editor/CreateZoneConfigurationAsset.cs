using UnityEngine;
using UnityEditor;

public class CreateZoneConfigurationAsset : Editor
{
    [MenuItem("Tools/Create Zone Configuration")]
    public static void CreateZoneConfig()
    {
        // Create the ZoneConfiguration asset
        ZoneConfiguration config = ScriptableObject.CreateInstance<ZoneConfiguration>();
        
        // Populate with 6 zones, each with 6 subzones, each with 6 maps
        for (int z = 0; z < 6; z++)
        {
            ZoneConfiguration.Zone zone = new ZoneConfiguration.Zone
            {
                zoneName = GetZoneName(z),
                subZones = new System.Collections.Generic.List<ZoneConfiguration.SubZone>()
            };
            
            for (int s = 0; s < 6; s++)
            {
                ZoneConfiguration.SubZone subZone = new ZoneConfiguration.SubZone
                {
                    subZoneName = GetSubZoneName(z, s),
                    maps = new System.Collections.Generic.List<ZoneConfiguration.ZoneMap>()
                };
                
                for (int m = 0; m < 6; m++)
                {
                    ZoneConfiguration.ZoneMap map = new ZoneConfiguration.ZoneMap
                    {
                        mapName = GetMapName(z, s, m),
                        mapWidth = 10,
                        mapHeight = 10
                    };
                    
                    subZone.maps.Add(map);
                }
                
                zone.subZones.Add(subZone);
            }
            
            config.zones.Add(zone);
        }
        
        // Save the asset
        string path = "Assets/ZoneConfiguration.asset";
        AssetDatabase.CreateAsset(config, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"ZoneConfiguration created at {path}");
        
        // Now assign it to WorldMapManager
        AssignToWorldMapManager(config);
    }
    
    private static void AssignToWorldMapManager(ZoneConfiguration config)
    {
        WorldMapManager manager = GameObject.FindFirstObjectByType<WorldMapManager>();
        if (manager != null)
        {
            SerializedObject so = new SerializedObject(manager);
            SerializedProperty prop = so.FindProperty("zoneConfig");
            if (prop != null)
            {
                prop.objectReferenceValue = config;
                so.ApplyModifiedProperties();
                Debug.Log("ZoneConfiguration assigned to WorldMapManager!");
            }
        }
        else
        {
            Debug.LogWarning("WorldMapManager not found in scene. Please assign ZoneConfiguration manually.");
        }
    }
    
    private static string GetZoneName(int zoneIndex)
    {
        string[] zoneNames = { "Verdant Plains", "Mystic Forest", "Frozen Peaks", "Desert Wastes", "Shadow Realm", "Celestial Heights" };
        return zoneNames[zoneIndex];
    }
    
    private static string GetSubZoneName(int zoneIndex, int subZoneIndex)
    {
        return $"District {subZoneIndex + 1}";
    }
    
    private static string GetMapName(int zoneIndex, int subZoneIndex, int mapIndex)
    {
        return $"Area {mapIndex + 1}";
    }
}
