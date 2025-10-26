using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

public class Maze3DSetup
{
    [MenuItem("Tools/Create 3D Maze Scene")]
    static void CreateMaze3DScene()
    {
        // Create new scene
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Remove default Main Camera (we'll create our own with the player)
        Camera defaultCam = Object.FindFirstObjectByType<Camera>();
        if (defaultCam != null && defaultCam.gameObject.name == "Main Camera")
        {
            Object.DestroyImmediate(defaultCam.gameObject);
        }

        // Find or create MapGenerator
        MapGenerator mapGen = Object.FindFirstObjectByType<MapGenerator>();
        if (mapGen == null)
        {
            GameObject mapGenObj = new GameObject("MapGenerator");
            mapGen = mapGenObj.AddComponent<MapGenerator>();
            MapVisualizer visualizer = mapGenObj.AddComponent<MapVisualizer>();
            visualizer.mapGenerator = mapGen;

            // Set default values
            mapGen.width = 10;
            mapGen.height = 10;
            mapGen.enemySpawnChance = 0.3f;
            mapGen.treasureSpawnChance = 0.1f;
            mapGen.secretRoomChance = 0.1f;

            // Generate initial maze
            mapGen.GenerateMaze();
        }

        // Verify maze was generated
        if (mapGen.grid == null)
        {
            Debug.LogError("Failed to generate maze grid!");
            return;
        }

        // Create Maze3DBuilder
        GameObject builderObj = new GameObject("Maze3DBuilder");
        Maze3DBuilder builder = builderObj.AddComponent<Maze3DBuilder>();
        builder.mapGenerator = mapGen;

        // Create placeholder materials
        builder.floorMaterial = CreatePlaceholderMaterial("FloorMaterial", Color.gray);
        builder.wallMaterial = CreatePlaceholderMaterial("WallMaterial", new Color(0.533f, 0.533f, 0.533f)); // #888
        builder.secretRoomFloorMaterial = CreatePlaceholderMaterial("SecretFloorMaterial", new Color(0.5f, 0f, 0.5f));
        builder.startMarkerMaterial = CreatePlaceholderMaterial("StartMarkerMaterial", Color.green);
        builder.bossMarkerMaterial = CreatePlaceholderMaterial("BossMarkerMaterial", Color.red);

        // Build the 3D maze
        Debug.Log($"Building 3D maze from {mapGen.width}x{mapGen.height} grid...");
        builder.BuildMaze3D();

        // Create First Person Controller
        GameObject playerObj = new GameObject("Player");
        playerObj.transform.position = Vector3.zero;

        // Create camera manually (before controller's Awake)
        GameObject camObj = new GameObject("PlayerCamera");
        camObj.tag = "MainCamera";
        camObj.transform.SetParent(playerObj.transform);
        camObj.transform.localPosition = new Vector3(0, 1.6f, 0);
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;

        // Add URP camera data if needed
        TryAddURPCameraData(camObj);

        // Add controller and assign camera
        FirstPersonMazeController fpController = playerObj.AddComponent<FirstPersonMazeController>();
        fpController.playerCamera = cam;
        fpController.transitionSpeed = 5f;
        fpController.rotationSpeed = 10f;

        // Position player at start
        fpController.MoveToStart(builder);

        // Setup lighting
        GameObject lightObj = new GameObject("Directional Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.transform.rotation = Quaternion.Euler(50, -30, 0);
        light.intensity = 2f; // Increase intensity for better visibility
        light.color = Color.white;

        // Set ambient lighting for better visibility
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.4f, 0.4f, 0.4f); // Gray ambient light

        // Create Minimap UI
        CreateMinimapUI(mapGen, fpController);

        Debug.Log($"Camera created at: {cam.transform.position}, rotation: {cam.transform.rotation.eulerAngles}");

        // Select the builder for inspection
        Selection.activeGameObject = builderObj;

        Debug.Log("3D Maze Scene created! Grid-based dungeon crawler controls:");
        Debug.Log("- Up/W: Move forward one cell");
        Debug.Log("- Down/S: Move backward one cell");
        Debug.Log("- Left/A: Turn left 90 degrees (instant)");
        Debug.Log("- Right/D: Turn right 90 degrees (instant)");

        // Save scene
        EditorSceneManager.SaveScene(newScene, "Assets/Scenes/Maze3D.unity");
    }

    [MenuItem("Tools/Rebuild 3D Maze")]
    static void RebuildMaze3D()
    {
        Maze3DBuilder builder = Object.FindFirstObjectByType<Maze3DBuilder>();
        if (builder == null)
        {
            Debug.LogError("No Maze3DBuilder found in scene! Create one first with Tools > Create 3D Maze Scene");
            return;
        }

        MapGenerator mapGen = builder.mapGenerator;
        if (mapGen == null)
        {
            Debug.LogError("Maze3DBuilder has no MapGenerator assigned!");
            return;
        }

        // Regenerate the 2D maze
        mapGen.GenerateMaze();

        // Rebuild the 3D maze
        builder.BuildMaze3D();

        // Reposition player at start
        FirstPersonMazeController player = Object.FindFirstObjectByType<FirstPersonMazeController>();
        if (player != null)
        {
            player.MoveToStart(builder);
        }

        Debug.Log("3D Maze rebuilt!");
    }

    static Material CreatePlaceholderMaterial(string name, Color color)
    {
        // Try URP shader first, fallback to Unlit/Color, then Standard
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        if (shader == null) shader = Shader.Find("Standard");

        Material mat = new Material(shader);
        mat.name = name;
        mat.color = color;

        Debug.Log($"Created material '{name}' using shader: {shader.name}");

        // Make materials brighter/emissive for visibility
        if (shader.name.Contains("Standard"))
        {
            mat.SetColor("_EmissionColor", color * 0.3f);
            mat.EnableKeyword("_EMISSION");
        }

        return mat;
    }

    static void TryAddURPCameraData(GameObject cameraObject)
    {
        // Try to add Universal Additional Camera Data component if URP is installed
        var cameraDataType = System.Type.GetType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
        if (cameraDataType != null)
        {
            cameraObject.AddComponent(cameraDataType);
            Debug.Log("Added URP camera data component");
        }
    }

    static void CreateMinimapUI(MapGenerator mapGen, FirstPersonMazeController player)
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("MinimapCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Create minimap container (positioned at top right)
        GameObject containerObj = new GameObject("MinimapContainer");
        containerObj.transform.SetParent(canvasObj.transform, false);
        RectTransform containerRect = containerObj.AddComponent<RectTransform>();

        // Position at top right with some padding
        containerRect.anchorMin = new Vector2(1, 1);
        containerRect.anchorMax = new Vector2(1, 1);
        containerRect.pivot = new Vector2(1, 1);
        containerRect.anchoredPosition = new Vector2(-20, -20); // 20 pixel padding from top-right
        containerRect.sizeDelta = new Vector2(200, 200); // 200x200 pixel container

        // Add background panel
        UnityEngine.UI.Image bgImage = containerObj.AddComponent<UnityEngine.UI.Image>();
        bgImage.color = new Color(0, 0, 0, 0.8f); // Semi-transparent black background

        // Create RawImage for minimap texture
        GameObject minimapImageObj = new GameObject("MinimapImage");
        minimapImageObj.transform.SetParent(containerObj.transform, false);
        RectTransform imageRect = minimapImageObj.AddComponent<RectTransform>();
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = new Vector2(5, 5); // 5 pixel padding
        imageRect.offsetMax = new Vector2(-5, -5); // 5 pixel padding
        UnityEngine.UI.RawImage rawImage = minimapImageObj.AddComponent<UnityEngine.UI.RawImage>();

        // Create MinimapRenderer component
        GameObject minimapRendererObj = new GameObject("MinimapRenderer");
        MinimapRenderer minimapRenderer = minimapRendererObj.AddComponent<MinimapRenderer>();
        minimapRenderer.mapGenerator = mapGen;
        minimapRenderer.player = player;
        minimapRenderer.minimapImage = rawImage;
        minimapRenderer.minimapContainer = containerRect;

        Debug.Log("Minimap UI created at top right corner");
    }
}

// Custom inspector for Maze3DBuilder
[CustomEditor(typeof(Maze3DBuilder))]
public class Maze3DBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Maze3DBuilder builder = (Maze3DBuilder)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("3D Maze Actions", EditorStyles.boldLabel);

