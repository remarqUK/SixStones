using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering.Universal;
using UnityEditor.Events;

public class Match3Setup : EditorWindow
{
    [MenuItem("Tools/Setup Match 3 Game")]
    public static void SetupMatch3Scene()
    {
        // Ensure URP is assigned in Graphics settings
        UniversalRenderPipelineAsset urpAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/UniversalRP.asset");
        if (urpAsset != null)
        {
            UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline = urpAsset;
            Debug.Log("URP asset assigned to Graphics settings");
        }
        else
        {
            Debug.LogError("Could not find URP asset at Assets/Settings/UniversalRP.asset");
        }

        // Ensure required directories exist
        if (!System.IO.Directory.Exists("Assets/Scenes"))
        {
            System.IO.Directory.CreateDirectory("Assets/Scenes");
        }
        if (!System.IO.Directory.Exists("Assets/Prefabs"))
        {
            System.IO.Directory.CreateDirectory("Assets/Prefabs");
        }
        if (!System.IO.Directory.Exists("Assets/Sprites"))
        {
            System.IO.Directory.CreateDirectory("Assets/Sprites");
        }

        // Create or load the Match3 scene
        Scene currentScene = EditorSceneManager.GetActiveScene();
        if (currentScene.name != "Match3")
        {
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(newScene, "Assets/Scenes/Match3.unity");
        }

        // Clear existing objects (only root objects, as children will be destroyed with parents)
        GameObject[] allObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject obj in allObjects)
        {
            DestroyImmediate(obj);
        }

        // Create Camera
        GameObject cameraObj = new GameObject("Main Camera");
        Camera camera = cameraObj.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
        camera.orthographic = true;
        camera.orthographicSize = 6f;
        cameraObj.tag = "MainCamera";
        cameraObj.transform.position = new Vector3(0, 0, -10);

        // Add URP Additional Camera Data component (required for URP)
        UniversalAdditionalCameraData cameraData = cameraObj.AddComponent<UniversalAdditionalCameraData>();
        cameraData.renderType = CameraRenderType.Base; // Set as base camera
        cameraData.renderShadows = false; // 2D game doesn't need shadows
        cameraData.requiresColorOption = CameraOverrideOption.Off;
        cameraData.requiresDepthOption = CameraOverrideOption.Off;
        cameraData.renderPostProcessing = false; // Disable post-processing for 2D

        // Load and assign the 2D renderer
        ScriptableRendererData renderer2D = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>("Assets/Settings/Renderer2D.asset");
        if (renderer2D != null)
        {
            cameraData.SetRenderer(0); // Use first renderer from URP settings
        }

        // Ensure camera is enabled
        camera.enabled = true;
        camera.targetDisplay = 0; // Render to main display

        // Create prefabs
        GameObject piecePrefab = CreateGamePiecePrefab();
        GameObject cellPrefab = CreateCellBackgroundPrefab();

        // Create Board
        GameObject boardObj = new GameObject("Board");
        Board board = boardObj.AddComponent<Board>();

        // Use reflection to set private fields (since they're serialized)
        SerializedObject boardSO = new SerializedObject(board);
        boardSO.FindProperty("gamePiecePrefab").objectReferenceValue = piecePrefab;
        boardSO.FindProperty("cellBackgroundPrefab").objectReferenceValue = cellPrefab;
        boardSO.FindProperty("width").intValue = 8;
        boardSO.FindProperty("height").intValue = 8;
        boardSO.FindProperty("cellSize").floatValue = 1f;
        boardSO.FindProperty("swapDuration").floatValue = 0.2f;
        boardSO.FindProperty("fallDuration").floatValue = 0.3f;

        // Load and assign Standard mode by default
        GameModeData standardMode = AssetDatabase.LoadAssetAtPath<GameModeData>("Assets/Resources/GameModes/StandardMode.asset");
        if (standardMode != null)
        {
            boardSO.FindProperty("currentMode").objectReferenceValue = standardMode;
        }

        boardSO.ApplyModifiedProperties();

        // Create Player Manager
        GameObject playerManagerObj = new GameObject("PlayerManager");
        PlayerManager playerManager = playerManagerObj.AddComponent<PlayerManager>();

        // Create CPU Player
        GameObject cpuPlayerObj = new GameObject("CPUPlayer");
        CPUPlayer cpuPlayer = cpuPlayerObj.AddComponent<CPUPlayer>();

        SerializedObject cpuSO = new SerializedObject(cpuPlayer);
        cpuSO.FindProperty("board").objectReferenceValue = board;
        cpuSO.FindProperty("playerManager").objectReferenceValue = playerManager;
        cpuSO.ApplyModifiedProperties();

        // Create Hint System
        GameObject hintSystemObj = new GameObject("HintSystem");
        HintSystem hintSystem = hintSystemObj.AddComponent<HintSystem>();

        SerializedObject hintSO = new SerializedObject(hintSystem);
        hintSO.FindProperty("board").objectReferenceValue = board;
        hintSO.ApplyModifiedProperties();

        // Create Gem Selector (before InputManager so we can reference it)
        GameObject gemSelectorObj = new GameObject("GemSelector");
        GemSelector gemSelector = gemSelectorObj.AddComponent<GemSelector>();

        SerializedObject gemSelectorSO = new SerializedObject(gemSelector);
        gemSelectorSO.FindProperty("board").objectReferenceValue = board;
        gemSelectorSO.FindProperty("playerManager").objectReferenceValue = playerManager;
        // gameManager will be wired up later after it's created
        gemSelectorSO.ApplyModifiedProperties();

        // Create Input Manager
        GameObject inputObj = new GameObject("InputManager");
        InputManager inputManager = inputObj.AddComponent<InputManager>();

        SerializedObject inputSO = new SerializedObject(inputManager);
        inputSO.FindProperty("board").objectReferenceValue = board;
        inputSO.FindProperty("mainCamera").objectReferenceValue = camera;
        inputSO.FindProperty("gemSelector").objectReferenceValue = gemSelector;
        inputSO.FindProperty("playerManager").objectReferenceValue = playerManager;
        inputSO.ApplyModifiedProperties();

