using UnityEngine;

//todo: largely untested
public class Projectile : MonoBehaviour
{
    [Header("General")] public float speed = 20f;
    public float range = 50f;
    public float damage = 10f;

    [ToggleHeader("useStatusEffects", "Status Effects")]
    public bool useStatusEffects;

    [ShowIf("useStatusEffects")] public StatusEffect statusEffect;
    [ShowIf("useStatusEffects")] public float statusEffectDuration = 3f;

    [ToggleHeader("useAOE", "Area of Effect")]
    public bool useAOE;

    [ShowIf("useAOE")] public float aoeRadius = 5f;

    [ToggleHeader("usePiercing", "Piercing")]
    public bool usePiercing;

    [ShowIf("usePiercing")] public int maxPierceTargets = 3;

    [Header("Lifetime Settings")] public float lifetime = 5f;

    Vector3 _startPosition;
    int _pierceCount;

    void Start()
    {
        _startPosition = transform.position;
        Destroy(gameObject, lifetime); // Destroy after lifetime expires
    }

    void Update()
    {
        MoveProjectile();

        // Destroy if exceeds range
        if (Vector3.Distance(_startPosition, transform.position) >= range)
            Destroy(gameObject);
    }

    void MoveProjectile() =>
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Health>(out var health))
        {
            if (useAOE)
                DealAreaDamage();
            else
                DealDirectDamage(health);

            if (usePiercing)
            {
                _pierceCount++;
                if (_pierceCount >= maxPierceTargets)
                    Destroy(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else if (!usePiercing) // Destroy if not piercing
        {
            Destroy(gameObject);
        }
    }

    void DealDirectDamage(Health target)
    {
        target.TakeDamage(damage);

        if (useStatusEffects)
        {
            var duration = statusEffectDuration > 0 ? statusEffectDuration : -1;
            target.ApplyStatusEffect(statusEffect, duration);
        }
    }

    void DealAreaDamage()
    {
        var hitColliders = Physics.OverlapSphere(transform.position, aoeRadius);

        foreach (var collider in hitColliders)
        {
            if (collider.TryGetComponent<Health>(out var health))
                DealDirectDamage(health);
        }

        Destroy(gameObject); // AOE projectiles typically destroy after explosion
    }
}