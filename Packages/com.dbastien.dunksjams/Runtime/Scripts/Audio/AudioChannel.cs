using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "‽/Audio/Audio Channel", fileName = "AudioChannel")]
public class AudioChannel : ScriptableObject
{
    [SerializeField] float volume = 1f;
    [SerializeField] bool isSingleTrack = false;
    [SerializeField] float crossfadeDuration = 1f;
    [SerializeField] AudioMixerGroup mixerGroup;

    public float Volume
    {
        get => volume;
        set => volume = Mathf.Clamp01(value);
    }

    public bool IsSingleTrack => isSingleTrack;
    public float CrossfadeDuration => crossfadeDuration;
    public AudioMixerGroup MixerGroup => mixerGroup;

    // Runtime state (managed by AudioSystem)
    [System.NonSerialized] public AudioSource SourceA;
    [System.NonSerialized] public AudioSource SourceB;
    [System.NonSerialized] public AudioSource ActiveSource;
    [System.NonSerialized] public Coroutine CrossfadeCoroutine;

    public float GetEffectiveVolume() => volume;
}