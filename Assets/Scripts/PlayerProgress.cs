using UnityEngine;

public class PlayerProgress : MonoBehaviour
{
    public static PlayerProgress Instance { get; private set; }

    [Header("Zone Configuration")]
    public ZoneConfiguration zoneConfig;

    [Header("Current Progress")]
    public int currentZone = 0;        // 0-5 for the 6 zones
    public int currentSubZone = 0;     // 0-5 for the 6 subzones per zone
    public int currentMap = 0;         // 0-5 for the 6 maps per subzone

    [Header("Position on Current Map")]
    public int positionX = 0;
    public int positionY = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProgress();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Get the current zone name
    public string GetCurrentZoneName()
    {
        if (zoneConfig == null) return "Unknown Zone";
        var zone = zoneConfig.GetZone(currentZone);
        return zone?.zoneName ?? "Unknown Zone";
    }

    // Get the current subzone name
    public string GetCurrentSubZoneName()
    {
        if (zoneConfig == null) return "Unknown SubZone";
        var subZone = zoneConfig.GetSubZone(currentZone, currentSubZone);
        return subZone?.subZoneName ?? "Unknown SubZone";
    }

    // Get the current map name
    public string GetCurrentMapName()
    {
        if (zoneConfig == null) return "Unknown Map";
        var map = zoneConfig.GetMap(currentZone, currentSubZone, currentMap);
        return map?.mapName ?? "Unknown Map";
    }

    // Advance to the next map
    public bool AdvanceToNextMap()
    {
        currentMap++;
        if (currentMap >= 6) // 6 maps per subzone
        {
            currentMap = 0;
            return AdvanceToNextSubZone();
        }
        SaveProgress();
        return true;
    }

    // Advance to the next subzone
    public bool AdvanceToNextSubZone()
    {
        currentSubZone++;
        if (currentSubZone >= 6) // 6 subzones per zone
        {
            currentSubZone = 0;
            return AdvanceToNextZone();
        }
        SaveProgress();
        return true;
    }

    // Advance to the next zone
    public bool AdvanceToNextZone()
    {
        currentZone++;
        if (currentZone >= 6) // 6 total zones
        {
            Debug.Log("Player has completed all zones!");
            return false; // No more zones
        }
        SaveProgress();
        return true;
    }

    // Set player position on current map
    public void SetPosition(int x, int y)
    {
        positionX = x;
        positionY = y;
        SaveProgress();
    }

    // Save progress to PlayerPrefs
    private void SaveProgress()
    {
        PlayerPrefs.SetInt("CurrentZone", currentZone);
        PlayerPrefs.SetInt("CurrentSubZone", currentSubZone);
        PlayerPrefs.SetInt("CurrentMap", currentMap);
        PlayerPrefs.SetInt("PositionX", positionX);
        PlayerPrefs.SetInt("PositionY", positionY);
        PlayerPrefs.Save();
    }

    // Load progress from PlayerPrefs
    private void LoadProgress()
    {
        currentZone = PlayerPrefs.GetInt("CurrentZone", 0);
        currentSubZone = PlayerPrefs.GetInt("CurrentSubZone", 0);
        currentMap = PlayerPrefs.GetInt("CurrentMap", 0);
        positionX = PlayerPrefs.GetInt("PositionX", 0);
        positionY = PlayerPrefs.GetInt("PositionY", 0);
    }

    // Reset progress to beginning
    public void ResetProgress()
    {
        currentZone = 0;
        currentSubZone = 0;
        currentMap = 0;
        positionX = 0;
        positionY = 0;
        SaveProgress();
    }

    // Get formatted progress string
    public string GetProgressString()
    {
        return $"{GetCurrentZoneName()} > {GetCurrentSubZoneName()} > {GetCurrentMapName()} ({positionX}, {positionY})";
    }
}
