using System;
using UnityEngine;

//todo: largely untested
public class Weapon : MonoBehaviour
{
    [Header("General")]
    public float baseDamage = 10f;
    public float range = 15f;

    [ToggleHeader("useAmmo", "Ammo")] public bool useAmmo = false;
    [ShowIf("useAmmo")] public int maxAmmo = 30;
    [ShowIf("useAmmo")] public bool infiniteAmmo = false;
    [ShowIf("useAmmo")] public float reloadTime = 2f;

    [ToggleHeader("useWarmup", "Warmup")] public bool useWarmup = false;
    [ShowIf("useWarmup")] public float warmupTime = 1f;

    [ToggleHeader("useCooldown", "Cooldown")] public bool useCooldown = false;
    [ShowIf("useCooldown")] public float cooldownTime = 1f;

    [ToggleHeader("useCriticalHits", "Critical Hits")] public bool useCriticalHits = false;
    [ShowIf("useCriticalHits")] [Range(0f, 1f)] public float critChance = 0.2f;
    [ShowIf("useCriticalHits")] [Range(1f, 3f)] public float critMultiplier = 2f;

    [ToggleHeader("useStatusEffects", "Status Effects")] public bool useStatusEffects = false;
    [ShowIf("useStatusEffects")] public StatusEffect statusEffect;
    [ShowIf("useStatusEffects")] public float statusEffectDuration = 3f;

    [ToggleHeader("useSpread", "Spread")] public bool useSpread = false;
    [ShowIf("useSpread")] [Range(0f, 30f)] public float spreadAngle = 10f;

    [ToggleHeader("useBurstFire", "Burst Fire")] public bool useBurstFire = false;
    [ShowIf("useBurstFire")] public int burstCount = 3;
    [ShowIf("useBurstFire")] public float burstDelay = 0.1f;

    [ToggleHeader("useAOE", "Area of Effect")] public bool useAOE = false;
    [ShowIf("useAOE")] public float aoeRadius = 5f;

    [ToggleHeader("usePiercing", "Piercing Shots")] public bool usePiercing = false;
    [ShowIf("usePiercing")] public int maxPierceTargets = 3;

    private bool _isWarmingUp;
    private bool _isCoolingDown;
    private int _currentAmmo;

    public event Action<int> OnAmmoChanged;
    public event Action OnReloadStarted;
    public event Action OnReloadFinished;

    private void Start()
    {
        if (useAmmo) Reload();
    }

    public void Fire(Vector3 direction)
    {
        if (_isWarmingUp || _isCoolingDown) return;

        if (useAmmo && _currentAmmo <= 0)
        {
            Debug.Log("Out of ammo!");
            if (!infiniteAmmo) Reload();
            return;
        }

        if (useWarmup)
        {
            _isWarmingUp = true;
            Invoke(nameof(FinishWarmup), warmupTime);
            return;
        }

        if (useBurstFire)
            StartCoroutine(FireBurst(direction));
        else
            PerformFire(direction);

        if (useAmmo && !infiniteAmmo)
        {
            --_currentAmmo;
            OnAmmoChanged?.Invoke(_currentAmmo);
        }

        if (useCooldown)
        {
            _isCoolingDown = true;
            Invoke(nameof(FinishCooldown), cooldownTime);
        }
    }

    System.Collections.IEnumerator FireBurst(Vector3 direction)
    {
        for (int i = 0; i < burstCount; ++i)
        {
            PerformFire(direction);
            yield return new WaitForSeconds(burstDelay);
        }
    }

    void PerformFire(Vector3 direction)
    {
        Vector3 shotDirection = ApplySpread(direction);

        if (useAOE)
            DealAreaDamage(shotDirection);
        else if (usePiercing)
            DealPiercingDamage(shotDirection);
        else
            DealDamage(shotDirection);
    }

    Vector3 ApplySpread(Vector3 direction)
    {
        if (!useSpread) return direction;

        return Quaternion.Euler(0, UnityEngine.Random.Range(-spreadAngle, spreadAngle), 0) * direction;
    }

    void DealDamage(Vector3 direction)
    {
        if (Physics.Raycast(transform.position, direction, out var hit, range))
        {
            if (hit.collider.TryGetComponent<Health>(out var health))
                ApplyDamageAndEffects(health);
        }
    }

    void DealPiercingDamage(Vector3 direction)
    {
        Ray ray = new Ray(transform.position, direction);
        RaycastHit[] hits = Physics.RaycastAll(ray, range);

        int pierceCount = 0;
        foreach (var hit in hits)
        {
            if (hit.collider.TryGetComponent<Health>(out var health))
            {
                ApplyDamageAndEffects(health);
                ++pierceCount;
                if (pierceCount >= maxPierceTargets) break;
            }
        }
    }

    void DealAreaDamage(Vector3 direction)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position + direction * range, aoeRadius);

        foreach (var collider in hitColliders)
        {
            if (collider.TryGetComponent<Health>(out var health))
                ApplyDamageAndEffects(health);
        }
    }

    void ApplyDamageAndEffects(Health health)
    {
        health.TakeDamage(CalculateDamage(), DamageType.Physical);

        if (useStatusEffects)
        {
            float duration = statusEffectDuration > 0 ? statusEffectDuration : -1;
            health.ApplyStatusEffect(statusEffect, duration);
        }
    }

    float CalculateDamage()
    {
        float damage = baseDamage;

        if (useCriticalHits && UnityEngine.Random.value <= critChance)
        {
            damage *= critMultiplier;
            Debug.Log("Critical hit!");
        }

        return damage;
    }

    private void Reload()
    {
        if (!useAmmo || infiniteAmmo || _currentAmmo >= maxAmmo) return;

        Debug.Log("Reloading...");
        OnReloadStarted?.Invoke();
        Invoke(nameof(FinishReload), reloadTime);
    }

    void FinishReload()
    {
        _currentAmmo = maxAmmo;
        OnAmmoChanged?.Invoke(_currentAmmo);
        OnReloadFinished?.Invoke();
    }

    void FinishWarmup() => _isWarmingUp = false;

    void FinishCooldown() => _isCoolingDown = false;
}