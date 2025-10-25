using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages game localization and language switching.
/// For Unity 6, this uses a simple string table approach.
/// In a production environment, you would use Unity's Localization package from Package Manager.
/// </summary>
public class LocalizationManager : MonoBehaviour
{
    private static LocalizationManager instance;
    public static LocalizationManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("LocalizationManager");
                instance = go.AddComponent<LocalizationManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    [Header("Settings")]
    [SerializeField] private SystemLanguage currentLanguage = SystemLanguage.English;

    // String tables for each language
    private Dictionary<SystemLanguage, Dictionary<string, string>> stringTables = new Dictionary<SystemLanguage, Dictionary<string, string>>();

    // Event fired when language changes
    public event System.Action<SystemLanguage> OnLanguageChanged;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeStringTables();
            LoadSavedLanguage();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeStringTables()
    {
        // English
        stringTables[SystemLanguage.English] = new Dictionary<string, string>
        {
            // UI
            { "ui_score", "Score" },
            { "ui_level", "Level" },
            { "ui_xp", "XP" },
            { "ui_gold", "Gold" },
            { "ui_hp", "HP" },
            { "ui_player", "Player" },
            { "ui_current_player", "Current" },

            // Menu
            { "menu_resume", "Resume" },
            { "menu_options", "Options" },
            { "menu_quit", "Quit" },
            { "menu_back", "Back" },
            { "menu_paused", "PAUSED" },

            // Options
            { "options_title", "OPTIONS" },
            { "options_game_volume", "Game Volume" },
            { "options_music_volume", "Music Volume" },
            { "options_dialog_volume", "Dialog Volume" },
            { "options_game_speed", "Game Speed" },
            { "options_language", "Language" },
            { "options_subtitles", "Subtitles" },
            { "options_colorblind_mode", "Color Blind Mode" },

            // Game Speed
            { "speed_slow", "Slow" },
            { "speed_medium", "Medium" },
            { "speed_fast", "Fast" },

            // Languages
            { "lang_english", "English" },
            { "lang_spanish", "Spanish" },
            { "lang_french", "French" },
            { "lang_german", "German" },
            { "lang_japanese", "Japanese" },
            { "lang_chinese", "Chinese" },

            // Game Messages
            { "msg_game_over", "Game Over" },
            { "msg_you_win", "You Win!" },
            { "msg_no_moves", "No possible moves!" },
            { "msg_bonus_turn", "Bonus Turn!" },

            // Colors
            { "color_red", "Red" },
            { "color_blue", "Blue" },
            { "color_green", "Green" },
            { "color_yellow", "Yellow" },
            { "color_purple", "Purple" },
            { "color_orange", "Orange" },
            { "color_white", "White" }
        };

        // Spanish
        stringTables[SystemLanguage.Spanish] = new Dictionary<string, string>
        {
            // UI
            { "ui_score", "Puntuación" },
            { "ui_level", "Nivel" },
            { "ui_xp", "XP" },
            { "ui_gold", "Oro" },
            { "ui_hp", "PV" },
            { "ui_player", "Jugador" },
            { "ui_current_player", "Actual" },

            // Menu
            { "menu_resume", "Continuar" },
            { "menu_options", "Opciones" },
            { "menu_quit", "Salir" },
            { "menu_back", "Volver" },
            { "menu_paused", "PAUSADO" },

            // Options
            { "options_title", "OPCIONES" },
            { "options_game_volume", "Volumen del Juego" },
            { "options_music_volume", "Volumen de Música" },
            { "options_dialog_volume", "Volumen de Diálogos" },
            { "options_game_speed", "Velocidad del Juego" },
            { "options_language", "Idioma" },
            { "options_subtitles", "Subtítulos" },
            { "options_colorblind_mode", "Modo Daltónico" },

            // Game Speed
            { "speed_slow", "Lento" },
            { "speed_medium", "Medio" },
            { "speed_fast", "Rápido" },

            // Languages
            { "lang_english", "Inglés" },
            { "lang_spanish", "Español" },
            { "lang_french", "Francés" },
            { "lang_german", "Alemán" },
            { "lang_japanese", "Japonés" },
            { "lang_chinese", "Chino" },

            // Game Messages
            { "msg_game_over", "Juego Terminado" },
            { "msg_you_win", "¡Ganaste!" },
            { "msg_no_moves", "¡No hay movimientos posibles!" },
            { "msg_bonus_turn", "¡Turno Bonus!" },

            // Colors
            { "color_red", "Rojo" },
            { "color_blue", "Azul" },
            { "color_green", "Verde" },
            { "color_yellow", "Amarillo" },
            { "color_purple", "Morado" },
            { "color_orange", "Naranja" },
            { "color_white", "Blanco" }
        };

        // French
        stringTables[SystemLanguage.French] = new Dictionary<string, string>
        {
            // UI
            { "ui_score", "Score" },
            { "ui_level", "Niveau" },
            { "ui_xp", "XP" },
            { "ui_gold", "Or" },
            { "ui_hp", "PV" },
            { "ui_player", "Joueur" },
            { "ui_current_player", "Actuel" },

            // Menu
            { "menu_resume", "Reprendre" },
            { "menu_options", "Options" },
            { "menu_quit", "Quitter" },
            { "menu_back", "Retour" },
            { "menu_paused", "PAUSE" },

            // Options
            { "options_title", "OPTIONS" },
            { "options_game_volume", "Volume du Jeu" },
            { "options_music_volume", "Volume de la Musique" },
            { "options_dialog_volume", "Volume des Dialogues" },
            { "options_game_speed", "Vitesse du Jeu" },
            { "options_language", "Langue" },
            { "options_subtitles", "Sous-titres" },
            { "options_colorblind_mode", "Mode Daltonien" },

            // Game Speed
            { "speed_slow", "Lent" },
            { "speed_medium", "Moyen" },
            { "speed_fast", "Rapide" },

            // Languages
            { "lang_english", "Anglais" },
            { "lang_spanish", "Espagnol" },
            { "lang_french", "Français" },
            { "lang_german", "Allemand" },
            { "lang_japanese", "Japonais" },
            { "lang_chinese", "Chinois" },

            // Game Messages
            { "msg_game_over", "Jeu Terminé" },
            { "msg_you_win", "Vous Avez Gagné!" },
            { "msg_no_moves", "Aucun mouvement possible!" },
            { "msg_bonus_turn", "Tour Bonus!" },

            // Colors
            { "color_red", "Rouge" },
            { "color_blue", "Bleu" },
            { "color_green", "Vert" },
            { "color_yellow", "Jaune" },
            { "color_purple", "Violet" },
            { "color_orange", "Orange" },
            { "color_white", "Blanc" }
        };

        // German
        stringTables[SystemLanguage.German] = new Dictionary<string, string>
        {
            // UI
            { "ui_score", "Punktzahl" },
            { "ui_level", "Level" },
            { "ui_xp", "XP" },
            { "ui_gold", "Gold" },
            { "ui_hp", "LP" },
            { "ui_player", "Spieler" },
            { "ui_current_player", "Aktuell" },

            // Menu
            { "menu_resume", "Fortsetzen" },
            { "menu_options", "Optionen" },
            { "menu_quit", "Beenden" },
            { "menu_back", "Zurück" },
            { "menu_paused", "PAUSIERT" },

            // Options
            { "options_title", "OPTIONEN" },
            { "options_game_volume", "Spiel-Lautstärke" },
            { "options_music_volume", "Musik-Lautstärke" },
            { "options_dialog_volume", "Dialog-Lautstärke" },
            { "options_game_speed", "Spielgeschwindigkeit" },
            { "options_language", "Sprache" },
            { "options_subtitles", "Untertitel" },
            { "options_colorblind_mode", "Farbenblind-Modus" },

            // Game Speed
            { "speed_slow", "Langsam" },
            { "speed_medium", "Mittel" },
            { "speed_fast", "Schnell" },

            // Languages
            { "lang_english", "Englisch" },
            { "lang_spanish", "Spanisch" },
            { "lang_french", "Französisch" },
            { "lang_german", "Deutsch" },
            { "lang_japanese", "Japanisch" },
            { "lang_chinese", "Chinesisch" },

            // Game Messages
            { "msg_game_over", "Spiel Vorbei" },
            { "msg_you_win", "Du Hast Gewonnen!" },
            { "msg_no_moves", "Keine möglichen Züge!" },
            { "msg_bonus_turn", "Bonus-Zug!" },

            // Colors
            { "color_red", "Rot" },
            { "color_blue", "Blau" },
            { "color_green", "Grün" },
            { "color_yellow", "Gelb" },
            { "color_purple", "Lila" },
            { "color_orange", "Orange" },
            { "color_white", "Weiß" }
        };
    }

