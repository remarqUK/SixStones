using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simple component for save slot buttons in the load game browser
/// This script doesn't need much logic - it's just a container for the button and text
/// The MainMenuManager will populate and wire up the buttons dynamically
/// </summary>
[RequireComponent(typeof(Button))]
public class SaveSlotButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI slotText;
    [SerializeField] private Image backgroundImage;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    /// <summary>
    /// Set the slot information display text
    /// </summary>
    public void SetSlotInfo(string info)
    {
        if (slotText != null)
        {
            slotText.text = info;
        }
    }

    /// <summary>
    /// Enable or disable this slot button
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }
    }

    /// <summary>
    /// Set the text color
    /// </summary>
    public void SetTextColor(Color color)
    {
        if (slotText != null)
        {
            slotText.color = color;
        }
    }

    /// <summary>
    /// Set the background color
    /// </summary>
    public void SetBackgroundColor(Color color)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = color;
        }
    }
}
