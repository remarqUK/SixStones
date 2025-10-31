using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Creates visual zone icon prefabs with themed colors and symbols
/// </summary>
public class ZoneIconVisuals : MonoBehaviour
{
    // Zone-specific color schemes
    private static readonly Color[] ZoneColors = new Color[]
    {
        new Color(0.4f, 0.8f, 0.3f),    // Verdant Plains - Green
        new Color(0.3f, 0.5f, 0.7f),    // Mystic Forest - Blue
        new Color(0.7f, 0.9f, 1.0f),    // Frozen Peaks - Ice Blue
        new Color(0.9f, 0.7f, 0.3f),    // Desert Wastes - Sandy Yellow
        new Color(0.5f, 0.2f, 0.6f),    // Shadow Realm - Purple
        new Color(1.0f, 0.9f, 0.6f)     // Celestial Heights - Golden
    };
    
    private static readonly Color[] ZoneAccentColors = new Color[]
    {
        new Color(0.2f, 0.5f, 0.1f),    // Verdant Plains - Dark Green
        new Color(0.1f, 0.2f, 0.4f),    // Mystic Forest - Dark Blue
        new Color(0.5f, 0.7f, 0.9f),    // Frozen Peaks - Light Blue
        new Color(0.7f, 0.5f, 0.1f),    // Desert Wastes - Dark Sand
        new Color(0.3f, 0.1f, 0.4f),    // Shadow Realm - Dark Purple
        new Color(0.9f, 0.7f, 0.3f)     // Celestial Heights - Bright Gold
    };
    
    private static readonly string[] ZoneSymbols = new string[]
    {
        "🌿",  // Verdant Plains - Herb
        "🌲",  // Mystic Forest - Tree
        "⛰️",  // Frozen Peaks - Mountain
        "🏜️",  // Desert Wastes - Desert
        "👁️",  // Shadow Realm - Eye
        "⭐"   // Celestial Heights - Star
    };
    
    // Alternative text-based symbols if emojis don't render
    private static readonly string[] ZoneSymbolsAlt = new string[]
    {
        "ZONE 1",   // Verdant Plains
        "ZONE 2",   // Mystic Forest
        "ZONE 3",   // Frozen Peaks
        "ZONE 4",   // Desert Wastes
        "ZONE 5",   // Shadow Realm
        "ZONE 6"    // Celestial Heights
    };
    