    /// <summary>
    /// Get localized string by key
    /// </summary>
    public string GetString(string key)
    {
        if (stringTables.ContainsKey(currentLanguage) && stringTables[currentLanguage].ContainsKey(key))
        {
            return stringTables[currentLanguage][key];
        }

        // Fallback to English
        if (stringTables.ContainsKey(SystemLanguage.English) && stringTables[SystemLanguage.English].ContainsKey(key))
        {
            return stringTables[SystemLanguage.English][key];
        }

        Debug.LogWarning($"Localization key not found: {key}");
        return $"[{key}]";
    }

    /// <summary>
    /// Get formatted localized string with parameters
    /// </summary>
    public string GetString(string key, params object[] args)
    {
        string text = GetString(key);
        return string.Format(text, args);
    }

    /// <summary>
    /// Set current language
    /// </summary>
    public void SetLanguage(SystemLanguage language)
    {
        if (!stringTables.ContainsKey(language))
        {
            Debug.LogWarning($"Language not supported: {language}. Falling back to English.");
            language = SystemLanguage.English;
        }

        currentLanguage = language;
        SaveLanguage();
        OnLanguageChanged?.Invoke(currentLanguage);

        Debug.Log($"Language changed to: {language}");
    }

    /// <summary>
    /// Set language from dropdown index
    /// </summary>
    public void SetLanguageFromDropdown(int index)
    {
        SystemLanguage[] supportedLanguages = {
            SystemLanguage.English,
            SystemLanguage.Spanish,
            SystemLanguage.French,
            SystemLanguage.German
        };

        if (index >= 0 && index < supportedLanguages.Length)
        {
            SetLanguage(supportedLanguages[index]);
        }
    }

