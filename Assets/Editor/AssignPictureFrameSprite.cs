using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor script to assign PictureFrame1 sprite to the PictureFrame GameObject
/// </summary>
public class AssignPictureFrameSprite
{
    [MenuItem("Tools/Assign Picture Frame Sprite")]
    public static void AssignSprite()
    {
        // Load the sprite from Resources
        Sprite frameSprite = Resources.Load<Sprite>("Characters/PictureFrame1");
        
        if (frameSprite == null)
        {
            Debug.LogError("Could not load PictureFrame1 sprite from Resources/Characters/");
            return;
        }
        
        // Find the PictureFrame GameObject in the active scene
        Scene activeScene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = activeScene.GetRootGameObjects();
        
        Image pictureFrameImage = null;
        
        foreach (GameObject root in rootObjects)
        {
            Transform pictureFrame = root.transform.Find("Canvas/ContentPanel/CharacterImagePanel/PictureFrame");
            if (pictureFrame != null)
            {
                pictureFrameImage = pictureFrame.GetComponent<Image>();
                break;
            }
        }
        
        if (pictureFrameImage == null)
        {
            Debug.LogError("Could not find PictureFrame GameObject with Image component");
            return;
        }
        
        // Assign the sprite
        pictureFrameImage.sprite = frameSprite;
        pictureFrameImage.preserveAspect = true;
        
        // Get the RectTransform and configure it to be slightly larger than CharacterImage
        RectTransform rectTransform = pictureFrameImage.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(450, 550); // Slightly larger than CharacterImage (400x500)
        }
        
        // Set sibling index to 0 so it renders behind CharacterImage
        pictureFrameImage.transform.SetSiblingIndex(0);
        
        // Mark the scene as dirty so it can be saved
        EditorSceneManager.MarkSceneDirty(activeScene);
        
        Debug.Log("Successfully assigned PictureFrame1 sprite and configured PictureFrame GameObject!");
    }
}
