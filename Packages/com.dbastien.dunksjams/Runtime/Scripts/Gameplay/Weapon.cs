using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

//todo: largely untested
public class Weapon : MonoBehaviour
{
    [Header("General")] public float baseDamage = 10f;
    public float range = 15f;

    [ToggleHeader("useAmmo", "Ammo")] public bool useAmmo;
    [ShowIf("useAmmo")] public int maxAmmo = 30;
    [ShowIf("useAmmo")] public bool infiniteAmmo;
    [ShowIf("useAmmo")] public float reloadTime = 2f;

    [ToggleHeader("useWarmup", "Warmup")] public bool useWarmup;
    [ShowIf("useWarmup")] public float warmupTime = 1f;

    [ToggleHeader("useCooldown", "Cooldown")]
    public bool useCooldown;

    [ShowIf("useCooldown")] public float cooldownTime = 1f;

    [ToggleHeader("useCriticalHits", "Critical Hits")]
    public bool useCriticalHits;

    [ShowIf("useCriticalHits")] [Range(0f, 1f)]
    public float critChance = 0.2f;

    [ShowIf("useCriticalHits")] [Range(1f, 3f)]
    public float critMultiplier = 2f;

    [ToggleHeader("useStatusEffects", "Status Effects")]
    public bool useStatusEffects;

    [ShowIf("useStatusEffects")] public StatusEffect statusEffect;
    [ShowIf("useStatusEffects")] public float statusEffectDuration = 3f;

    [ToggleHeader("useSpread", "Spread")] public bool useSpread;
    [ShowIf("useSpread")] [Range(0f, 30f)] public float spreadAngle = 10f;

    [ToggleHeader("useBurstFire", "Burst Fire")]
    public bool useBurstFire;

    [ShowIf("useBurstFire")] public int burstCount = 3;
    [ShowIf("useBurstFire")] public float burstDelay = 0.1f;

    [ToggleHeader("useAOE", "Area of Effect")]
    public bool useAOE;

    [ShowIf("useAOE")] public float aoeRadius = 5f;

    [ToggleHeader("usePiercing", "Piercing Shots")]
    public bool usePiercing;

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
            DLog.Log("Out of ammo!");
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

    private IEnumerator FireBurst(Vector3 direction)
    {
        for (var i = 0; i < burstCount; ++i)
        {
            PerformFire(direction);
            yield return new WaitForSeconds(burstDelay);
        }
    }

    private void PerformFire(Vector3 direction)
    {
        Vector3 shotDirection = ApplySpread(direction);

        if (useAOE)
            DealAreaDamage(shotDirection);
        else if (usePiercing)
            DealPiercingDamage(shotDirection);
        else
            DealDamage(shotDirection);
    }

    private Vector3 ApplySpread(Vector3 direction)
    {
        if (!useSpread) return direction;

        return Quaternion.Euler(0, Random.Range(-spreadAngle, spreadAngle), 0) * direction;
    }

    private void DealDamage(Vector3 direction)
    {
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, range))
            if (hit.collider.TryGetComponent<Health>(out Health health))
                ApplyDamageAndEffects(health);
    }

    private void DealPiercingDamage(Vector3 direction)
    {
        var ray = new Ray(transform.position, direction);
        RaycastHit[] hits = Physics.RaycastAll(ray, range);

        var pierceCount = 0;
        foreach (RaycastHit hit in hits)
            if (hit.collider.TryGetComponent<Health>(out Health health))
            {
                ApplyDamageAndEffects(health);
                ++pierceCount;
                if (pierceCount >= maxPierceTargets) break;
            }
    }

    private void DealAreaDamage(Vector3 direction)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position + direction * range, aoeRadius);

        foreach (Collider collider in hitColliders)
            if (collider.TryGetComponent<Health>(out Health health))
                ApplyDamageAndEffects(health);
    }

    private void ApplyDamageAndEffects(Health health)
    {
        health.TakeDamage(CalculateDamage());

        if (useStatusEffects)
        {
            float duration = statusEffectDuration > 0 ? statusEffectDuration : -1;
            health.ApplyStatusEffect(statusEffect, duration);
        }
    }

    private float CalculateDamage()
    {
        float damage = baseDamage;

        if (useCriticalHits && Random.value <= critChance)
        {
            damage *= critMultiplier;
            DLog.Log("Critical hit!");
        }

        return damage;
    }

    private void Reload()
    {
        if (!useAmmo || infiniteAmmo || _currentAmmo >= maxAmmo) return;

        DLog.Log("Reloading...");
        OnReloadStarted?.Invoke();
        Invoke(nameof(FinishReload), reloadTime);
    }

    private void FinishReload()
    {
        _currentAmmo = maxAmmo;
        OnAmmoChanged?.Invoke(_currentAmmo);
        OnReloadFinished?.Invoke();
    }

    private void FinishWarmup() => _isWarmingUp = false;

    private void FinishCooldown() => _isCoolingDown = false;
}