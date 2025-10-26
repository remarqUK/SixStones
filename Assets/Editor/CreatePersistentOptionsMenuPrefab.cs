using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Creates a prefab for the persistent global options menu
/// The prefab will be automatically instantiated on first access from any scene
/// </summary>
public class CreatePersistentOptionsMenuPrefab : EditorWindow
{
    [MenuItem("Tools/Create Persistent Options Menu Prefab")]
    public static void CreatePrefab()
    {
        // Create Resources folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        // Check if prefab already exists
        string prefabPath = "Assets/Resources/GlobalOptionsManager.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
        {
            bool overwrite = EditorUtility.DisplayDialog("Prefab Exists",
                "GlobalOptionsManager prefab already exists. Do you want to recreate it?",
                "Yes, Recreate", "Cancel");

            if (!overwrite)
            {
                Debug.Log("Prefab creation cancelled");
                return;
            }
        }

        // Create temporary GameObject to make into prefab
        GameObject managerObj = new GameObject("GlobalOptionsManager");
        GlobalOptionsManager manager = managerObj.AddComponent<GlobalOptionsManager>();
        OptionsMenuController controller = managerObj.AddComponent<OptionsMenuController>();

        // Create persistent canvas as child
        GameObject canvasObj = new GameObject("PersistentOptionsCanvas");
        canvasObj.transform.SetParent(managerObj.transform, false);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // Render on top of everything

        var canvasScaler = canvasObj.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        canvasScaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Create Options Panel (full screen overlay)
        GameObject panelObj = CreateOptionsPanel(canvasObj);

        // Create Volume Sliders (top section)
        GameObject gameVolumeSliderObj = CreateSlider("GameVolumeSlider", panelObj, new Vector2(0, 250), "Game Volume:");
        GameObject musicVolumeSliderObj = CreateSlider("MusicVolumeSlider", panelObj, new Vector2(0, 180), "Music Volume:");
        GameObject dialogVolumeSliderObj = CreateSlider("DialogVolumeSlider", panelObj, new Vector2(0, 110), "Dialog Volume:");

        // Create Game Speed Dropdown (middle section)
        GameObject speedDropdownObj = CreateDropdown("GameSpeedDropdown", panelObj, new Vector2(0, 20), "Game Speed:");

        // Create Language Dropdown
        GameObject langDropdownObj = CreateDropdown("LanguageDropdown", panelObj, new Vector2(0, -80), "Language:");

        // Create Close Button (bottom)
        GameObject closeButtonObj = CreateButton("CloseButton", panelObj, new Vector2(0, -200), "Close");

        // Wire up controller using SerializedObject
        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("optionsPanel").objectReferenceValue = panelObj;
        so.FindProperty("gameVolumeSlider").objectReferenceValue = gameVolumeSliderObj.GetComponent<Slider>();
        so.FindProperty("musicVolumeSlider").objectReferenceValue = musicVolumeSliderObj.GetComponent<Slider>();
        so.FindProperty("dialogVolumeSlider").objectReferenceValue = dialogVolumeSliderObj.GetComponent<Slider>();
        so.FindProperty("gameSpeedDropdown").objectReferenceValue = speedDropdownObj.GetComponent<TMP_Dropdown>();
        so.FindProperty("languageDropdown").objectReferenceValue = langDropdownObj.GetComponent<TMP_Dropdown>();
        so.FindProperty("closeButton").objectReferenceValue = closeButtonObj.GetComponent<Button>();
        so.FindProperty("pauseTimeWhenOpen").boolValue = true; // Always pause when opened globally
        so.ApplyModifiedProperties();

        // Wire up manager references using SerializedObject
        SerializedObject managerSO = new SerializedObject(manager);
        managerSO.FindProperty("optionsMenuController").objectReferenceValue = controller;
        managerSO.FindProperty("persistentCanvas").objectReferenceValue = canvas;
        managerSO.ApplyModifiedProperties();

        // Create prefab
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(managerObj, prefabPath);

        // Clean up temporary GameObject
        DestroyImmediate(managerObj);

        Debug.Log("<color=green>GlobalOptionsManager prefab created successfully!</color>");
        Debug.Log($"Prefab location: {prefabPath}");
        Debug.Log("The prefab will be automatically instantiated when first accessed from any scene.");
        Debug.Log("Access options by pressing Escape (pause menu) then clicking Settings button.");

        // Select the prefab
        Selection.activeObject = prefab;
    }

