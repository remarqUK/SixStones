using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// Editor tool to create the loading screen scene
/// </summary>
public class LoadingScreenSetup : EditorWindow
{
    [MenuItem("Tools/Create Loading Screen")]
    public static void CreateLoadingScreen()
    {
        // Create new scene
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Create Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // Add CanvasGroup for fading
        CanvasGroup canvasGroup = canvasObj.AddComponent<CanvasGroup>();

        // Create Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform, false);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.15f, 1f); // Dark blue-gray background
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;

        // Create Title Text "Six Stones"
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Six Stones";
        titleText.fontSize = 120;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(0.9f, 0.85f, 0.7f, 1f); // Warm golden color
        titleText.fontStyle = FontStyles.Bold;

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0, 0);
        titleRect.sizeDelta = new Vector2(800, 200);

        // Create Loading Text
        GameObject loadingObj = new GameObject("LoadingText");
        loadingObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI loadingText = loadingObj.AddComponent<TextMeshProUGUI>();
        loadingText.text = "Loading...";
        loadingText.fontSize = 36;
        loadingText.alignment = TextAlignmentOptions.Center;
        loadingText.color = new Color(0.7f, 0.7f, 0.7f, 1f);

        RectTransform loadingRect = loadingObj.GetComponent<RectTransform>();
        loadingRect.anchorMin = new Vector2(0.5f, 0.5f);
        loadingRect.anchorMax = new Vector2(0.5f, 0.5f);
        loadingRect.pivot = new Vector2(0.5f, 0.5f);
        loadingRect.anchoredPosition = new Vector2(0, -150);
        loadingRect.sizeDelta = new Vector2(400, 100);

        // Add pulsing animation to loading text
        LoadingTextPulse pulseComponent = loadingObj.AddComponent<LoadingTextPulse>();

        // Create LoadingManager
        GameObject managerObj = new GameObject("LoadingManager");
        LoadingManager loadingManager = managerObj.AddComponent<LoadingManager>();

        // Use reflection to set the canvasGroup reference
        var canvasGroupField = typeof(LoadingManager).GetField("canvasGroup",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (canvasGroupField != null)
        {
            canvasGroupField.SetValue(loadingManager, canvasGroup);
        }

        // Create EventSystem if it doesn't exist
        if (GameObject.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // Create Main Camera
        GameObject cameraObj = new GameObject("Main Camera");
        Camera camera = cameraObj.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f);
        camera.orthographic = true;
        camera.orthographicSize = 5;
        cameraObj.tag = "MainCamera";
        cameraObj.AddComponent<AudioListener>();

        // Add Universal Additional Camera Data for URP (Unity 6)
        var cameraDataType = System.Type.GetType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
        if (cameraDataType != null)
        {
            cameraObj.AddComponent(cameraDataType);
        }

        // Save the scene
        string scenePath = "Assets/Scenes/LoadingScreen.unity";
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(newScene, scenePath);

        Debug.Log($"Loading screen created at {scenePath}");
        Debug.Log("Next steps:");
        Debug.Log("1. Add LoadingScreen scene to Build Settings (File > Build Settings)");
        Debug.Log("2. Make sure LoadingScreen is the FIRST scene (index 0)");
        Debug.Log("3. Add Match3 scene as the second scene (index 1)");

        EditorUtility.DisplayDialog("Loading Screen Created",
            "Loading screen scene created successfully!\n\n" +
            "Next steps:\n" +
            "1. Go to File > Build Settings\n" +
            "2. Add LoadingScreen scene (drag it to the list)\n" +
            "3. Make sure LoadingScreen is at index 0\n" +
            "4. Add Match3 scene at index 1",
            "OK");
    }
}
