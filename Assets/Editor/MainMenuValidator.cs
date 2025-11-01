using UnityEngine;
using UnityEditor;

/// <summary>
/// Validates the MainMenuManager setup and helps diagnose issues
/// </summary>
public class MainMenuValidator : Editor
{
    [MenuItem("Tools/Main Menu/Validate Setup")]
    public static void ValidateSetup()
    {
        MainMenuManager manager = GameObject.FindFirstObjectByType<MainMenuManager>();

        if (manager == null)
        {
            EditorUtility.DisplayDialog("Validation Failed",
                "No MainMenuManager found in scene!\n\n" +
                "Please add MainMenuManager component to a GameObject in your scene.",
                "OK");
            return;
        }

        SerializedObject so = new SerializedObject(manager);

        string report = "=== MAIN MENU VALIDATION ===\n\n";
        bool allValid = true;

        // Check New Game Button
        var newGameButton = so.FindProperty("newGameButton").objectReferenceValue;
        report += $"New Game Button: {(newGameButton != null ? "✓ Assigned" : "✗ MISSING")}\n";
        if (newGameButton == null) allValid = false;

        // Check Continue Button
        var continueButton = so.FindProperty("continueButton").objectReferenceValue;
        report += $"Continue Button: {(continueButton != null ? "✓ Assigned" : "✗ MISSING")}\n";
        if (continueButton == null) allValid = false;

        // Check Load Game Button
        var loadGameButton = so.FindProperty("loadGameButton").objectReferenceValue;
        report += $"Load Game Button: {(loadGameButton != null ? "✓ Assigned" : "✗ MISSING")}\n";
        if (loadGameButton == null) allValid = false;

        // Check Settings Button
        var settingsButton = so.FindProperty("settingsButton").objectReferenceValue;
        report += $"Settings Button: {(settingsButton != null ? "✓ Assigned" : "✗ MISSING")}\n";
        if (settingsButton == null) allValid = false;

        // Check Exit Button
        var exitButton = so.FindProperty("exitButton").objectReferenceValue;
        report += $"Exit Button: {(exitButton != null ? "✓ Assigned" : "✗ MISSING")}\n";
        if (exitButton == null) allValid = false;

        report += "\n--- Load Game Panel Components ---\n";

        // Check Load Game Panel
        var loadGamePanel = so.FindProperty("loadGamePanel").objectReferenceValue;
        report += $"Load Game Panel: {(loadGamePanel != null ? "✓ Assigned" : "✗ MISSING")}\n";
        if (loadGamePanel == null) allValid = false;

        // Check Save Slot Container
        var saveSlotContainer = so.FindProperty("saveSlotContainer").objectReferenceValue;
        report += $"Save Slot Container: {(saveSlotContainer != null ? "✓ Assigned" : "✗ MISSING")}\n";
        if (saveSlotContainer == null) allValid = false;

        // Check Save Slot Button Prefab
        var saveSlotButtonPrefab = so.FindProperty("saveSlotButtonPrefab").objectReferenceValue;
        report += $"Save Slot Button Prefab: {(saveSlotButtonPrefab != null ? "✓ Assigned" : "✗ MISSING")}\n";
        if (saveSlotButtonPrefab == null) allValid = false;

        // Check Close Browser Button
        var closeBrowserButton = so.FindProperty("closeBrowserButton").objectReferenceValue;
        report += $"Close Browser Button: {(closeBrowserButton != null ? "✓ Assigned" : "✗ MISSING")}\n";
        if (closeBrowserButton == null) allValid = false;

        report += "\n=== RESULT ===\n";
        if (allValid)
        {
            report += "✓ All components are assigned!\n";
            report += "\nYour Main Menu is ready to use.";
        }
        else
        {
            report += "✗ Some components are missing!\n\n";
            report += "To fix missing Load Game components:\n";
            report += "1. Tools > Main Menu > Create Load Game Panel\n";
            report += "2. Tools > Main Menu > Create Save Slot Button Prefab\n";
            report += "\nTo recreate the entire main menu:\n";
            report += "Tools > Create Main Menu";
        }

        Debug.Log(report);
        EditorUtility.DisplayDialog(allValid ? "Validation Passed" : "Validation Failed", report, "OK");

        // Select the manager in hierarchy
        Selection.activeObject = manager;
    }
}
