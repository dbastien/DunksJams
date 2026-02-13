using UnityEngine;

public class PlayAudioEvent : GameEvent
{
    public AudioClipReference ClipReference { get; }
    public Vector3? Position { get; }
    public float VolumeScale { get; }

    public PlayAudioEvent(AudioClipReference clipReference, float volumeScale = 1f)
    {
        ClipReference = clipReference;
        VolumeScale = volumeScale;
        Position = null;
    }

    public PlayAudioEvent(AudioClipReference clipReference, Vector3 position, float volumeScale = 1f)
    {
        ClipReference = clipReference;
        Position = position;
        VolumeScale = volumeScale;
    }
}