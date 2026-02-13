using UnityEngine;

[CreateAssetMenu(menuName = "‽/Audio/Audio Clip Reference", fileName = "AudioClipReference")]
public class AudioClipReference : ScriptableObject
{
    [SerializeField] AudioCategory category;
    [SerializeField] AudioChannel channel;
    [SerializeField] AudioClip[] clips;
    [SerializeField] SelectionMode selectionMode = SelectionMode.Random;
    [SerializeField] [Range(0f, 1f)] float volume = 1f;
    [SerializeField] [Range(-3f, 3f)] float pitch = 1f;
    [SerializeField] [Range(0f, 0.5f)] float volumeVariance;
    [SerializeField] [Range(0f, 0.5f)] float pitchVariance;
    [SerializeField] [Range(0f, 1f)] float spatialBlend;

    public enum SelectionMode
    {
        Random,
        Sequence,
        RandomNotSameTwice
    }

    int lastSelectedClipIndex = -1;
    int sequenceIndex = -1;

    public AudioClip Clip => GetSelectedClip();
    public AudioCategory Category => category;
    public AudioChannel Channel => channel;
    public float Volume => volume + Random.Range(-volumeVariance, volumeVariance);
    public float Pitch => pitch + Random.Range(-pitchVariance, pitchVariance);
    public float SpatialBlend => spatialBlend;

    AudioClip GetSelectedClip()
    {
        if (clips == null || clips.Length == 0) return null;
        if (clips.Length == 1) return clips[0];

        int index = 0;
        switch (selectionMode)
        {
            case SelectionMode.Random:
                index = Random.Range(0, clips.Length);
                break;
            case SelectionMode.Sequence:
                sequenceIndex = (sequenceIndex + 1) % clips.Length;
                index = sequenceIndex;
                break;
            case SelectionMode.RandomNotSameTwice:
                index = Random.Range(0, clips.Length);
                if (index == lastSelectedClipIndex)
                {
                    index = (index + 1) % clips.Length;
                }
                lastSelectedClipIndex = index;
                break;
        }
        return clips[index];
    }

    public void Play(float delay = 0f) => AudioSystem.Instance?.PlayOneShot(this, delay);
    public void Play3D(Vector3 position, float delay = 0f) => AudioSystem.Instance?.PlayOneShot3D(this, position, delay);
}