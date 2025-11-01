using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manages the character selection screen
/// Displays a list of character archetypes and their attributes
/// </summary>
public class CharacterSelectionManager : MonoBehaviour
{
    [Header("Character Data")]
    [SerializeField] private List<CharacterData> availableCharacters = new List<CharacterData>();
    [SerializeField] private int defaultCharacterIndex = 0; // Knight by default

    [Header("UI References - Character List")]
    [SerializeField] private Transform characterButtonContainer;
    [SerializeField] private GameObject characterButtonPrefab;

    [Header("UI References - Character Display")]
    [SerializeField] private Image characterImage;
    [SerializeField] private Sprite placeholderSprite;
    [SerializeField] private TextMeshProUGUI attributesText;

    [Header("UI References - Buttons")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button backButton;

    [Header("Scene Settings")]
    [SerializeField] private SceneIdentifier gameScene = SceneIdentifier.Match3;

    private CharacterData selectedCharacter;
    private List<Button> characterButtons = new List<Button>();

    private void Start()
    {
        // Create character selection buttons
        CreateCharacterButtons();

        // Select default character (Knight)
        if (availableCharacters.Count > 0)
        {
            int initialIndex = Mathf.Clamp(defaultCharacterIndex, 0, availableCharacters.Count - 1);
            SelectCharacter(availableCharacters[initialIndex]);
        }

        // Wire up UI buttons
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirm);

        if (backButton != null)
            backButton.onClick.AddListener(OnBack);
    }

    private void CreateCharacterButtons()
    {
        if (characterButtonContainer == null || characterButtonPrefab == null)
        {
            Debug.LogError("[CharacterSelection] Character button container or prefab not assigned!");
            return;
        }

        // Clear existing buttons
        foreach (Transform child in characterButtonContainer)
        {
            Destroy(child.gameObject);
        }
        characterButtons.Clear();

        // Create a button for each character
        foreach (var character in availableCharacters)
        {
            GameObject buttonObj = Instantiate(characterButtonPrefab, characterButtonContainer);
            Button button = buttonObj.GetComponent<Button>();

            // Find the Text component (character name at bottom)
            Transform textTransform = buttonObj.transform.Find("Text");
            if (textTransform != null)
            {
                TextMeshProUGUI nameText = textTransform.GetComponent<TextMeshProUGUI>();
                if (nameText != null)
                {
                    nameText.text = character.characterName;
                }
            }

            // Find the CharacterImage (placeholder at top)
            Transform imageTransform = buttonObj.transform.Find("CharacterImage");
            if (imageTransform != null)
            {
                Image charImage = imageTransform.GetComponent<Image>();
                if (charImage != null && character.characterSprite != null)
                {
                    charImage.sprite = character.characterSprite;
                }
            }

            if (button != null)
            {
                CharacterData characterRef = character; // Capture for closure
                button.onClick.AddListener(() => SelectCharacter(characterRef));
                characterButtons.Add(button);
            }

            Debug.Log($"[CharacterSelection] Created button for {character.characterName}");
        }
    }

    private void SelectCharacter(CharacterData character)
    {
        if (character == null) return;

        selectedCharacter = character;
        Debug.Log($"[CharacterSelection] Selected character: {character.characterName}");

        // Update character image
        if (characterImage != null)
        {
            if (character.characterSprite != null)
            {
                characterImage.sprite = character.characterSprite;
            }
            else if (placeholderSprite != null)
            {
                characterImage.sprite = placeholderSprite;
            }
        }

        // Update attributes text
        if (attributesText != null)
        {
            attributesText.text = character.GetAttributesSummary();
        }

        // Update button visuals to show selection
        UpdateButtonVisuals();
    }

    private void UpdateButtonVisuals()
    {
        for (int i = 0; i < characterButtons.Count && i < availableCharacters.Count; i++)
        {
            Button button = characterButtons[i];
            CharacterData character = availableCharacters[i];

            if (button != null)
            {
                // Highlight selected button
                ColorBlock colors = button.colors;
                if (character == selectedCharacter)
                {
                    colors.normalColor = new Color(0.4f, 0.6f, 0.8f, 1f); // Light blue for selected
                }
                else
                {
                    colors.normalColor = new Color(0.2f, 0.25f, 0.3f, 1f); // Default gray
                }
                button.colors = colors;
            }
        }
    }

    private void OnConfirm()
    {
        if (selectedCharacter == null)
        {
            Debug.LogWarning("[CharacterSelection] No character selected!");
            return;
        }

        Debug.Log($"[CharacterSelection] Confirmed character: {selectedCharacter.characterName}");

        // TODO: Save selected character to persistent data
        PlayerPrefs.SetString("SelectedCharacter", selectedCharacter.name);
        PlayerPrefs.Save();

        // Load game scene
        SceneHelper.LoadScene(gameScene);
    }

    private void OnBack()
    {
        Debug.Log("[CharacterSelection] Returning to main menu...");
        SceneHelper.LoadScene(SceneIdentifier.MainMenu);
    }
}