    private static GameObject CreateOptionsPanel(GameObject canvas)
    {
        GameObject panel = new GameObject("OptionsPanel");
        panel.transform.SetParent(canvas.transform, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        Image image = panel.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0.9f); // Dark semi-transparent background

        // Start inactive
        panel.SetActive(false);

        return panel;
    }

    private static GameObject CreateDropdown(string name, GameObject parent, Vector2 position, string labelText)
    {
        // Create container
        GameObject container = new GameObject(name + "Container");
        container.transform.SetParent(parent.transform, false);

        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = position;
        containerRect.sizeDelta = new Vector2(400, 80);

        // Create label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(container.transform, false);

        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0.5f);
        labelRect.anchorMax = new Vector2(0, 0.5f);
        labelRect.pivot = new Vector2(0, 0.5f);
        labelRect.anchoredPosition = Vector2.zero;
        labelRect.sizeDelta = new Vector2(150, 40);

        TextMeshProUGUI labelTMP = labelObj.AddComponent<TextMeshProUGUI>();
        labelTMP.text = labelText;
        labelTMP.fontSize = 24;
        labelTMP.color = Color.white;
        labelTMP.alignment = TextAlignmentOptions.MidlineLeft;

        // Create dropdown
        GameObject dropdownObj = new GameObject(name);
        dropdownObj.transform.SetParent(container.transform, false);

        RectTransform dropdownRect = dropdownObj.AddComponent<RectTransform>();
        dropdownRect.anchorMin = new Vector2(1, 0.5f);
        dropdownRect.anchorMax = new Vector2(1, 0.5f);
        dropdownRect.pivot = new Vector2(1, 0.5f);
        dropdownRect.anchoredPosition = Vector2.zero;
        dropdownRect.sizeDelta = new Vector2(220, 40);

        Image dropdownImage = dropdownObj.AddComponent<Image>();
        dropdownImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        TMP_Dropdown dropdown = dropdownObj.AddComponent<TMP_Dropdown>();

        // Create dropdown label
        GameObject dropdownLabelObj = new GameObject("Label");
        dropdownLabelObj.transform.SetParent(dropdownObj.transform, false);

        RectTransform dropdownLabelRect = dropdownLabelObj.AddComponent<RectTransform>();
        dropdownLabelRect.anchorMin = Vector2.zero;
        dropdownLabelRect.anchorMax = Vector2.one;
        dropdownLabelRect.offsetMin = new Vector2(10, 0);
        dropdownLabelRect.offsetMax = new Vector2(-25, 0);

        TextMeshProUGUI dropdownLabelTMP = dropdownLabelObj.AddComponent<TextMeshProUGUI>();
        dropdownLabelTMP.fontSize = 20;
        dropdownLabelTMP.color = Color.white;
        dropdownLabelTMP.alignment = TextAlignmentOptions.MidlineLeft;

        dropdown.captionText = dropdownLabelTMP;

        // Create dropdown template
        GameObject template = new GameObject("Template");
        template.transform.SetParent(dropdownObj.transform, false);
        template.SetActive(false);

        RectTransform templateRect = template.AddComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(0, 0);
        templateRect.anchorMax = new Vector2(1, 0);
        templateRect.pivot = new Vector2(0.5f, 1);
        templateRect.anchoredPosition = new Vector2(0, 0);
        templateRect.sizeDelta = new Vector2(0, 150);

        Image templateImage = template.AddComponent<Image>();
        templateImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        // Create Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(template.transform, false);

        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;

