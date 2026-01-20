using UnityEngine;

public class SelfDestructAfterEffectsDone : MonoBehaviour
{
    private ParticleSystem[] particleSystems;
    private AudioSource[] audioSources;

    public void Awake()
    {
        this.particleSystems = this.GetComponents<ParticleSystem>();
        this.audioSources = this.GetComponents<AudioSource>();
    }

    public void Update()
    {
        for (var i = 0; i < this.particleSystems.Length; ++i)
        {
            if (this.particleSystems[i].IsAlive())
            {
                return;
            }
        }

        for (var j = 0; j < this.audioSources.Length; ++j)
        {
            if (this.audioSources[j].isPlaying)
            {
                return;
            }
        }

        Object.Destroy(this.gameObject);
    }
}
