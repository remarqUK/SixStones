using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ZoneConfiguration", menuName = "Game/Zone Configuration")]
public class ZoneConfiguration : ScriptableObject
{
    public List<Zone> zones = new List<Zone>();

    [Serializable]
    public class Zone
    {
        public string zoneName;
        [Tooltip("Cutscene ID to play when entering this zone")]
        public string cutsceneId;
        public List<SubZone> subZones = new List<SubZone>();
    }

    [Serializable]
    public class SubZone
    {
        public string subZoneName;
        [Tooltip("Cutscene ID to play when entering this subzone")]
        public string cutsceneId;
        public List<ZoneMap> maps = new List<ZoneMap>();
    }

    [Serializable]
    public class ZoneMap
    {
        public string mapName;
        [Tooltip("Cutscene ID to play when entering this map")]
        public string cutsceneId;
        public int mapWidth = 10;
        public int mapHeight = 10;
    }

    // Helper methods to access zone data
    public Zone GetZone(int zoneIndex)
    {
        if (zoneIndex >= 0 && zoneIndex < zones.Count)
            return zones[zoneIndex];
        return null;
    }

    public SubZone GetSubZone(int zoneIndex, int subZoneIndex)
    {
        Zone zone = GetZone(zoneIndex);
        if (zone != null && subZoneIndex >= 0 && subZoneIndex < zone.subZones.Count)
            return zone.subZones[subZoneIndex];
        return null;
    }

    public ZoneMap GetMap(int zoneIndex, int subZoneIndex, int mapIndex)
    {
        SubZone subZone = GetSubZone(zoneIndex, subZoneIndex);
        if (subZone != null && mapIndex >= 0 && mapIndex < subZone.maps.Count)
            return subZone.maps[mapIndex];
        return null;
    }

    public int GetTotalZones() => zones.Count;
    public int GetTotalSubZones(int zoneIndex) => GetZone(zoneIndex)?.subZones.Count ?? 0;
    public int GetTotalMaps(int zoneIndex, int subZoneIndex) => GetSubZone(zoneIndex, subZoneIndex)?.maps.Count ?? 0;
}
