using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Creates a simplified persistent options menu with just volume sliders
/// This version avoids the complex dropdown template issues
/// </summary>
public class CreateSimpleOptionsMenuPrefab : EditorWindow
{
    [MenuItem("Tools/Create Simple Options Menu Prefab")]
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

            // Delete old prefab
            AssetDatabase.DeleteAsset(prefabPath);
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

        // Create Volume Sliders
        GameObject gameVolumeSliderObj = CreateSlider("GameVolumeSlider", panelObj, new Vector2(0, 200), "Game Volume:");
        GameObject musicVolumeSliderObj = CreateSlider("MusicVolumeSlider", panelObj, new Vector2(0, 130), "Music Volume:");
        GameObject dialogVolumeSliderObj = CreateSlider("DialogVolumeSlider", panelObj, new Vector2(0, 60), "Dialog Volume:");

        // Create Dropdowns
        GameObject speedDropdownObj = CreateDropdown("GameSpeedDropdown", panelObj, new Vector2(0, -30), "Game Speed:");
        GameObject langDropdownObj = CreateDropdown("LanguageDropdown", panelObj, new Vector2(0, -120), "Language:");

        // Create Close Button
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
        so.FindProperty("pauseTimeWhenOpen").boolValue = true;
        so.ApplyModifiedProperties();

        // Wire up manager references
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
        Debug.Log("Includes: Game/Music/Dialog volume sliders + Game Speed/Language dropdowns + Close button");

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
        image.color = new Color(0, 0, 0, 0.9f);

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

        // Create dropdown background
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

        // Create dropdown label (caption text)
        GameObject captionObj = new GameObject("Label");
        captionObj.transform.SetParent(dropdownObj.transform, false);

        RectTransform captionRect = captionObj.AddComponent<RectTransform>();
        captionRect.anchorMin = Vector2.zero;
        captionRect.anchorMax = Vector2.one;
        captionRect.offsetMin = new Vector2(10, 2);
        captionRect.offsetMax = new Vector2(-25, -2);

        TextMeshProUGUI captionText = captionObj.AddComponent<TextMeshProUGUI>();
        captionText.fontSize = 20;
        captionText.color = Color.white;
        captionText.alignment = TextAlignmentOptions.MidlineLeft;

        dropdown.captionText = captionText;

        // Create arrow
        GameObject arrowObj = new GameObject("Arrow");
        arrowObj.transform.SetParent(dropdownObj.transform, false);

        RectTransform arrowRect = arrowObj.AddComponent<RectTransform>();
        arrowRect.anchorMin = new Vector2(1, 0.5f);
        arrowRect.anchorMax = new Vector2(1, 0.5f);
        arrowRect.pivot = new Vector2(0.5f, 0.5f);
        arrowRect.anchoredPosition = new Vector2(-12, 0);
        arrowRect.sizeDelta = new Vector2(16, 16);

        Image arrowImage = arrowObj.AddComponent<Image>();
        arrowImage.color = Color.white;
        // Note: Arrow sprite would normally be assigned here

        // Create template
        GameObject template = new GameObject("Template");
        template.transform.SetParent(dropdownObj.transform, false);

        RectTransform templateRect = template.AddComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(0, 0);
        templateRect.anchorMax = new Vector2(1, 0);
        templateRect.pivot = new Vector2(0.5f, 1);
        templateRect.anchoredPosition = new Vector2(0, 2);
        templateRect.sizeDelta = new Vector2(0, 150);

        Image templateBg = template.AddComponent<Image>();
        templateBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        // Create Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(template.transform, false);

        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = new Vector2(-18, 0);
        viewportRect.pivot = new Vector2(0, 1);

        Image viewportImg = viewport.AddComponent<Image>();
        viewportImg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        UnityEngine.UI.Mask viewportMask = viewport.AddComponent<UnityEngine.UI.Mask>();
        viewportMask.showMaskGraphic = false;

