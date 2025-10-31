using UnityEngine;
using System;

/// <summary>
/// ScriptableObject to manage all cutscenes in the game
/// </summary>
[CreateAssetMenu(fileName = "CutsceneDatabase", menuName = "SixStones/Cutscene Database")]
public class CutsceneDatabase : ScriptableObject
{
    [SerializeField] private Cutscene[] cutscenes;
    
    public Cutscene GetCutsceneById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;
        
        foreach (var cutscene in cutscenes)
        {
            if (cutscene.cutsceneId == id)
                return cutscene;
        }
        
        return null;
    }
    
    public Cutscene[] GetAllCutscenes()
    {
        return cutscenes;
    }
}