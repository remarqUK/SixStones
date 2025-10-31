using UnityEngine;

/// <summary>
/// Manages game audio settings - volume controls for master, music, and SFX
/// </summary>
public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("AudioManager");
                instance = go.AddComponent<AudioManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    [Header("Volume Settings")]
    [SerializeField] [Range(0f, 1f)] private float gameVolume = 1f;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.7f;
    [SerializeField] [Range(0f, 1f)] private float dialogVolume = 0.8f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip singleMatchSound;

    // Event fired when volume changes
    public event System.Action OnVolumeChanged;

    private AudioSource sfxAudioSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Create AudioSource for SFX
            sfxAudioSource = gameObject.AddComponent<AudioSource>();
            sfxAudioSource.playOnAwake = false;

            LoadSavedVolumes();

            // Set initial SFX volume
            UpdateSFXVolume();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Set game volume (0-1)
    /// </summary>
    public void SetGameVolume(float volume)
    {
        gameVolume = Mathf.Clamp01(volume);
        AudioListener.volume = gameVolume;
        UpdateSFXVolume();
        SaveVolumes();
        OnVolumeChanged?.Invoke();
        Debug.Log($"Game volume set to: {gameVolume:F2}");
    }

    /// <summary>
    /// Set music volume (0-1)
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        SaveVolumes();
        OnVolumeChanged?.Invoke();
        Debug.Log($"Music volume set to: {musicVolume:F2}");
        // TODO: Update music AudioSource volume when music system is implemented
    }

    /// <summary>
    /// Set dialog volume (0-1)
    /// </summary>
    public void SetDialogVolume(float volume)
    {
        dialogVolume = Mathf.Clamp01(volume);
        SaveVolumes();
        OnVolumeChanged?.Invoke();
        Debug.Log($"Dialog volume set to: {dialogVolume:F2}");
        // TODO: Update dialog AudioSources volume when dialog system is implemented
    }

    /// <summary>
    /// Get game volume
    /// </summary>
    public float GetGameVolume()
    {
        return gameVolume;
    }

    /// <summary>
    /// Get music volume
    /// </summary>
    public float GetMusicVolume()
    {
        return musicVolume;
    }

    /// <summary>
    /// Get dialog volume
    /// </summary>
    public float GetDialogVolume()
    {
        return dialogVolume;
    }

    /// <summary>
    /// Save volume preferences
    /// </summary>
    private void SaveVolumes()
    {
        PlayerPrefs.SetFloat("GameVolume", gameVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("DialogVolume", dialogVolume);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Load saved volume preferences
    /// </summary>
    private void LoadSavedVolumes()
    {
        gameVolume = PlayerPrefs.GetFloat("GameVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        dialogVolume = PlayerPrefs.GetFloat("DialogVolume", 0.8f);

        // Apply game volume
        AudioListener.volume = gameVolume;

        Debug.Log($"Loaded volumes - Game: {gameVolume:F2}, Music: {musicVolume:F2}, Dialog: {dialogVolume:F2}");
    }

    /// <summary>
    /// Update the SFX AudioSource volume based on game volume
    /// </summary>
    private void UpdateSFXVolume()
    {
        if (sfxAudioSource != null)
        {
            sfxAudioSource.volume = gameVolume;
        }
    }

    /// <summary>
    /// Play the single match sound effect (for 3-gem matches)
    /// </summary>
    public void PlaySingleMatchSound()
    {
        if (singleMatchSound != null && sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(singleMatchSound);
        }
    }

    /// <summary>
    /// Play a sound effect with the current game volume
    /// </summary>
    public void PlaySoundEffect(AudioClip clip)
    {
        if (clip != null && sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(clip);
        }
    }
}