        UnityEngine.UI.Mask viewportMask = viewport.AddComponent<UnityEngine.UI.Mask>();
        viewportMask.showMaskGraphic = false;

        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        // Create Scrollbar
        GameObject scrollbar = new GameObject("Scrollbar");
        scrollbar.transform.SetParent(template.transform, false);

        RectTransform scrollbarRect = scrollbar.AddComponent<RectTransform>();
        scrollbarRect.anchorMin = new Vector2(1, 0);
        scrollbarRect.anchorMax = Vector2.one;
        scrollbarRect.pivot = Vector2.one;
        scrollbarRect.sizeDelta = new Vector2(20, 0);

        Image scrollbarImage = scrollbar.AddComponent<Image>();
        scrollbarImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);

        UnityEngine.UI.Scrollbar scrollbarComponent = scrollbar.AddComponent<UnityEngine.UI.Scrollbar>();
        scrollbarComponent.direction = UnityEngine.UI.Scrollbar.Direction.BottomToTop;

        // Scrollbar Sliding Area
        GameObject slidingArea = new GameObject("Sliding Area");
        slidingArea.transform.SetParent(scrollbar.transform, false);

        RectTransform slidingAreaRect = slidingArea.AddComponent<RectTransform>();
        slidingAreaRect.anchorMin = Vector2.zero;
        slidingAreaRect.anchorMax = Vector2.one;
        slidingAreaRect.offsetMin = new Vector2(10, 10);
        slidingAreaRect.offsetMax = new Vector2(-10, -10);

        // Scrollbar Handle
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(slidingArea.transform, false);

        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 20);

        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);

        scrollbarComponent.handleRect = handleRect;
        scrollbarComponent.targetGraphic = handleImage;

        // Create Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);

        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 30);

        // Add ScrollRect to viewport
        UnityEngine.UI.ScrollRect scrollRect = viewport.AddComponent<UnityEngine.UI.ScrollRect>();
        scrollRect.content = contentRect;
        scrollRect.viewport = viewportRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.verticalScrollbar = scrollbarComponent;
        scrollRect.verticalScrollbarVisibility = UnityEngine.UI.ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        scrollRect.scrollSensitivity = 10;

        // Create Item (Toggle)
        GameObject item = new GameObject("Item");
        item.transform.SetParent(content.transform, false);

        RectTransform itemRect = item.AddComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0, 0.5f);
        itemRect.anchorMax = new Vector2(1, 0.5f);
        itemRect.pivot = new Vector2(0.5f, 0.5f);
        itemRect.sizeDelta = new Vector2(0, 30);

        // Item Background
        Image itemBackground = item.AddComponent<Image>();
        itemBackground.color = new Color(0.25f, 0.25f, 0.25f, 1f);

        UnityEngine.UI.Toggle itemToggle = item.AddComponent<UnityEngine.UI.Toggle>();
        itemToggle.targetGraphic = itemBackground;
        itemToggle.isOn = true;

        // Set navigation to none to avoid warnings
        UnityEngine.UI.Navigation nav = new UnityEngine.UI.Navigation();
        nav.mode = UnityEngine.UI.Navigation.Mode.None;
        itemToggle.navigation = nav;

        // Item Checkmark
        GameObject checkmark = new GameObject("Item Checkmark");
        checkmark.transform.SetParent(item.transform, false);

        RectTransform checkmarkRect = checkmark.AddComponent<RectTransform>();
        checkmarkRect.anchorMin = new Vector2(0, 0.5f);
        checkmarkRect.anchorMax = new Vector2(0, 0.5f);
        checkmarkRect.pivot = new Vector2(0.5f, 0.5f);
        checkmarkRect.anchoredPosition = new Vector2(15, 0);
        checkmarkRect.sizeDelta = new Vector2(20, 20);

        Image checkmarkImage = checkmark.AddComponent<Image>();
        checkmarkImage.color = Color.white;

        itemToggle.graphic = checkmarkImage;

        // Item Label
        GameObject itemLabel = new GameObject("Item Label");
        itemLabel.transform.SetParent(item.transform, false);

        RectTransform itemLabelRect = itemLabel.AddComponent<RectTransform>();
        itemLabelRect.anchorMin = Vector2.zero;
        itemLabelRect.anchorMax = Vector2.one;
        itemLabelRect.offsetMin = new Vector2(40, 0);
        itemLabelRect.offsetMax = new Vector2(-10, 0);

        TextMeshProUGUI itemLabelText = itemLabel.AddComponent<TextMeshProUGUI>();
        itemLabelText.fontSize = 18;
        itemLabelText.color = Color.white;
        itemLabelText.alignment = TextAlignmentOptions.MidlineLeft;

        dropdown.template = templateRect;
        dropdown.itemText = itemLabelText;

        // Verify the template has a Toggle component
        UnityEngine.UI.Toggle toggleCheck = template.GetComponentInChildren<UnityEngine.UI.Toggle>();
        if (toggleCheck == null)
        {
            Debug.LogError("Dropdown template validation failed - no Toggle found!");
        }
        else
        {
            Debug.Log($"Dropdown template created successfully with Toggle at: {toggleCheck.gameObject.name}");
        }

        return dropdownObj;
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
        btnRect.sizeDelta = new Vector2(250, 60);

        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.25f, 0.3f, 1f);

        Button btn = btnObj.AddComponent<Button>();

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

    private static GameObject CreateSlider(string name, GameObject parent, Vector2 position, string labelText)
    {
        // Create container
        GameObject container = new GameObject(name + "Container");
        container.transform.SetParent(parent.transform, false);

        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = position;
        containerRect.sizeDelta = new Vector2(500, 60);

        // Create label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(container.transform, false);

        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0.5f);
        labelRect.anchorMax = new Vector2(0, 0.5f);
        labelRect.pivot = new Vector2(0, 0.5f);
        labelRect.anchoredPosition = Vector2.zero;
        labelRect.sizeDelta = new Vector2(180, 40);

        TextMeshProUGUI labelTMP = labelObj.AddComponent<TextMeshProUGUI>();
        labelTMP.text = labelText;
        labelTMP.fontSize = 22;
        labelTMP.color = Color.white;
        labelTMP.alignment = TextAlignmentOptions.MidlineLeft;

        // Create slider
        GameObject sliderObj = new GameObject(name);
        sliderObj.transform.SetParent(container.transform, false);

        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(1, 0.5f);
        sliderRect.anchorMax = new Vector2(1, 0.5f);
        sliderRect.pivot = new Vector2(1, 0.5f);
        sliderRect.anchoredPosition = Vector2.zero;
        sliderRect.sizeDelta = new Vector2(280, 30);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.wholeNumbers = false;

        // Create background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);

        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        // Create fill area
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(sliderObj.transform, false);

        RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(10, 0);
        fillAreaRect.offsetMax = new Vector2(-10, 0);

        // Create fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform, false);

        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = new Color(0.3f, 0.6f, 0.9f, 1f);

        slider.fillRect = fillRect;

        // Create handle slide area
        GameObject handleAreaObj = new GameObject("Handle Slide Area");
        handleAreaObj.transform.SetParent(sliderObj.transform, false);

        RectTransform handleAreaRect = handleAreaObj.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(10, 0);
        handleAreaRect.offsetMax = new Vector2(-10, 0);

        // Create handle
        GameObject handleObj = new GameObject("Handle");
        handleObj.transform.SetParent(handleAreaObj.transform, false);

        RectTransform handleRect = handleObj.AddComponent<RectTransform>();
        handleRect.anchorMin = new Vector2(0.5f, 0.5f);
        handleRect.anchorMax = new Vector2(0.5f, 0.5f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.sizeDelta = new Vector2(20, 30);

        Image handleImage = handleObj.AddComponent<Image>();
        handleImage.color = Color.white;

        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;

        return sliderObj;
    }
}
