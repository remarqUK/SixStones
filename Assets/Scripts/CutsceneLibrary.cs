using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject that holds all cutscenes in the game
/// </summary>
[CreateAssetMenu(fileName = "CutsceneLibrary", menuName = "SixStones/Cutscene Library")]
public class CutsceneLibrary : ScriptableObject
{
    [Tooltip("Master list of all cutscenes in the game")]
    public List<Cutscene> cutscenes = new List<Cutscene>();
    
    /// <summary>
    /// Get a cutscene by its ID
    /// </summary>
    public Cutscene GetCutscene(string cutsceneId)
    {
        if (string.IsNullOrEmpty(cutsceneId))
            return null;
        
        return cutscenes.Find(c => c.cutsceneId == cutsceneId);
    }
    
    /// <summary>
    /// Check if a cutscene exists
    /// </summary>
    public bool HasCutscene(string cutsceneId)
    {
        return GetCutscene(cutsceneId) != null;
    }
}
