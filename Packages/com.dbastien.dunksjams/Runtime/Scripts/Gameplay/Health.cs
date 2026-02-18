using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

//todo: largely untested
public class Health : MonoBehaviour
{
    [ToggleHeader("useDeath", "Death")] public bool useDeath = true;

    [Header("HP")] public int maxHP = 100;
    public int HPModifier;
    public bool useOverheal;
    [ShowIf("useOverheal")] public int maxOverheal = 150;

    [ToggleHeader("useArmor", "Armor")] public bool useArmor;
    [ShowIf("useArmor")] public int armor = 10;

    [ToggleHeader("useShield", "Shield")] public bool useShield = true;
    [ShowIf("useShield")] public int maxShield = 50;
    [ShowIf("useShield")] public bool applyArmorToShield = true;
    [ShowIf("useShield")] public bool shieldRegenerates = true;
    [ShowIf("useShield")] public float shieldRegenRate = 10f;
    [ShowIf("useShield")] public int shieldModifier;

    [ToggleHeader("useRegen", "Regeneration")]
    public bool useRegen;

    [ShowIf("useRegen")] public float regenRate = 5f;

    [ToggleHeader("useInvulnerability", "Invulnerability")]
    public bool useInvulnerability = true;

    [ShowIf("useInvulnerability")] public float invulnerabilityTime = 2f;

    [ToggleHeader("useCritHit", "Critical Hit")]
    public bool useCritHit;

    [ShowIf("useCritHit")] [Range(0f, 1f)] public float critChance = 0.2f;
    [ShowIf("useCritHit")] [Range(1f, 3f)] public float critMultiplier = 2f;

    [Header("Damage Resistances")] public SerializableDictionary<DamageType, float> resistances = new();

    [Header("Status Effects")] public float poisonDamagePerSecond = 3f;
    public float burnDamagePerSecond = 6f;

    public int MaxHPEffective => maxHP + HPModifier;
    public int MaxShieldEffective => maxShield + shieldModifier;

    private bool _isInvulnerable, _isDead;
    private float _currentHP, _currentShield, _invulnerabilityTimer;

    private readonly List<StatusEffectInstance> _activeStatusEffects = new();

    public event Action<int> OnHPChanged, OnShieldChanged;
    public event Action<StatusEffect> OnStatusEffectApplied;
    public event Action OnDeath;

    [Serializable]
    public class StatusEffectInstance
    {
        public StatusEffect effectType;
        public float timer; // -1 indicates a permanent effect
    }

    private void Start() => ResetHP();

    private void Update()
    {
        if (_isInvulnerable && (_invulnerabilityTimer -= Time.deltaTime) <= 0)
            _isInvulnerable = false;

        if (useRegen && _currentHP < MaxHPEffective)
            _currentHP = Mathf.Min(_currentHP + regenRate * Time.deltaTime, MaxHPEffective);

        if (shieldRegenerates && _currentShield < MaxShieldEffective)
            _currentShield = Mathf.Min(_currentShield + shieldRegenRate * Time.deltaTime, MaxShieldEffective);

        UpdateStatusEffects();
        OnHPChanged?.Invoke(Mathf.FloorToInt(_currentHP));
    }

    public void TakeDamage(float dam, DamageType damType = DamageType.Physical)
    {
        if (_isInvulnerable || _isDead) return;

        dam = ApplyResistance(dam, damType);

        if (useCritHit && Random.value <= critChance) dam *= critMultiplier;

        if (useArmor && armor > 0 && damType == DamageType.Physical) dam = Mathf.Max(0, dam - armor);

        if (useShield && _currentShield > 0)
        {
            if (applyArmorToShield) dam = Mathf.Max(0, dam - armor);

            _currentShield -= dam;
            OnShieldChanged?.Invoke(Mathf.FloorToInt(_currentShield = Mathf.Max(0, _currentShield)));

            if (_currentShield <= 0) dam = -_currentShield; // Apply remaining damage to HP
            else return; // No HP damage if shield absorbed all
        }

        if (dam > 0) ChangeHP(-dam);

        if (_currentHP <= 0 && useDeath) Die();

        if (useInvulnerability) EnableInvulnerability(invulnerabilityTime);
    }

    public void EnableInvulnerability(float duration)
    {
        if (duration <= 0) return;

        _isInvulnerable = true;
        _invulnerabilityTimer = duration;
        DLog.Log($"Invulnerability enabled for {duration} seconds.");
    }

    public void RestoreShield(float amount)
    {
        if (!useShield) return;
        _currentShield = Mathf.Min(_currentShield + amount, MaxShieldEffective);
        OnShieldChanged?.Invoke(Mathf.FloorToInt(_currentShield));
    }

    public void ApplyStatusEffect(StatusEffect effect, float duration = -1f)
    {
        if (_isDead) return;

        StatusEffectInstance existingEffect = _activeStatusEffects.Find(e => e.effectType == effect);

        if (existingEffect != null)
        {
            if (existingEffect.timer == -1 || duration == -1)
                return; // Ignore re-adding a permanent effect
            existingEffect.timer = duration; // Refresh duration for timed effects
        }
        else
        {
            _activeStatusEffects.Add(new StatusEffectInstance { effectType = effect, timer = duration });
            OnStatusEffectApplied?.Invoke(effect);
        }
    }

    public void RemoveStatusEffect(StatusEffect effect) =>
        _activeStatusEffects.RemoveAll(e => e.effectType == effect);

    private void UpdateStatusEffects()
    {
        for (int i = _activeStatusEffects.Count - 1; i >= 0; --i)
        {
            StatusEffectInstance effect = _activeStatusEffects[i];
            if (effect.timer > 0)
                effect.timer -= Time.deltaTime;

            if (effect.effectType == StatusEffect.Poison)
                TakeDamage(poisonDamagePerSecond * Time.deltaTime, DamageType.Poison);
            else if (effect.effectType == StatusEffect.Burn)
                TakeDamage(burnDamagePerSecond * Time.deltaTime, DamageType.Fire);

            if (effect.timer == 0)
                _activeStatusEffects.RemoveAt(i);
        }
    }

    private float ApplyResistance(float dam, DamageType damType) =>
        resistances.TryGetValue(damType, out float resistance) ? dam * (1 - resistance) : dam;

    private void Die()
    {
        _isDead = true;
        _isInvulnerable = false;
        OnDeath?.Invoke();
    }

    public void ResetHP()
    {
        _currentShield = useShield ? MaxShieldEffective : 0;
        _isInvulnerable = false;
        _isDead = false;
        _activeStatusEffects.Clear();
        SetHP(MaxHPEffective);
        OnShieldChanged?.Invoke(Mathf.FloorToInt(_currentShield));
    }

    public void Heal(float amount)
    {
        if (_isDead) return;
        ChangeHP(amount);
    }

    public void ChangeHP(float amount)
    {
        float newHP = Mathf.Clamp(_currentHP + amount, 0, MaxHPEffective);
        if (Mathf.Approximately(newHP, _currentHP)) return;
        _currentHP = newHP;
        OnHPChanged?.Invoke(Mathf.FloorToInt(_currentHP));
    }

    public void SetHP(float val) =>
        ChangeHP(Mathf.Clamp(val, 0, MaxHPEffective) - _currentHP);
}