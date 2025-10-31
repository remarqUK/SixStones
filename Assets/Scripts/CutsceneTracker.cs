using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tracks which cutscenes have been viewed by the player
/// </summary>
public class CutsceneTracker : MonoBehaviour
{
    private static CutsceneTracker instance;
    public static CutsceneTracker Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("CutsceneTracker");
                instance = obj.AddComponent<CutsceneTracker>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }
    
    [SerializeField] private CutsceneLibrary cutsceneLibrary;
    
    private HashSet<string> viewedCutscenes = new HashSet<string>();
    private const string VIEWED_CUTSCENES_KEY = "ViewedCutscenes";
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        LoadViewedCutscenes();
    }
    
    public bool HasViewed(string cutsceneId)
    {
        if (string.IsNullOrEmpty(cutsceneId))
            return false;
        
        return viewedCutscenes.Contains(cutsceneId);
    }
    
    public void MarkAsViewed(string cutsceneId)
    {
        if (string.IsNullOrEmpty(cutsceneId))
            return;
        
        if (!viewedCutscenes.Contains(cutsceneId))
        {
            viewedCutscenes.Add(cutsceneId);
            SaveViewedCutscenes();
            Debug.Log($"Cutscene '{cutsceneId}' marked as viewed");
        }
    }
    
    public bool ShouldPlayCutscene(string cutsceneId)
    {
        if (string.IsNullOrEmpty(cutsceneId))
            return false;
        
        if (cutsceneLibrary == null)
        {
            Debug.LogWarning("CutsceneLibrary not assigned!");
            return false;
        }
        
        Cutscene cutscene = cutsceneLibrary.GetCutscene(cutsceneId);
        if (cutscene == null)
        {
            Debug.LogWarning($"Cutscene '{cutsceneId}' not found in library");
            return false;
        }
        
        if (cutscene.playOnce && HasViewed(cutsceneId))
        {
            return false;
        }
        
        return true;
    }
    
    public void PlayCutscene(string cutsceneId, System.Action onComplete = null)
    {
        if (!ShouldPlayCutscene(cutsceneId))
        {
            onComplete?.Invoke();
            return;
        }
        
        Cutscene cutscene = cutsceneLibrary.GetCutscene(cutsceneId);
        if (cutscene == null)
        {
            onComplete?.Invoke();
            return;
        }
        
        Debug.Log($"Playing cutscene: {cutscene.cutsceneName} ({cutscene.cutsceneType})");
        
        MarkAsViewed(cutsceneId);
        onComplete?.Invoke();
    }
    
    public void ResetViewedCutscenes()
    {
        viewedCutscenes.Clear();
        SaveViewedCutscenes();
        Debug.Log("All viewed cutscenes reset");
    }
    
    private void SaveViewedCutscenes()
    {
        string json = JsonUtility.ToJson(new ViewedCutscenesList { cutsceneIds = new List<string>(viewedCutscenes) });
        PlayerPrefs.SetString(VIEWED_CUTSCENES_KEY, json);
        PlayerPrefs.Save();
    }
    
    private void LoadViewedCutscenes()
    {
        string json = PlayerPrefs.GetString(VIEWED_CUTSCENES_KEY, "");
        if (!string.IsNullOrEmpty(json))
        {
            ViewedCutscenesList list = JsonUtility.FromJson<ViewedCutscenesList>(json);
            viewedCutscenes = new HashSet<string>(list.cutsceneIds);
        }
    }
    
    [System.Serializable]
    private class ViewedCutscenesList
    {
        public List<string> cutsceneIds;
    }
}
