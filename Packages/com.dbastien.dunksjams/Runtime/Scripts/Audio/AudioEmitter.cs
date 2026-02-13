using UnityEngine;

[DisallowMultipleComponent]
public class AudioEmitter : MonoBehaviour
{
    [SerializeField] AudioClipReference audioClip;
    [SerializeField] bool playOnAwake;
    [SerializeField] bool loop;
    [SerializeField] bool is3D = true;

    AudioSource activeSource;

    void Awake()
    {
        if (playOnAwake && audioClip != null)
            Play();
    }

    public void Play()
    {
        if (audioClip == null || audioClip.Clip == null) return;

        Stop();

        if (loop)
        {
            activeSource = AudioSystem.Instance?.PlayLooped(audioClip.Clip, audioClip.Volume, is3D);
            if (activeSource != null)
            {
                activeSource.transform.SetParent(transform);
                activeSource.transform.localPosition = Vector3.zero;
            }
        }
        else
        {
            if (is3D)
                audioClip.Play3D(transform.position);
            else
                audioClip.Play();
        }
    }

    public void Stop()
    {
        if (activeSource != null)
        {
            AudioSystem.Instance?.StopLooped(activeSource);
            activeSource = null;
        }
    }

    void OnDestroy() => Stop();
}