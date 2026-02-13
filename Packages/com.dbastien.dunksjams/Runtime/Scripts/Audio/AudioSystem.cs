using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[DisallowMultipleComponent]
public class AudioSystem : SingletonEagerBehaviour<AudioSystem>
{
    [Header("Pooling")] [SerializeField] int initialPoolSize = 10;
    [SerializeField] int maxPoolSize = 50;

    [Header("Volume")] [SerializeField] [Range(0f, 1f)]
    float masterVolume = 1f;

    [SerializeField] [Range(0f, 1f)] float musicVolume = 1f;
    [SerializeField] [Range(0f, 1f)] float sfxVolume = 1f;

    [Header("Music")] [SerializeField] float musicCrossfadeDuration = 1f;

    ObjectPool<AudioSource> audioSourcePool;
    readonly List<AudioSource> activeAudioSources = new();
    readonly List<AudioSource> pausedAudioSources = new();

    AudioSource musicSourceA;
    AudioSource musicSourceB;
    AudioSource activeMusicSource;
    Coroutine musicCrossfadeCoroutine;

    public float MasterVolume
    {
        get => masterVolume;
        set => masterVolume = Mathf.Clamp01(value);
    }

    public float MusicVolume
    {
        get => musicVolume;
        set => musicVolume = Mathf.Clamp01(value);
    }

    public float SfxVolume
    {
        get => sfxVolume;
        set => sfxVolume = Mathf.Clamp01(value);
    }

    protected override void InitInternal()
    {
        audioSourcePool = new ObjectPool<AudioSource>(
            CreateAudioSource,
            OnGetAudioSource,
            OnReleaseAudioSource,
            OnDestroyAudioSource,
            defaultCapacity: initialPoolSize,
            maxSize: maxPoolSize
        );

        musicSourceA = CreateMusicSource("MusicSourceA");
        musicSourceB = CreateMusicSource("MusicSourceB");
        activeMusicSource = musicSourceA;
    }

    AudioSource CreateAudioSource()
    {
        var go = new GameObject("PooledAudioSource");
        go.transform.SetParent(transform);
        return go.AddComponent<AudioSource>();
    }

