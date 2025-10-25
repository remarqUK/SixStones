using UnityEngine;

/// <summary>
/// Simple component to wire up language dropdown to LocalizationManager
/// </summary>
public class LanguageSettings : MonoBehaviour
{
    /// <summary>
    /// Called by dropdown when value changes
    /// </summary>
    public void SetLanguageFromDropdown(int index)
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.SetLanguageFromDropdown(index);
        }
    }
}
