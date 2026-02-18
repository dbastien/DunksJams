using UnityEngine;

public class SelfDestructAfterEffectsDone : MonoBehaviour
{
    private ParticleSystem[] particleSystems;
    private AudioSource[] audioSources;

    public void Awake()
    {
        particleSystems = GetComponents<ParticleSystem>();
        audioSources = GetComponents<AudioSource>();
    }

    public void Update()
    {
        for (var i = 0; i < particleSystems.Length; ++i)
            if (particleSystems[i].IsAlive())
                return;

        for (var j = 0; j < audioSources.Length; ++j)
            if (audioSources[j].isPlaying)
                return;

        Destroy(gameObject);
    }
}