using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// Editor tool to create the character selection scene
/// </summary>
public class CharacterSelectionSetup : EditorWindow
{
    [MenuItem("Tools/Create Character Selection Scene")]
    public static void CreateCharacterSelectionScene()
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
        bgImage.color = new Color(0.15f, 0.1f, 0.2f, 1f); // Dark purple
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Create Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Select Your Character";
        titleText.fontSize = 60;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(0.9f, 0.85f, 0.7f, 1f);
        titleText.fontStyle = FontStyles.Bold;

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -80);
        titleRect.sizeDelta = new Vector2(800, 100);

        // Create Character Buttons Container (top)
        GameObject buttonContainer = new GameObject("CharacterButtonsContainer");
        buttonContainer.transform.SetParent(canvasObj.transform, false);
        RectTransform buttonContainerRect = buttonContainer.AddComponent<RectTransform>();
        buttonContainerRect.anchorMin = new Vector2(0.5f, 1f);
        buttonContainerRect.anchorMax = new Vector2(0.5f, 1f);
        buttonContainerRect.anchoredPosition = new Vector2(0, -200);
        buttonContainerRect.sizeDelta = new Vector2(1400, 100);

        // Add horizontal layout group for character buttons
        HorizontalLayoutGroup layoutGroup = buttonContainer.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.spacing = 20;
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = true;

        // Create Content Panel (bottom section)
        GameObject contentPanel = new GameObject("ContentPanel");
        contentPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform contentRect = contentPanel.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.anchoredPosition = new Vector2(0, -80);
        contentRect.sizeDelta = new Vector2(1600, 600);

        // Create Character Image Panel (left side)
        GameObject imagePanel = new GameObject("CharacterImagePanel");
        imagePanel.transform.SetParent(contentPanel.transform, false);
        RectTransform imagePanelRect = imagePanel.AddComponent<RectTransform>();
        imagePanelRect.anchorMin = new Vector2(0f, 0f);
        imagePanelRect.anchorMax = new Vector2(0.5f, 1f);
        imagePanelRect.anchoredPosition = Vector2.zero;
        imagePanelRect.sizeDelta = new Vector2(-20, 0);

        // Add background to image panel
        Image imagePanelBg = imagePanel.AddComponent<Image>();
        imagePanelBg.color = new Color(0.1f, 0.08f, 0.15f, 0.8f);

        // Create Character Image
        GameObject charImageObj = new GameObject("CharacterImage");
        charImageObj.transform.SetParent(imagePanel.transform, false);
        RectTransform charImageRect = charImageObj.AddComponent<RectTransform>();
        charImageRect.anchorMin = new Vector2(0.5f, 0.5f);
        charImageRect.anchorMax = new Vector2(0.5f, 0.5f);
        charImageRect.anchoredPosition = Vector2.zero;
        charImageRect.sizeDelta = new Vector2(400, 500);

        Image charImage = charImageObj.AddComponent<Image>();
        charImage.color = new Color(0.3f, 0.3f, 0.3f, 1f); // Gray placeholder
        charImage.preserveAspect = true;

        // Add placeholder text
        GameObject placeholderObj = new GameObject("PlaceholderText");
        placeholderObj.transform.SetParent(charImageObj.transform, false);
        TextMeshProUGUI placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
        placeholderText.text = "Character\nPortrait";
        placeholderText.fontSize = 32;
        placeholderText.alignment = TextAlignmentOptions.Center;
        placeholderText.color = new Color(0.6f, 0.6f, 0.6f, 1f);

        RectTransform placeholderRect = placeholderObj.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.sizeDelta = Vector2.zero;

        // Create Attributes Panel (right side)
        GameObject attributesPanel = new GameObject("AttributesPanel");
        attributesPanel.transform.SetParent(contentPanel.transform, false);
        RectTransform attrPanelRect = attributesPanel.AddComponent<RectTransform>();
        attrPanelRect.anchorMin = new Vector2(0.5f, 0f);
        attrPanelRect.anchorMax = new Vector2(1f, 1f);
        attrPanelRect.anchoredPosition = Vector2.zero;
        attrPanelRect.sizeDelta = new Vector2(-20, 0);

        // Add background to attributes panel
        Image attrPanelBg = attributesPanel.AddComponent<Image>();
        attrPanelBg.color = new Color(0.1f, 0.08f, 0.15f, 0.8f);

        // Create Attributes Text
        GameObject attrTextObj = new GameObject("AttributesText");
        attrTextObj.transform.SetParent(attributesPanel.transform, false);
        TextMeshProUGUI attrText = attrTextObj.AddComponent<TextMeshProUGUI>();
        attrText.text = "Select a character to view attributes";
        attrText.fontSize = 24;
        attrText.alignment = TextAlignmentOptions.TopLeft;
        attrText.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        RectTransform attrTextRect = attrTextObj.GetComponent<RectTransform>();
        attrTextRect.anchorMin = Vector2.zero;
        attrTextRect.anchorMax = Vector2.one;
        attrTextRect.sizeDelta = new Vector2(-40, -40);
        attrTextRect.anchoredPosition = Vector2.zero;

        // Create Confirm Button
        GameObject confirmBtn = CreateButton("ConfirmButton", "Select Character", canvasObj, new Vector2(200, -500), new Vector2(250, 70));
        Image confirmBtnImage = confirmBtn.GetComponent<Image>();
        confirmBtnImage.color = new Color(0.2f, 0.6f, 0.3f, 1f); // Green

        // Create Back Button
        GameObject backBtn = CreateButton("BackButton", "Back to Menu", canvasObj, new Vector2(-200, -500), new Vector2(250, 70));
        Image backBtnImage = backBtn.GetComponent<Image>();
        backBtnImage.color = new Color(0.6f, 0.2f, 0.2f, 1f); // Red

        // Create CharacterSelectionManager
        GameObject managerObj = new GameObject("CharacterSelectionManager");
        CharacterSelectionManager manager = managerObj.AddComponent<CharacterSelectionManager>();

        // Wire up references
        SerializedObject so = new SerializedObject(manager);
        so.FindProperty("characterButtonContainer").objectReferenceValue = buttonContainer.transform;
        so.FindProperty("characterImage").objectReferenceValue = charImage;
        so.FindProperty("attributesText").objectReferenceValue = attrText;
        so.FindProperty("confirmButton").objectReferenceValue = confirmBtn.GetComponent<Button>();
        so.FindProperty("backButton").objectReferenceValue = backBtn.GetComponent<Button>();
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
        camera.backgroundColor = new Color(0.15f, 0.1f, 0.2f, 1f);
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
        string scenePath = "Assets/Scenes/CharacterSelection.unity";
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(newScene, scenePath);

        Debug.Log($"Character selection scene created at {scenePath}");
        EditorUtility.DisplayDialog("Character Selection Scene Created",
            "Character selection scene created successfully!\n\n" +
            "Next steps:\n" +
            "1. Create character button prefab: Tools > Character Selection > Create Character Button Prefab\n" +
            "2. Create character data assets: Tools > Character Selection > Create Default Characters\n" +
            "3. Add CharacterSelection to Build Settings",
            "OK");
    }

    [MenuItem("Tools/Character Selection/Create Character Button Prefab")]
    public static void CreateCharacterButtonPrefab()
    {
        GameObject buttonObj = new GameObject("CharacterButton");

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(150, 180); // Taller to fit image + text

        Button button = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.25f, 0.3f, 1f);

        // Set button colors
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.25f, 0.3f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.35f, 0.4f, 1f);
        colors.pressedColor = new Color(0.15f, 0.2f, 0.25f, 1f);
        colors.selectedColor = new Color(0.4f, 0.6f, 0.8f, 1f);
        button.colors = colors;

        // Create image placeholder (top)
        GameObject imageObj = new GameObject("CharacterImage");
        imageObj.transform.SetParent(buttonObj.transform, false);

        RectTransform imageRect = imageObj.AddComponent<RectTransform>();
        imageRect.anchorMin = new Vector2(0.5f, 1f);
        imageRect.anchorMax = new Vector2(0.5f, 1f);
        imageRect.anchoredPosition = new Vector2(0, -70); // Position from top
        imageRect.sizeDelta = new Vector2(120, 120);

        Image charImage = imageObj.AddComponent<Image>();
        charImage.color = new Color(0.4f, 0.4f, 0.4f, 1f); // Gray placeholder
        charImage.preserveAspect = true;

        // Add "Portrait" text on the placeholder
        GameObject portraitTextObj = new GameObject("PortraitText");
        portraitTextObj.transform.SetParent(imageObj.transform, false);

        TextMeshProUGUI portraitText = portraitTextObj.AddComponent<TextMeshProUGUI>();
        portraitText.text = "Portrait";
        portraitText.fontSize = 14;
        portraitText.alignment = TextAlignmentOptions.Center;
        portraitText.color = new Color(0.7f, 0.7f, 0.7f, 1f);

        RectTransform portraitTextRect = portraitTextObj.GetComponent<RectTransform>();
        portraitTextRect.anchorMin = Vector2.zero;
        portraitTextRect.anchorMax = Vector2.one;
        portraitTextRect.sizeDelta = Vector2.zero;

        // Create text child (bottom - character name)
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "Character";
        text.fontSize = 20;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.fontStyle = FontStyles.Bold;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(1f, 0f);
        textRect.anchoredPosition = new Vector2(0, 20); // Position from bottom
        textRect.sizeDelta = new Vector2(0, 40);

        // Save as prefab
        string path = "Assets/Prefabs/UI/CharacterButton.prefab";
        System.IO.Directory.CreateDirectory("Assets/Prefabs/UI");
        PrefabUtility.SaveAsPrefabAsset(buttonObj, path);
        DestroyImmediate(buttonObj);

        // Try to wire up to CharacterSelectionManager
        CharacterSelectionManager manager = GameObject.FindFirstObjectByType<CharacterSelectionManager>();
        if (manager != null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            SerializedObject so = new SerializedObject(manager);
            so.FindProperty("characterButtonPrefab").objectReferenceValue = prefab;
            so.ApplyModifiedProperties();
        }

        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        EditorUtility.DisplayDialog("Success", $"Character button prefab created at:\n{path}", "OK");
    }

    [MenuItem("Tools/Character Selection/Create Default Characters")]
    public static void CreateDefaultCharacters()
    {
        System.IO.Directory.CreateDirectory("Assets/Resources/Characters");

        // Knight (default)
        CreateCharacter("Knight", "A noble warrior skilled in combat and defense",
            strength: 16, dexterity: 10, constitution: 14, intelligence: 8, wisdom: 10, charisma: 12,
            health: 120, mana: 40, moveSpeed: 5f,
            new string[] { "Shield Bash", "Defensive Stance", "Rally Cry" });

        // Barbarian
        CreateCharacter("Barbarian", "A fierce warrior who channels rage in battle",
            strength: 18, dexterity: 12, constitution: 16, intelligence: 6, wisdom: 8, charisma: 8,
            health: 140, mana: 30, moveSpeed: 5.5f,
            new string[] { "Rage", "Reckless Attack", "Danger Sense" });

        // Wizard
        CreateCharacter("Wizard", "A master of arcane magic and powerful spells",
            strength: 6, dexterity: 10, constitution: 10, intelligence: 18, wisdom: 14, charisma: 10,
            health: 80, mana: 150, moveSpeed: 4.5f,
            new string[] { "Fireball", "Arcane Shield", "Teleport" });

        // Rogue
        CreateCharacter("Rogue", "A nimble and cunning expert in stealth and precision",
            strength: 10, dexterity: 18, constitution: 10, intelligence: 12, wisdom: 12, charisma: 14,
            health: 90, mana: 60, moveSpeed: 6f,
            new string[] { "Sneak Attack", "Evasion", "Cunning Action" });

        // Cleric
        CreateCharacter("Cleric", "A divine spellcaster who heals and protects allies",
            strength: 12, dexterity: 8, constitution: 12, intelligence: 10, wisdom: 16, charisma: 14,
            health: 100, mana: 120, moveSpeed: 5f,
            new string[] { "Healing Word", "Turn Undead", "Divine Strike" });

        // Ranger
        CreateCharacter("Ranger", "A skilled archer and tracker, master of wilderness survival",
            strength: 12, dexterity: 16, constitution: 12, intelligence: 10, wisdom: 14, charisma: 10,
            health: 100, mana: 70, moveSpeed: 5.5f,
            new string[] { "Hunter's Mark", "Favored Enemy", "Natural Explorer" });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success",
            "Created 6 default character archetypes:\n\n" +
            "- Knight (default)\n" +
            "- Barbarian\n" +
            "- Wizard\n" +
            "- Rogue\n" +
            "- Cleric\n" +
            "- Ranger\n\n" +
            "Assets saved to: Assets/Resources/Characters/\n\n" +
            "These need to be manually assigned to the CharacterSelectionManager's 'Available Characters' list.",
            "OK");

        // Select the characters folder
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>("Assets/Resources/Characters");
    }

    private static void CreateCharacter(string name, string description,
        int strength, int dexterity, int constitution, int intelligence, int wisdom, int charisma,
        int health, int mana, float moveSpeed, string[] features)
    {
        CharacterData character = ScriptableObject.CreateInstance<CharacterData>();
        character.characterName = name;
        character.description = description;
        character.strength = strength;
        character.dexterity = dexterity;
        character.constitution = constitution;
        character.intelligence = intelligence;
        character.wisdom = wisdom;
        character.charisma = charisma;
        character.baseHealth = health;
        character.baseMana = mana;
        character.moveSpeed = moveSpeed;
        character.classFeatures = features;

        string path = $"Assets/Resources/Characters/{name}.asset";
        AssetDatabase.CreateAsset(character, path);
        Debug.Log($"Created character: {name} at {path}");
    }

    private static GameObject CreateButton(string name, string text, GameObject parent, Vector2 position, Vector2 size)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent.transform, false);

        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = position;
        btnRect.sizeDelta = size;

        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.25f, 0.3f, 1f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.2f, 0.25f, 0.3f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.35f, 0.4f, 1f);
        colors.pressedColor = new Color(0.15f, 0.2f, 0.25f, 1f);
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
