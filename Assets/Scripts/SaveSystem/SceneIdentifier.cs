using UnityEngine;

/// <summary>
/// Enum for identifying game scenes
/// Use this instead of scene names to allow for renaming
/// </summary>
public enum SceneIdentifier
{
    MainMenu = 0,
    LoadingScreen = 1,
    WorldMap = 2,
    SubZoneMap = 3,
    Match3 = 4,
    Maze3D = 5,
    CharacterSelection = 6,
    Unknown = 99
}

/// <summary>
/// Helper class to convert between scene names and identifiers
/// </summary>
public static class SceneHelper
{
    /// <summary>
    /// Get the scene name from identifier
    /// </summary>
    public static string GetSceneName(SceneIdentifier identifier)
    {
        return identifier switch
        {
            SceneIdentifier.MainMenu => "MainMenu",
            SceneIdentifier.LoadingScreen => "LoadingScreen",
            SceneIdentifier.WorldMap => "WorldMap",
            SceneIdentifier.SubZoneMap => "SubZoneMap",
            SceneIdentifier.Match3 => "Match3",
            SceneIdentifier.Maze3D => "Maze3D",
            SceneIdentifier.CharacterSelection => "CharacterSelection",
            _ => "MainMenu" // Default to main menu if unknown
        };
    }

    /// <summary>
    /// Get the identifier from scene name
    /// </summary>
    public static SceneIdentifier GetIdentifier(string sceneName)
    {
        return sceneName switch
        {
            "MainMenu" => SceneIdentifier.MainMenu,
            "LoadingScreen" => SceneIdentifier.LoadingScreen,
            "WorldMap" => SceneIdentifier.WorldMap,
            "SubZoneMap" => SceneIdentifier.SubZoneMap,
            "Match3" => SceneIdentifier.Match3,
            "Maze3D" => SceneIdentifier.Maze3D,
            "CharacterSelection" => SceneIdentifier.CharacterSelection,
            _ => SceneIdentifier.Unknown
        };
    }

    /// <summary>
    /// Get the current scene identifier
    /// </summary>
    public static SceneIdentifier GetCurrentScene()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        return GetIdentifier(sceneName);
    }

    /// <summary>
    /// Load a scene by identifier
    /// </summary>
    public static void LoadScene(SceneIdentifier identifier)
    {
        string sceneName = GetSceneName(identifier);
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Check if a scene identifier is a gameplay scene (not menu/loading)
    /// </summary>
    public static bool IsGameplayScene(SceneIdentifier identifier)
    {
        return identifier switch
        {
            SceneIdentifier.WorldMap => true,
            SceneIdentifier.SubZoneMap => true,
            SceneIdentifier.Match3 => true,
            SceneIdentifier.Maze3D => true,
            _ => false
        };
    }
}
