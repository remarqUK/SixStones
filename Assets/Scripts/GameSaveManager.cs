using UnityEngine;

/// <summary>
/// Simple save/load system using PlayerPrefs
/// Tracks whether a save game exists
/// </summary>
public static class GameSaveManager
{
    private const string SAVE_KEY = "GameSave_Exists";
    private const string SAVE_LEVEL = "GameSave_Level";
    private const string SAVE_SCORE_P1 = "GameSave_Score_P1";
    private const string SAVE_SCORE_P2 = "GameSave_Score_P2";
    private const string SAVE_HEALTH_P1 = "GameSave_Health_P1";
    private const string SAVE_HEALTH_P2 = "GameSave_Health_P2";
    private const string SAVE_XP = "GameSave_XP";
    private const string SAVE_GOLD = "GameSave_Gold";

    /// <summary>
    /// Check if a save game exists
    /// </summary>
    public static bool HasSaveGame()
    {
        return PlayerPrefs.GetInt(SAVE_KEY, 0) == 1;
    }

    /// <summary>
    /// Mark that a save game exists
    /// </summary>
    public static void MarkSaveGameExists()
    {
        PlayerPrefs.SetInt(SAVE_KEY, 1);
        PlayerPrefs.Save();
        Debug.Log("Save game marked as existing");
    }

    /// <summary>
    /// Delete the save game
    /// </summary>
    public static void DeleteSaveGame()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.DeleteKey(SAVE_LEVEL);
        PlayerPrefs.DeleteKey(SAVE_SCORE_P1);
        PlayerPrefs.DeleteKey(SAVE_SCORE_P2);
        PlayerPrefs.DeleteKey(SAVE_HEALTH_P1);
        PlayerPrefs.DeleteKey(SAVE_HEALTH_P2);
        PlayerPrefs.DeleteKey(SAVE_XP);
        PlayerPrefs.DeleteKey(SAVE_GOLD);
        PlayerPrefs.Save();
        Debug.Log("Save game deleted");
    }

    /// <summary>
    /// Save game state
    /// </summary>
    public static void SaveGame(int level, int scoreP1, int scoreP2, int healthP1, int healthP2, int xp, int gold)
    {
        PlayerPrefs.SetInt(SAVE_KEY, 1);
        PlayerPrefs.SetInt(SAVE_LEVEL, level);
        PlayerPrefs.SetInt(SAVE_SCORE_P1, scoreP1);
        PlayerPrefs.SetInt(SAVE_SCORE_P2, scoreP2);
        PlayerPrefs.SetInt(SAVE_HEALTH_P1, healthP1);
        PlayerPrefs.SetInt(SAVE_HEALTH_P2, healthP2);
        PlayerPrefs.SetInt(SAVE_XP, xp);
        PlayerPrefs.SetInt(SAVE_GOLD, gold);
        PlayerPrefs.Save();
        Debug.Log($"Game saved - Level: {level}, Score P1: {scoreP1}, Score P2: {scoreP2}");
    }

    /// <summary>
    /// Load saved level
    /// </summary>
    public static int LoadLevel()
    {
        return PlayerPrefs.GetInt(SAVE_LEVEL, 1);
    }

    /// <summary>
    /// Load saved scores
    /// </summary>
    public static (int p1, int p2) LoadScores()
    {
        return (PlayerPrefs.GetInt(SAVE_SCORE_P1, 0), PlayerPrefs.GetInt(SAVE_SCORE_P2, 0));
    }

    /// <summary>
    /// Load saved health
    /// </summary>
    public static (int p1, int p2) LoadHealth()
    {
        return (PlayerPrefs.GetInt(SAVE_HEALTH_P1, 100), PlayerPrefs.GetInt(SAVE_HEALTH_P2, 100));
    }

    /// <summary>
    /// Load saved XP
    /// </summary>
    public static int LoadXP()
    {
        return PlayerPrefs.GetInt(SAVE_XP, 0);
    }

    /// <summary>
    /// Load saved gold
    /// </summary>
    public static int LoadGold()
    {
        return PlayerPrefs.GetInt(SAVE_GOLD, 0);
    }
}
