using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MapGenerator generator = (MapGenerator)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Maze Generation", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate New Maze", GUILayout.Height(40)))
        {
            generator.GenerateMaze();
            EditorUtility.SetDirty(generator);
            SceneView.RepaintAll();
        }

        if (generator.grid != null)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                $"Current Maze: {generator.width}x{generator.height}\n" +
                $"Start: ({generator.startPosition.x}, {generator.startPosition.y})\n" +
                $"Boss: ({generator.bossPosition.x}, {generator.bossPosition.y})",
                MessageType.Info
            );
        }
        else
        {
            EditorGUILayout.HelpBox("No maze generated yet. Click 'Generate New Maze' to create one.", MessageType.Warning);
        }

        // Add visualizer if missing
        EditorGUILayout.Space(10);
        MapVisualizer visualizer = generator.GetComponent<MapVisualizer>();
        if (visualizer == null)
        {
            if (GUILayout.Button("Add Map Visualizer"))
            {
                visualizer = generator.gameObject.AddComponent<MapVisualizer>();
                visualizer.mapGenerator = generator;
                EditorUtility.SetDirty(generator.gameObject);
            }
        }
        else
        {
            if (GUILayout.Button("Create Runtime Visualization"))
            {
                visualizer.CreateRuntimeVisualization();
            }
        }
    }
}

public class MapGeneratorTool
{
    [MenuItem("Tools/Create Map Generator")]
    static void CreateMapGenerator()
    {
        // Check if one already exists
        MapGenerator existing = Object.FindFirstObjectByType<MapGenerator>();
        if (existing != null)
        {
            Debug.LogWarning("MapGenerator already exists in the scene!");
            Selection.activeGameObject = existing.gameObject;
            EditorGUIUtility.PingObject(existing.gameObject);
            return;
        }

        // Create new GameObject with MapGenerator and MapVisualizer
        GameObject mapObj = new GameObject("MapGenerator");
        MapGenerator generator = mapObj.AddComponent<MapGenerator>();
        MapVisualizer visualizer = mapObj.AddComponent<MapVisualizer>();

        // Set default values
        generator.width = 10;
        generator.height = 10;
        generator.enemySpawnChance = 0.3f;
        generator.treasureSpawnChance = 0.1f;

        // Link visualizer to generator
        visualizer.mapGenerator = generator;

        // Generate initial maze
        generator.GenerateMaze();

        // Select the created object
        Selection.activeGameObject = mapObj;
        EditorGUIUtility.PingObject(mapObj);

        Debug.Log("MapGenerator created! Select it in the hierarchy and click 'Generate New Maze' in the inspector, or view it in the Scene view.");
    }

    [MenuItem("Tools/Test Map Generation (Various Sizes)")]
    static void TestMapGeneration()
    {
        // Create a test window or just log results
        Debug.Log("=== Map Generation Test ===");

        int[] testSizes = { 5, 10, 15, 20 };

        foreach (int size in testSizes)
        {
            MapGenerator testGen = new GameObject("TestGenerator").AddComponent<MapGenerator>();
            testGen.width = size;
            testGen.height = size;
            testGen.randomSeed = 12345; // Fixed seed for reproducibility

            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            testGen.GenerateMaze();
            sw.Stop();

            Debug.Log($"[{size}x{size}] Generated in {sw.ElapsedMilliseconds}ms - Start: {testGen.startPosition}, Boss: {testGen.bossPosition}");

            // Clean up test object
            Object.DestroyImmediate(testGen.gameObject);
        }

        Debug.Log("=== Test Complete ===");
    }
}
