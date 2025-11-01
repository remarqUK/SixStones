using UnityEngine;

/// <summary>
/// Helper methods for save system integration with existing managers
/// Add these methods to your existing manager classes or use these as extension methods
/// </summary>
public static class SaveSystemHelpers
{
    // ==================== LevelSystem Extensions ====================

    /// <summary>
    /// Set level directly (add this to LevelSystem.cs)
    /// </summary>
    public static void SetLevel_Example(this LevelSystem levelSystem, int level)
    {
        // Implementation example - add to LevelSystem.cs:
        /*
        public void SetLevel(int newLevel)
        {
            currentLevel = Mathf.Max(1, newLevel);
            Debug.Log($"Level set to {currentLevel}");
            onLevelUp?.Invoke(currentLevel);
        }
        */
    }

    /// <summary>
    /// Set XP directly (add this to LevelSystem.cs)
    /// </summary>
    public static void SetXP_Example(this LevelSystem levelSystem, int xp)
    {
        // Implementation example - add to LevelSystem.cs:
        /*
        public void SetXP(int newXP)
        {
            currentXP = Mathf.Max(0, newXP);
            int requiredXP = GetXPRequiredForLevel(currentLevel);
            onXPChanged?.Invoke(currentXP, requiredXP, currentLevel);
            Debug.Log($"XP set to {currentXP}/{requiredXP}");
        }
        */
    }

    // ==================== CurrencyManager Extensions ====================

    /// <summary>
    /// Set gold directly (add this to CurrencyManager.cs)
    /// </summary>
    public static void SetGold_Example(this CurrencyManager currencyManager, int amount)
    {
        // Implementation example - add to CurrencyManager.cs:
        /*
        public void SetGold(int amount)
        {
            currentGold = Mathf.Max(0, amount);
            onGoldChanged?.Invoke(currentGold);
            Debug.Log($"Gold set to {currentGold}");
        }
        */
    }

    // ==================== SpellManager Extensions ====================

    /// <summary>
    /// Clear all spells (add this to SpellManager.cs)
    /// </summary>
    public static void ClearAllSpells_Example()
    {
        // Implementation example - add to SpellManager.cs:
        /*
        public void ClearAllSpells()
        {
            learnedSpells.Clear();
            preparedSpells.Clear();
            spellCooldowns.Clear();
            gemCharges.Clear();
            Debug.Log("All spells cleared");
        }
        */
    }

    /// <summary>
    /// Get spell by name (add this to SpellManager.cs)
    /// </summary>
    public static void GetSpellByName_Example()
    {
        // Implementation example - add to SpellManager.cs:
        /*
        public SpellData GetSpellByName(string spellName)
        {
            // Search in all available spells
            return allSpells.FirstOrDefault(spell =>
                spell.spellName.Equals(spellName, System.StringComparison.OrdinalIgnoreCase));
        }
        */
    }

    /// <summary>
    /// Set cooldown for spell (add this to SpellManager.cs)
    /// </summary>
    public static void SetCooldown_Example()
    {
        // Implementation example - add to SpellManager.cs:
        /*
        public void SetCooldown(SpellData spell, float cooldownTime)
        {
            if (spell != null)
            {
                spellCooldowns[spell] = cooldownTime;
            }
        }
        */
    }

    /// <summary>
    /// Set gem charges for spell (add this to SpellManager.cs)
    /// </summary>
    public static void SetGemCharges_Example()
    {
        // Implementation example - add to SpellManager.cs:
        /*
        public void SetGemCharges(SpellData spell, int charges)
        {
            if (spell != null)
            {
                gemCharges[spell] = Mathf.Max(0, charges);
            }
        }
        */
    }

    // ==================== StatusEffectManager Extensions ====================

    /// <summary>
    /// Clear all status effects (add this to StatusEffectManager.cs)
    /// </summary>
    public static void ClearAllEffects_Example()
    {
        // Implementation example - add to StatusEffectManager.cs:
        /*
        public void ClearAllEffects()
        {
            activeEffects.Clear();
            Debug.Log("All status effects cleared");
        }
        */
    }

    /// <summary>
    /// Get status effect by name (add this to StatusEffectManager.cs)
    /// </summary>
    public static void GetStatusEffectByName_Example()
    {
        // Implementation example - add to StatusEffectManager.cs:
        /*
        public StatusEffectData GetStatusEffectByName(string effectName)
        {
            // Search in all available status effects
            return allStatusEffects.FirstOrDefault(effect =>
                effect.effectName.Equals(effectName, System.StringComparison.OrdinalIgnoreCase));
        }
        */
    }

    // ==================== FirstPersonMazeController Extensions ====================

    /// <summary>
    /// Set player position in maze (add this to FirstPersonMazeController.cs)
    /// </summary>
    public static void SetPosition_Example()
    {
        // Implementation example - add to FirstPersonMazeController.cs:
        /*
        public void SetPosition(int x, int y)
        {
            currentX = x;
            currentY = y;
            UpdatePlayerTransform();
            Debug.Log($"Player position set to ({x}, {y})");
        }
        */
    }

    // ==================== GlobalOptionsManager Extensions ====================

    /// <summary>
    /// Set volume methods (add these to GlobalOptionsManager.cs if missing)
    /// </summary>
    public static void VolumeControls_Example()
    {
        // Implementation example - add to GlobalOptionsManager.cs:
        /*
        public void SetMasterVolume(float volume)
        {
            MasterVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }

        public void SetMusicVolume(float volume)
        {
            MusicVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }

        public void SetSFXVolume(float volume)
        {
            SFXVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }
        */
    }

    // ==================== LocalizationManager Extensions ====================

    /// <summary>
    /// Set language (add this to LocalizationManager.cs if missing)
    /// </summary>
    public static void SetLanguage_Example()
    {
        // Implementation example - add to LocalizationManager.cs:
        /*
        public void SetLanguage(string languageCode)
        {
            CurrentLanguage = languageCode;
            LoadLanguageStrings(languageCode);
            Debug.Log($"Language set to: {languageCode}");
        }
        */
    }

    // ==================== GameManager Extensions ====================

    /// <summary>
    /// Update UI methods (verify these exist in GameManager.cs)
    /// </summary>
    public static void UpdateUI_Example()
    {
        // Verify these methods exist in GameManager.cs:
        /*
        public void UpdateHealthUI()
        {
            // Update health bar displays
        }

        public void UpdateScoreUI()
        {
            // Update score displays
        }
        */
    }
}
