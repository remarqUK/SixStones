using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor utility to setup the SubZoneMap scene
/// </summary>
public class SubZoneMapSetup : Editor
{
    [MenuItem("Tools/Setup SubZone Map Scene")]
    public static void SetupSubZoneMapScene()
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
        GameObject panelObj = new GameObject("SubZoneMapPanel");
        panelObj.transform.SetParent(canvasObj.transform);
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;
        
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.15f, 1f);
        
        // Create zone name title
        GameObject titleObj = new GameObject("ZoneNameTitle");
        titleObj.transform.SetParent(panelObj.transform);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -50);
        titleRect.sizeDelta = new Vector2(600, 60);
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "SELECT DISTRICT";
        titleText.fontSize = 36;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;
        
        // Create subzone icon container
        GameObject containerObj = new GameObject("SubZoneIconContainer");
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
        instructionsText.text = "← → / D-Pad to navigate | ENTER / SPACE / A-Button to select | ESC / B-Button to return";
        instructionsText.fontSize = 18;
        instructionsText.alignment = TextAlignmentOptions.Center;
        instructionsText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        
        // Create Main Camera
        GameObject cameraObj = new GameObject("Main Camera");
        cameraObj.tag = "MainCamera";
        
        Camera camera = cameraObj.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f);
        camera.orthographic = false;
        camera.fieldOfView = 60f;
        camera.nearClipPlane = 0.3f;
        camera.farClipPlane = 1000f;
        
        cameraObj.AddComponent<AudioListener>();
        cameraObj.transform.position = new Vector3(0, 0, -10);
        
        // Add URP camera data if using URP
        var urpCameraType = System.Type.GetType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
        if (urpCameraType != null)
        {
            cameraObj.AddComponent(urpCameraType);
        }
        
        // Create SubZoneMapManager GameObject
        GameObject managerObj = new GameObject("SubZoneMapManager");
        SubZoneMapManager manager = managerObj.AddComponent<SubZoneMapManager>();
        
        // Try to find ZoneConfiguration asset
        string[] guids = AssetDatabase.FindAssets("t:ZoneConfiguration");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            ZoneConfiguration zoneConfig = AssetDatabase.LoadAssetAtPath<ZoneConfiguration>(path);
            
            var field = typeof(SubZoneMapManager).GetField("zoneConfig", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(manager, zoneConfig);
            }
        }
        
        // Set the container reference
        var containerField = typeof(SubZoneMapManager).GetField("subZoneIconContainer", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (containerField != null)
        {
            containerField.SetValue(manager, containerObj.transform);
        }
        
        // Set the zone name text reference
        var titleField = typeof(SubZoneMapManager).GetField("zoneNameText", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (titleField != null)
        {
            titleField.SetValue(manager, titleText);
        }
        
        Selection.activeGameObject = managerObj;
        
        Debug.Log("SubZoneMap scene setup complete!");
    }
}