    /// <summary>
    /// Setup the visual appearance for a zone icon
    /// </summary>
    public static void SetupZoneIconVisuals(GameObject iconObject, int zoneIndex, string zoneName, bool isUnlocked)
    {
        if (iconObject == null || zoneIndex < 0 || zoneIndex >= ZoneColors.Length)
            return;
        
        // Get or create the main icon image
        Image iconImage = iconObject.GetComponent<Image>();
        if (iconImage == null)
        {
            iconImage = iconObject.AddComponent<Image>();
        }
        
        // Create a rounded square sprite if needed
        if (iconImage.sprite == null)
        {
            iconImage.sprite = CreateRoundedSquareSprite();
        }
        
        // Set the color based on zone
        iconImage.color = isUnlocked ? ZoneColors[zoneIndex] : Color.gray;
        
        // Create or update the border/frame
        GameObject borderObj = iconObject.transform.Find("Border")?.gameObject;
        if (borderObj == null)
        {
            borderObj = new GameObject("Border");
            borderObj.transform.SetParent(iconObject.transform);
            borderObj.transform.SetAsFirstSibling(); // Put behind
            
            RectTransform borderRect = borderObj.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.sizeDelta = new Vector2(10, 10); // Slightly larger
            borderRect.anchoredPosition = Vector2.zero;
            
            Image borderImage = borderObj.AddComponent<Image>();
            borderImage.sprite = CreateRoundedSquareSprite();
            borderImage.color = ZoneAccentColors[zoneIndex];
        }
        
        // Create or update the symbol/icon
        GameObject symbolObj = iconObject.transform.Find("Symbol")?.gameObject;
        if (symbolObj == null)
        {
            symbolObj = new GameObject("Symbol");
            symbolObj.transform.SetParent(iconObject.transform);
            
            RectTransform symbolRect = symbolObj.AddComponent<RectTransform>();
            symbolRect.anchorMin = new Vector2(0.5f, 0.5f);
            symbolRect.anchorMax = new Vector2(0.5f, 0.5f);
            symbolRect.sizeDelta = new Vector2(60, 60);
            symbolRect.anchoredPosition = Vector2.zero;
            
            TextMeshProUGUI symbolText = symbolObj.AddComponent<TextMeshProUGUI>();
            symbolText.text = ZoneSymbolsAlt[zoneIndex]; // Use alt symbols for compatibility
            symbolText.fontSize = 24;
            symbolText.alignment = TextAlignmentOptions.Center;
            symbolText.color = isUnlocked ? ZoneAccentColors[zoneIndex] : new Color(0.3f, 0.3f, 0.3f);
            symbolText.fontStyle = FontStyles.Bold;
        }
        
        // Update existing label if present
        TextMeshProUGUI labelText = iconObject.GetComponentInChildren<TextMeshProUGUI>();
        if (labelText != null && labelText.gameObject.name == "Label")
        {
            labelText.text = zoneName;
            labelText.color = isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f);
        }
        else if (labelText == null)
        {
            // Create label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(iconObject.transform);
            
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0f);
            labelRect.anchorMax = new Vector2(0.5f, 0f);
            labelRect.anchoredPosition = new Vector2(0, -60);
            labelRect.sizeDelta = new Vector2(150, 40);
            
            TextMeshProUGUI text = labelObj.AddComponent<TextMeshProUGUI>();
            text.text = zoneName;
            text.fontSize = 16;
            text.alignment = TextAlignmentOptions.Center;
            text.color = isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            text.fontStyle = FontStyles.Bold;
            text.textWrappingMode = TMPro.TextWrappingModes.Normal;
        }
        
        // Add particle effect holder for future enhancement
        GameObject particlesObj = iconObject.transform.Find("Particles")?.gameObject;
        if (particlesObj == null && isUnlocked)
        {
            particlesObj = new GameObject("Particles");
            particlesObj.transform.SetParent(iconObject.transform);
            particlesObj.transform.localPosition = Vector3.zero;
            // Particle system can be added here later
        }
    }
    
    /// <summary>
    /// Creates a simple rounded square sprite programmatically
    /// </summary>
    private static Sprite CreateRoundedSquareSprite()
    {
        int size = 128;
        int cornerRadius = 16;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        
        Color[] pixels = new Color[size * size];
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Calculate distance from corners
                bool isInCorner = false;
                
                // Top-left corner
                if (x < cornerRadius && y >= size - cornerRadius)
                {
                    float dx = cornerRadius - x;
                    float dy = y - (size - cornerRadius);
                    isInCorner = dx * dx + dy * dy > cornerRadius * cornerRadius;
                }
                // Top-right corner
                else if (x >= size - cornerRadius && y >= size - cornerRadius)
                {
                    float dx = x - (size - cornerRadius);
                    float dy = y - (size - cornerRadius);
                    isInCorner = dx * dx + dy * dy > cornerRadius * cornerRadius;
                }
                // Bottom-left corner
                else if (x < cornerRadius && y < cornerRadius)
                {
                    float dx = cornerRadius - x;
                    float dy = cornerRadius - y;
                    isInCorner = dx * dx + dy * dy > cornerRadius * cornerRadius;
                }
                // Bottom-right corner
                else if (x >= size - cornerRadius && y < cornerRadius)
                {
                    float dx = x - (size - cornerRadius);
                    float dy = cornerRadius - y;
                    isInCorner = dx * dx + dy * dy > cornerRadius * cornerRadius;
                }
                
                pixels[y * size + x] = isInCorner ? Color.clear : Color.white;
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
    }
}
