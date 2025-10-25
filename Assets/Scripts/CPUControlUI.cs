using UnityEngine;
using TMPro;

/// <summary>
/// UI controller for CPU auto-play toggle button
/// </summary>
public class CPUControlUI : MonoBehaviour
{
    [SerializeField] private CPUPlayer cpuPlayer;
    [SerializeField] private TextMeshProUGUI buttonText;

    private void Start()
    {
        Debug.Log($"CPUControlUI Start - cpuPlayer: {(cpuPlayer != null ? "OK" : "NULL")}, buttonText: {(buttonText != null ? "OK" : "NULL")}");
        UpdateButtonText();
    }

    /// <summary>
    /// Called by the toggle button
    /// </summary>
    public void OnToggleAutoPlay()
    {
        Debug.Log($"OnToggleAutoPlay called - cpuPlayer is {(cpuPlayer != null ? "not null" : "NULL")}");
        if (cpuPlayer != null)
        {
            cpuPlayer.ToggleAutoPlay();
            UpdateButtonText();
        }
        else
        {
            Debug.LogError("CPUPlayer reference is null in CPUControlUI!");
        }
    }

    /// <summary>
    /// Called by the manual move button (when auto-play is off)
    /// </summary>
    public void OnManualMove()
    {
        Debug.Log($"OnManualMove called - cpuPlayer is {(cpuPlayer != null ? "not null" : "NULL")}");
        if (cpuPlayer != null)
        {
            cpuPlayer.MakeMoveNow();
        }
        else
        {
            Debug.LogError("CPUPlayer reference is null in CPUControlUI!");
        }
    }

    private void UpdateButtonText()
    {
        Debug.Log($"UpdateButtonText - buttonText: {(buttonText != null ? "OK" : "NULL")}, cpuPlayer: {(cpuPlayer != null ? "OK" : "NULL")}");
        if (buttonText != null && cpuPlayer != null)
        {
            buttonText.text = cpuPlayer.AutoPlay ? "Auto-Play: ON" : "Auto-Play: OFF";
            Debug.Log($"Button text updated to: {buttonText.text}");
        }
        else
        {
            Debug.LogWarning($"Cannot update button text - buttonText null: {buttonText == null}, cpuPlayer null: {cpuPlayer == null}");
        }
    }
}
