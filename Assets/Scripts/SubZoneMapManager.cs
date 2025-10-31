using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manages the subzone map scene where players select which subzone to enter within a zone
/// </summary>
public class SubZoneMapManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ZoneConfiguration zoneConfig;
    [SerializeField] private Transform subZoneIconContainer;
    [SerializeField] private GameObject subZoneIconPrefab;
    [SerializeField] private TextMeshProUGUI zoneNameText;
    
    [Header("UI Settings")]
    [SerializeField] private float iconSpacing = 150f;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color lockedColor = Color.gray;
    
    [Header("Navigation")]
    [SerializeField] private float inputDelay = 0.2f;
    
    private List<SubZoneIcon> subZoneIcons = new List<SubZoneIcon>();
    private int selectedSubZoneIndex = 0;
    private int currentZoneIndex = 0;
    private float lastInputTime = 0f;
    
    private void Start()
    {
        // Get the current zone from PlayerProgress
        if (PlayerProgress.Instance != null)
        {
            currentZoneIndex = PlayerProgress.Instance.currentZone;
        }
        
        // Update zone title
        if (zoneConfig != null && zoneNameText != null)
        {
            var zone = zoneConfig.GetZone(currentZoneIndex);
            if (zone != null)
            {
                zoneNameText.text = zone.zoneName.ToUpper();
            }
        }
        
        CreateSubZoneIcons();
        
        // Set initial selection to player's current subzone
        if (PlayerProgress.Instance != null)
        {
            selectedSubZoneIndex = PlayerProgress.Instance.currentSubZone;
        }
        
        UpdateSelection();
    }
    
    private void Update()
    {
        HandleInput();
    }
    
    private void CreateSubZoneIcons()
    {
        if (zoneConfig == null)
        {
            Debug.LogError("ZoneConfiguration is not assigned!");
            return;
        }
        
        var zone = zoneConfig.GetZone(currentZoneIndex);
        if (zone == null)
        {
            Debug.LogError($"Zone {currentZoneIndex} not found!");
            return;
        }
        
        int totalSubZones = zone.subZones.Count;
        
        // Calculate starting position to center the icons
        float totalWidth = (totalSubZones - 1) * iconSpacing;
        float startX = -totalWidth / 2f;
        
        for (int i = 0; i < totalSubZones; i++)
        {
            var subZone = zone.subZones[i];
            if (subZone == null) continue;
            
            // Create subzone icon
            GameObject iconObj;
            if (subZoneIconPrefab != null)
            {
                iconObj = Instantiate(subZoneIconPrefab, subZoneIconContainer);
            }
            else
            {
                iconObj = CreateDefaultSubZoneIcon();
                iconObj.transform.SetParent(subZoneIconContainer);
            }
            
            // Position the icon
            RectTransform rectTransform = iconObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = new Vector2(startX + (i * iconSpacing), 0);
            }
            
            // Setup the subzone icon component
            SubZoneIcon subZoneIcon = iconObj.GetComponent<SubZoneIcon>();
            if (subZoneIcon == null)
            {
                subZoneIcon = iconObj.AddComponent<SubZoneIcon>();
            }
            
            subZoneIcon.Initialize(i, subZone.subZoneName, IsSubZoneUnlocked(i));
            subZoneIcons.Add(subZoneIcon);
        }
    }
    
    private GameObject CreateDefaultSubZoneIcon()
    {
        GameObject obj = new GameObject("SubZoneIcon");
        
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(100, 100);
        
        Image img = obj.AddComponent<Image>();
        img.color = Color.white;
        
        return obj;
    }
    
    private bool IsSubZoneUnlocked(int subZoneIndex)
    {
        if (PlayerProgress.Instance == null)
            return subZoneIndex == 0;
        
        // Check if this is the current zone
        if (currentZoneIndex < PlayerProgress.Instance.currentZone)
        {
            // Previous zones are fully unlocked
            return true;
        }
        else if (currentZoneIndex == PlayerProgress.Instance.currentZone)
        {
            // Current zone - unlock progressively
            return subZoneIndex <= PlayerProgress.Instance.currentSubZone;
        }
        else
        {
            // Future zones - locked
            return false;
        }
    }
    
    private void HandleInput()
    {
        if (Time.time - lastInputTime < inputDelay)
            return;
        
        if (Keyboard.current == null && Gamepad.current == null)
            return;
        
        bool inputDetected = false;
        
        // Left arrow or D-pad left
        if ((Keyboard.current?.leftArrowKey.wasPressedThisFrame ?? false) ||
            (Gamepad.current?.dpad.left.wasPressedThisFrame ?? false) ||
            (Gamepad.current?.leftStick.left.wasPressedThisFrame ?? false))
        {
            selectedSubZoneIndex--;
            if (selectedSubZoneIndex < 0)
                selectedSubZoneIndex = subZoneIcons.Count - 1;
            inputDetected = true;
        }
        // Right arrow or D-pad right
        else if ((Keyboard.current?.rightArrowKey.wasPressedThisFrame ?? false) ||
                 (Gamepad.current?.dpad.right.wasPressedThisFrame ?? false) ||
                 (Gamepad.current?.leftStick.right.wasPressedThisFrame ?? false))
        {
            selectedSubZoneIndex++;
            if (selectedSubZoneIndex >= subZoneIcons.Count)
                selectedSubZoneIndex = 0;
            inputDetected = true;
        }
        // Enter, Space, or A button - select subzone
        else if ((Keyboard.current?.enterKey.wasPressedThisFrame ?? false) ||
                 (Keyboard.current?.spaceKey.wasPressedThisFrame ?? false) ||
                 (Gamepad.current?.aButton.wasPressedThisFrame ?? false))
        {
            SelectCurrentSubZone();
            return;
        }
        // Escape or B button - return to world map
        else if ((Keyboard.current?.escapeKey.wasPressedThisFrame ?? false) ||
                 (Gamepad.current?.bButton.wasPressedThisFrame ?? false))
        {
            ReturnToWorldMap();
            return;
        }
        
        if (inputDetected)
        {
            lastInputTime = Time.time;
            UpdateSelection();
        }
    }
    
    private void UpdateSelection()
    {
        for (int i = 0; i < subZoneIcons.Count; i++)
        {
            bool isSelected = i == selectedSubZoneIndex;
            Color targetColor;
            
            if (isSelected)
            {
                targetColor = selectedColor;
            }
            else if (subZoneIcons[i].IsUnlocked)
            {
                targetColor = unlockedColor;
            }
            else
            {
                targetColor = lockedColor;
            }
            
            subZoneIcons[i].SetColor(targetColor);
            subZoneIcons[i].SetSelected(isSelected);
        }
    }
    
