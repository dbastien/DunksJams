using UnityEngine;

[DisallowMultipleComponent]
public class AudioEmitter : MonoBehaviour
{
    [SerializeField] AudioClipReference audioClip;
    [SerializeField] bool playOnAwake;
    [SerializeField] bool loop;
    [SerializeField] bool is3D = true;

    [SerializeField] float delay;

    AudioSource activeSource;

    void Awake()
    {
        if (playOnAwake && audioClip != null)
            Play();
    }

    public void Play()
    {
        if (audioClip == null) return;

        Stop();

        if (loop)
        {
            activeSource = AudioSystem.Instance?.PlayLooped(audioClip.Clip, audioClip.Volume, is3D, audioClip.Category);
            if (activeSource != null)
            {
                activeSource.transform.SetParent(transform);
                activeSource.transform.localPosition = Vector3.zero;
                if (audioClip.Channel != null)
                {
                    activeSource.outputAudioMixerGroup = audioClip.Channel.MixerGroup;
                    activeSource.volume *= audioClip.Channel.GetEffectiveVolume();
                }
            }
        }
        else
        {
            if (is3D)
                audioClip.Play3D(transform.position, delay);
            else
                audioClip.Play(delay);
        }
    }

    public void Stop()
    {
        if (activeSource == null) return;
        AudioSystem.Instance?.StopLooped(activeSource);
        activeSource = null;
    }

    void OnDestroy() => Stop();
}