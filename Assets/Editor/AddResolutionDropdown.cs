using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor utility to add a resolution dropdown to the Options Menu
/// </summary>
public class AddResolutionDropdown : EditorWindow
{
    [MenuItem("Tools/Add Resolution Dropdown to Options")]
    public static void AddResolutionDropdownToOptions()
    {
        // Load the GlobalOptionsManager prefab
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/GlobalOptionsManager.prefab");
        if (prefab == null)
        {
            Debug.LogError("Could not find GlobalOptionsManager.prefab");
            return;
        }

        // Instantiate the prefab for editing
        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (instance == null)
        {
            Debug.LogError("Could not instantiate prefab");
            return;
        }

        // Find the OptionsMenuController
        OptionsMenuController controller = instance.GetComponentInChildren<OptionsMenuController>();
        if (controller == null)
        {
            Debug.LogError("Could not find OptionsMenuController");
            DestroyImmediate(instance);
            return;
        }

        // Find the language dropdown container as reference
        Transform languageContainer = FindChildRecursive(instance.transform, "LanguageDropdownContainer");
        if (languageContainer == null)
        {
            Debug.LogError("Could not find LanguageDropdownContainer");
            DestroyImmediate(instance);
            return;
        }

        // Duplicate the language dropdown container
        GameObject resolutionContainer = Instantiate(languageContainer.gameObject, languageContainer.parent);
        resolutionContainer.name = "ResolutionDropdownContainer";

        // Position it below the language dropdown
        RectTransform resRect = resolutionContainer.GetComponent<RectTransform>();
        RectTransform langRect = languageContainer.GetComponent<RectTransform>();
        resRect.anchoredPosition = langRect.anchoredPosition + new Vector2(0, -60);

        // Find and update the label
        TMP_Text label = resolutionContainer.GetComponentInChildren<TMP_Text>();
        if (label != null && label.name.Contains("Label"))
        {
            label.text = "Resolution:";
        }

        // Find the dropdown component
        TMP_Dropdown dropdown = resolutionContainer.GetComponentInChildren<TMP_Dropdown>();
        if (dropdown != null)
        {
            // Clear default options
            dropdown.ClearOptions();
            dropdown.options.Add(new TMP_Dropdown.OptionData("1920 x 1080"));

            // Assign to controller using SerializedObject
            SerializedObject serializedController = new SerializedObject(controller);
            SerializedProperty resDropdownProp = serializedController.FindProperty("resolutionDropdown");
            if (resDropdownProp != null)
            {
                resDropdownProp.objectReferenceValue = dropdown;
                serializedController.ApplyModifiedProperties();
                Debug.Log("Resolution dropdown assigned to controller");
            }
        }

        // Apply changes to prefab
        PrefabUtility.SaveAsPrefabAsset(instance, AssetDatabase.GetAssetPath(prefab));
        DestroyImmediate(instance);

        Debug.Log("Resolution dropdown added successfully!");
        EditorUtility.DisplayDialog("Success", "Resolution dropdown has been added to the Options Menu", "OK");
    }

    private static Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform found = FindChildRecursive(child, name);
            if (found != null)
                return found;
        }
        return null;
    }
}
