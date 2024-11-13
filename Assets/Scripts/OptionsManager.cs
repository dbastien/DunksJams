using UnityEngine;
using System;

[Serializable]
public class OptionsManager
{
    // Display
    public int resolutionIndex = 0;
    public bool isFullscreen = true;

    // Graphics
    public int graphicsQuality = 2;
    public bool vsyncEnabled = true;

    // Audio
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    // Gameplay
    [Range(0.1f, 10f)] public float mouseSensitivity = 1f;
    public bool invertYAxis;

    // Defaults
    readonly int defaultResolutionIndex = 0;
    readonly bool defaultFullscreen = true;
    readonly float defaultMasterVolume = 1f;
    readonly float defaultMusicVolume = 1f;
    readonly float defaultSfxVolume = 1f;
    readonly int defaultGraphicsQuality = 2;
    readonly bool defaultVsyncEnabled = true;
    readonly float defaultMouseSensitivity = 1f;
    readonly bool defaultInvertYAxis = false;

    // Events for UI or other components to subscribe to
    public event Action OnSettingsChanged;

    public void ApplySettings()
    {
        // Apply display
        Resolution resolution = Screen.resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, isFullscreen);

        // Apply graphics
        QualitySettings.SetQualityLevel(graphicsQuality);
        QualitySettings.vSyncCount = vsyncEnabled ? 1 : 0;

        // Apply audio
        AudioListener.volume = masterVolume;
        // Assuming you have a separate audio manager for music and SFX, you would apply those here as well

        OnSettingsChanged?.Invoke();
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SfxVolume", sfxVolume);
        PlayerPrefs.SetInt("GraphicsQuality", graphicsQuality);
        PlayerPrefs.SetInt("VsyncEnabled", vsyncEnabled ? 1 : 0);
        PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivity);
        PlayerPrefs.SetInt("InvertYAxis", invertYAxis ? 1 : 0);

        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", defaultResolutionIndex);
        isFullscreen = PlayerPrefs.GetInt("Fullscreen", defaultFullscreen ? 1 : 0) == 1;
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", defaultMasterVolume);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", defaultMusicVolume);
        sfxVolume = PlayerPrefs.GetFloat("SfxVolume", defaultSfxVolume);
        graphicsQuality = PlayerPrefs.GetInt("GraphicsQuality", defaultGraphicsQuality);
        vsyncEnabled = PlayerPrefs.GetInt("VsyncEnabled", defaultVsyncEnabled ? 1 : 0) == 1;
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", defaultMouseSensitivity);
        invertYAxis = PlayerPrefs.GetInt("InvertYAxis", defaultInvertYAxis ? 1 : 0) == 1;

        ApplySettings();
    }

    public void ResetToDefaults()
    {
        resolutionIndex = defaultResolutionIndex;
        isFullscreen = defaultFullscreen;
        masterVolume = defaultMasterVolume;
        musicVolume = defaultMusicVolume;
        sfxVolume = defaultSfxVolume;
        graphicsQuality = defaultGraphicsQuality;
        vsyncEnabled = defaultVsyncEnabled;
        mouseSensitivity = defaultMouseSensitivity;
        invertYAxis = defaultInvertYAxis;

        ApplySettings();
    }
}