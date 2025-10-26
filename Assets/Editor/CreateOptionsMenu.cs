using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// Creates a standalone options menu UI that can be used in any scene
/// </summary>
public class CreateOptionsMenu : EditorWindow
{
    [MenuItem("Tools/Create Options Menu UI")]
    public static void CreateOptionsMenuUI()
    {
        // Ask which scene to add to
        int choice = EditorUtility.DisplayDialogComplex("Create Options Menu",
            "Which scene do you want to add the Options Menu to?",
            "MainMenu", "Match3", "Both");

        switch (choice)
        {
            case 0: // MainMenu
                AddOptionsToScene("Assets/Scenes/MainMenu.unity", false);
                break;
            case 1: // Match3
                AddOptionsToScene("Assets/Scenes/Match3.unity", true);
                break;
            case 2: // Both
                AddOptionsToScene("Assets/Scenes/MainMenu.unity", false);
                AddOptionsToScene("Assets/Scenes/Match3.unity", true);
                break;
        }
    }

    private static void AddOptionsToScene(string scenePath, bool pauseWhenOpen)
    {
        // Open the scene
        var scene = EditorSceneManager.OpenScene(scenePath);

        // Find existing Canvas
        Canvas canvas = GameObject.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError($"No Canvas found in {scenePath}!");
            return;
        }

        // Check if OptionsMenuController already exists
        OptionsMenuController existing = GameObject.FindFirstObjectByType<OptionsMenuController>();
        if (existing != null)
        {
            Debug.Log($"OptionsMenuController already exists in {scenePath}. Updating it instead.");

            // Update existing
            SerializedObject so = new SerializedObject(existing);
            so.FindProperty("pauseTimeWhenOpen").boolValue = pauseWhenOpen;
            so.ApplyModifiedProperties();

            // Ensure GlobalOptionsAccess component exists
            // Suppress obsolete warning - this script is for backward compatibility with old system
#pragma warning disable CS0618
            GlobalOptionsAccess globalAccess = existing.GetComponent<GlobalOptionsAccess>();
            if (globalAccess == null)
            {
                Debug.Log($"Adding GlobalOptionsAccess component to existing OptionsMenuController in {scenePath}");
                globalAccess = existing.gameObject.AddComponent<GlobalOptionsAccess>();

                // Wire up the reference
                SerializedObject globalSO = new SerializedObject(globalAccess);
                globalSO.FindProperty("optionsMenuController").objectReferenceValue = existing;
                globalSO.ApplyModifiedProperties();
            }
#pragma warning restore CS0618

            EditorSceneManager.SaveScene(scene);
            return;
        }

        // Create OptionsMenu GameObject with controller and global access
        GameObject optionsMenuObj = new GameObject("OptionsMenuController");
        OptionsMenuController controller = optionsMenuObj.AddComponent<OptionsMenuController>();

        // Suppress obsolete warning - this script is for backward compatibility with old system
#pragma warning disable CS0618
        GlobalOptionsAccess newGlobalAccess = optionsMenuObj.AddComponent<GlobalOptionsAccess>();
#pragma warning restore CS0618

        // Create Options Panel (full screen overlay)
        GameObject panelObj = CreateOptionsPanel(canvas.gameObject);

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
        SerializedObject so2 = new SerializedObject(controller);
        so2.FindProperty("optionsPanel").objectReferenceValue = panelObj;
        so2.FindProperty("gameVolumeSlider").objectReferenceValue = gameVolumeSliderObj.GetComponent<Slider>();
        so2.FindProperty("musicVolumeSlider").objectReferenceValue = musicVolumeSliderObj.GetComponent<Slider>();
        so2.FindProperty("dialogVolumeSlider").objectReferenceValue = dialogVolumeSliderObj.GetComponent<Slider>();
        so2.FindProperty("gameSpeedDropdown").objectReferenceValue = speedDropdownObj.GetComponent<TMP_Dropdown>();
        so2.FindProperty("languageDropdown").objectReferenceValue = langDropdownObj.GetComponent<TMP_Dropdown>();
        so2.FindProperty("closeButton").objectReferenceValue = closeButtonObj.GetComponent<Button>();
        so2.FindProperty("pauseTimeWhenOpen").boolValue = pauseWhenOpen;
        so2.ApplyModifiedProperties();

        // Save scene
        EditorSceneManager.SaveScene(scene);

        Debug.Log($"<color=green>Options menu added to {scenePath}!</color>");
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

        // Create dropdown template (simplified - Unity will create full template)
        GameObject template = new GameObject("Template");
        template.transform.SetParent(dropdownObj.transform, false);
        template.SetActive(false);

        RectTransform templateRect = template.AddComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(0, 0);
        templateRect.anchorMax = new Vector2(1, 0);
        templateRect.pivot = new Vector2(0.5f, 1);
        templateRect.anchoredPosition = new Vector2(0, 0);
        templateRect.sizeDelta = new Vector2(0, 150);

        dropdown.template = templateRect;

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
        fillImage.color = new Color(0.3f, 0.6f, 0.9f, 1f); // Blue fill

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
