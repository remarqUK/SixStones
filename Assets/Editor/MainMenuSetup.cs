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
        GameObject loadGameBtn = CreateMenuButton("LoadGameButton", "Load Game", menuContainer, 2);
        GameObject settingsBtn = CreateMenuButton("SettingsButton", "Settings", menuContainer, 3);
        GameObject exitBtn = CreateMenuButton("ExitButton", "Exit", menuContainer, 4);

        // Note: Settings panel is no longer needed - GlobalOptionsManager handles options globally

        // Create MainMenuManager
        GameObject managerObj = new GameObject("MainMenuManager");
        MainMenuManager menuManager = managerObj.AddComponent<MainMenuManager>();

        // Wire up references using SerializedObject
        SerializedObject so = new SerializedObject(menuManager);
        so.FindProperty("newGameButton").objectReferenceValue = newGameBtn.GetComponent<Button>();
        so.FindProperty("continueButton").objectReferenceValue = continueBtn.GetComponent<Button>();
        so.FindProperty("loadGameButton").objectReferenceValue = loadGameBtn.GetComponent<Button>();
        so.FindProperty("settingsButton").objectReferenceValue = settingsBtn.GetComponent<Button>();
        so.FindProperty("exitButton").objectReferenceValue = exitBtn.GetComponent<Button>();
        so.FindProperty("continueButtonText").objectReferenceValue = continueBtn.GetComponentInChildren<TextMeshProUGUI>();
        so.FindProperty("loadGameButtonText").objectReferenceValue = loadGameBtn.GetComponentInChildren<TextMeshProUGUI>();
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

    [MenuItem("Tools/Main Menu/Create Load Game Panel")]
    public static void CreateLoadGamePanel()
    {
        // Find the Canvas in the scene
        Canvas canvas = GameObject.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "No Canvas found in scene! Please create the Main Menu scene first.", "OK");
            return;
        }

        // Create Load Game Panel
        GameObject loadGamePanel = new GameObject("LoadGamePanel");
        loadGamePanel.transform.SetParent(canvas.transform, false);

        // Add Image component for background
        Image panelImage = loadGamePanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.95f);

        // Set RectTransform to fill screen
        RectTransform panelRect = loadGamePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;

        // Create Title Text
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(loadGamePanel.transform, false);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Load Game";
        titleText.fontSize = 48;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -80);
        titleRect.sizeDelta = new Vector2(600, 80);

        // Create Scroll View for save slots
        GameObject scrollViewObj = new GameObject("SaveSlotsScrollView");
        scrollViewObj.transform.SetParent(loadGamePanel.transform, false);

        RectTransform scrollRect = scrollViewObj.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0.5f, 0.5f);
        scrollRect.anchorMax = new Vector2(0.5f, 0.5f);
        scrollRect.anchoredPosition = new Vector2(0, 0);
        scrollRect.sizeDelta = new Vector2(800, 600);

        // Add ScrollRect component
        ScrollRect scrollComponent = scrollViewObj.AddComponent<ScrollRect>();

        // Add Image for scroll view background
        Image scrollBg = scrollViewObj.AddComponent<Image>();
        scrollBg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        // Create Viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(scrollViewObj.transform, false);

        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.anchoredPosition = Vector2.zero;

        viewportObj.AddComponent<Image>().color = new Color(0, 0, 0, 0); // Transparent
        viewportObj.AddComponent<Mask>().showMaskGraphic = false;

        // Create Content container
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);

        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 1000);

        // Add VerticalLayoutGroup for automatic spacing
        VerticalLayoutGroup layoutGroup = contentObj.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 10;
        layoutGroup.padding = new RectOffset(20, 20, 20, 20);
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;

        // Add ContentSizeFitter
        ContentSizeFitter fitter = contentObj.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Connect ScrollRect to Viewport and Content
        scrollComponent.viewport = viewportRect;
        scrollComponent.content = contentRect;
        scrollComponent.horizontal = false;
        scrollComponent.vertical = true;

        // Create Close Button
        GameObject closeButtonObj = new GameObject("CloseButton");
        closeButtonObj.transform.SetParent(loadGamePanel.transform, false);

        RectTransform closeRect = closeButtonObj.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.5f, 0f);
        closeRect.anchorMax = new Vector2(0.5f, 0f);
        closeRect.anchoredPosition = new Vector2(0, 80);
        closeRect.sizeDelta = new Vector2(200, 60);

        Button closeButton = closeButtonObj.AddComponent<Button>();
        Image closeImage = closeButtonObj.AddComponent<Image>();
        closeImage.color = new Color(0.8f, 0.2f, 0.2f, 1f);

        GameObject closeTextObj = new GameObject("Text");
        closeTextObj.transform.SetParent(closeButtonObj.transform, false);
        TextMeshProUGUI closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
        closeText.text = "Close";
        closeText.fontSize = 24;
        closeText.alignment = TextAlignmentOptions.Center;
        closeText.color = Color.white;

        RectTransform closeTextRect = closeTextObj.GetComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.sizeDelta = Vector2.zero;
        closeTextRect.anchoredPosition = Vector2.zero;

        // Hide the panel initially
        loadGamePanel.SetActive(false);

        // Try to wire up to MainMenuManager
        MainMenuManager menuManager = GameObject.FindFirstObjectByType<MainMenuManager>();
        if (menuManager != null)
        {
            SerializedObject so = new SerializedObject(menuManager);
            so.FindProperty("loadGamePanel").objectReferenceValue = loadGamePanel;
            so.FindProperty("saveSlotContainer").objectReferenceValue = contentObj.transform;
            so.FindProperty("closeBrowserButton").objectReferenceValue = closeButton;
            so.ApplyModifiedProperties();
        }

        // Select the panel in hierarchy
        Selection.activeGameObject = loadGamePanel;

        EditorUtility.DisplayDialog("Success",
            "Load Game Panel created successfully!\n\n" +
            "Next steps:\n" +
            "1. Create Save Slot Button Prefab using:\n   Tools > Main Menu > Create Save Slot Button Prefab\n" +
            "2. Assign the prefab to MainMenuManager's 'Save Slot Button Prefab' field",
            "OK");
    }

    [MenuItem("Tools/Main Menu/Create Save Slot Button Prefab")]
    public static void CreateSaveSlotButtonPrefab()
    {
        // Create button GameObject
        GameObject buttonObj = new GameObject("SaveSlotButton");

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(700, 120);

        Button button = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.3f, 1f);

        // Add SaveSlotButton component
        buttonObj.AddComponent<SaveSlotButton>();

        // Create text child
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "Slot 1\nLevel X - Zone Y/Z\n2025-01-01 12:00:00\nPlaytime: 1h 23m";
        text.fontSize = 20;
        text.alignment = TextAlignmentOptions.Left;
        text.color = Color.white;

        // Try to load and assign TMP default font
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (font == null)
        {
            // Try alternative path
            font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }
        if (font != null)
        {
            text.font = font;
            Debug.Log("[CreateSaveSlotButton] TMP font assigned successfully");
        }
        else
        {
            Debug.LogWarning("[CreateSaveSlotButton] Could not find TMP default font. Text might not display correctly. Run Window > TextMeshPro > Import TMP Essential Resources.");
        }

        // Enable auto-sizing to fit text better
        text.enableAutoSizing = true;
        text.fontSizeMin = 14;
        text.fontSizeMax = 20;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = new Vector2(-20, -20);
        textRect.anchoredPosition = Vector2.zero;

        // Save as prefab
        string path = "Assets/Prefabs/UI/SaveSlotButton.prefab";

        // Ensure directory exists
        System.IO.Directory.CreateDirectory("Assets/Prefabs/UI");

        // Save prefab
        PrefabUtility.SaveAsPrefabAsset(buttonObj, path);

        // Destroy the temporary object
        DestroyImmediate(buttonObj);

        // Select the prefab
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        Selection.activeObject = prefab;

        // Try to wire up to MainMenuManager
        MainMenuManager menuManager = GameObject.FindFirstObjectByType<MainMenuManager>();
        if (menuManager != null)
        {
            SerializedObject so = new SerializedObject(menuManager);
            so.FindProperty("saveSlotButtonPrefab").objectReferenceValue = prefab;
            so.ApplyModifiedProperties();
        }

        EditorUtility.DisplayDialog("Success",
            $"Save Slot Button Prefab created at:\n{path}\n\n" +
            "Prefab has been auto-assigned to MainMenuManager!",
            "OK");
    }

    [MenuItem("Tools/Main Menu/Fix Save Slot Button Font")]
    public static void FixSaveSlotButtonFont()
    {
        // Load the existing prefab
        string path = "Assets/Prefabs/UI/SaveSlotButton.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

        if (prefab == null)
        {
            EditorUtility.DisplayDialog("Error",
                "Could not find SaveSlotButton prefab at:\n" + path + "\n\n" +
                "Please run: Tools > Main Menu > Create Save Slot Button Prefab",
                "OK");
            return;
        }

        // Get the TextMeshProUGUI component
        TextMeshProUGUI text = prefab.GetComponentInChildren<TextMeshProUGUI>();
        if (text == null)
        {
            EditorUtility.DisplayDialog("Error",
                "SaveSlotButton prefab has no TextMeshProUGUI component!",
                "OK");
            return;
        }

        // Load the TMP font
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (font == null)
        {
            font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }

        if (font == null)
        {
            EditorUtility.DisplayDialog("Error",
                "Could not find TMP font!\n\n" +
                "Please import TMP Essentials:\n" +
                "Window > TextMeshPro > Import TMP Essential Resources",
                "OK");
            return;
        }

        // Assign the font to the prefab
        text.font = font;

        // Enable auto-sizing
        text.enableAutoSizing = true;
        text.fontSizeMin = 14;
        text.fontSizeMax = 20;

        // Mark the prefab as dirty and save
        EditorUtility.SetDirty(prefab);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success",
            "TMP font has been assigned to SaveSlotButton prefab!\n\n" +
            "The text should now display correctly in the Load Game dialog.",
            "OK");

        Debug.Log("[FixSaveSlotButtonFont] Font assigned successfully to prefab");
    }
}
