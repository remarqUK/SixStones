using UnityEngine;
using UnityEditor;

/// <summary>
/// Automatically assigns character assets when the scene loads
/// </summary>
[InitializeOnLoad]
public class AutoAssignCharacters
{
    static AutoAssignCharacters()
    {
        EditorApplication.delayCall += AssignCharactersIfNeeded;
    }

    private static void AssignCharactersIfNeeded()
    {
        // Only run if CharacterSelection scene is active
        var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (activeScene.name != "CharacterSelection")
            return;

        CharacterSelectionManager manager = GameObject.FindFirstObjectByType<CharacterSelectionManager>();
        if (manager == null)
            return;

        SerializedObject so = new SerializedObject(manager);
        SerializedProperty availableCharacters = so.FindProperty("availableCharacters");

        // Check if already assigned
        if (availableCharacters.arraySize == 6)
            return;

        Debug.Log("[AutoAssignCharacters] Assigning character assets...");

        // Load all character assets
        CharacterData knight = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Resources/Characters/Knight.asset");
        CharacterData barbarian = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Resources/Characters/Barbarian.asset");
        CharacterData wizard = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Resources/Characters/Wizard.asset");
        CharacterData rogue = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Resources/Characters/Rogue.asset");
        CharacterData cleric = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Resources/Characters/Cleric.asset");
        CharacterData ranger = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Resources/Characters/Ranger.asset");

        if (knight == null || barbarian == null || wizard == null || rogue == null || cleric == null || ranger == null)
        {
            Debug.LogWarning("[AutoAssignCharacters] Some character assets not found!");
            return;
        }

        // Assign to manager
        availableCharacters.arraySize = 6;
        availableCharacters.GetArrayElementAtIndex(0).objectReferenceValue = knight;
        availableCharacters.GetArrayElementAtIndex(1).objectReferenceValue = barbarian;
        availableCharacters.GetArrayElementAtIndex(2).objectReferenceValue = wizard;
        availableCharacters.GetArrayElementAtIndex(3).objectReferenceValue = rogue;
        availableCharacters.GetArrayElementAtIndex(4).objectReferenceValue = cleric;
        availableCharacters.GetArrayElementAtIndex(5).objectReferenceValue = ranger;

        so.FindProperty("defaultCharacterIndex").intValue = 0; // Knight is default

        so.ApplyModifiedProperties();

        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(activeScene);

        Debug.Log("[AutoAssignCharacters] Successfully assigned 6 characters!");
    }
}
