using UnityEditor;
using UnityEngine;
using System.Linq;

public class BuildScript
{
    [MenuItem("Tools/Build Match 3 Game")]
    public static void BuildGame()
    {
        BuildGameInternal();
    }

    public static void BuildGameCommandLine()
    {
        BuildGameInternal();
        EditorApplication.Exit(0);
    }

    private static void BuildGameInternal()
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        // If no scenes are enabled, try to find Match3 scene
        if (scenes.Length == 0)
        {
            string match3Scene = "Assets/Scenes/Match3.unity";
            if (System.IO.File.Exists(match3Scene))
            {
                scenes = new string[] { match3Scene };
                Debug.Log($"Building with scene: {match3Scene}");
            }
            else
            {
                Debug.LogError("No scenes found to build! Please create the Match3 scene using Tools > Setup Match 3 Game");
                return;
            }
        }

        string buildPath = "Build/Match3Game.exe";

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        Debug.Log($"Starting build with {scenes.Length} scene(s)...");

        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        var summary = report.summary;

        if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"Build succeeded: {summary.totalSize} bytes");
            Debug.Log($"Build location: {buildPath}");
            EditorUtility.DisplayDialog("Build Complete",
                $"Build succeeded!\n\nSize: {summary.totalSize / (1024 * 1024)} MB\nLocation: {buildPath}",
                "OK");
        }
        else
        {
            Debug.LogError($"Build failed with result: {summary.result}");
            if (summary.totalErrors > 0)
            {
                Debug.LogError($"Total errors: {summary.totalErrors}");
            }
            EditorUtility.DisplayDialog("Build Failed",
                $"Build failed!\n\nErrors: {summary.totalErrors}\nCheck the console for details.",
                "OK");
        }
    }

    public static void CreateGameModesCommandLine()
    {
        Debug.Log("Creating game modes from command line...");
        CreateGameModes.CreateDefaultModes();
        Debug.Log("Game modes creation complete!");
    }

    [MenuItem("Tools/Validate Match 3 Scripts")]
    public static void ValidateScripts()
    {
        Debug.Log("=== Validating Match 3 Scripts ===");

        // Check for required scripts
        string[] requiredScripts = new string[]
        {
            "Assets/Scripts/GamePiece.cs",
            "Assets/Scripts/Board.cs",
            "Assets/Scripts/MatchDetector.cs",
            "Assets/Scripts/InputManager.cs",
            "Assets/Scripts/GameManager.cs"
        };

        bool allFound = true;
        foreach (string script in requiredScripts)
        {
            if (System.IO.File.Exists(script))
            {
                Debug.Log($"✓ Found: {script}");
            }
            else
            {
                Debug.LogError($"✗ Missing: {script}");
                allFound = false;
            }
        }

        // Try to find the types to verify compilation
        System.Type[] requiredTypes = new System.Type[]
        {
            typeof(GamePiece),
            typeof(Board),
            typeof(MatchDetector),
            typeof(InputManager),
            typeof(GameManager)
        };

        bool allCompiled = true;
        foreach (var type in requiredTypes)
        {
            if (type != null)
            {
                Debug.Log($"✓ Compiled: {type.Name}");
            }
            else
            {
                Debug.LogError($"✗ Not compiled: {type?.Name ?? "Unknown"}");
                allCompiled = false;
            }
        }

        if (allFound && allCompiled)
        {
            Debug.Log("=== All scripts validated successfully! ===");
            EditorUtility.DisplayDialog("Validation Complete",
                "All Match 3 scripts are present and compiled successfully!",
                "OK");
        }
        else
        {
            Debug.LogWarning("=== Validation found issues - check the console for details ===");
        }
    }
}
