using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fixes the PauseMenuPanel RectTransform to fill the entire screen
/// </summary>
public class FixPauseMenuUI : MonoBehaviour
{
    private void Awake()
    {
        // Find the PauseMenuPanel
        GameObject pauseMenuPanel = GameObject.Find("PauseMenuPanel");
        if (pauseMenuPanel == null)
        {
            Debug.LogWarning("FixPauseMenuUI: Could not find PauseMenuPanel");
            return;
        }

        // Get the RectTransform
        RectTransform rectTransform = pauseMenuPanel.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogWarning("FixPauseMenuUI: PauseMenuPanel does not have RectTransform");
            return;
        }

        // Set anchors to stretch across entire screen
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;

        Debug.Log("FixPauseMenuUI: Fixed PauseMenuPanel to fill screen");

        // Also fix the text
        Transform pausedText = pauseMenuPanel.transform.Find("PausedText");
        if (pausedText != null)
        {
            RectTransform textRect = pausedText.GetComponent<RectTransform>();
            if (textRect != null)
            {
                textRect.anchorMin = new Vector2(0, 0);
                textRect.anchorMax = new Vector2(1, 1);
                textRect.anchoredPosition = Vector2.zero;
                textRect.sizeDelta = new Vector2(-40, -40);
                Debug.Log("FixPauseMenuUI: Fixed PausedText to fill panel with margins");
            }
        }

        // Fix the Image color to black
        Image image = pauseMenuPanel.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(0, 0, 0, 0.95f);
            Debug.Log("FixPauseMenuUI: Fixed Image color to black");
        }

        // Destroy this script after fixing (we only need it once)
        Destroy(this);
    }
}
