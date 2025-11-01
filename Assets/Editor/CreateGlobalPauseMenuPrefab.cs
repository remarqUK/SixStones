using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Creates a persistent global pause menu that works across all scenes
/// This creates a prefab at Assets/Resources/GlobalPauseMenu.prefab
/// </summary>
public class CreateGlobalPauseMenuPrefab : EditorWindow
{
    [MenuItem("Tools/Create Global Pause Menu Prefab")]
    public static void CreatePrefab()
    {
        // Create Resources folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        // Check if prefab already exists
        string prefabPath = "Assets/Resources/GlobalPauseMenu.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
        {
            bool overwrite = EditorUtility.DisplayDialog("Prefab Exists",
                "GlobalPauseMenu prefab already exists. Do you want to recreate it?",
                "Yes, Recreate", "Cancel");

            if (!overwrite)
            {
                Debug.Log("Prefab creation cancelled");
                return;
            }

            // Delete old prefab
            AssetDatabase.DeleteAsset(prefabPath);
        }

        // Create temporary GameObject to make into prefab
        GameObject managerObj = new GameObject("GlobalPauseMenu");
        PauseMenu pauseMenu = managerObj.AddComponent<PauseMenu>();

        // Create persistent canvas as child
        GameObject canvasObj = new GameObject("PersistentPauseCanvas");
        canvasObj.transform.SetParent(managerObj.transform, false);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // Just below GlobalOptionsManager (1000)

        var canvasScaler = canvasObj.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        canvasScaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Create Pause Menu Panel (full screen overlay)
        GameObject panelObj = CreatePausePanel(canvasObj);

        // Create buttons (vertically stacked)
        GameObject resumeBtn = CreateButton("ResumeButton", panelObj, new Vector2(0, 100), "Resume");
        GameObject optionsBtn = CreateButton("OptionsButton", panelObj, new Vector2(0, 30), "Settings");
        GameObject restartBtn = CreateButton("RestartButton", panelObj, new Vector2(0, -40), "Restart Game");
        GameObject mainMenuBtn = CreateButton("MainMenuButton", panelObj, new Vector2(0, -110), "Return to Main Menu");

        // Wire up button events using SerializedObject (can't use AddListener in Editor scripts)
        Button resumeButton = resumeBtn.GetComponent<Button>();
        Button optionsButton = optionsBtn.GetComponent<Button>();
        Button restartButton = restartBtn.GetComponent<Button>();
        Button mainMenuButton = mainMenuBtn.GetComponent<Button>();

        // Add UnityEvents manually
        UnityEditor.Events.UnityEventTools.AddPersistentListener(resumeButton.onClick, pauseMenu.ResumeGame);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(optionsButton.onClick, pauseMenu.ShowOptions);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(restartButton.onClick, pauseMenu.RestartGame);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(mainMenuButton.onClick, pauseMenu.ReturnToMainMenu);

        // Wire up PauseMenu references
        SerializedObject pauseMenuSO = new SerializedObject(pauseMenu);
        pauseMenuSO.FindProperty("pauseMenuPanel").objectReferenceValue = panelObj;
        pauseMenuSO.FindProperty("persistentCanvas").objectReferenceValue = canvas;
        pauseMenuSO.ApplyModifiedProperties();

        // Create prefab
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(managerObj, prefabPath);

        // Clean up temporary GameObject
        DestroyImmediate(managerObj);

        Debug.Log("<color=green>GlobalPauseMenu prefab created successfully!</color>");
        Debug.Log($"Prefab location: {prefabPath}");
        Debug.Log("Features: Works across all scenes (Match3, Maze3D, zone maps), persists via DontDestroyOnLoad");
        Debug.Log("Usage: Press Escape or gamepad Start button to open pause menu in any scene");

        // Select the prefab
        Selection.activeObject = prefab;
    }

    private static GameObject CreatePausePanel(GameObject canvas)
    {
        GameObject panel = new GameObject("PauseMenuPanel");
        panel.transform.SetParent(canvas.transform, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        Image image = panel.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0.95f); // Nearly black overlay

        // Start inactive
        panel.SetActive(false);

        return panel;
    }

    private static GameObject CreateButton(string name, GameObject parent, Vector2 position, string text)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent.transform, false);

        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = position;
        btnRect.sizeDelta = new Vector2(350, 60);

        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.25f, 0.3f, 1f);

        Button btn = btnObj.AddComponent<Button>();

        // Set button colors for hover/click feedback
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.2f, 0.25f, 0.3f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.35f, 0.4f, 1f);
        colors.pressedColor = new Color(0.15f, 0.2f, 0.25f, 1f);
        colors.selectedColor = new Color(0.25f, 0.3f, 0.35f, 1f);
        btn.colors = colors;

        // Create button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
        btnText.text = text;
        btnText.fontSize = 28;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.white;

        return btnObj;
    }
}
