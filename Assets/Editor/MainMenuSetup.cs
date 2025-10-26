using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Editor tool to create the main menu scene
/// </summary>
public class MainMenuSetup : EditorWindow
{
    [MenuItem("Tools/Create Main Menu")]
    public static void CreateMainMenu()
    {
        // Create new scene
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Create Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // Create Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform, false);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.15f, 1f); // Dark blue-gray
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Create Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Six Stones";
        titleText.fontSize = 120;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(0.9f, 0.85f, 0.7f, 1f); // Golden
        titleText.fontStyle = FontStyles.Bold;

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0, 300);
        titleRect.sizeDelta = new Vector2(800, 200);

        // Create Menu Container
        GameObject menuContainer = new GameObject("MenuContainer");
        menuContainer.transform.SetParent(canvasObj.transform, false);
        RectTransform menuRect = menuContainer.AddComponent<RectTransform>();
        menuRect.anchorMin = new Vector2(0.5f, 0.5f);
        menuRect.anchorMax = new Vector2(0.5f, 0.5f);
        menuRect.pivot = new Vector2(0.5f, 0.5f);
        menuRect.anchoredPosition = new Vector2(0, -50);
        menuRect.sizeDelta = new Vector2(400, 500);

        // Create Buttons
        GameObject newGameBtn = CreateMenuButton("NewGameButton", "New Game", menuContainer, 0);
        GameObject continueBtn = CreateMenuButton("ContinueButton", "Continue", menuContainer, 1);
        GameObject settingsBtn = CreateMenuButton("SettingsButton", "Settings", menuContainer, 2);
        GameObject exitBtn = CreateMenuButton("ExitButton", "Exit", menuContainer, 3);

        // Note: Settings panel is no longer needed - GlobalOptionsManager handles options globally

        // Create MainMenuManager
        GameObject managerObj = new GameObject("MainMenuManager");
        MainMenuManager menuManager = managerObj.AddComponent<MainMenuManager>();

        // Wire up references using SerializedObject
        SerializedObject so = new SerializedObject(menuManager);
        so.FindProperty("newGameButton").objectReferenceValue = newGameBtn.GetComponent<Button>();
        so.FindProperty("continueButton").objectReferenceValue = continueBtn.GetComponent<Button>();
        so.FindProperty("settingsButton").objectReferenceValue = settingsBtn.GetComponent<Button>();
        so.FindProperty("exitButton").objectReferenceValue = exitBtn.GetComponent<Button>();
        so.FindProperty("continueButtonText").objectReferenceValue = continueBtn.GetComponentInChildren<TextMeshProUGUI>();
        so.ApplyModifiedProperties();

        // Create EventSystem
        if (GameObject.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // Create Main Camera
        GameObject cameraObj = new GameObject("Main Camera");
        Camera camera = cameraObj.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f);
        camera.orthographic = true;
        camera.orthographicSize = 5;
        cameraObj.tag = "MainCamera";
        cameraObj.AddComponent<AudioListener>();

        // Add URP Additional Camera Data
        var cameraDataType = System.Type.GetType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
        if (cameraDataType != null)
        {
            cameraObj.AddComponent(cameraDataType);
        }

        // Save scene
        string scenePath = "Assets/Scenes/MainMenu.unity";
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(newScene, scenePath);

        Debug.Log($"Main menu created at {scenePath}");
        EditorUtility.DisplayDialog("Main Menu Created",
            "Main menu scene created successfully!\n\n" +
            "Don't forget to add it to Build Settings:\n" +
            "1. File > Build Profiles\n" +
            "2. Add MainMenu as the first scene (index 0)\n" +
            "3. Ensure LoadingScreen and Match3 follow",
            "OK");
    }

    private static GameObject CreateMenuButton(string name, string text, GameObject parent, int index)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent.transform, false);

        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 1f);
        btnRect.anchorMax = new Vector2(0.5f, 1f);
        btnRect.pivot = new Vector2(0.5f, 1f);
        btnRect.anchoredPosition = new Vector2(0, -index * 110 - 10);
        btnRect.sizeDelta = new Vector2(350, 90);

        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.25f, 0.3f, 1f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.2f, 0.25f, 0.3f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.35f, 0.4f, 1f);
        colors.pressedColor = new Color(0.15f, 0.2f, 0.25f, 1f);
        colors.disabledColor = new Color(0.15f, 0.15f, 0.15f, 0.5f);
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
        btnText.fontSize = 36;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        return btnObj;
    }

    private static GameObject CreateSettingsPanel(GameObject parent)
    {
        GameObject panel = new GameObject("SettingsPanel");
        panel.transform.SetParent(parent.transform, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.85f);

        // Settings message
        GameObject msgObj = new GameObject("SettingsMessage");
        msgObj.transform.SetParent(panel.transform, false);

        RectTransform msgRect = msgObj.AddComponent<RectTransform>();
        msgRect.anchorMin = new Vector2(0.5f, 0.5f);
        msgRect.anchorMax = new Vector2(0.5f, 0.5f);
        msgRect.pivot = new Vector2(0.5f, 0.5f);
        msgRect.anchoredPosition = new Vector2(0, 50);
        msgRect.sizeDelta = new Vector2(600, 200);

        TextMeshProUGUI msgText = msgObj.AddComponent<TextMeshProUGUI>();
        msgText.text = "Settings will be available in-game\nPress ESC during gameplay";
        msgText.fontSize = 32;
        msgText.alignment = TextAlignmentOptions.Center;
        msgText.color = Color.white;

        // Back button
        GameObject backBtn = CreateMenuButton("BackButton", "Back", panel, 0);
        RectTransform backRect = backBtn.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0.5f, 0.5f);
        backRect.anchorMax = new Vector2(0.5f, 0.5f);
        backRect.pivot = new Vector2(0.5f, 0.5f);
        backRect.anchoredPosition = new Vector2(0, -100);

        return panel;
    }
}
