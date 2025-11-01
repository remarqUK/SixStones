using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manages the world map scene where players can select which zone to travel to
/// </summary>
public class WorldMapManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ZoneConfiguration zoneConfig;
    [SerializeField] private Transform zoneIconContainer;
    [SerializeField] private GameObject zoneIconPrefab;
    
    [Header("UI Settings")]
    [SerializeField] private float iconSpacing = 150f;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color lockedColor = Color.gray;
    
    [Header("Navigation")]
    [SerializeField] private float inputDelay = 0.2f;
    
    private List<ZoneIcon> zoneIcons = new List<ZoneIcon>();
    private int selectedZoneIndex = 0;
    private float lastInputTime = 0f;
    
    private void Start()
    {
        CreateZoneIcons();
        
        // Set initial selection to player's current zone
        if (PlayerProgress.Instance != null)
        {
            selectedZoneIndex = PlayerProgress.Instance.currentZone;
        }
        
        UpdateSelection();
    }
    
    private void Update()
    {
        HandleInput();
    }
    
    private void CreateZoneIcons()
    {
        if (zoneConfig == null)
        {
            Debug.LogError("ZoneConfiguration is not assigned!");
            return;
        }
        
        int totalZones = zoneConfig.GetTotalZones();
        
        // Calculate starting position to center the icons
        float totalWidth = (totalZones - 1) * iconSpacing;
        float startX = -totalWidth / 2f;
        
        for (int i = 0; i < totalZones; i++)
        {
            var zone = zoneConfig.GetZone(i);
            if (zone == null) continue;
            
            // Create zone icon
            GameObject iconObj;
            if (zoneIconPrefab != null)
            {
                iconObj = Instantiate(zoneIconPrefab, zoneIconContainer);
            }
            else
            {
                // Create simple default icon if no prefab assigned
                iconObj = CreateDefaultZoneIcon();
                iconObj.transform.SetParent(zoneIconContainer);
            }
            
            // Position the icon
            RectTransform rectTransform = iconObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = new Vector2(startX + (i * iconSpacing), 0);
            }
            
            // Setup the zone icon component
            ZoneIcon zoneIcon = iconObj.GetComponent<ZoneIcon>();
            if (zoneIcon == null)
            {
                zoneIcon = iconObj.AddComponent<ZoneIcon>();
            }
            
            zoneIcon.Initialize(i, zone.zoneName, IsZoneUnlocked(i));
            zoneIcons.Add(zoneIcon);
        }
    }
    
private GameObject CreateDefaultZoneIcon()
    {
        // Create a simple UI panel as default icon
        GameObject obj = new GameObject("ZoneIcon");
        
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(100, 100);
        
        Image img = obj.AddComponent<Image>();
        img.color = Color.white;
        
        return obj;
    }
    
    private bool IsZoneUnlocked(int zoneIndex)
    {
        // For now, unlock zones progressively based on player's current zone
        if (PlayerProgress.Instance == null)
            return zoneIndex == 0;
        
        return zoneIndex <= PlayerProgress.Instance.currentZone;
    }
    