        // Create Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);

        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 28);

        // Create Item (THIS IS THE CRITICAL PART - must have Toggle)
        GameObject item = new GameObject("Item");
        item.transform.SetParent(content.transform, false);

        RectTransform itemRect = item.AddComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0, 0.5f);
        itemRect.anchorMax = new Vector2(1, 0.5f);
        itemRect.pivot = new Vector2(0.5f, 0.5f);
        itemRect.sizeDelta = new Vector2(0, 20);

        // Item background
        Image itemBg = item.AddComponent<Image>();
        itemBg.color = new Color(0.25f, 0.25f, 0.25f, 1f);

        // CRITICAL: Add Toggle component to Item
        UnityEngine.UI.Toggle toggle = item.AddComponent<UnityEngine.UI.Toggle>();
        toggle.targetGraphic = itemBg;
        toggle.isOn = true;

        // Set toggle colors
        UnityEngine.UI.ColorBlock colors = toggle.colors;
        colors.normalColor = new Color(0.25f, 0.25f, 0.25f, 1f);
        colors.highlightedColor = new Color(0.35f, 0.35f, 0.35f, 1f);
        colors.pressedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        colors.selectedColor = new Color(0.35f, 0.35f, 0.35f, 1f);
        toggle.colors = colors;

        // Item Checkmark
        GameObject checkmark = new GameObject("Item Checkmark");
        checkmark.transform.SetParent(item.transform, false);

        RectTransform checkRect = checkmark.AddComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0, 0.5f);
        checkRect.anchorMax = new Vector2(0, 0.5f);
        checkRect.pivot = new Vector2(0.5f, 0.5f);
        checkRect.anchoredPosition = new Vector2(10, 0);
        checkRect.sizeDelta = new Vector2(16, 16);

        Image checkImg = checkmark.AddComponent<Image>();
        checkImg.color = Color.white;

        toggle.graphic = checkImg;

        // Item Label
        GameObject itemLabel = new GameObject("Item Label");
        itemLabel.transform.SetParent(item.transform, false);

        RectTransform itemLabelRect = itemLabel.AddComponent<RectTransform>();
        itemLabelRect.anchorMin = Vector2.zero;
        itemLabelRect.anchorMax = Vector2.one;
        itemLabelRect.offsetMin = new Vector2(20, 1);
        itemLabelRect.offsetMax = new Vector2(-10, -2);

        TextMeshProUGUI itemText = itemLabel.AddComponent<TextMeshProUGUI>();
        itemText.fontSize = 18;
        itemText.color = Color.white;
        itemText.alignment = TextAlignmentOptions.MidlineLeft;

        // Create Scrollbar
        GameObject scrollbar = new GameObject("Scrollbar");
        scrollbar.transform.SetParent(template.transform, false);

        RectTransform scrollRect = scrollbar.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(1, 0);
        scrollRect.anchorMax = Vector2.one;
        scrollRect.pivot = Vector2.one;
        scrollRect.sizeDelta = new Vector2(18, 0);

        Image scrollImg = scrollbar.AddComponent<Image>();
        scrollImg.color = new Color(0.1f, 0.1f, 0.1f, 1f);

        UnityEngine.UI.Scrollbar scrollbarComp = scrollbar.AddComponent<UnityEngine.UI.Scrollbar>();
        scrollbarComp.direction = UnityEngine.UI.Scrollbar.Direction.BottomToTop;

        // Scrollbar Sliding Area
        GameObject slidingArea = new GameObject("Sliding Area");
        slidingArea.transform.SetParent(scrollbar.transform, false);

        RectTransform slidingRect = slidingArea.AddComponent<RectTransform>();
        slidingRect.anchorMin = Vector2.zero;
        slidingRect.anchorMax = Vector2.one;
        slidingRect.offsetMin = new Vector2(10, 10);
        slidingRect.offsetMax = new Vector2(-10, -10);

        // Scrollbar Handle
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(slidingArea.transform, false);

        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(18, 18);

        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = new Color(0.5f, 0.5f, 0.5f, 1f);

        scrollbarComp.handleRect = handleRect;
        scrollbarComp.targetGraphic = handleImg;

        // Setup ScrollRect on viewport
        UnityEngine.UI.ScrollRect scrollRectComp = viewport.AddComponent<UnityEngine.UI.ScrollRect>();
        scrollRectComp.content = contentRect;
        scrollRectComp.viewport = viewportRect;
        scrollRectComp.horizontal = false;
        scrollRectComp.vertical = true;
        scrollRectComp.verticalScrollbar = scrollbarComp;

        // Assign template and item text to dropdown
        dropdown.template = templateRect;
        dropdown.itemText = itemText;

        // Hide template by default
        template.SetActive(false);

        return dropdownObj;
    }
}