    /// <summary>
    /// Get current language
    /// </summary>
    public SystemLanguage GetCurrentLanguage()
    {
        return currentLanguage;
    }

    /// <summary>
    /// Get list of supported languages
    /// </summary>
    public List<SystemLanguage> GetSupportedLanguages()
    {
        return new List<SystemLanguage>(stringTables.Keys);
    }

    /// <summary>
    /// Get language display name
    /// </summary>
    public string GetLanguageDisplayName(SystemLanguage language)
    {
        return language switch
        {
            SystemLanguage.English => "English",
            SystemLanguage.Spanish => "Español",
            SystemLanguage.French => "Français",
            SystemLanguage.German => "Deutsch",
            SystemLanguage.Japanese => "日本語",
            SystemLanguage.Chinese => "中文",
            _ => language.ToString()
        };
    }

    /// <summary>
    /// Save language preference
    /// </summary>
    private void SaveLanguage()
    {
        PlayerPrefs.SetString("Language", currentLanguage.ToString());
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Load saved language preference
    /// </summary>
    private void LoadSavedLanguage()
    {
        if (PlayerPrefs.HasKey("Language"))
        {
            string savedLanguage = PlayerPrefs.GetString("Language");
            if (System.Enum.TryParse(savedLanguage, out SystemLanguage language))
            {
                SetLanguage(language);
            }
        }
        else
        {
            // Use system language if available
            SystemLanguage systemLang = Application.systemLanguage;
            if (stringTables.ContainsKey(systemLang))
            {
                SetLanguage(systemLang);
            }
        }
    }
}
