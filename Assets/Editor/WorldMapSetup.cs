using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor utility to setup the WorldMap scene
/// </summary>
public class WorldMapSetup : Editor
{
    [MenuItem("Tools/Setup World Map Scene")]
    public static void SetupWorldMapScene()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create EventSystem if it doesn't exist
        if (GameObject.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
        
        // Create main panel
        GameObject panelObj = new GameObject("WorldMapPanel");
        panelObj.transform.SetParent(canvasObj.transform);
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;
        
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.15f, 1f); // Dark blue background
        
        // Create title text
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelObj.transform);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -50);
        titleRect.sizeDelta = new Vector2(600, 60);
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "SELECT YOUR DESTINATION";
        titleText.fontSize = 36;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;
        
        // Create zone icon container
        GameObject containerObj = new GameObject("ZoneIconContainer");
        containerObj.transform.SetParent(panelObj.transform);
        RectTransform containerRect = containerObj.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = new Vector2(1200, 200);
        
        // Create instructions text
        GameObject instructionsObj = new GameObject("Instructions");
        instructionsObj.transform.SetParent(panelObj.transform);
        RectTransform instructionsRect = instructionsObj.AddComponent<RectTransform>();
        instructionsRect.anchorMin = new Vector2(0.5f, 0f);
        instructionsRect.anchorMax = new Vector2(0.5f, 0f);
        instructionsRect.anchoredPosition = new Vector2(0, 50);
        instructionsRect.sizeDelta = new Vector2(800, 40);
        
        TextMeshProUGUI instructionsText = instructionsObj.AddComponent<TextMeshProUGUI>();
        instructionsText.text = "← → to navigate | ENTER/SPACE to select | ESC to return";
        instructionsText.fontSize = 18;
        instructionsText.alignment = TextAlignmentOptions.Center;
        instructionsText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        
        // Create WorldMapManager GameObject
        GameObject managerObj = new GameObject("WorldMapManager");
        WorldMapManager manager = managerObj.AddComponent<WorldMapManager>();
        
        // Try to find ZoneConfiguration asset
        string[] guids = AssetDatabase.FindAssets("t:ZoneConfiguration");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            ZoneConfiguration zoneConfig = AssetDatabase.LoadAssetAtPath<ZoneConfiguration>(path);
            
            // Use reflection to set the private field
            var field = typeof(WorldMapManager).GetField("zoneConfig", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(manager, zoneConfig);
            }
        }
        
        // Set the container reference using reflection
        var containerField = typeof(WorldMapManager).GetField("zoneIconContainer", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (containerField != null)
        {
            containerField.SetValue(manager, containerObj.transform);
        }
        
        // Select the manager for easy inspector access
        Selection.activeGameObject = managerObj;
        
        Debug.Log("WorldMap scene setup complete! Please assign ZoneConfiguration in the WorldMapManager inspector if not auto-assigned.");
    }
}