        // Check if MapGenerator is assigned
        if (builder.mapGenerator == null)
        {
            EditorGUILayout.HelpBox("No MapGenerator assigned! Assign one in the inspector above.", MessageType.Warning);
        }
        else if (builder.mapGenerator.grid == null)
        {
            EditorGUILayout.HelpBox("MapGenerator has no grid! Use 'Regenerate Map & Rebuild 3D' button.", MessageType.Warning);
        }

        GUI.enabled = builder.mapGenerator != null && builder.mapGenerator.grid != null;
        if (GUILayout.Button("Build 3D Maze", GUILayout.Height(40)))
        {
            builder.BuildMaze3D();
        }
        GUI.enabled = true;

        GUI.enabled = builder.mapGenerator != null;
        if (GUILayout.Button("Regenerate Map & Rebuild 3D", GUILayout.Height(40)))
        {
            builder.mapGenerator.GenerateMaze();
            builder.BuildMaze3D();

            // Reposition player
            FirstPersonMazeController player = FindFirstObjectByType<FirstPersonMazeController>();
            if (player != null)
            {
                player.MoveToStart(builder);
            }
        }
        GUI.enabled = true;

        if (GUILayout.Button("Position Player at Start"))
        {
            FirstPersonMazeController player = FindFirstObjectByType<FirstPersonMazeController>();
            if (player != null)
            {
                player.MoveToStart(builder);
            }
            else
            {
                Debug.LogWarning("No FirstPersonMazeController found in scene!");
            }
        }
    }
}