    AudioSource CreateMusicSource(string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform);
        var source = go.AddComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = false;
        return source;
    }

    void OnGetAudioSource(AudioSource source)
    {
        source.gameObject.SetActive(true);
        activeAudioSources.Add(source);
    }

    void OnReleaseAudioSource(AudioSource source)
    {
        activeAudioSources.Remove(source);
        source.Stop();
        source.clip = null;
        source.loop = false;
        source.spatialBlend = 0f;
        source.gameObject.SetActive(false);
    }

    void OnDestroyAudioSource(AudioSource source)
    {
        if (source != null) Destroy(source.gameObject);
    }

    public void PlayOneShot(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;

        var source = audioSourcePool.Get();
        source.clip = clip;
        source.volume = volumeScale * sfxVolume * masterVolume;
        source.spatialBlend = 0f;
        source.Play();

        StartCoroutine(ReturnToPoolWhenDone(source, clip.length));
    }

    public void PlayOneShot3D(AudioClip clip, Vector3 position, float volumeScale = 1f, float spatialBlend = 1f)
    {
        if (clip == null) return;

        var source = audioSourcePool.Get();
        source.transform.position = position;
        source.clip = clip;
        source.volume = volumeScale * sfxVolume * masterVolume;
        source.spatialBlend = spatialBlend;
        source.Play();

        StartCoroutine(ReturnToPoolWhenDone(source, clip.length));
    }

    public AudioSource PlayLooped(AudioClip clip, float volumeScale = 1f, bool is3D = false)
    {
        if (clip == null) return null;

        var source = audioSourcePool.Get();
        source.clip = clip;
        source.volume = volumeScale * sfxVolume * masterVolume;
        source.spatialBlend = is3D ? 1f : 0f;
        source.loop = true;
        source.Play();

        return source;
    }

    public void StopLooped(AudioSource source)
    {
        if (source == null) return;
        audioSourcePool.Release(source);
    }

    public void PlayMusic(AudioClip clip, bool crossfade = true)
    {
        if (clip == null) return;

        if (musicCrossfadeCoroutine != null)
            StopCoroutine(musicCrossfadeCoroutine);

        if (!crossfade || activeMusicSource.clip == null)
        {
            activeMusicSource.clip = clip;
            activeMusicSource.volume = musicVolume * masterVolume;
            activeMusicSource.Play();
        }
        else
        {
            var nextSource = activeMusicSource == musicSourceA ? musicSourceB : musicSourceA;
            musicCrossfadeCoroutine = StartCoroutine(CrossfadeMusic(activeMusicSource, nextSource, clip));
        }
    }

    IEnumerator CrossfadeMusic(AudioSource from, AudioSource to, AudioClip newClip)
    {
        to.clip = newClip;
        to.volume = 0f;
        to.Play();

        var elapsed = 0f;
        var targetVolume = musicVolume * masterVolume;

        while (elapsed < musicCrossfadeDuration)
        {
            elapsed += Time.deltaTime;
            var t = elapsed / musicCrossfadeDuration;

            from.volume = Mathf.Lerp(targetVolume, 0f, t);
            to.volume = Mathf.Lerp(0f, targetVolume, t);

            yield return null;
        }

        from.Stop();
        from.volume = targetVolume;
        to.volume = targetVolume;
        activeMusicSource = to;
        musicCrossfadeCoroutine = null;
    }

    public void StopMusic(bool fadeOut = false)
    {
        if (fadeOut)
        {
            if (musicCrossfadeCoroutine != null)
                StopCoroutine(musicCrossfadeCoroutine);
            musicCrossfadeCoroutine = StartCoroutine(FadeOutMusic(activeMusicSource));
        }
        else
        {
            activeMusicSource.Stop();
        }
    }

    IEnumerator FadeOutMusic(AudioSource source)
    {
        var startVolume = source.volume;
        var elapsed = 0f;

        while (elapsed < musicCrossfadeDuration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / musicCrossfadeDuration);
            yield return null;
        }

        source.Stop();
        source.volume = startVolume;
        musicCrossfadeCoroutine = null;
    }

    public void PauseAll()
    {
        foreach (var source in activeAudioSources)
        {
            if (source.isPlaying)
            {
                source.Pause();
                pausedAudioSources.Add(source);
            }
        }

        if (activeMusicSource.isPlaying) activeMusicSource.Pause();
    }

    public void ResumeAll()
    {
        foreach (var source in pausedAudioSources) source.UnPause();
        pausedAudioSources.Clear();

        activeMusicSource.UnPause();
    }

    public void StopAll()
    {
        foreach (var source in activeAudioSources.ToArray()) audioSourcePool.Release(source);
        pausedAudioSources.Clear();

        activeMusicSource.Stop();
    }

    public void FadeIn(AudioSource source, float duration)
    {
        if (source == null) return;
        StartCoroutine(FadeVolume(source, 0f, source.volume, duration));
    }

    public void FadeOut(AudioSource source, float duration, bool stopAfter = true)
    {
        if (source == null) return;
        StartCoroutine(FadeVolume(source, source.volume, 0f, duration, stopAfter));
    }

    IEnumerator FadeVolume(AudioSource source, float fromVolume, float toVolume, float duration,
        bool stopAfter = false)
    {
        var elapsed = 0f;
        source.volume = fromVolume;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(fromVolume, toVolume, elapsed / duration);
            yield return null;
        }

        source.volume = toVolume;
        if (stopAfter) source.Stop();
    }

    IEnumerator ReturnToPoolWhenDone(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSourcePool.Release(source);
    }

    void Update()
    {
        // Update all active sources with current volume settings
        foreach (var source in activeAudioSources)
        {
            if (!source.loop) continue;
            source.volume = sfxVolume * masterVolume;
        }

        if (activeMusicSource.isPlaying && musicCrossfadeCoroutine == null)
            activeMusicSource.volume = musicVolume * masterVolume;
    }
}