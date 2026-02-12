using UnityEngine;

[CreateAssetMenu(menuName = "‽/Audio/Audio Clip Reference", fileName = "AudioClipReference")]
public class AudioClipReference : ScriptableObject
{
    [SerializeField] AudioClip clip;
    [SerializeField, Range(0f, 1f)] float volume = 1f;
    [SerializeField, Range(-3f, 3f)] float pitch = 1f;
    [SerializeField, Range(0f, 0.5f)] float volumeVariance;
    [SerializeField, Range(0f, 0.5f)] float pitchVariance;
    [SerializeField, Range(0f, 1f)] float spatialBlend;

    public AudioClip Clip => clip;
    public float Volume => volume + Random.Range(-volumeVariance, volumeVariance);
    public float Pitch => pitch + Random.Range(-pitchVariance, pitchVariance);
    public float SpatialBlend => spatialBlend;

    public void Play() => AudioSystem.Instance?.PlayOneShot(clip, Volume);
    public void Play3D(Vector3 position) => AudioSystem.Instance?.PlayOneShot3D(clip, position, Volume, spatialBlend);
}
