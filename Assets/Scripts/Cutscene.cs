using UnityEngine;
using System;

/// <summary>
/// Defines a cutscene with its metadata and resource location
/// </summary>
[Serializable]
public class Cutscene
{
    [Tooltip("Unique identifier for this cutscene")]
    public string cutsceneId;
    
    [Tooltip("Display name for the cutscene")]
    public string cutsceneName;
    
    [Tooltip("Path to the cutscene resource (video, scene, or prefab)")]
    public string resourceLink;
    
    [Tooltip("Type of cutscene resource")]
    public CutsceneType cutsceneType;
    
    [Tooltip("Should this cutscene play only once?")]
    public bool playOnce = true;
    
    [Tooltip("Duration in seconds (optional, for videos)")]
    public float duration = 0f;
}

public enum CutsceneType
{
    Video,          // Video file
    Scene,          // Unity scene
    Prefab,         // Prefab with animation/dialogue
    Dialogue        // Text-based dialogue system
}