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
        GameObject gameVolumeSliderObj = CreateSlider("GameVolumeSlider", panelObj, new Vector2(0, 100), "Game Volume:");
        GameObject musicVolumeSliderObj = CreateSlider("MusicVolumeSlider", panelObj, new Vector2(0, 30), "Music Volume:");
        GameObject dialogVolumeSliderObj = CreateSlider("DialogVolumeSlider", panelObj, new Vector2(0, -40), "Dialog Volume:");

        // Create Close Button
        GameObject closeButtonObj = CreateButton("CloseButton", panelObj, new Vector2(0, -150), "Close");

        // Wire up controller using SerializedObject
        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("optionsPanel").objectReferenceValue = panelObj;
        so.FindProperty("gameVolumeSlider").objectReferenceValue = gameVolumeSliderObj.GetComponent<Slider>();
        so.FindProperty("musicVolumeSlider").objectReferenceValue = musicVolumeSliderObj.GetComponent<Slider>();
        so.FindProperty("dialogVolumeSlider").objectReferenceValue = dialogVolumeSliderObj.GetComponent<Slider>();
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

        Debug.Log("<color=green>Simple GlobalOptionsManager prefab created successfully!</color>");
        Debug.Log($"Prefab location: {prefabPath}");
        Debug.Log("Includes: Game/Music/Dialog volume sliders + Close button");
        Debug.Log("Note: Game Speed and Language dropdowns removed to avoid template issues.");
        Debug.Log("You can add them manually in the Unity Editor if needed.");

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
}
