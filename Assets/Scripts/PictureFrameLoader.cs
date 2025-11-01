using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Runtime component to load and assign the PictureFrame1 sprite
/// </summary>
public class PictureFrameLoader : MonoBehaviour
{
    private void Awake()
    {
        // Load the sprite from Resources
        Sprite frameSprite = Resources.Load<Sprite>("Characters/PictureFrame1");
        
        if (frameSprite == null)
        {
            Debug.LogError("Could not load PictureFrame1 sprite from Resources/Characters/");
            return;
        }
        
        // Get the Image component
        Image image = GetComponent<Image>();
        if (image != null)
        {
            image.sprite = frameSprite;
            image.preserveAspect = true;
        }
        
        // Configure the RectTransform
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(450, 550); // Slightly larger than CharacterImage (400x500)
        }
        
        // Set sibling index to 0 so it renders behind CharacterImage
        transform.SetSiblingIndex(0);
    }
}
