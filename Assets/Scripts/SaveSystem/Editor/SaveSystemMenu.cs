using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Unity Editor menu items for testing the save system
/// Access via Tools > Save System menu
/// </summary>
public class SaveSystemMenu
{
    private const string MENU_ROOT = "Tools/Save System/";

    [MenuItem(MENU_ROOT + "Save Game", false, 1)]
    public static void SaveGame()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Save Game can only be called during Play Mode");
            return;
        }

        bool success = EnhancedGameSaveManager.SaveGame();
        if (success)
        {
            EditorUtility.DisplayDialog("Save System", "Game saved successfully!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Save System", "Failed to save game. Check console for errors.", "OK");
        }
    }

    [MenuItem(MENU_ROOT + "Load Game", false, 2)]
    public static void LoadGame()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Load Game can only be called during Play Mode");
            return;
        }

        if (!EnhancedGameSaveManager.HasSaveGame())
        {
            EditorUtility.DisplayDialog("Save System", "No save file found!", "OK");
            return;
        }

        SaveData data = EnhancedGameSaveManager.LoadGame();
        if (data != null)
        {
            EnhancedGameSaveManager.RestoreSaveData(data);
            EditorUtility.DisplayDialog("Save System", "Game loaded successfully!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Save System", "Failed to load game. Check console for errors.", "OK");
        }
    }

    [MenuItem(MENU_ROOT + "Delete Save", false, 3)]
    public static void DeleteSave()
    {
        if (!EnhancedGameSaveManager.HasSaveGame())
        {
            EditorUtility.DisplayDialog("Save System", "No save file found!", "OK");
            return;
        }

        if (EditorUtility.DisplayDialog("Delete Save", "Are you sure you want to delete the save file?", "Yes", "No"))
        {
            EnhancedGameSaveManager.DeleteSaveGame();
            EditorUtility.DisplayDialog("Save System", "Save file deleted!", "OK");
        }
    }

    [MenuItem(MENU_ROOT + "Show Save Info", false, 4)]
    public static void ShowSaveInfo()
    {
        if (!EnhancedGameSaveManager.HasSaveGame())
        {
            EditorUtility.DisplayDialog("Save System", "No save file found!", "OK");
            return;
        }

        SaveFileInfo info = EnhancedGameSaveManager.GetSaveFileInfo();
        if (info != null)
        {
            string message = $"Save Date: {info.saveDate}\n";
            message += $"Player Level: {info.playerLevel}\n";
            message += $"Current Zone: {info.currentZone}\n";
            message += $"Current SubZone: {info.currentSubZone}\n";
            message += $"Playtime: {info.GetPlaytimeFormatted()}\n";
            message += $"File Size: {info.fileSizeKB} KB";

            EditorUtility.DisplayDialog("Save File Info", message, "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Save System", "Failed to read save file info.", "OK");
        }
    }

    [MenuItem(MENU_ROOT + "Export Save to Console", false, 5)]
    public static void ExportSaveToConsole()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Export Save can only be called during Play Mode");
            return;
        }

        string json = EnhancedGameSaveManager.ExportSaveToJson();
        Debug.Log("=== SAVE DATA JSON ===");
        Debug.Log(json);
        Debug.Log("=== END SAVE DATA ===");

        EditorUtility.DisplayDialog("Save System", "Save data exported to console!", "OK");
    }

    [MenuItem(MENU_ROOT + "Open Save Folder", false, 20)]
    public static void OpenSaveFolder()
    {
        string savePath = Path.Combine(Application.persistentDataPath, "Saves");

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        EditorUtility.RevealInFinder(savePath);
    }

    [MenuItem(MENU_ROOT + "Show Save Path", false, 21)]
    public static void ShowSavePath()
    {
        string savePath = Path.Combine(Application.persistentDataPath, "Saves", "gamesave.json");
        Debug.Log($"Save file path: {savePath}");

        EditorUtility.DisplayDialog("Save Path", savePath, "OK");
    }

    // Validate menu items based on play mode
    [MenuItem(MENU_ROOT + "Save Game", true)]
    public static bool ValidateSaveGame()
    {
        return Application.isPlaying;
    }

    [MenuItem(MENU_ROOT + "Load Game", true)]
    public static bool ValidateLoadGame()
    {
        return Application.isPlaying;
    }

    [MenuItem(MENU_ROOT + "Export Save to Console", true)]
    public static bool ValidateExportSave()
    {
        return Application.isPlaying;
    }
}
