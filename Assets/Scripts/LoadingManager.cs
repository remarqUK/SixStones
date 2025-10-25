using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Manages the loading screen and asynchronous scene loading
/// </summary>
public class LoadingManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string sceneToLoad = "MainMenu";
    [SerializeField] private float minimumLoadTime = 2.0f; // Minimum time to show loading screen

    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;

    private void Start()
    {
        // Ensure canvas is visible
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        // Start loading the game scene
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        // Track minimum load time
        float startTime = Time.time;

        // Start loading the scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);

        // Prevent the scene from activating immediately
        asyncLoad.allowSceneActivation = false;

        // Wait while the scene loads
        while (!asyncLoad.isDone)
        {
            // Scene is loaded when progress reaches 0.9 (Unity limitation)
            if (asyncLoad.progress >= 0.9f)
            {
                // Ensure minimum load time has passed
                float elapsedTime = Time.time - startTime;
                if (elapsedTime < minimumLoadTime)
                {
                    yield return new WaitForSeconds(minimumLoadTime - elapsedTime);
                }

                // Optional: Fade out
                if (canvasGroup != null)
                {
                    yield return StartCoroutine(FadeOut());
                }

                // Activate the scene
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    private IEnumerator FadeOut()
    {
        float fadeTime = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
}
