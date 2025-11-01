using UnityEngine;
using UnityEditor;

/// <summary>
/// One-time script to assign character assets to the CharacterSelectionManager
/// </summary>
public class AssignCharacters : EditorWindow
{
    [MenuItem("Tools/Character Selection/Assign Characters to Manager")]
    public static void AssignCharactersToManager()
    {
        // Find the manager in the scene
        CharacterSelectionManager manager = GameObject.FindFirstObjectByType<CharacterSelectionManager>();
        if (manager == null)
        {
            EditorUtility.DisplayDialog("Error",
                "CharacterSelectionManager not found in scene!\n\n" +
                "Please open the CharacterSelection scene first.",
                "OK");
            return;
        }

        // Load all character assets
        CharacterData knight = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Resources/Characters/Knight.asset");
        CharacterData barbarian = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Resources/Characters/Barbarian.asset");
        CharacterData wizard = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Resources/Characters/Wizard.asset");
        CharacterData rogue = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Resources/Characters/Rogue.asset");
        CharacterData cleric = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Resources/Characters/Cleric.asset");

        // Check if all assets loaded
        if (knight == null || barbarian == null || wizard == null || rogue == null || cleric == null)
        {
            EditorUtility.DisplayDialog("Error",
                "Could not load all character assets!\n\n" +
                "Make sure you've run: Tools > Character Selection > Create Default Characters",
                "OK");
            return;
        }

        // Assign to manager
        SerializedObject so = new SerializedObject(manager);
        SerializedProperty availableCharacters = so.FindProperty("availableCharacters");

        availableCharacters.arraySize = 5;
        availableCharacters.GetArrayElementAtIndex(0).objectReferenceValue = knight;
        availableCharacters.GetArrayElementAtIndex(1).objectReferenceValue = barbarian;
        availableCharacters.GetArrayElementAtIndex(2).objectReferenceValue = wizard;
        availableCharacters.GetArrayElementAtIndex(3).objectReferenceValue = rogue;
        availableCharacters.GetArrayElementAtIndex(4).objectReferenceValue = cleric;

        so.FindProperty("defaultCharacterIndex").intValue = 0; // Knight is default

        so.ApplyModifiedProperties();

        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);

        Debug.Log("[AssignCharacters] Successfully assigned 5 characters to CharacterSelectionManager!");
        EditorUtility.DisplayDialog("Success",
            "Successfully assigned characters to CharacterSelectionManager:\n\n" +
            "1. Knight (default)\n" +
            "2. Barbarian\n" +
            "3. Wizard\n" +
            "4. Rogue\n" +
            "5. Cleric\n\n" +
            "You can now run the scene to test character selection!",
            "OK");
    }
}