private void SelectCurrentSubZone()
    {
        if (selectedSubZoneIndex < 0 || selectedSubZoneIndex >= subZoneIcons.Count)
            return;
        
        SubZoneIcon selectedIcon = subZoneIcons[selectedSubZoneIndex];
        
        if (!selectedIcon.IsUnlocked)
        {
            Debug.Log($"SubZone {selectedIcon.SubZoneName} is locked!");
            return;
        }
        
        // Update player progress to this subzone
        if (PlayerProgress.Instance != null)
        {
            PlayerProgress.Instance.currentSubZone = selectedSubZoneIndex;
            PlayerProgress.Instance.currentMap = 0;
            PlayerProgress.Instance.SetPosition(0, 0);
        }
        
        Debug.Log($"Entering {selectedIcon.SubZoneName}");
        
        // Check for subzone cutscene
        var zone = zoneConfig?.GetZone(currentZoneIndex);
        var subZone = zone?.subZones[selectedSubZoneIndex];
        
        if (subZone != null && !string.IsNullOrEmpty(subZone.cutsceneId))
        {
            if (CutsceneTracker.Instance.ShouldPlayCutscene(subZone.cutsceneId))
            {
                CutsceneTracker.Instance.PlayCutscene(subZone.cutsceneId, () => {
                    SceneManager.LoadScene("Match3");
                });
                return;
            }
        }
        
        // Load the match-3 scene (or could load a map selection scene)
        SceneManager.LoadScene("Match3");
    }
    
    private void ReturnToWorldMap()
    {
        SceneManager.LoadScene("WorldMap");
    }
}

/// <summary>
/// Component for individual subzone icons
/// </summary>
public class SubZoneIcon : MonoBehaviour
{
    public int SubZoneIndex { get; private set; }
    public string SubZoneName { get; private set; }
    public bool IsUnlocked { get; private set; }
    
    private Image iconImage;
    private TextMeshProUGUI labelText;
    private GameObject selectionIndicator;
    
    private void Awake()
    {
        iconImage = GetComponent<Image>();
        labelText = GetComponentInChildren<TextMeshProUGUI>();
    }
    
public void Initialize(int index, string subZoneName, bool unlocked)
    {
        SubZoneIndex = index;
        SubZoneName = subZoneName;
        IsUnlocked = unlocked;
        
        // Apply visual styling
        SubZoneIconVisuals.SetupSubZoneIconVisuals(gameObject, index, subZoneName, unlocked);
        
        CreateSelectionIndicator();
    }
    
    private void CreateSelectionIndicator()
    {
        selectionIndicator = new GameObject("SelectionIndicator");
        selectionIndicator.transform.SetParent(transform);
        
        RectTransform rect = selectionIndicator.AddComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(110, 110);
        
        Image img = selectionIndicator.AddComponent<Image>();
        img.color = new Color(1, 1, 0, 0.5f);
        
        selectionIndicator.transform.SetAsFirstSibling();
        selectionIndicator.SetActive(false);
    }
    
    public void SetColor(Color color)
    {
        if (iconImage != null)
        {
            iconImage.color = color;
        }
    }
    
    public void SetSelected(bool selected)
    {
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(selected);
        }
        
        if (selected)
        {
            transform.localScale = Vector3.one * 1.2f;
        }
        else
        {
            transform.localScale = Vector3.one;
        }
    }
}
