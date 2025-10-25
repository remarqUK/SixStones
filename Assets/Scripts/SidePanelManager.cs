using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the side panels that appear on the left and right of the game board
/// </summary>
public class SidePanelManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform leftPanel;
    [SerializeField] private RectTransform rightPanel;
    [SerializeField] private Image leftPanelImage;
    [SerializeField] private Image rightPanelImage;
    [SerializeField] private Outline leftPanelOutline;
    [SerializeField] private Outline rightPanelOutline;

    [Header("Panel Settings")]
    [SerializeField] private Color panelColor = new Color(0.1f, 0.1f, 0.15f, 1f);
    [SerializeField] private float panelWidth = 300f; // Width in pixels

    [Header("Border Settings")]
    [SerializeField] private Color activeBorderColor = new Color(1f, 0.8f, 0.2f, 1f); // Gold color
    [SerializeField] private Vector2 borderEffectDistance = new Vector2(3f, 3f);

    private void Start()
    {
        UpdatePanels();
        SetupBorders();

        // Set initial active player (Player 1)
        SetActivePlayer(PlayerManager.Player.Player1);
    }

    private void SetupBorders()
    {
        // Configure left panel border
        if (leftPanelOutline != null)
        {
            leftPanelOutline.effectColor = activeBorderColor;
            leftPanelOutline.effectDistance = borderEffectDistance;
            leftPanelOutline.enabled = false; // Initially hidden
        }

        // Configure right panel border
        if (rightPanelOutline != null)
        {
            rightPanelOutline.effectColor = activeBorderColor;
            rightPanelOutline.effectDistance = borderEffectDistance;
            rightPanelOutline.enabled = false; // Initially hidden
        }
    }

    private void UpdatePanels()
    {
        // Set panel colors
        if (leftPanelImage != null)
        {
            leftPanelImage.color = panelColor;
        }

        if (rightPanelImage != null)
        {
            rightPanelImage.color = panelColor;
        }

        // Update panel sizes and positions
        if (leftPanel != null)
        {
            leftPanel.sizeDelta = new Vector2(panelWidth, 0);
        }

        if (rightPanel != null)
        {
            rightPanel.sizeDelta = new Vector2(panelWidth, 0);
        }
    }

    /// <summary>
    /// Set the width of the side panels
    /// </summary>
    public void SetPanelWidth(float width)
    {
        panelWidth = width;
        UpdatePanels();
    }

    /// <summary>
    /// Set the color of the side panels
    /// </summary>
    public void SetPanelColor(Color color)
    {
        panelColor = color;
        UpdatePanels();
    }

    /// <summary>
    /// Set which player's panel should show the active border
    /// </summary>
    public void SetActivePlayer(PlayerManager.Player activePlayer)
    {
        if (leftPanelOutline != null)
        {
            leftPanelOutline.enabled = (activePlayer == PlayerManager.Player.Player1);
        }

        if (rightPanelOutline != null)
        {
            rightPanelOutline.enabled = (activePlayer == PlayerManager.Player.Player2);
        }

        Debug.Log($"SidePanelManager: Border set to {activePlayer}");
    }

    /// <summary>
    /// Set the border color
    /// </summary>
    public void SetBorderColor(Color color)
    {
        activeBorderColor = color;
        if (leftPanelOutline != null)
        {
            leftPanelOutline.effectColor = color;
        }
        if (rightPanelOutline != null)
        {
            rightPanelOutline.effectColor = color;
        }
    }

    /// <summary>
    /// Set the border thickness
    /// </summary>
    public void SetBorderThickness(Vector2 thickness)
    {
        borderEffectDistance = thickness;
        if (leftPanelOutline != null)
        {
            leftPanelOutline.effectDistance = thickness;
        }
        if (rightPanelOutline != null)
        {
            rightPanelOutline.effectDistance = thickness;
        }
    }
}
