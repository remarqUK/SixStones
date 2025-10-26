using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// Adds a "Return to Main Menu" button to the pause menu
/// </summary>
public class AddReturnToMainMenuButton : EditorWindow
{
    [MenuItem("Tools/Add Return to Main Menu Button")]
    public static void AddButton()
    {
        // Check if Match3 scene is open
        var currentScene = EditorSceneManager.GetActiveScene();
        if (currentScene.name != "Match3")
        {
            // Try to open Match3 scene
            string scenePath = "Assets/Scenes/Match3.unity";
            if (System.IO.File.Exists(scenePath))
            {
                EditorSceneManager.OpenScene(scenePath);
            }
            else
            {
                Debug.LogError("Match3 scene not found! Make sure it exists at Assets/Scenes/Match3.unity");
                return;
            }
        }

        // Find PauseMenu component
        PauseMenu pauseMenu = GameObject.FindFirstObjectByType<PauseMenu>();
        if (pauseMenu == null)
        {
            Debug.LogError("PauseMenu component not found in Match3 scene!");
            return;
        }

        // Get the pause menu panel using SerializedObject
        SerializedObject so = new SerializedObject(pauseMenu);
        GameObject pauseMenuPanel = so.FindProperty("pauseMenuPanel").objectReferenceValue as GameObject;

        if (pauseMenuPanel == null)
        {
            Debug.LogError("Pause menu panel not found!");
            return;
        }

        // Find the MenuContainer inside PauseMenuPanel
        Transform menuContainerTransform = pauseMenuPanel.transform.Find("MenuContainer");
        if (menuContainerTransform == null)
        {
            Debug.LogError("MenuContainer not found inside PauseMenuPanel!");
            return;
        }

        GameObject menuContainer = menuContainerTransform.gameObject;

        // Check if button already exists
        Transform existingButton = menuContainer.transform.Find("ReturnToMainMenuButton");
        if (existingButton != null)
        {
            Debug.Log("Return to Main Menu button already exists!");
            Selection.activeGameObject = existingButton.gameObject;
            return;
        }

        // Create the button - positioned at -280 (below Restart Game which is at -200)
        GameObject buttonObj = new GameObject("ReturnToMainMenuButton");
        buttonObj.transform.SetParent(menuContainer.transform, false);

        RectTransform btnRect = buttonObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);

        // Position it at -280 to match the pause menu button layout
        btnRect.anchoredPosition = new Vector2(0, -280);
        btnRect.sizeDelta = new Vector2(250, 60); // Match other buttons

        Image btnImage = buttonObj.AddComponent<Image>();
        btnImage.color = new Color(0.3f, 0.3f, 0.3f); // Match other buttons - dark gray

        Button btn = buttonObj.AddComponent<Button>();

        // Match the ColorBlock settings from other pause menu buttons
        UnityEngine.UI.ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.3f, 0.3f, 0.3f);
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f);
        colors.pressedColor = new Color(0.2f, 0.2f, 0.2f);
        btn.colors = colors;

        // Create button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "Return to Main Menu";
        btnText.fontSize = 24;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.white;

        // Wire up the button to call PauseMenu.ReturnToMainMenu() using persistent listener
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, pauseMenu.ReturnToMainMenu);
        EditorUtility.SetDirty(buttonObj);

        // Mark scene as dirty and save
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("<color=green>Return to Main Menu button added successfully!</color>");
        Debug.Log("Button positioned at y=-280 (below Restart Game button)");
        Debug.Log("Button layout: Resume(120) > Options(40) > Save&Continue(-40) > Save&Quit(-120) > Restart(-200) > ReturnToMainMenu(-280)");

        // Select the new button
        Selection.activeGameObject = buttonObj;
    }
}
