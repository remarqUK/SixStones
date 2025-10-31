using UnityEditor;

public class AddScenesToBuild
{
    [MenuItem("Tools/Add WorldMap and SubZoneMap to Build")]
    static void AddScenes()
    {
        var scenes = EditorBuildSettings.scenes;
        var sceneList = new System.Collections.Generic.List<EditorBuildSettingsScene>(scenes);
        
        // Add WorldMap
        string worldMapPath = "Assets/Scenes/WorldMap.unity/WorldMap.unity";
        if (!System.Array.Exists(scenes, s => s.path == worldMapPath))
        {
            sceneList.Add(new EditorBuildSettingsScene(worldMapPath, true));
        }
        
        // Add SubZoneMap
        string subZonePath = "Assets/Scenes/SubZoneMap.unity/SubZoneMap.unity";
        if (!System.Array.Exists(scenes, s => s.path == subZonePath))
        {
            sceneList.Add(new EditorBuildSettingsScene(subZonePath, true));
        }
        
        EditorBuildSettings.scenes = sceneList.ToArray();
        UnityEngine.Debug.Log("Added WorldMap and SubZoneMap to build settings");
    }
}
