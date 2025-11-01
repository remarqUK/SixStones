using UnityEditor;

public class AddScenesToBuild
{
    [MenuItem("Tools/Add All Scenes to Build")]
    static void AddScenes()
    {
        var scenes = EditorBuildSettings.scenes;
        var sceneList = new System.Collections.Generic.List<EditorBuildSettingsScene>(scenes);

        bool addedAny = false;

        // Add WorldMap
        string worldMapPath = "Assets/Scenes/WorldMap.unity/WorldMap.unity";
        if (!System.Array.Exists(scenes, s => s.path == worldMapPath))
        {
            sceneList.Add(new EditorBuildSettingsScene(worldMapPath, true));
            addedAny = true;
            UnityEngine.Debug.Log("Added WorldMap to build settings");
        }

        // Add SubZoneMap
        string subZonePath = "Assets/Scenes/SubZoneMap.unity/SubZoneMap.unity";
        if (!System.Array.Exists(scenes, s => s.path == subZonePath))
        {
            sceneList.Add(new EditorBuildSettingsScene(subZonePath, true));
            addedAny = true;
            UnityEngine.Debug.Log("Added SubZoneMap to build settings");
        }

        // Add CharacterSelection
        string charSelectPath = "Assets/Scenes/CharacterSelection.unity";
        if (!System.Array.Exists(scenes, s => s.path == charSelectPath))
        {
            sceneList.Add(new EditorBuildSettingsScene(charSelectPath, true));
            addedAny = true;
            UnityEngine.Debug.Log("Added CharacterSelection to build settings");
        }

        if (addedAny)
        {
            EditorBuildSettings.scenes = sceneList.ToArray();
            UnityEngine.Debug.Log("Build settings updated successfully!");
        }
        else
        {
            UnityEngine.Debug.Log("All scenes already in build settings");
        }
    }
}
