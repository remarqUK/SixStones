using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Creates visual subzone icon placeholders with themed styling
/// </summary>
public class SubZoneIconVisuals : MonoBehaviour
{
    // District-specific color schemes (gradient from light to dark blue)
    private static readonly Color[] SubZoneColors = new Color[]
    {
        new Color(0.7f, 0.7f, 1.0f),    // District 1 - Light Blue
        new Color(0.6f, 0.6f, 0.95f),   // District 2 - 
        new Color(0.5f, 0.5f, 0.9f),    // District 3 -
        new Color(0.4f, 0.4f, 0.85f),   // District 4 -
        new Color(0.35f, 0.35f, 0.8f),  // District 5 -
        new Color(0.3f, 0.3f, 0.75f)    // District 6 - Dark Blue
    };
    
    private static readonly Color[] SubZoneBorderColors = new Color[]
    {
        new Color(0.5f, 0.5f, 0.9f),    // District 1 border
        new Color(0.45f, 0.45f, 0.85f), // District 2 border
        new Color(0.4f, 0.4f, 0.8f),    // District 3 border
        new Color(0.35f, 0.35f, 0.75f), // District 4 border
        new Color(0.3f, 0.3f, 0.7f),    // District 5 border
        new Color(0.25f, 0.25f, 0.65f)  // District 6 border
    };
    
    /// <summary>
    /// Setup the visual appearance for a subzone icon
    /// </summary>
    public static void SetupSubZoneIconVisuals(GameObject iconObject, int subZoneIndex, string subZoneName, bool isUnlocked)
    {
        if (iconObject == null || subZoneIndex < 0 || subZoneIndex >= SubZoneColors.Length)
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
        
        // Set the color based on subzone index
        iconImage.color = isUnlocked ? SubZoneColors[subZoneIndex] : Color.gray;
        
        // Create or update the border/frame
        GameObject borderObj = iconObject.transform.Find("Border")?.gameObject;
        if (borderObj == null)
        {
            borderObj = new GameObject("Border");
            borderObj.transform.SetParent(iconObject.transform);
            borderObj.transform.SetAsFirstSibling();
            
            RectTransform borderRect = borderObj.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.sizeDelta = new Vector2(10, 10);
            borderRect.anchoredPosition = Vector2.zero;
            
            Image borderImage = borderObj.AddComponent<Image>();
            borderImage.sprite = CreateRoundedSquareSprite();
            borderImage.color = isUnlocked ? SubZoneBorderColors[subZoneIndex] : new Color(0.3f, 0.3f, 0.3f);
        }
        
        // Create or update the number display
        GameObject numberObj = iconObject.transform.Find("Number")?.gameObject;
        if (numberObj == null)
        {
            numberObj = new GameObject("Number");
            numberObj.transform.SetParent(iconObject.transform);
            
            RectTransform numberRect = numberObj.AddComponent<RectTransform>();
            numberRect.anchorMin = new Vector2(0.5f, 0.5f);
            numberRect.anchorMax = new Vector2(0.5f, 0.5f);
            numberRect.sizeDelta = new Vector2(80, 80);
            numberRect.anchoredPosition = Vector2.zero;
            
            TextMeshProUGUI numberText = numberObj.AddComponent<TextMeshProUGUI>();
            numberText.text = (subZoneIndex + 1).ToString();
            numberText.fontSize = 56;
            numberText.alignment = TextAlignmentOptions.Center;
            numberText.color = isUnlocked ? Color.white : new Color(0.4f, 0.4f, 0.4f);
            numberText.fontStyle = FontStyles.Bold;
            
            // Add subtle shadow for depth
            var shadow = numberObj.AddComponent<UnityEngine.UI.Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.5f);
            shadow.effectDistance = new Vector2(2, -2);
        }
        else
        {
            TextMeshProUGUI numberText = numberObj.GetComponent<TextMeshProUGUI>();
            if (numberText != null)
            {
                numberText.color = isUnlocked ? Color.white : new Color(0.4f, 0.4f, 0.4f);
            }
        }
        
        // Create progress indicator (small circles showing map completion)
        GameObject progressObj = iconObject.transform.Find("Progress")?.gameObject;
        if (progressObj == null && isUnlocked)
        {
            progressObj = new GameObject("Progress");
            progressObj.transform.SetParent(iconObject.transform);
            
            RectTransform progressRect = progressObj.AddComponent<RectTransform>();
            progressRect.anchorMin = new Vector2(0.5f, 0f);
            progressRect.anchorMax = new Vector2(0.5f, 0f);
            progressRect.anchoredPosition = new Vector2(0, 15);
            progressRect.sizeDelta = new Vector2(90, 10);
            
            // Create 6 small dots for the 6 maps in this subzone
            for (int i = 0; i < 6; i++)
            {
                GameObject dot = new GameObject($"Dot{i}");
                dot.transform.SetParent(progressObj.transform);
                
                RectTransform dotRect = dot.AddComponent<RectTransform>();
                dotRect.anchorMin = new Vector2(0.5f, 0.5f);
                dotRect.anchorMax = new Vector2(0.5f, 0.5f);
                dotRect.sizeDelta = new Vector2(8, 8);
                dotRect.anchoredPosition = new Vector2(-37.5f + (i * 15), 0);
                
                Image dotImage = dot.AddComponent<Image>();
                dotImage.color = new Color(0.3f, 0.3f, 0.5f, 0.5f); // Inactive dot
                
                // Make dots circular
                var mask = dot.AddComponent<Mask>();
                mask.showMaskGraphic = false;
            }
        }
        
        // Update or create label
        TextMeshProUGUI labelText = null;
        foreach (Transform child in iconObject.transform)
        {
            if (child.name == "Label")
            {
                labelText = child.GetComponent<TextMeshProUGUI>();
                break;
            }
        }
        
        if (labelText != null)
        {
            labelText.text = subZoneName;
            labelText.color = isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f);
        }
        else
        {
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(iconObject.transform);
            
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0f);
            labelRect.anchorMax = new Vector2(0.5f, 0f);
            labelRect.anchoredPosition = new Vector2(0, -70);
            labelRect.sizeDelta = new Vector2(150, 50);
            
            labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = subZoneName;
            labelText.fontSize = 16;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            labelText.fontStyle = FontStyles.Bold;
            labelText.textWrappingMode = TMPro.TextWrappingModes.Normal;
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
                bool isInCorner = false;
                
                // Check all four corners
                if (x < cornerRadius && y >= size - cornerRadius)
                {
                    float dx = cornerRadius - x;
                    float dy = y - (size - cornerRadius);
                    isInCorner = dx * dx + dy * dy > cornerRadius * cornerRadius;
                }
                else if (x >= size - cornerRadius && y >= size - cornerRadius)
                {
                    float dx = x - (size - cornerRadius);
                    float dy = y - (size - cornerRadius);
                    isInCorner = dx * dx + dy * dy > cornerRadius * cornerRadius;
                }
                else if (x < cornerRadius && y < cornerRadius)
                {
                    float dx = cornerRadius - x;
                    float dy = cornerRadius - y;
                    isInCorner = dx * dx + dy * dy > cornerRadius * cornerRadius;
                }
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
