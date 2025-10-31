using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tracks which cutscenes the player has already viewed
/// </summary>
public class CutsceneProgress : MonoBehaviour
{
    private static CutsceneProgress instance;
    public static CutsceneProgress Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("CutsceneProgress");
                instance = go.AddComponent<CutsceneProgress>();
                DontDestroyOnLoad(go);
                instance.LoadProgress();
            }
            return instance;
        }
    }
    
    private HashSet<string> viewedCutscenes = new HashSet<string>();
    private const string VIEWED_CUTSCENES_KEY = "ViewedCutscenes";
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProgress();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Check if a cutscene has been viewed
    /// </summary>
    public bool HasViewedCutscene(string cutsceneId)
    {
        if (string.IsNullOrEmpty(cutsceneId))
            return true; // No cutscene = already "viewed"
        
        return viewedCutscenes.Contains(cutsceneId);
    }
    
    /// <summary>
    /// Mark a cutscene as viewed
    /// </summary>
    public void MarkCutsceneAsViewed(string cutsceneId)
    {
        if (string.IsNullOrEmpty(cutsceneId))
            return;
        
        if (!viewedCutscenes.Contains(cutsceneId))
        {
            viewedCutscenes.Add(cutsceneId);
            SaveProgress();
            Debug.Log($"Marked cutscene '{cutsceneId}' as viewed");
        }
    }
    
    /// <summary>
    /// Clear a specific cutscene from viewed list (for debugging/testing)
    /// </summary>
    public void ResetCutscene(string cutsceneId)
    {
        if (viewedCutscenes.Contains(cutsceneId))
        {
            viewedCutscenes.Remove(cutsceneId);
            SaveProgress();
            Debug.Log($"Reset cutscene '{cutsceneId}'");
        }
    }
    
    /// <summary>
    /// Clear all viewed cutscenes
    /// </summary>
    public void ResetAllCutscenes()
    {
        viewedCutscenes.Clear();
        SaveProgress();
        Debug.Log("Reset all cutscenes");
    }
    
    /// <summary>
    /// Get list of all viewed cutscene IDs
    /// </summary>
    public string[] GetViewedCutscenes()
    {
        string[] result = new string[viewedCutscenes.Count];
        viewedCutscenes.CopyTo(result);
        return result;
    }
    
    private void SaveProgress()
    {
        string[] cutsceneArray = GetViewedCutscenes();
        string json = JsonUtility.ToJson(new CutsceneData { cutsceneIds = cutsceneArray });
        PlayerPrefs.SetString(VIEWED_CUTSCENES_KEY, json);
        PlayerPrefs.Save();
    }
    
    private void LoadProgress()
    {
        viewedCutscenes.Clear();
        
        string json = PlayerPrefs.GetString(VIEWED_CUTSCENES_KEY, "");
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                CutsceneData data = JsonUtility.FromJson<CutsceneData>(json);
                if (data != null && data.cutsceneIds != null)
                {
                    foreach (string id in data.cutsceneIds)
                    {
                        viewedCutscenes.Add(id);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load cutscene progress: {e.Message}");
            }
        }
    }
    
    [System.Serializable]
    private class CutsceneData
    {
        public string[] cutsceneIds;
    }
}