private void HandleInput()
    {
        // Prevent too rapid input
        if (Time.time - lastInputTime < inputDelay)
            return;
        
        if (Keyboard.current == null && Gamepad.current == null)
            return;
        
        bool inputDetected = false;
        
        // Left arrow or D-pad left - previous zone
        if ((Keyboard.current?.leftArrowKey.wasPressedThisFrame ?? false) ||
            (Gamepad.current?.dpad.left.wasPressedThisFrame ?? false) ||
            (Gamepad.current?.leftStick.left.wasPressedThisFrame ?? false))
        {
            selectedZoneIndex--;
            if (selectedZoneIndex < 0)
                selectedZoneIndex = zoneIcons.Count - 1; // Wrap around
            inputDetected = true;
        }
        // Right arrow or D-pad right - next zone
        else if ((Keyboard.current?.rightArrowKey.wasPressedThisFrame ?? false) ||
                 (Gamepad.current?.dpad.right.wasPressedThisFrame ?? false) ||
                 (Gamepad.current?.leftStick.right.wasPressedThisFrame ?? false))
        {
            selectedZoneIndex++;
            if (selectedZoneIndex >= zoneIcons.Count)
                selectedZoneIndex = 0; // Wrap around
            inputDetected = true;
        }
        // Enter, Space, or gamepad button A - select zone
        else if ((Keyboard.current?.enterKey.wasPressedThisFrame ?? false) ||
                 (Keyboard.current?.spaceKey.wasPressedThisFrame ?? false) ||
                 (Gamepad.current?.aButton.wasPressedThisFrame ?? false))
        {
            SelectCurrentZone();
            return;
        }
        // Gamepad button B - return to main menu (Escape now opens pause menu via PauseMenu.cs)
        else if (Gamepad.current?.bButton.wasPressedThisFrame ?? false)
        {
            ReturnToMainMenu();
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
        for (int i = 0; i < zoneIcons.Count; i++)
        {
            bool isSelected = i == selectedZoneIndex;
            Color targetColor;
            
            if (isSelected)
            {
                targetColor = selectedColor;
            }
            else if (zoneIcons[i].IsUnlocked)
            {
                targetColor = unlockedColor;
            }
            else
            {
                targetColor = lockedColor;
            }
            
            zoneIcons[i].SetColor(targetColor);
            zoneIcons[i].SetSelected(isSelected);
        }
    }
    
private void SelectCurrentZone()
    {
        if (selectedZoneIndex < 0 || selectedZoneIndex >= zoneIcons.Count)
            return;
        
        ZoneIcon selectedIcon = zoneIcons[selectedZoneIndex];
        
        if (!selectedIcon.IsUnlocked)
        {
            Debug.Log($"Zone {selectedIcon.ZoneName} is locked!");
            return;
        }
        
        if (PlayerProgress.Instance != null)
        {
            PlayerProgress.Instance.currentZone = selectedZoneIndex;
            PlayerProgress.Instance.currentSubZone = 0;
            PlayerProgress.Instance.currentMap = 0;
            PlayerProgress.Instance.SetPosition(0, 0);
        }
        
        Debug.Log($"Traveling to {selectedIcon.ZoneName}");

        // Check for zone cutscene
        var zone = zoneConfig?.GetZone(selectedZoneIndex);
        if (zone != null && !string.IsNullOrEmpty(zone.cutsceneId))
        {
            if (CutsceneTracker.Instance.ShouldPlayCutscene(zone.cutsceneId))
            {
                CutsceneTracker.Instance.PlayCutscene(zone.cutsceneId, () => {
                    SceneHelper.LoadScene(SceneIdentifier.SubZoneMap);
                });
                return;
            }
        }

        SceneHelper.LoadScene(SceneIdentifier.SubZoneMap);
    }

    private void ReturnToMainMenu()
    {
        SceneHelper.LoadScene(SceneIdentifier.MainMenu);
    }
}

/// <summary>
/// Component for individual zone icons on the world map
/// </summary>
public class ZoneIcon : MonoBehaviour
{
    public int ZoneIndex { get; private set; }
    public string ZoneName { get; private set; }
    public bool IsUnlocked { get; private set; }
    
    private Image iconImage;
    private TextMeshProUGUI labelText;
    private GameObject selectionIndicator;
    
    private void Awake()
    {
        iconImage = GetComponent<Image>();
        labelText = GetComponentInChildren<TextMeshProUGUI>();
    }
    
public void Initialize(int index, string zoneName, bool unlocked)
    {
        ZoneIndex = index;
        ZoneName = zoneName;
        IsUnlocked = unlocked;
        
        // Apply visual styling
        ZoneIconVisuals.SetupZoneIconVisuals(gameObject, index, zoneName, unlocked);
        
        if (labelText != null)
        {
            labelText.text = zoneName;
        }
        
        // Create selection indicator (a border or highlight)
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
        img.color = new Color(1, 1, 0, 0.5f); // Semi-transparent yellow
        
        selectionIndicator.transform.SetAsFirstSibling(); // Put behind icon
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
        
        // Scale up selected icon slightly
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
