using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Displays a dialog when the player levels up
/// </summary>
public class LevelUpDialog : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button okButton;

    [Header("Settings")]
    [SerializeField] private bool pauseGameOnShow = true;
    [SerializeField] private bool playSound = true;

    private void Start()
    {
        // Hide dialog initially
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }

        // Wire up OK button
        if (okButton != null)
        {
            okButton.onClick.AddListener(DismissDialog);
        }

        // Subscribe to level up event
        if (LevelSystem.Instance != null)
        {
            LevelSystem.Instance.onLevelUp.AddListener(OnLevelUp);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (okButton != null)
        {
            okButton.onClick.RemoveListener(DismissDialog);
        }

        if (LevelSystem.Instance != null)
        {
            LevelSystem.Instance.onLevelUp.RemoveListener(OnLevelUp);
        }
    }

    /// <summary>
    /// Called when player levels up
    /// </summary>
    private void OnLevelUp(int newLevel)
    {
        ShowLevelUpDialog(newLevel);
    }

    /// <summary>
    /// Show the level up dialog
    /// </summary>
    public void ShowLevelUpDialog(int newLevel)
    {
        if (dialogPanel == null) return;

        // Update text
        if (titleText != null)
        {
            titleText.text = "Level Up!";
        }

        if (messageText != null)
        {
            int goldBonus = CurrencyManager.CalculateLevelUpBonus(newLevel);
            messageText.text = $"Congratulations!\n\nYou have reached <b>Level {newLevel}</b>!\n\n+{goldBonus} Gold";
        }

        // Show dialog
        dialogPanel.SetActive(true);

        // Pause game if desired
        if (pauseGameOnShow)
        {
            Time.timeScale = 0f;
        }

        // Play sound effect (placeholder)
        if (playSound)
        {
            // TODO: Play level up sound effect
            Debug.Log("Level up sound!");
        }

        Debug.Log($"Level up dialog shown for level {newLevel}");
    }

    /// <summary>
    /// Dismiss the dialog
    /// </summary>
    public void DismissDialog()
    {
        if (dialogPanel == null) return;

        // Hide dialog
        dialogPanel.SetActive(false);

        // Unpause game
        if (pauseGameOnShow)
        {
            Time.timeScale = 1f;
        }

        Debug.Log("Level up dialog dismissed");
    }

    /// <summary>
    /// Show a custom dialog with any message
    /// </summary>
    public void ShowCustomDialog(string title, string message)
    {
        if (dialogPanel == null) return;

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (messageText != null)
        {
            messageText.text = message;
        }

        dialogPanel.SetActive(true);

        if (pauseGameOnShow)
        {
            Time.timeScale = 0f;
        }
    }
}
