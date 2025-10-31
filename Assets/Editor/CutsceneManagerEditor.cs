using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Editor window for managing and testing the cutscene system
/// </summary>
public class CutsceneManagerEditor : EditorWindow
{
    private CutsceneLibrary cutsceneLibrary;
    private ZoneConfiguration zoneConfiguration;
    private Vector2 scrollPosition;
    private bool showViewedCutscenes = true;
    private bool showCutsceneList = true;
    private bool showValidation = true;
    
    [MenuItem("Tools/Cutscene Manager")]
    public static void ShowWindow()
    {
        GetWindow<CutsceneManagerEditor>("Cutscene Manager");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Cutscene System Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Asset References
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Asset References", EditorStyles.boldLabel);
        cutsceneLibrary = EditorGUILayout.ObjectField("Cutscene Library", cutsceneLibrary, typeof(CutsceneLibrary), false) as CutsceneLibrary;
        zoneConfiguration = EditorGUILayout.ObjectField("Zone Configuration", zoneConfiguration, typeof(ZoneConfiguration), false) as ZoneConfiguration;
        
        if (GUILayout.Button("Find Assets Automatically"))
        {
            FindAssets();
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space();
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // Cutscene Library Section
        if (cutsceneLibrary != null)
        {
            DrawCutsceneLibrarySection();
        }
        
        EditorGUILayout.Space();
        
        // Validation Section
        if (cutsceneLibrary != null && zoneConfiguration != null)
        {
            DrawValidationSection();
        }
        
        EditorGUILayout.Space();
        
        // Viewed Cutscenes Section
        DrawViewedCutscenesSection();
        
        EditorGUILayout.EndScrollView();
    }
    
    private void FindAssets()
    {
        // Find CutsceneLibrary
        string[] libraryGuids = AssetDatabase.FindAssets("t:CutsceneLibrary");
        if (libraryGuids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(libraryGuids[0]);
            cutsceneLibrary = AssetDatabase.LoadAssetAtPath<CutsceneLibrary>(path);
            Debug.Log($"Found CutsceneLibrary at: {path}");
        }
        
        // Find ZoneConfiguration
        string[] zoneGuids = AssetDatabase.FindAssets("t:ZoneConfiguration");
        if (zoneGuids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(zoneGuids[0]);
            zoneConfiguration = AssetDatabase.LoadAssetAtPath<ZoneConfiguration>(path);
            Debug.Log($"Found ZoneConfiguration at: {path}");
        }
    }
    
    private void DrawCutsceneLibrarySection()
    {
        EditorGUILayout.BeginVertical("box");
        
        showCutsceneList = EditorGUILayout.Foldout(showCutsceneList, $"Cutscene Library ({cutsceneLibrary.cutscenes.Count} cutscenes)", true);
        
        if (showCutsceneList)
        {
            EditorGUI.indentLevel++;
            
            foreach (var cutscene in cutsceneLibrary.cutscenes)
            {
                EditorGUILayout.BeginHorizontal("box");
                
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField($"ID: {cutscene.cutsceneId}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Name: {cutscene.cutsceneName}");
                EditorGUILayout.LabelField($"Type: {cutscene.cutsceneType}");
                EditorGUILayout.LabelField($"Resource: {cutscene.resourceLink}");
                EditorGUILayout.LabelField($"Play Once: {cutscene.playOnce}");
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.BeginVertical(GUILayout.Width(100));
                
                bool hasViewed = Application.isPlaying && CutsceneTracker.Instance.HasViewed(cutscene.cutsceneId);
                GUI.enabled = Application.isPlaying;
                
                if (hasViewed)
                {
                    GUI.color = Color.green;
                    GUILayout.Label("✓ Viewed");
                    GUI.color = Color.white;
                }
                else
                {
                    GUI.color = Color.yellow;
                    GUILayout.Label("○ Not Viewed");
                    GUI.color = Color.white;
                }
                
                if (GUILayout.Button("Test Play"))
                {
                    TestPlayCutscene(cutscene.cutsceneId);
                }
                
                GUI.enabled = true;
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(5);
            }
            
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Create New Cutscene"))
            {
                CreateNewCutscene();
            }
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawValidationSection()
    {
        EditorGUILayout.BeginVertical("box");
        
        showValidation = EditorGUILayout.Foldout(showValidation, "Validation & Usage Report", true);
        
        if (showValidation)
        {
            EditorGUI.indentLevel++;
            
            if (GUILayout.Button("Validate Cutscene References"))
            {
                bool isValid = CutsceneHelper.ValidateCutsceneReferences(zoneConfiguration, cutsceneLibrary);
                if (isValid)
                {
                    EditorUtility.DisplayDialog("Validation Success", "All cutscene references are valid!", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Validation Failed", "Some cutscene references are invalid. Check the console for details.", "OK");
                }
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Generate Usage Report"))
            {
                string report = CutsceneHelper.GetCutsceneUsageReport(zoneConfiguration);
                Debug.Log(report);
                EditorUtility.DisplayDialog("Usage Report", report, "OK");
            }
            
            EditorGUILayout.Space();
            
            // Show quick stats
            EditorGUILayout.LabelField("Quick Stats:", EditorStyles.boldLabel);
            List<string> allIds = CutsceneHelper.GetAllUniqueCutsceneIds(zoneConfiguration);
            EditorGUILayout.LabelField($"Unique cutscenes referenced: {allIds.Count}");
            EditorGUILayout.LabelField($"Zone cutscenes: {CutsceneHelper.GetAllZoneCutsceneIds(zoneConfiguration).Count}");
            EditorGUILayout.LabelField($"SubZone cutscenes: {CutsceneHelper.GetAllSubZoneCutsceneIds(zoneConfiguration).Count}");
            EditorGUILayout.LabelField($"Map cutscenes: {CutsceneHelper.GetAllMapCutsceneIds(zoneConfiguration).Count}");
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawViewedCutscenesSection()
    {
        EditorGUILayout.BeginVertical("box");
        
        GUI.enabled = Application.isPlaying;
        
        showViewedCutscenes = EditorGUILayout.Foldout(showViewedCutscenes, "Viewed Cutscenes (Runtime Only)", true);
        
        if (showViewedCutscenes)
        {
            EditorGUI.indentLevel++;
            
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to manage viewed cutscenes", MessageType.Info);
            }
            else if (cutsceneLibrary != null)
            {
                string report = CutsceneHelper.GetViewedCutscenesReport(cutsceneLibrary);
                EditorGUILayout.TextArea(report, GUILayout.Height(150));
                
                EditorGUILayout.Space();
                
                if (GUILayout.Button("Reset All Viewed Cutscenes"))
                {
                    if (EditorUtility.DisplayDialog("Reset Viewed Cutscenes", 
                        "This will clear all viewed cutscene history. Continue?", 
                        "Yes", "Cancel"))
                    {
                        CutsceneTracker.Instance.ResetViewedCutscenes();
                        Debug.Log("All viewed cutscenes have been reset");
                    }
                }
            }
            
            EditorGUI.indentLevel--;
        }
        
        GUI.enabled = true;
        EditorGUILayout.EndVertical();
    }
    
    private void TestPlayCutscene(string cutsceneId)
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Not In Play Mode", "Enter Play Mode to test cutscenes", "OK");
            return;
        }
        
        CutsceneTracker.Instance.PlayCutscene(cutsceneId, () => {
            Debug.Log($"Test cutscene '{cutsceneId}' completed");
        });
    }
    
    private void CreateNewCutscene()
    {
        // This would add a new cutscene to the library
        // For now, just inform the user to do it manually
        EditorUtility.DisplayDialog("Create Cutscene", 
            "Select your CutsceneLibrary asset in the Project window and add a new cutscene entry in the Inspector.", 
            "OK");
    }
}
