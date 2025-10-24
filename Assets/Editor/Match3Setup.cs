using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering.Universal;

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

        // Create Input Manager
        GameObject inputObj = new GameObject("InputManager");
        InputManager inputManager = inputObj.AddComponent<InputManager>();

        SerializedObject inputSO = new SerializedObject(inputManager);
        inputSO.FindProperty("board").objectReferenceValue = board;
        inputSO.FindProperty("mainCamera").objectReferenceValue = camera;
        inputSO.ApplyModifiedProperties();

        // Create Canvas for UI
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Create Score Text
        GameObject scoreObj = new GameObject("ScoreText");
        scoreObj.transform.SetParent(canvasObj.transform);
        RectTransform scoreRect = scoreObj.AddComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0, 1);
        scoreRect.anchorMax = new Vector2(0, 1);
        scoreRect.pivot = new Vector2(0, 1);
        scoreRect.anchoredPosition = new Vector2(20, -20);
        scoreRect.sizeDelta = new Vector2(300, 60);

        TMPro.TextMeshProUGUI scoreText = scoreObj.AddComponent<TMPro.TextMeshProUGUI>();
        scoreText.text = "Score: 0";
        scoreText.fontSize = 36;
        scoreText.color = Color.white;
        scoreText.alignment = TMPro.TextAlignmentOptions.Left;

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
        player1ColorScoresText.text = "<b>Player 1 Colors:</b>\nRed: 0\nBlue: 0\nGreen: 0\nYellow: 0\nPurple: 0\nOrange: 0";
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
        player2ColorScoresText.text = "<b>Player 2 Colors:</b>\nRed: 0\nBlue: 0\nGreen: 0\nYellow: 0\nPurple: 0\nOrange: 0";
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
        player1ScoreRect.anchoredPosition = new Vector2(20, -80);
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
        player2ScoreRect.anchoredPosition = new Vector2(-20, -80);
        player2ScoreRect.sizeDelta = new Vector2(300, 50);

        TMPro.TextMeshProUGUI player2ScoreText = player2ScoreObj.AddComponent<TMPro.TextMeshProUGUI>();
        player2ScoreText.text = "Player 2: 0";
        player2ScoreText.fontSize = 28;
        player2ScoreText.color = new Color(1f, 0.5f, 0.5f); // Light red
        player2ScoreText.alignment = TMPro.TextAlignmentOptions.Right;

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
        gmSO.FindProperty("pointsPerPiece").intValue = 1;
        gmSO.FindProperty("maxMoves").intValue = 30;
        gmSO.ApplyModifiedProperties();

        // Connect GameManager, PlayerManager, and CPUPlayer to Board
        boardSO.FindProperty("gameManager").objectReferenceValue = gameManager;
        boardSO.FindProperty("playerManager").objectReferenceValue = playerManager;
        boardSO.FindProperty("cpuPlayer").objectReferenceValue = cpuPlayer;
        boardSO.ApplyModifiedProperties();

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
