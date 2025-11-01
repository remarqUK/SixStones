using UnityEngine;
using UnityEditor;

/// <summary>
/// Quick script to create Ranger character
/// </summary>
public class CreateRanger : EditorWindow
{
    [MenuItem("Tools/Character Selection/Create Ranger Only")]
    public static void CreateRangerCharacter()
    {
        System.IO.Directory.CreateDirectory("Assets/Resources/Characters");

        CharacterData character = ScriptableObject.CreateInstance<CharacterData>();
        character.characterName = "Ranger";
        character.description = "A skilled archer and tracker, master of wilderness survival";
        character.strength = 12;
        character.dexterity = 16;
        character.constitution = 12;
        character.intelligence = 10;
        character.wisdom = 14;
        character.charisma = 10;
        character.baseHealth = 100;
        character.baseMana = 70;
        character.moveSpeed = 5.5f;
        character.classFeatures = new string[] { "Hunter's Mark", "Favored Enemy", "Natural Explorer" };

        string path = "Assets/Resources/Characters/Ranger.asset";
        AssetDatabase.CreateAsset(character, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Created Ranger character at {path}");
        EditorUtility.DisplayDialog("Success", "Ranger character created!\n\nAssets/Resources/Characters/Ranger.asset", "OK");

        Selection.activeObject = character;
    }
}
