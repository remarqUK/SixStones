using UnityEngine;
using TMPro;

/// <summary>
/// Simple pulsing animation for loading text
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class LoadingTextPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float minAlpha = 0.3f;
    [SerializeField] private float maxAlpha = 1f;

    private TextMeshProUGUI textComponent;
    private Color originalColor;

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        originalColor = textComponent.color;
    }

    private void Update()
    {
        // Pulse alpha using sine wave
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
        Color newColor = originalColor;
        newColor.a = alpha;
        textComponent.color = newColor;
    }
}
