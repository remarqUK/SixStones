using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Utility class with helper methods for working with the cutscene system
/// </summary>
public static class CutsceneHelper
{
    /// <summary>
    /// Checks if a zone has an assigned cutscene
    /// </summary>
    public static bool ZoneHasCutscene(ZoneConfiguration.Zone zone)
    {
        return zone != null && !string.IsNullOrEmpty(zone.cutsceneId);
    }
    
    /// <summary>
    /// Checks if a subzone has an assigned cutscene
    /// </summary>
    public static bool SubZoneHasCutscene(ZoneConfiguration.SubZone subZone)
    {
        return subZone != null && !string.IsNullOrEmpty(subZone.cutsceneId);
    }
    
    /// <summary>
    /// Checks if a map has an assigned cutscene
    /// </summary>
    public static bool MapHasCutscene(ZoneConfiguration.ZoneMap map)
    {
        return map != null && !string.IsNullOrEmpty(map.cutsceneId);
    }
    
    /// <summary>
    /// Gets all cutscene IDs assigned to zones in the configuration
    /// </summary>
    public static List<string> GetAllZoneCutsceneIds(ZoneConfiguration config)
    {
        List<string> cutsceneIds = new List<string>();
        
        if (config == null) return cutsceneIds;
        
        foreach (var zone in config.zones)
        {
            if (ZoneHasCutscene(zone))
            {
                cutsceneIds.Add(zone.cutsceneId);
            }
        }
        
        return cutsceneIds;
    }
    
    /// <summary>
    /// Gets all cutscene IDs assigned to subzones in the configuration
    /// </summary>
    public static List<string> GetAllSubZoneCutsceneIds(ZoneConfiguration config)
    {
        List<string> cutsceneIds = new List<string>();
        
        if (config == null) return cutsceneIds;
        
        foreach (var zone in config.zones)
        {
            foreach (var subZone in zone.subZones)
            {
                if (SubZoneHasCutscene(subZone))
                {
                    cutsceneIds.Add(subZone.cutsceneId);
                }
            }
        }
        
        return cutsceneIds;
    }
    
    /// <summary>
    /// Gets all cutscene IDs assigned to maps in the configuration
    /// </summary>
    public static List<string> GetAllMapCutsceneIds(ZoneConfiguration config)
    {
        List<string> cutsceneIds = new List<string>();
        
        if (config == null) return cutsceneIds;
        
        foreach (var zone in config.zones)
        {
            foreach (var subZone in zone.subZones)
            {
                foreach (var map in subZone.maps)
                {
                    if (MapHasCutscene(map))
                    {
                        cutsceneIds.Add(map.cutsceneId);
                    }
                }
            }
        }
        
        return cutsceneIds;
    }
    
    /// <summary>
    /// Gets all unique cutscene IDs used in the entire zone configuration
    /// </summary>
    public static List<string> GetAllUniqueCutsceneIds(ZoneConfiguration config)
    {
        HashSet<string> uniqueIds = new HashSet<string>();
        
        uniqueIds.UnionWith(GetAllZoneCutsceneIds(config));
        uniqueIds.UnionWith(GetAllSubZoneCutsceneIds(config));
        uniqueIds.UnionWith(GetAllMapCutsceneIds(config));
        
        return new List<string>(uniqueIds);
    }
    
    /// <summary>
    /// Validates that all cutscene IDs in the zone configuration exist in the cutscene library
    /// </summary>
    public static bool ValidateCutsceneReferences(ZoneConfiguration config, CutsceneLibrary library)
    {
        if (config == null || library == null)
        {
            Debug.LogError("Cannot validate: ZoneConfiguration or CutsceneLibrary is null");
            return false;
        }
        
        List<string> allIds = GetAllUniqueCutsceneIds(config);
        bool allValid = true;
        
        foreach (string id in allIds)
        {
            if (!library.HasCutscene(id))
            {
                Debug.LogError($"Cutscene ID '{id}' is referenced in ZoneConfiguration but not found in CutsceneLibrary!");
                allValid = false;
            }
        }
        
        if (allValid)
        {
            Debug.Log($"All {allIds.Count} cutscene references are valid!");
        }
        
        return allValid;
    }
    
    /// <summary>
    /// Gets a report of cutscene usage across the zone configuration
    /// </summary>
    public static string GetCutsceneUsageReport(ZoneConfiguration config)
    {
        if (config == null)
            return "ZoneConfiguration is null";
        
        int zoneCount = GetAllZoneCutsceneIds(config).Count;
        int subZoneCount = GetAllSubZoneCutsceneIds(config).Count;
        int mapCount = GetAllMapCutsceneIds(config).Count;
        int uniqueCount = GetAllUniqueCutsceneIds(config).Count;
        
        return $"Cutscene Usage Report:\n" +
               $"- Zone cutscenes: {zoneCount}\n" +
               $"- SubZone cutscenes: {subZoneCount}\n" +
               $"- Map cutscenes: {mapCount}\n" +
               $"- Total cutscenes referenced: {zoneCount + subZoneCount + mapCount}\n" +
               $"- Unique cutscene IDs: {uniqueCount}";
    }
    
    /// <summary>
    /// Gets a report of viewed cutscenes
    /// </summary>
    public static string GetViewedCutscenesReport(CutsceneLibrary library)
    {
        if (library == null)
            return "CutsceneLibrary is null";
        
        int totalCutscenes = library.cutscenes.Count;
        int viewedCount = 0;
        List<string> viewedCutscenes = new List<string>();
        
        foreach (var cutscene in library.cutscenes)
        {
            if (CutsceneTracker.Instance.HasViewed(cutscene.cutsceneId))
            {
                viewedCount++;
                viewedCutscenes.Add(cutscene.cutsceneName);
            }
        }
        
        string report = $"Viewed Cutscenes Report:\n" +
                       $"- Total cutscenes in library: {totalCutscenes}\n" +
                       $"- Viewed: {viewedCount}\n" +
                       $"- Unviewed: {totalCutscenes - viewedCount}\n";
        
        if (viewedCount > 0)
        {
            report += $"\nViewed cutscenes:\n";
            foreach (string name in viewedCutscenes)
            {
                report += $"  - {name}\n";
            }
        }
        
        return report;
    }
    
    /// <summary>
    /// Creates a quick cutscene test instance (useful for debugging)
    /// </summary>
    public static Cutscene CreateTestCutscene(string id, string name, CutsceneType type = CutsceneType.Dialogue)
    {
        return new Cutscene
        {
            cutsceneId = id,
            cutsceneName = name,
            resourceLink = $"Test/{id}",
            cutsceneType = type,
            playOnce = true,
            duration = 0f
        };
    }
}
