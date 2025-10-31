using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;
using TMPro;

/// <summary>
/// Example implementation of cutscene playback for different cutscene types
/// This is a template - customize it for your game's needs
/// </summary>
public class CutscenePlayer : MonoBehaviour
{
    [Header("Video Cutscenes")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoDisplay;
    [SerializeField] private GameObject videoContainer;
    
    [Header("Dialogue Cutscenes")]
    [SerializeField] private GameObject dialogueContainer;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private Image dialogueBackground;
    
    [Header("UI Controls")]
    [SerializeField] private GameObject skipButton;
    [SerializeField] private TextMeshProUGUI skipPromptText;
    
    private System.Action onCompleteCallback;
    private bool isPlaying = false;
    private bool canSkip = true;
    
    private void Awake()
    {
        // Hide all cutscene UI by default
        HideAllCutsceneUI();
    }
    
    private void Update()
    {
        if (isPlaying && canSkip)
        {
            // Check for skip input
            if (Input.GetKeyDown(KeyCode.Escape) || 
                Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.Return))
            {
                SkipCutscene();
            }
        }
    }
    
    /// <summary>
    /// Main entry point for playing cutscenes
    /// Call this from CutsceneTracker.PlayCutscene()
    /// </summary>
    public void PlayCutscene(Cutscene cutscene, System.Action onComplete)
    {
        if (cutscene == null)
        {
            Debug.LogWarning("Cannot play null cutscene");
            onComplete?.Invoke();
            return;
        }
        
        isPlaying = true;
        onCompleteCallback = onComplete;
        canSkip = !cutscene.playOnce; // Can't skip first-time story cutscenes
        
        // Show skip prompt if applicable
        if (skipButton != null)
            skipButton.SetActive(canSkip);
        
        // Route to appropriate player based on type
        switch (cutscene.cutsceneType)
        {
            case CutsceneType.Video:
                PlayVideoCutscene(cutscene);
                break;
                
            case CutsceneType.Dialogue:
                PlayDialogueCutscene(cutscene);
                break;
                
            case CutsceneType.Scene:
                PlaySceneCutscene(cutscene);
                break;
                
            case CutsceneType.Prefab:
                PlayPrefabCutscene(cutscene);
                break;
                
            default:
                Debug.LogWarning($"Cutscene type {cutscene.cutsceneType} not implemented");
                CompleteCutscene();
                break;
        }
    }
    
    #region Video Cutscenes
    
    private void PlayVideoCutscene(Cutscene cutscene)
    {
        if (videoPlayer == null || videoDisplay == null)
        {
            Debug.LogError("Video player components not assigned!");
            CompleteCutscene();
            return;
        }
        
        // Show video container
        if (videoContainer != null)
            videoContainer.SetActive(true);
        
        // Setup video player
        videoPlayer.url = cutscene.resourceLink;
        videoPlayer.loopPointReached += OnVideoFinished;
        
        // Prepare the video
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += (vp) => {
            videoPlayer.Play();
        };
        
        Debug.Log($"Playing video cutscene: {cutscene.cutsceneName}");
    }
    
    private void OnVideoFinished(VideoPlayer vp)
    {
        vp.loopPointReached -= OnVideoFinished;
        CompleteCutscene();
    }
    
    #endregion
    
    #region Dialogue Cutscenes
    
    private void PlayDialogueCutscene(Cutscene cutscene)
    {
        if (dialogueContainer == null || dialogueText == null)
        {
            Debug.LogError("Dialogue UI components not assigned!");
            CompleteCutscene();
            return;
        }
        
        // Show dialogue container
        dialogueContainer.SetActive(true);
        
        // In a real implementation, you'd load dialogue data from the resourceLink
        // For this example, we'll show placeholder text
        StartCoroutine(ShowDialogueSequence(cutscene));
    }
    
    private IEnumerator ShowDialogueSequence(Cutscene cutscene)
    {
        // Example dialogue sequence - replace with actual dialogue system
        string[] dialogueLines = new string[]
        {
            "This is an example dialogue cutscene.",
            $"Loading from: {cutscene.resourceLink}",
            "In a real implementation, you'd load dialogue data here.",
            "Press Space or Escape to skip..."
        };
        
        foreach (string line in dialogueLines)
        {
            if (!isPlaying) break; // If skipped
            
            dialogueText.text = line;
            yield return new WaitForSeconds(2f);
        }
        
        CompleteCutscene();
    }
    
    #endregion
    
    #region Scene Cutscenes
    
    private void PlaySceneCutscene(Cutscene cutscene)
    {
        Debug.Log($"Loading scene cutscene: {cutscene.resourceLink}");
        
        // Load the cutscene scene additively
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            cutscene.resourceLink, 
            UnityEngine.SceneManagement.LoadSceneMode.Additive
        );
        
        // In a real implementation, the scene would send a completion event
        // For now, complete after the estimated duration
        if (cutscene.duration > 0)
        {
            StartCoroutine(CompleteAfterDelay(cutscene.duration));
        }
        else
        {
            // Default to 5 seconds if no duration specified
            StartCoroutine(CompleteAfterDelay(5f));
        }
    }
    
    #endregion
    
    #region Prefab Cutscenes
    
    private void PlayPrefabCutscene(Cutscene cutscene)
    {
        Debug.Log($"Instantiating prefab cutscene: {cutscene.resourceLink}");
        
        // Load and instantiate the prefab
        GameObject prefab = Resources.Load<GameObject>(cutscene.resourceLink);
        
        if (prefab != null)
        {
            GameObject instance = Instantiate(prefab);
            
            // Look for a cutscene controller component on the prefab
            ICutsceneController controller = instance.GetComponent<ICutsceneController>();
            if (controller != null)
            {
                controller.Play(() => {
                    Destroy(instance);
                    CompleteCutscene();
                });
            }
            else
            {
                // No controller found, complete after duration
                if (cutscene.duration > 0)
                {
                    StartCoroutine(CompleteAfterDelayAndDestroy(cutscene.duration, instance));
                }
                else
                {
                    Destroy(instance);
                    CompleteCutscene();
                }
            }
        }
        else
        {
            Debug.LogError($"Could not load prefab: {cutscene.resourceLink}");
            CompleteCutscene();
        }
    }
    
    #endregion
    
    #region Completion & Cleanup
    
    private void SkipCutscene()
    {
        if (!canSkip) return;
        
        Debug.Log("Skipping cutscene");
        
        // Stop any playing video
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }
        
        // Stop any coroutines
        StopAllCoroutines();
        
        CompleteCutscene();
    }
    
    private void CompleteCutscene()
    {
        isPlaying = false;
        
        // Hide all UI
        HideAllCutsceneUI();
        
        // Call completion callback
        onCompleteCallback?.Invoke();
        onCompleteCallback = null;
        
        Debug.Log("Cutscene completed");
    }
    
    private void HideAllCutsceneUI()
    {
        if (videoContainer != null)
            videoContainer.SetActive(false);
        
        if (dialogueContainer != null)
            dialogueContainer.SetActive(false);
        
        if (skipButton != null)
            skipButton.SetActive(false);
    }
    
    private IEnumerator CompleteAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CompleteCutscene();
    }
    
    private IEnumerator CompleteAfterDelayAndDestroy(float delay, GameObject obj)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
            Destroy(obj);
        CompleteCutscene();
    }
    
    #endregion
}

/// <summary>
/// Interface for prefab-based cutscenes to implement
/// </summary>
public interface ICutsceneController
{
    void Play(System.Action onComplete);
}
