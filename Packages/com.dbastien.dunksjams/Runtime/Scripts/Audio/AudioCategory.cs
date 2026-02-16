using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "‽/Audio/Audio Category", fileName = "AudioCategory")]
public class AudioCategory : ScriptableObject
{
    [SerializeField] AudioCategory parentCategory;
    [SerializeField] [Range(0f, 1f)] float volume = 1f;
    [SerializeField] AudioMixerGroup mixerGroup;

    public float Volume
    {
        get => volume;
        set => volume = Mathf.Clamp01(value);
    }

    public AudioMixerGroup MixerGroup => mixerGroup != null ? mixerGroup : (parentCategory != null ? parentCategory.MixerGroup : null);

    public float GetEffectiveVolume()
    {
        float effectiveVolume = volume;
        if (parentCategory != null) effectiveVolume *= parentCategory.GetEffectiveVolume();
        return effectiveVolume;
    }
}