        // Create Canvas for UI
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Create EventSystem (required for UI interaction)
        GameObject eventSystemObj = new GameObject("EventSystem");
        eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        // Create Score Text (Player 1 - left side)
        GameObject scoreObj = new GameObject("ScoreText");
        scoreObj.transform.SetParent(canvasObj.transform);
        RectTransform scoreRect = scoreObj.AddComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0, 1);
        scoreRect.anchorMax = new Vector2(0, 1);
        scoreRect.pivot = new Vector2(0, 1);
        scoreRect.anchoredPosition = new Vector2(20, -110);
        scoreRect.sizeDelta = new Vector2(300, 60);

        TMPro.TextMeshProUGUI scoreText = scoreObj.AddComponent<TMPro.TextMeshProUGUI>();
        scoreText.text = "Score: 0";
        scoreText.fontSize = 36;
        scoreText.color = Color.white;
        scoreText.alignment = TMPro.TextAlignmentOptions.Left;

        // Create Level Text (Player 1 - left side)
        GameObject levelObj = new GameObject("LevelText");
        levelObj.transform.SetParent(canvasObj.transform);
        RectTransform levelRect = levelObj.AddComponent<RectTransform>();
        levelRect.anchorMin = new Vector2(0, 1);
        levelRect.anchorMax = new Vector2(0, 1);
        levelRect.pivot = new Vector2(0, 1);
        levelRect.anchoredPosition = new Vector2(20, -180);
        levelRect.sizeDelta = new Vector2(300, 40);

        TMPro.TextMeshProUGUI levelText = levelObj.AddComponent<TMPro.TextMeshProUGUI>();
        levelText.text = "Level: 1";
        levelText.fontSize = 28;
        levelText.color = new Color(1f, 0.9f, 0.3f); // Yellow
        levelText.alignment = TMPro.TextAlignmentOptions.Left;

        // Create XP Text (Player 1 - left side)
        GameObject xpObj = new GameObject("XPText");
        xpObj.transform.SetParent(canvasObj.transform);
        RectTransform xpRect = xpObj.AddComponent<RectTransform>();
        xpRect.anchorMin = new Vector2(0, 1);
        xpRect.anchorMax = new Vector2(0, 1);
        xpRect.pivot = new Vector2(0, 1);
        xpRect.anchoredPosition = new Vector2(20, -220);
        xpRect.sizeDelta = new Vector2(300, 40);

        TMPro.TextMeshProUGUI xpText = xpObj.AddComponent<TMPro.TextMeshProUGUI>();
        xpText.text = "XP: 0/100";
        xpText.fontSize = 24;
        xpText.color = new Color(0.5f, 1f, 0.5f); // Light green
        xpText.alignment = TMPro.TextAlignmentOptions.Left;

        // Create Gold Text (Player 1 - left side)
        GameObject goldObj = new GameObject("GoldText");
        goldObj.transform.SetParent(canvasObj.transform);
        RectTransform goldRect = goldObj.AddComponent<RectTransform>();
        goldRect.anchorMin = new Vector2(0, 1);
        goldRect.anchorMax = new Vector2(0, 1);
        goldRect.pivot = new Vector2(0, 1);
        goldRect.anchoredPosition = new Vector2(20, -260);
        goldRect.sizeDelta = new Vector2(300, 40);

        TMPro.TextMeshProUGUI goldText = goldObj.AddComponent<TMPro.TextMeshProUGUI>();
        goldText.text = "Gold: 100";
        goldText.fontSize = 24;
        goldText.color = new Color(1f, 0.84f, 0f); // Gold color
        goldText.alignment = TMPro.TextAlignmentOptions.Left;

        // Create Player 1 Color Scores Text (bottom left)
        GameObject player1ColorScoresObj = new GameObject("Player1ColorScoresText");
        player1ColorScoresObj.transform.SetParent(canvasObj.transform);
        RectTransform player1ColorScoresRect = player1ColorScoresObj.AddComponent<RectTransform>();
        player1ColorScoresRect.anchorMin = new Vector2(0, 0);
        player1ColorScoresRect.anchorMax = new Vector2(0, 0);
        player1ColorScoresRect.pivot = new Vector2(0, 0);
        player1ColorScoresRect.anchoredPosition = new Vector2(20, 20);
        player1ColorScoresRect.sizeDelta = new Vector2(250, 300);

        TMPro.TextMeshProUGUI player1ColorScoresText = player1ColorScoresObj.AddComponent<TMPro.TextMeshProUGUI>();
        player1ColorScoresText.text = "<b>Player 1 Colors:</b>\nRed: 0\nBlue: 0\nGreen: 0\nYellow: 0\nPurple: 0\nOrange: 0\nWhite: 0";
        player1ColorScoresText.fontSize = 24;
        player1ColorScoresText.color = new Color(0.5f, 0.8f, 1f); // Light blue
        player1ColorScoresText.alignment = TMPro.TextAlignmentOptions.TopLeft;
        player1ColorScoresText.textWrappingMode = TMPro.TextWrappingModes.NoWrap;

        // Create Player 2 Color Scores Text (bottom right)
        GameObject player2ColorScoresObj = new GameObject("Player2ColorScoresText");
        player2ColorScoresObj.transform.SetParent(canvasObj.transform);
        RectTransform player2ColorScoresRect = player2ColorScoresObj.AddComponent<RectTransform>();
        player2ColorScoresRect.anchorMin = new Vector2(1, 0);
        player2ColorScoresRect.anchorMax = new Vector2(1, 0);
        player2ColorScoresRect.pivot = new Vector2(1, 0);
        player2ColorScoresRect.anchoredPosition = new Vector2(-20, 20);
        player2ColorScoresRect.sizeDelta = new Vector2(250, 300);

        TMPro.TextMeshProUGUI player2ColorScoresText = player2ColorScoresObj.AddComponent<TMPro.TextMeshProUGUI>();
        player2ColorScoresText.text = "<b>Player 2 Colors:</b>\nRed: 0\nBlue: 0\nGreen: 0\nYellow: 0\nPurple: 0\nOrange: 0\nWhite: 0";
        player2ColorScoresText.fontSize = 24;
        player2ColorScoresText.color = new Color(1f, 0.5f, 0.5f); // Light red
        player2ColorScoresText.alignment = TMPro.TextAlignmentOptions.TopRight;
        player2ColorScoresText.textWrappingMode = TMPro.TextWrappingModes.NoWrap;

        // Create Current Player Text (top center)
        GameObject currentPlayerObj = new GameObject("CurrentPlayerText");
        currentPlayerObj.transform.SetParent(canvasObj.transform);
        RectTransform currentPlayerRect = currentPlayerObj.AddComponent<RectTransform>();
        currentPlayerRect.anchorMin = new Vector2(0.5f, 1);
        currentPlayerRect.anchorMax = new Vector2(0.5f, 1);
        currentPlayerRect.pivot = new Vector2(0.5f, 1);
        currentPlayerRect.anchoredPosition = new Vector2(0, -20);
        currentPlayerRect.sizeDelta = new Vector2(400, 60);

        TMPro.TextMeshProUGUI currentPlayerText = currentPlayerObj.AddComponent<TMPro.TextMeshProUGUI>();
        currentPlayerText.text = "Current: Player 1";
        currentPlayerText.fontSize = 32;
        currentPlayerText.color = new Color(1f, 0.9f, 0.3f); // Yellow-ish
        currentPlayerText.alignment = TMPro.TextAlignmentOptions.Top;
        currentPlayerText.fontStyle = TMPro.FontStyles.Bold;

        // Create Player 1 Score Text
        GameObject player1ScoreObj = new GameObject("Player1ScoreText");
        player1ScoreObj.transform.SetParent(canvasObj.transform);
        RectTransform player1ScoreRect = player1ScoreObj.AddComponent<RectTransform>();
        player1ScoreRect.anchorMin = new Vector2(0, 1);
        player1ScoreRect.anchorMax = new Vector2(0, 1);
        player1ScoreRect.pivot = new Vector2(0, 1);
        player1ScoreRect.anchoredPosition = new Vector2(20, -20);
        player1ScoreRect.sizeDelta = new Vector2(300, 50);

        TMPro.TextMeshProUGUI player1ScoreText = player1ScoreObj.AddComponent<TMPro.TextMeshProUGUI>();
        player1ScoreText.text = "Player 1: 0";
        player1ScoreText.fontSize = 28;
        player1ScoreText.color = new Color(0.5f, 0.8f, 1f); // Light blue
        player1ScoreText.alignment = TMPro.TextAlignmentOptions.Left;

        // Create Player 2 Score Text
        GameObject player2ScoreObj = new GameObject("Player2ScoreText");
        player2ScoreObj.transform.SetParent(canvasObj.transform);
        RectTransform player2ScoreRect = player2ScoreObj.AddComponent<RectTransform>();
        player2ScoreRect.anchorMin = new Vector2(1, 1);
        player2ScoreRect.anchorMax = new Vector2(1, 1);
        player2ScoreRect.pivot = new Vector2(1, 1);
        player2ScoreRect.anchoredPosition = new Vector2(-20, -20);
        player2ScoreRect.sizeDelta = new Vector2(300, 50);

        TMPro.TextMeshProUGUI player2ScoreText = player2ScoreObj.AddComponent<TMPro.TextMeshProUGUI>();
        player2ScoreText.text = "Player 2: 0";
        player2ScoreText.fontSize = 28;
        player2ScoreText.color = new Color(1f, 0.5f, 0.5f); // Light red
        player2ScoreText.alignment = TMPro.TextAlignmentOptions.Right;

        // Create Player 1 Health Text
        GameObject player1HealthObj = new GameObject("Player1HealthText");
        player1HealthObj.transform.SetParent(canvasObj.transform);
        RectTransform player1HealthRect = player1HealthObj.AddComponent<RectTransform>();
        player1HealthRect.anchorMin = new Vector2(0, 1);
        player1HealthRect.anchorMax = new Vector2(0, 1);
        player1HealthRect.pivot = new Vector2(0, 1);
        player1HealthRect.anchoredPosition = new Vector2(20, -300);
        player1HealthRect.sizeDelta = new Vector2(300, 50);

        TMPro.TextMeshProUGUI player1HealthText = player1HealthObj.AddComponent<TMPro.TextMeshProUGUI>();
        player1HealthText.text = "HP: 100";
        player1HealthText.fontSize = 32;
        player1HealthText.color = new Color(0.3f, 1f, 0.3f); // Bright green
        player1HealthText.alignment = TMPro.TextAlignmentOptions.Left;
        player1HealthText.fontStyle = TMPro.FontStyles.Bold;

        // Create Player 2 Health Text
        GameObject player2HealthObj = new GameObject("Player2HealthText");
        player2HealthObj.transform.SetParent(canvasObj.transform);
        RectTransform player2HealthRect = player2HealthObj.AddComponent<RectTransform>();
        player2HealthRect.anchorMin = new Vector2(1, 1);
        player2HealthRect.anchorMax = new Vector2(1, 1);
        player2HealthRect.pivot = new Vector2(1, 1);
        player2HealthRect.anchoredPosition = new Vector2(-20, -60);
        player2HealthRect.sizeDelta = new Vector2(300, 50);

        TMPro.TextMeshProUGUI player2HealthText = player2HealthObj.AddComponent<TMPro.TextMeshProUGUI>();
        player2HealthText.text = "HP: 100";
        player2HealthText.fontSize = 32;
        player2HealthText.color = new Color(0.3f, 1f, 0.3f); // Bright green
        player2HealthText.alignment = TMPro.TextAlignmentOptions.Right;
        player2HealthText.fontStyle = TMPro.FontStyles.Bold;

        // Create Game Manager
        GameObject gameManagerObj = new GameObject("GameManager");
        GameManager gameManager = gameManagerObj.AddComponent<GameManager>();

        SerializedObject gmSO = new SerializedObject(gameManager);
        gmSO.FindProperty("board").objectReferenceValue = board;
        gmSO.FindProperty("playerManager").objectReferenceValue = playerManager;
        gmSO.FindProperty("scoreText").objectReferenceValue = scoreText;
        gmSO.FindProperty("player1ColorScoresText").objectReferenceValue = player1ColorScoresText;
        gmSO.FindProperty("player2ColorScoresText").objectReferenceValue = player2ColorScoresText;
        gmSO.FindProperty("currentPlayerText").objectReferenceValue = currentPlayerText;
        gmSO.FindProperty("player1ScoreText").objectReferenceValue = player1ScoreText;
        gmSO.FindProperty("player2ScoreText").objectReferenceValue = player2ScoreText;
        gmSO.FindProperty("player1HealthText").objectReferenceValue = player1HealthText;
        gmSO.FindProperty("player2HealthText").objectReferenceValue = player2HealthText;
        gmSO.FindProperty("levelText").objectReferenceValue = levelText;
        gmSO.FindProperty("xpText").objectReferenceValue = xpText;
        gmSO.FindProperty("goldText").objectReferenceValue = goldText;
        gmSO.FindProperty("pointsPerPiece").intValue = 1;
        gmSO.FindProperty("maxMoves").intValue = 30;
        gmSO.FindProperty("startingHealth").intValue = 100;
        gmSO.FindProperty("player1Strength").floatValue = 1.0f;
        gmSO.FindProperty("player2Strength").floatValue = 1.0f;
        gmSO.ApplyModifiedProperties();

        // Create GameSpeedSettings
        GameObject gameSpeedSettingsObj = new GameObject("GameSpeedSettings");
        GameSpeedSettings gameSpeedSettings = gameSpeedSettingsObj.AddComponent<GameSpeedSettings>();

        // Create LevelSystem
        GameObject levelSystemObj = new GameObject("LevelSystem");
        LevelSystem levelSystem = levelSystemObj.AddComponent<LevelSystem>();

        // Create CurrencyManager
        GameObject currencyManagerObj = new GameObject("CurrencyManager");
        CurrencyManager currencyManager = currencyManagerObj.AddComponent<CurrencyManager>();

        // Create GemTypeManager
        GameObject gemTypeManagerObj = new GameObject("GemTypeManager");
        GemTypeManager gemTypeManager = gemTypeManagerObj.AddComponent<GemTypeManager>();

        // Load gem types from Assets/GemTypes folder
        string[] gemTypeGuids = AssetDatabase.FindAssets("t:GemTypeData", new[] { "Assets/GemTypes" });
        SerializedObject gemTypeSO = new SerializedObject(gemTypeManager);
        SerializedProperty gemTypesArray = gemTypeSO.FindProperty("gemTypes");
        gemTypesArray.ClearArray();

        foreach (string guid in gemTypeGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GemTypeData gemTypeData = AssetDatabase.LoadAssetAtPath<GemTypeData>(path);
            if (gemTypeData != null)
            {
                gemTypesArray.InsertArrayElementAtIndex(gemTypesArray.arraySize);
                gemTypesArray.GetArrayElementAtIndex(gemTypesArray.arraySize - 1).objectReferenceValue = gemTypeData;
            }
        }
        gemTypeSO.ApplyModifiedProperties();

        // Create SpellManager
        GameObject spellManagerObj = new GameObject("SpellManager");
        SpellManager spellManager = spellManagerObj.AddComponent<SpellManager>();

        // Create SpellEffectHandler
        GameObject spellEffectHandlerObj = new GameObject("SpellEffectHandler");
        SpellEffectHandler spellEffectHandler = spellEffectHandlerObj.AddComponent<SpellEffectHandler>();

        SerializedObject spellEffectSO = new SerializedObject(spellEffectHandler);
        spellEffectSO.FindProperty("gameManager").objectReferenceValue = gameManager;
        spellEffectSO.FindProperty("board").objectReferenceValue = board;
        spellEffectSO.FindProperty("playerManager").objectReferenceValue = playerManager;
        spellEffectSO.ApplyModifiedProperties();

        // Create StatusEffectManager
        GameObject statusEffectManagerObj = new GameObject("StatusEffectManager");
        StatusEffectManager statusEffectManager = statusEffectManagerObj.AddComponent<StatusEffectManager>();

        SerializedObject statusEffectSO = new SerializedObject(statusEffectManager);
        statusEffectSO.FindProperty("gameManager").objectReferenceValue = gameManager;
        statusEffectSO.FindProperty("playerManager").objectReferenceValue = playerManager;
        statusEffectSO.ApplyModifiedProperties();

        // Connect GameManager, PlayerManager, and CPUPlayer to Board
        boardSO.FindProperty("gameManager").objectReferenceValue = gameManager;
        boardSO.FindProperty("playerManager").objectReferenceValue = playerManager;
        boardSO.FindProperty("cpuPlayer").objectReferenceValue = cpuPlayer;
        boardSO.ApplyModifiedProperties();

        // Wire up GameManager to GemSelector (now that it exists)
        gemSelectorSO.Update();
        gemSelectorSO.FindProperty("gameManager").objectReferenceValue = gameManager;
        gemSelectorSO.ApplyModifiedProperties();

        // Create CPU Control UI
        GameObject cpuControlObj = new GameObject("CPUControlUI");
        CPUControlUI cpuControlUI = cpuControlObj.AddComponent<CPUControlUI>();

        SerializedObject cpuControlSO = new SerializedObject(cpuControlUI);
        cpuControlSO.FindProperty("cpuPlayer").objectReferenceValue = cpuPlayer;

        // Create Auto-Play Toggle Button
        GameObject autoPlayButtonObj = new GameObject("AutoPlayButton");
        autoPlayButtonObj.transform.SetParent(canvasObj.transform);
        RectTransform autoPlayButtonRect = autoPlayButtonObj.AddComponent<RectTransform>();
        autoPlayButtonRect.anchorMin = new Vector2(0.5f, 0);
        autoPlayButtonRect.anchorMax = new Vector2(0.5f, 0);
        autoPlayButtonRect.pivot = new Vector2(0.5f, 0);
        autoPlayButtonRect.anchoredPosition = new Vector2(-80, 20);
        autoPlayButtonRect.sizeDelta = new Vector2(150, 40);

        UnityEngine.UI.Image autoPlayButtonImage = autoPlayButtonObj.AddComponent<UnityEngine.UI.Image>();
        autoPlayButtonImage.color = new Color(0.2f, 0.3f, 0.8f);

        UnityEngine.UI.Button autoPlayButton = autoPlayButtonObj.AddComponent<UnityEngine.UI.Button>();
        UnityEngine.UI.ColorBlock autoPlayColors = autoPlayButton.colors;
        autoPlayColors.normalColor = new Color(0.2f, 0.3f, 0.8f);
        autoPlayColors.highlightedColor = new Color(0.3f, 0.4f, 0.9f);
        autoPlayColors.pressedColor = new Color(0.1f, 0.2f, 0.7f);
        autoPlayButton.colors = autoPlayColors;

        // Add button text
        GameObject autoPlayTextObj = new GameObject("Text");
        autoPlayTextObj.transform.SetParent(autoPlayButtonObj.transform);
        RectTransform autoPlayTextRect = autoPlayTextObj.AddComponent<RectTransform>();
        autoPlayTextRect.anchorMin = Vector2.zero;
        autoPlayTextRect.anchorMax = Vector2.one;
        autoPlayTextRect.sizeDelta = Vector2.zero;
        autoPlayTextRect.anchoredPosition = Vector2.zero;

        TMPro.TextMeshProUGUI autoPlayButtonText = autoPlayTextObj.AddComponent<TMPro.TextMeshProUGUI>();
        autoPlayButtonText.text = "Auto-Play: ON";
        autoPlayButtonText.fontSize = 16;
        autoPlayButtonText.color = Color.white;
        autoPlayButtonText.alignment = TMPro.TextAlignmentOptions.Center;

        // Wire up button click using UnityEventTools
        UnityEventTools.AddPersistentListener(autoPlayButton.onClick, cpuControlUI.OnToggleAutoPlay);
        EditorUtility.SetDirty(autoPlayButtonObj);

        // Create Manual Move Button
        GameObject manualMoveButtonObj = new GameObject("ManualMoveButton");
        manualMoveButtonObj.transform.SetParent(canvasObj.transform);
        RectTransform manualMoveButtonRect = manualMoveButtonObj.AddComponent<RectTransform>();
        manualMoveButtonRect.anchorMin = new Vector2(0.5f, 0);
        manualMoveButtonRect.anchorMax = new Vector2(0.5f, 0);
        manualMoveButtonRect.pivot = new Vector2(0.5f, 0);
        manualMoveButtonRect.anchoredPosition = new Vector2(80, 20);
        manualMoveButtonRect.sizeDelta = new Vector2(150, 40);

        UnityEngine.UI.Image manualMoveButtonImage = manualMoveButtonObj.AddComponent<UnityEngine.UI.Image>();
        manualMoveButtonImage.color = new Color(0.3f, 0.7f, 0.3f);

        UnityEngine.UI.Button manualMoveButton = manualMoveButtonObj.AddComponent<UnityEngine.UI.Button>();
        UnityEngine.UI.ColorBlock manualMoveColors = manualMoveButton.colors;
        manualMoveColors.normalColor = new Color(0.3f, 0.7f, 0.3f);
        manualMoveColors.highlightedColor = new Color(0.4f, 0.8f, 0.4f);
        manualMoveColors.pressedColor = new Color(0.2f, 0.6f, 0.2f);
        manualMoveButton.colors = manualMoveColors;

        // Add button text
        GameObject manualMoveTextObj = new GameObject("Text");
        manualMoveTextObj.transform.SetParent(manualMoveButtonObj.transform);
        RectTransform manualMoveTextRect = manualMoveTextObj.AddComponent<RectTransform>();
        manualMoveTextRect.anchorMin = Vector2.zero;
        manualMoveTextRect.anchorMax = Vector2.one;
        manualMoveTextRect.sizeDelta = Vector2.zero;
        manualMoveTextRect.anchoredPosition = Vector2.zero;

        TMPro.TextMeshProUGUI manualMoveButtonText = manualMoveTextObj.AddComponent<TMPro.TextMeshProUGUI>();
        manualMoveButtonText.text = "CPU Move Now";
        manualMoveButtonText.fontSize = 16;
        manualMoveButtonText.color = Color.white;
        manualMoveButtonText.alignment = TMPro.TextAlignmentOptions.Center;

        // Wire up button click using UnityEventTools
        UnityEventTools.AddPersistentListener(manualMoveButton.onClick, cpuControlUI.OnManualMove);
        EditorUtility.SetDirty(manualMoveButtonObj);

        // Connect button text to CPU control UI
        cpuControlSO.FindProperty("buttonText").objectReferenceValue = autoPlayButtonText;
        cpuControlSO.ApplyModifiedProperties();

        // Create Restart Game Button (bottom center)
        GameObject resetButtonObj = new GameObject("RestartGameButton");
        resetButtonObj.transform.SetParent(canvasObj.transform);
        RectTransform resetButtonRect = resetButtonObj.AddComponent<RectTransform>();
        resetButtonRect.anchorMin = new Vector2(0.5f, 0);
        resetButtonRect.anchorMax = new Vector2(0.5f, 0);
        resetButtonRect.pivot = new Vector2(0.5f, 0);
        resetButtonRect.anchoredPosition = new Vector2(0, 70);
        resetButtonRect.sizeDelta = new Vector2(150, 40);

        UnityEngine.UI.Image resetButtonImage = resetButtonObj.AddComponent<UnityEngine.UI.Image>();
        resetButtonImage.color = new Color(0.8f, 0.3f, 0.3f);

        UnityEngine.UI.Button resetButton = resetButtonObj.AddComponent<UnityEngine.UI.Button>();
        UnityEngine.UI.ColorBlock resetColors = resetButton.colors;
        resetColors.normalColor = new Color(0.8f, 0.3f, 0.3f);
        resetColors.highlightedColor = new Color(0.9f, 0.4f, 0.4f);
        resetColors.pressedColor = new Color(0.7f, 0.2f, 0.2f);
        resetButton.colors = resetColors;

        // Add button text
        GameObject resetTextObj = new GameObject("Text");
        resetTextObj.transform.SetParent(resetButtonObj.transform);
        RectTransform resetTextRect = resetTextObj.AddComponent<RectTransform>();
        resetTextRect.anchorMin = Vector2.zero;
        resetTextRect.anchorMax = Vector2.one;
        resetTextRect.sizeDelta = Vector2.zero;
        resetTextRect.anchoredPosition = Vector2.zero;

        TMPro.TextMeshProUGUI resetButtonText = resetTextObj.AddComponent<TMPro.TextMeshProUGUI>();
        resetButtonText.text = "Restart Game";
        resetButtonText.fontSize = 16;
        resetButtonText.color = Color.white;
        resetButtonText.alignment = TMPro.TextAlignmentOptions.Center;

        // Wire up button click to GameManager.ResetGame() - full game restart
        UnityEventTools.AddPersistentListener(resetButton.onClick, gameManager.ResetGame);
        EditorUtility.SetDirty(resetButtonObj);

        // Create Pause Menu
        GameObject pauseMenuObj = new GameObject("PauseMenu");
        PauseMenu pauseMenu = pauseMenuObj.AddComponent<PauseMenu>();

        // Create pause menu panel (full screen, semi-transparent background)
        GameObject pausePanelObj = new GameObject("PauseMenuPanel");
        pausePanelObj.transform.SetParent(canvasObj.transform);
        RectTransform pausePanelRect = pausePanelObj.AddComponent<RectTransform>();
        pausePanelRect.anchorMin = Vector2.zero;
        pausePanelRect.anchorMax = Vector2.one;
        pausePanelRect.sizeDelta = Vector2.zero;
        pausePanelRect.anchoredPosition = Vector2.zero;

        UnityEngine.UI.Image pausePanelImage = pausePanelObj.AddComponent<UnityEngine.UI.Image>();
        pausePanelImage.color = new Color(0f, 0f, 0f, 0.8f); // Semi-transparent black

        // Create menu container (centered)
        GameObject menuContainerObj = new GameObject("MenuContainer");
        menuContainerObj.transform.SetParent(pausePanelObj.transform);
        RectTransform menuContainerRect = menuContainerObj.AddComponent<RectTransform>();
        menuContainerRect.anchorMin = new Vector2(0.5f, 0.5f);
        menuContainerRect.anchorMax = new Vector2(0.5f, 0.5f);
        menuContainerRect.pivot = new Vector2(0.5f, 0.5f);
        menuContainerRect.anchoredPosition = Vector2.zero;
        menuContainerRect.sizeDelta = new Vector2(300, 400);

        // Helper method to create menu buttons
        UnityEngine.UI.Button CreateMenuButton(string buttonName, string buttonText, float yPosition)
        {
            GameObject buttonObj = new GameObject(buttonName);
            buttonObj.transform.SetParent(menuContainerObj.transform);
            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = new Vector2(0, yPosition);
            buttonRect.sizeDelta = new Vector2(250, 60);

            UnityEngine.UI.Image buttonImage = buttonObj.AddComponent<UnityEngine.UI.Image>();
            buttonImage.color = new Color(0.3f, 0.3f, 0.3f);

            UnityEngine.UI.Button button = buttonObj.AddComponent<UnityEngine.UI.Button>();
            UnityEngine.UI.ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.3f, 0.3f, 0.3f);
            colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f);
            colors.pressedColor = new Color(0.2f, 0.2f, 0.2f);
            button.colors = colors;

            // Add button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            TMPro.TextMeshProUGUI buttonTextComponent = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            buttonTextComponent.text = buttonText;
            buttonTextComponent.fontSize = 24;
            buttonTextComponent.color = Color.white;
            buttonTextComponent.alignment = TMPro.TextAlignmentOptions.Center;

            return button;
        }

        // Create the four menu buttons and wire them up
        UnityEngine.UI.Button resumeButton = CreateMenuButton("ResumeButton", "Resume Game", 120);
        UnityEventTools.AddPersistentListener(resumeButton.onClick, pauseMenu.ResumeGame);
        EditorUtility.SetDirty(resumeButton.gameObject);

        UnityEngine.UI.Button optionsButton = CreateMenuButton("OptionsButton", "Options", 40);
        UnityEventTools.AddPersistentListener(optionsButton.onClick, pauseMenu.ShowOptions);
        EditorUtility.SetDirty(optionsButton.gameObject);

        UnityEngine.UI.Button saveContinueButton = CreateMenuButton("SaveContinueButton", "Save & Continue", -40);
        UnityEventTools.AddPersistentListener(saveContinueButton.onClick, pauseMenu.SaveAndContinue);
        EditorUtility.SetDirty(saveContinueButton.gameObject);

        UnityEngine.UI.Button saveQuitButton = CreateMenuButton("SaveQuitButton", "Save & Quit", -120);
        UnityEventTools.AddPersistentListener(saveQuitButton.onClick, pauseMenu.SaveAndQuit);
        EditorUtility.SetDirty(saveQuitButton.gameObject);

        // Connect pause menu panel to PauseMenu script
        SerializedObject pauseMenuSO = new SerializedObject(pauseMenu);
        pauseMenuSO.FindProperty("pauseMenuPanel").objectReferenceValue = pausePanelObj;
        pauseMenuSO.ApplyModifiedProperties();

        // Create Options Panel (full screen, semi-transparent background)
        GameObject optionsPanelObj = new GameObject("OptionsPanel");
        optionsPanelObj.transform.SetParent(canvasObj.transform);
        RectTransform optionsPanelRect = optionsPanelObj.AddComponent<RectTransform>();
        optionsPanelRect.anchorMin = Vector2.zero;
        optionsPanelRect.anchorMax = Vector2.one;
        optionsPanelRect.sizeDelta = Vector2.zero;
        optionsPanelRect.anchoredPosition = Vector2.zero;

        UnityEngine.UI.Image optionsPanelImage = optionsPanelObj.AddComponent<UnityEngine.UI.Image>();
        optionsPanelImage.color = new Color(0f, 0f, 0f, 0.9f); // Semi-transparent black

        // Create options container (centered, larger than pause menu)
        GameObject optionsContainerObj = new GameObject("OptionsContainer");
        optionsContainerObj.transform.SetParent(optionsPanelObj.transform);
        RectTransform optionsContainerRect = optionsContainerObj.AddComponent<RectTransform>();
        optionsContainerRect.anchorMin = new Vector2(0.5f, 0.5f);
        optionsContainerRect.anchorMax = new Vector2(0.5f, 0.5f);
        optionsContainerRect.pivot = new Vector2(0.5f, 0.5f);
        optionsContainerRect.anchoredPosition = Vector2.zero;
        optionsContainerRect.sizeDelta = new Vector2(500, 600);

        // Add background to options container
        UnityEngine.UI.Image optionsContainerBg = optionsContainerObj.AddComponent<UnityEngine.UI.Image>();
        optionsContainerBg.color = new Color(0.2f, 0.2f, 0.2f);

        // Options title
        GameObject optionsTitleObj = new GameObject("OptionsTitle");
        optionsTitleObj.transform.SetParent(optionsContainerObj.transform);
        RectTransform optionsTitleRect = optionsTitleObj.AddComponent<RectTransform>();
        optionsTitleRect.anchorMin = new Vector2(0.5f, 1f);
        optionsTitleRect.anchorMax = new Vector2(0.5f, 1f);
        optionsTitleRect.pivot = new Vector2(0.5f, 1f);
        optionsTitleRect.anchoredPosition = new Vector2(0, -20);
        optionsTitleRect.sizeDelta = new Vector2(400, 50);

        TMPro.TextMeshProUGUI optionsTitle = optionsTitleObj.AddComponent<TMPro.TextMeshProUGUI>();
        optionsTitle.text = "OPTIONS";
        optionsTitle.fontSize = 36;
        optionsTitle.color = Color.white;
        optionsTitle.alignment = TMPro.TextAlignmentOptions.Center;
        optionsTitle.fontStyle = TMPro.FontStyles.Bold;

        float yPos = -100;
        float ySpacing = 70;

        // Helper to create a label
        TMPro.TextMeshProUGUI CreateLabel(string labelName, string labelText, float y)
        {
            GameObject labelObj = new GameObject(labelName);
            labelObj.transform.SetParent(optionsContainerObj.transform);
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 1f);
            labelRect.anchorMax = new Vector2(0f, 1f);
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.anchoredPosition = new Vector2(30, y);
            labelRect.sizeDelta = new Vector2(150, 30);

            TMPro.TextMeshProUGUI label = labelObj.AddComponent<TMPro.TextMeshProUGUI>();
            label.text = labelText;
            label.fontSize = 18;
            label.color = Color.white;
            label.alignment = TMPro.TextAlignmentOptions.Left;

            return label;
        }

        // Helper to create a slider
        UnityEngine.UI.Slider CreateSlider(string sliderName, float y)
        {
            GameObject sliderObj = new GameObject(sliderName);
            sliderObj.transform.SetParent(optionsContainerObj.transform);
            RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0f, 1f);
            sliderRect.anchorMax = new Vector2(0f, 1f);
            sliderRect.pivot = new Vector2(0f, 0.5f);
            sliderRect.anchoredPosition = new Vector2(200, y);
            sliderRect.sizeDelta = new Vector2(250, 20);

            UnityEngine.UI.Slider slider = sliderObj.AddComponent<UnityEngine.UI.Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;

            // Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(sliderObj.transform);
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            bgRect.anchoredPosition = Vector2.zero;
            UnityEngine.UI.Image bgImage = bgObj.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f);

            // Fill Area
            GameObject fillAreaObj = new GameObject("Fill Area");
            fillAreaObj.transform.SetParent(sliderObj.transform);
            RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.sizeDelta = new Vector2(-10, -10);
            fillAreaRect.anchoredPosition = Vector2.zero;

            // Fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillAreaObj.transform);
            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            fillRect.anchoredPosition = Vector2.zero;
            UnityEngine.UI.Image fillImage = fillObj.AddComponent<UnityEngine.UI.Image>();
            fillImage.color = new Color(0f, 0.7f, 0f);

            // Handle Slide Area
            GameObject handleSlideAreaObj = new GameObject("Handle Slide Area");
            handleSlideAreaObj.transform.SetParent(sliderObj.transform);
            RectTransform handleSlideAreaRect = handleSlideAreaObj.AddComponent<RectTransform>();
            handleSlideAreaRect.anchorMin = Vector2.zero;
            handleSlideAreaRect.anchorMax = Vector2.one;
            handleSlideAreaRect.sizeDelta = new Vector2(-10, 0);
            handleSlideAreaRect.anchoredPosition = Vector2.zero;

            // Handle
            GameObject handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(handleSlideAreaObj.transform);
            RectTransform handleRect = handleObj.AddComponent<RectTransform>();
            handleRect.anchorMin = new Vector2(0f, 0.5f);
            handleRect.anchorMax = new Vector2(0f, 0.5f);
            handleRect.pivot = new Vector2(0.5f, 0.5f);
            handleRect.sizeDelta = new Vector2(20, 20);
            UnityEngine.UI.Image handleImage = handleObj.AddComponent<UnityEngine.UI.Image>();
            handleImage.color = Color.white;

            // Wire up slider components
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;

            return slider;
        }

        // Game Volume
        CreateLabel("GameVolumeLabel", "Game Volume:", yPos);
        UnityEngine.UI.Slider gameVolumeSlider = CreateSlider("GameVolumeSlider", yPos);

        yPos -= ySpacing;

        // Music Volume
        CreateLabel("MusicVolumeLabel", "Music Volume:", yPos);
        UnityEngine.UI.Slider musicVolumeSlider = CreateSlider("MusicVolumeSlider", yPos);

        yPos -= ySpacing;

        // Dialog Volume
        CreateLabel("DialogVolumeLabel", "Dialog Volume:", yPos);
        UnityEngine.UI.Slider dialogVolumeSlider = CreateSlider("DialogVolumeSlider", yPos);

        yPos -= ySpacing;

        // Game Speed Dropdown
        CreateLabel("GameSpeedLabel", "Game Speed:", yPos);

        GameObject gameSpeedDropdownObj = new GameObject("GameSpeedDropdown");
        gameSpeedDropdownObj.transform.SetParent(optionsContainerObj.transform);
        RectTransform gameSpeedDropdownRect = gameSpeedDropdownObj.AddComponent<RectTransform>();
        gameSpeedDropdownRect.anchorMin = new Vector2(0f, 1f);
        gameSpeedDropdownRect.anchorMax = new Vector2(0f, 1f);
        gameSpeedDropdownRect.pivot = new Vector2(0f, 0.5f);
        gameSpeedDropdownRect.anchoredPosition = new Vector2(200, yPos);
        gameSpeedDropdownRect.sizeDelta = new Vector2(250, 30);

        UnityEngine.UI.Image dropdownBg = gameSpeedDropdownObj.AddComponent<UnityEngine.UI.Image>();
        dropdownBg.color = new Color(0.3f, 0.3f, 0.3f);

        TMPro.TMP_Dropdown gameSpeedDropdown = gameSpeedDropdownObj.AddComponent<TMPro.TMP_Dropdown>();

        // Dropdown label (selected text)
        GameObject dropdownLabelObj = new GameObject("Label");
        dropdownLabelObj.transform.SetParent(gameSpeedDropdownObj.transform);
        RectTransform dropdownLabelRect = dropdownLabelObj.AddComponent<RectTransform>();
        dropdownLabelRect.anchorMin = Vector2.zero;
        dropdownLabelRect.anchorMax = Vector2.one;
        dropdownLabelRect.offsetMin = new Vector2(10, 2);
        dropdownLabelRect.offsetMax = new Vector2(-25, -2);

        TMPro.TextMeshProUGUI dropdownLabel = dropdownLabelObj.AddComponent<TMPro.TextMeshProUGUI>();
        dropdownLabel.text = ""; // Will be filled by dropdown
        dropdownLabel.fontSize = 16;
        dropdownLabel.color = Color.white;
        dropdownLabel.alignment = TMPro.TextAlignmentOptions.Left;
        dropdownLabel.enableAutoSizing = false;
        dropdownLabel.overflowMode = TMPro.TextOverflowModes.Overflow;

        // Dropdown arrow
        GameObject dropdownArrowObj = new GameObject("Arrow");
        dropdownArrowObj.transform.SetParent(gameSpeedDropdownObj.transform);
        RectTransform dropdownArrowRect = dropdownArrowObj.AddComponent<RectTransform>();
        dropdownArrowRect.anchorMin = new Vector2(1f, 0.5f);
        dropdownArrowRect.anchorMax = new Vector2(1f, 0.5f);
        dropdownArrowRect.pivot = new Vector2(0.5f, 0.5f);
        dropdownArrowRect.anchoredPosition = new Vector2(-15, 0);
        dropdownArrowRect.sizeDelta = new Vector2(10, 10);

        TMPro.TextMeshProUGUI dropdownArrow = dropdownArrowObj.AddComponent<TMPro.TextMeshProUGUI>();
        dropdownArrow.text = "▼";
        dropdownArrow.fontSize = 12;
        dropdownArrow.color = Color.white;
        dropdownArrow.alignment = TMPro.TextAlignmentOptions.Center;

        // Dropdown template
        GameObject dropdownTemplateObj = new GameObject("Template");
        dropdownTemplateObj.transform.SetParent(gameSpeedDropdownObj.transform);
        RectTransform dropdownTemplateRect = dropdownTemplateObj.AddComponent<RectTransform>();
        dropdownTemplateRect.anchorMin = new Vector2(0f, 0f);
        dropdownTemplateRect.anchorMax = new Vector2(1f, 0f);
        dropdownTemplateRect.pivot = new Vector2(0.5f, 1f);
        dropdownTemplateRect.anchoredPosition = new Vector2(0, 2);
        dropdownTemplateRect.sizeDelta = new Vector2(0, 90);

        UnityEngine.UI.Image dropdownTemplateBg = dropdownTemplateObj.AddComponent<UnityEngine.UI.Image>();
        dropdownTemplateBg.color = new Color(0.3f, 0.3f, 0.3f);

        // Add CanvasGroup for fade animations (required by TMP_Dropdown)
        CanvasGroup dropdownTemplateCanvasGroup = dropdownTemplateObj.AddComponent<CanvasGroup>();

        UnityEngine.UI.ScrollRect scrollRect = dropdownTemplateObj.AddComponent<UnityEngine.UI.ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 10;

        // Viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(dropdownTemplateObj.transform);
        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(0, 0);
        viewportRect.offsetMax = new Vector2(0, 0);

        UnityEngine.UI.Image viewportImage = viewportObj.AddComponent<UnityEngine.UI.Image>();
        viewportImage.color = new Color(0.3f, 0.3f, 0.3f);
        UnityEngine.UI.Mask viewportMask = viewportObj.AddComponent<UnityEngine.UI.Mask>();
        viewportMask.showMaskGraphic = false;

        // Content
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform);
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 90);

        // Add ToggleGroup to content for dropdown items
        UnityEngine.UI.ToggleGroup toggleGroup = contentObj.AddComponent<UnityEngine.UI.ToggleGroup>();

        // Item
        GameObject itemObj = new GameObject("Item");
        itemObj.transform.SetParent(contentObj.transform);
        RectTransform itemRect = itemObj.AddComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0f, 0.5f);
        itemRect.anchorMax = new Vector2(1f, 0.5f);
        itemRect.pivot = new Vector2(0.5f, 0.5f);
        itemRect.sizeDelta = new Vector2(0, 30);

        UnityEngine.UI.Toggle itemToggle = itemObj.AddComponent<UnityEngine.UI.Toggle>();
        itemToggle.group = toggleGroup;
        itemToggle.isOn = true;
        itemToggle.transition = UnityEngine.UI.Selectable.Transition.ColorTint;

        // Item Background (child GameObject with Image for targetGraphic)
        GameObject itemBgObj = new GameObject("Item Background");
        itemBgObj.transform.SetParent(itemObj.transform);
        RectTransform itemBgRect = itemBgObj.AddComponent<RectTransform>();
        itemBgRect.anchorMin = Vector2.zero;
        itemBgRect.anchorMax = Vector2.one;
        itemBgRect.offsetMin = Vector2.zero;
        itemBgRect.offsetMax = Vector2.zero;

        UnityEngine.UI.Image itemBg = itemBgObj.AddComponent<UnityEngine.UI.Image>();
        itemBg.color = new Color(0.3f, 0.3f, 0.3f);
        itemToggle.targetGraphic = itemBg;

        UnityEngine.UI.ColorBlock itemColors = itemToggle.colors;
        itemColors.normalColor = new Color(0.3f, 0.3f, 0.3f);
        itemColors.highlightedColor = new Color(0.4f, 0.4f, 0.4f);
        itemColors.pressedColor = new Color(0.25f, 0.25f, 0.25f);
        itemColors.selectedColor = new Color(0.35f, 0.35f, 0.35f);
        itemToggle.colors = itemColors;

        // Item Checkmark (Image component, not TextMeshProUGUI)
        GameObject itemCheckmarkObj = new GameObject("Item Checkmark");
        itemCheckmarkObj.transform.SetParent(itemObj.transform);
        RectTransform itemCheckmarkRect = itemCheckmarkObj.AddComponent<RectTransform>();
        itemCheckmarkRect.anchorMin = new Vector2(0f, 0.5f);
        itemCheckmarkRect.anchorMax = new Vector2(0f, 0.5f);
        itemCheckmarkRect.pivot = new Vector2(0.5f, 0.5f);
        itemCheckmarkRect.anchoredPosition = new Vector2(15, 0);
        itemCheckmarkRect.sizeDelta = new Vector2(15, 15);

        UnityEngine.UI.Image itemCheckmark = itemCheckmarkObj.AddComponent<UnityEngine.UI.Image>();
        itemCheckmark.color = Color.white;

        itemToggle.graphic = itemCheckmark;

        // Item label
        GameObject itemLabelObj = new GameObject("Item Label");
        itemLabelObj.transform.SetParent(itemObj.transform);
        RectTransform itemLabelRect = itemLabelObj.AddComponent<RectTransform>();
        itemLabelRect.anchorMin = new Vector2(0f, 0f);
        itemLabelRect.anchorMax = new Vector2(1f, 1f);
        itemLabelRect.pivot = new Vector2(0.5f, 0.5f);
        itemLabelRect.offsetMin = new Vector2(30, 1);
        itemLabelRect.offsetMax = new Vector2(-5, -2);
        itemLabelRect.anchoredPosition = Vector2.zero;

        TMPro.TextMeshProUGUI itemLabel = itemLabelObj.AddComponent<TMPro.TextMeshProUGUI>();
        itemLabel.text = "Option";
        itemLabel.fontSize = 16;
        itemLabel.color = Color.white;
        itemLabel.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
        itemLabel.enableAutoSizing = false;
        itemLabel.overflowMode = TMPro.TextOverflowModes.Overflow;
        itemLabel.textWrappingMode = TMPro.TextWrappingModes.NoWrap;

        // Ensure font is assigned
        if (itemLabel.font == null)
        {
            itemLabel.font = TMPro.TMP_Settings.defaultFontAsset;
        }

        // Wire up ScrollRect
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;

        dropdownTemplateObj.SetActive(false);

        // Wire up dropdown components
        gameSpeedDropdown.captionText = dropdownLabel;
        gameSpeedDropdown.itemText = itemLabel;
        gameSpeedDropdown.template = dropdownTemplateRect;

        // Add options AFTER all components are wired up
        gameSpeedDropdown.options.Clear();
        gameSpeedDropdown.options.Add(new TMPro.TMP_Dropdown.OptionData("Slow"));
        gameSpeedDropdown.options.Add(new TMPro.TMP_Dropdown.OptionData("Medium"));
        gameSpeedDropdown.options.Add(new TMPro.TMP_Dropdown.OptionData("Fast"));

        // Set default value and refresh (must be done AFTER components are wired and options added)
        gameSpeedDropdown.value = 1; // Default to Medium (matches GameSpeedSettings default)
        gameSpeedDropdown.RefreshShownValue();

        // Wire up dropdown to GameSpeedSettings
        UnityEventTools.AddPersistentListener(gameSpeedDropdown.onValueChanged, gameSpeedSettings.SetGameSpeedFromDropdown);
        EditorUtility.SetDirty(gameSpeedDropdownObj);

        yPos -= ySpacing;

        // Helper to create a toggle
        UnityEngine.UI.Toggle CreateToggle(string toggleName, string labelText, float y)
        {
            GameObject toggleObj = new GameObject(toggleName);
            toggleObj.transform.SetParent(optionsContainerObj.transform);
            RectTransform toggleRect = toggleObj.AddComponent<RectTransform>();
            toggleRect.anchorMin = new Vector2(0f, 1f);
            toggleRect.anchorMax = new Vector2(0f, 1f);
            toggleRect.pivot = new Vector2(0f, 0.5f);
            toggleRect.anchoredPosition = new Vector2(200, y);
            toggleRect.sizeDelta = new Vector2(250, 30);

            UnityEngine.UI.Toggle toggle = toggleObj.AddComponent<UnityEngine.UI.Toggle>();

            // Background
            GameObject toggleBgObj = new GameObject("Background");
            toggleBgObj.transform.SetParent(toggleObj.transform);
            RectTransform toggleBgRect = toggleBgObj.AddComponent<RectTransform>();
            toggleBgRect.anchorMin = new Vector2(0f, 0.5f);
            toggleBgRect.anchorMax = new Vector2(0f, 0.5f);
            toggleBgRect.pivot = new Vector2(0f, 0.5f);
            toggleBgRect.anchoredPosition = Vector2.zero;
            toggleBgRect.sizeDelta = new Vector2(30, 30);

            UnityEngine.UI.Image toggleBgImage = toggleBgObj.AddComponent<UnityEngine.UI.Image>();
            toggleBgImage.color = new Color(0.3f, 0.3f, 0.3f);

            // Checkmark
            GameObject checkmarkObj = new GameObject("Checkmark");
            checkmarkObj.transform.SetParent(toggleBgObj.transform);
            RectTransform checkmarkRect = checkmarkObj.AddComponent<RectTransform>();
            checkmarkRect.anchorMin = Vector2.zero;
            checkmarkRect.anchorMax = Vector2.one;
            checkmarkRect.sizeDelta = Vector2.zero;
            checkmarkRect.anchoredPosition = Vector2.zero;

            TMPro.TextMeshProUGUI checkmark = checkmarkObj.AddComponent<TMPro.TextMeshProUGUI>();
            checkmark.text = "X";
            checkmark.fontSize = 24;
            checkmark.color = new Color(0f, 0.7f, 0f);
            checkmark.alignment = TMPro.TextAlignmentOptions.Center;

            // Label
            GameObject toggleLabelObj = new GameObject("Label");
            toggleLabelObj.transform.SetParent(toggleObj.transform);
            RectTransform toggleLabelRect = toggleLabelObj.AddComponent<RectTransform>();
            toggleLabelRect.anchorMin = new Vector2(0f, 0.5f);
            toggleLabelRect.anchorMax = new Vector2(1f, 0.5f);
            toggleLabelRect.pivot = new Vector2(0f, 0.5f);
            toggleLabelRect.anchoredPosition = new Vector2(40, 0);
            toggleLabelRect.sizeDelta = new Vector2(-40, 30);

            TMPro.TextMeshProUGUI toggleLabel = toggleLabelObj.AddComponent<TMPro.TextMeshProUGUI>();
            toggleLabel.text = labelText;
            toggleLabel.fontSize = 18;
            toggleLabel.color = Color.white;
            toggleLabel.alignment = TMPro.TextAlignmentOptions.Left;

            toggle.graphic = checkmark;
            toggle.isOn = false;

            return toggle;
        }

        // Subtitles toggle
        CreateLabel("SubtitlesLabel", "Subtitles:", yPos);
        UnityEngine.UI.Toggle subtitlesToggle = CreateToggle("SubtitlesToggle", "Enable Subtitles", yPos);

        yPos -= ySpacing;

        // Color Blind Mode toggle
        CreateLabel("ColorBlindModeLabel", "Color Blind Mode:", yPos);
        UnityEngine.UI.Toggle colorBlindToggle = CreateToggle("ColorBlindModeToggle", "Enable Color Blind Mode", yPos);

        yPos -= 80;

        // Back button
        GameObject backButtonObj = new GameObject("BackButton");
        backButtonObj.transform.SetParent(optionsContainerObj.transform);
        RectTransform backButtonRect = backButtonObj.AddComponent<RectTransform>();
        backButtonRect.anchorMin = new Vector2(0.5f, 0f);
        backButtonRect.anchorMax = new Vector2(0.5f, 0f);
        backButtonRect.pivot = new Vector2(0.5f, 0f);
        backButtonRect.anchoredPosition = new Vector2(0, 20);
        backButtonRect.sizeDelta = new Vector2(200, 50);

        UnityEngine.UI.Image backButtonImage = backButtonObj.AddComponent<UnityEngine.UI.Image>();
        backButtonImage.color = new Color(0.3f, 0.3f, 0.3f);

        UnityEngine.UI.Button backButton = backButtonObj.AddComponent<UnityEngine.UI.Button>();
        UnityEngine.UI.ColorBlock backButtonColors = backButton.colors;
        backButtonColors.normalColor = new Color(0.3f, 0.3f, 0.3f);
        backButtonColors.highlightedColor = new Color(0.4f, 0.4f, 0.4f);
        backButtonColors.pressedColor = new Color(0.2f, 0.2f, 0.2f);
        backButton.colors = backButtonColors;

        // Back button text
        GameObject backButtonTextObj = new GameObject("Text");
        backButtonTextObj.transform.SetParent(backButtonObj.transform);
        RectTransform backButtonTextRect = backButtonTextObj.AddComponent<RectTransform>();
        backButtonTextRect.anchorMin = Vector2.zero;
        backButtonTextRect.anchorMax = Vector2.one;
        backButtonTextRect.sizeDelta = Vector2.zero;
        backButtonTextRect.anchoredPosition = Vector2.zero;

        TMPro.TextMeshProUGUI backButtonText = backButtonTextObj.AddComponent<TMPro.TextMeshProUGUI>();
        backButtonText.text = "Back";
        backButtonText.fontSize = 24;
        backButtonText.color = Color.white;
        backButtonText.alignment = TMPro.TextAlignmentOptions.Center;

        // Wire up back button to close options
        UnityEventTools.AddPersistentListener(backButton.onClick, pauseMenu.CloseOptions);
        EditorUtility.SetDirty(backButtonObj);

        // Connect options panel to PauseMenu script
        pauseMenuSO.Update();
        pauseMenuSO.FindProperty("optionsPanel").objectReferenceValue = optionsPanelObj;
        pauseMenuSO.ApplyModifiedProperties();

        // Hide options panel by default
        optionsPanelObj.SetActive(false);

        // Hide pause menu by default
        pausePanelObj.SetActive(false);

        // Save scene
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

        Debug.Log("Match 3 scene setup complete! Press Play to test.");
        EditorUtility.DisplayDialog("Setup Complete",
            "Match 3 scene has been created successfully!\n\n" +
            "The scene is now ready to play. Press the Play button to test the game.\n\n" +
            "Scene saved at: Assets/Scenes/Match3.unity", "OK");
    }

    private static GameObject CreateGamePiecePrefab()
    {
        // Create a temporary GameObject
        GameObject piece = new GameObject("GamePiece");

        // Add SpriteRenderer
        SpriteRenderer sr = piece.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.sortingOrder = 1;

        // Add CircleCollider2D for mouse detection
        CircleCollider2D collider = piece.AddComponent<CircleCollider2D>();
        collider.radius = 0.45f;

        // Add GamePiece script
        GamePiece gamePiece = piece.AddComponent<GamePiece>();

        // Use reflection to set the sprite renderer field
        SerializedObject pieceSO = new SerializedObject(gamePiece);
        pieceSO.FindProperty("spriteRenderer").objectReferenceValue = sr;
        pieceSO.ApplyModifiedProperties();

        // Save as prefab
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(piece, "Assets/Prefabs/GamePiece.prefab");

        // Clean up temporary object
        DestroyImmediate(piece);

        return prefab;
    }

    private static GameObject CreateCellBackgroundPrefab()
    {
        GameObject cell = new GameObject("CellBackground");

        SpriteRenderer sr = cell.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = new Color(0.2f, 0.2f, 0.3f);
        sr.sortingOrder = -1;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(cell, "Assets/Prefabs/CellBackground.prefab");
        DestroyImmediate(cell);

        return prefab;
    }

    private static Sprite CreateCircleSprite()
    {
        // Create a circle texture
        int size = 128;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 4;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= radius)
                {
                    pixels[y * size + x] = Color.white;
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        // Ensure Sprites directory exists
        if (!System.IO.Directory.Exists("Assets/Sprites"))
        {
            System.IO.Directory.CreateDirectory("Assets/Sprites");
        }

        // Save texture
        System.IO.File.WriteAllBytes("Assets/Sprites/CircleSprite.png", texture.EncodeToPNG());
        AssetDatabase.Refresh();

        // Import as sprite
        TextureImporter importer = AssetImporter.GetAtPath("Assets/Sprites/CircleSprite.png") as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 100;
            importer.filterMode = FilterMode.Bilinear;
            AssetDatabase.ImportAsset("Assets/Sprites/CircleSprite.png");
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/CircleSprite.png");
    }

    private static Sprite CreateSquareSprite()
    {
        int size = 100;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }

        texture.SetPixels(pixels);
        texture.Apply();

        // Ensure Sprites directory exists
        if (!System.IO.Directory.Exists("Assets/Sprites"))
        {
            System.IO.Directory.CreateDirectory("Assets/Sprites");
        }

        System.IO.File.WriteAllBytes("Assets/Sprites/SquareSprite.png", texture.EncodeToPNG());
        AssetDatabase.Refresh();

        TextureImporter importer = AssetImporter.GetAtPath("Assets/Sprites/SquareSprite.png") as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 100;
            importer.filterMode = FilterMode.Bilinear;
            AssetDatabase.ImportAsset("Assets/Sprites/SquareSprite.png");
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SquareSprite.png");
    }
}
