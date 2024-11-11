using System.Collections.Generic;
using UnityEngine;

//todo: largely untested
public class Health : MonoBehaviour
{
    public enum DamageType { Physical, Fire, Poison }
    public enum StatusEffect { Poison, Burn }

    [ToggleHeader("useDeath", "Death")] public bool useDeath = true;
    
    [Header("HP")]
    public int maxHP = 100;
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
    
    [ToggleHeader("useRegen", "Regeneration")] public bool useRegen;
    [ShowIf("useRegen")] public float regenRate = 5f;
    
    [ToggleHeader("useInvulnerability", "Invulnerability")] public bool useInvulnerability = true;
    [ShowIf("useInvulnerability")] public float invulnerabilityTime = 2f;

    [ToggleHeader("useCritHit", "Critical Hit")] public bool useCritHit;
    [ShowIf("useCritHit")] [Range(0f, 1f)] public float critChance = 0.2f;
    [ShowIf("useCritHit")] [Range(1f, 3f)] public float critMultiplier = 2f;

    [Header("Damage Resistances")]
    public SerializableDictionary<DamageType, float> resistances = new();

    [Header("Status Effects")]
    public float poisonDamagePerSecond = 3f;
    public float burnDamagePerSecond = 6f;
    public float statusEffectDuration = 5f;

    public int MaxHPEffective => maxHP + HPModifier;
    public int MaxShieldEffective => maxShield + shieldModifier;
    
    bool _isInvulnerable, _isDead;
    float _currentHP, _currentShield, _invulnerabilityTimer;

    List<StatusEffectInstance> _activeStatusEffects = new();
    
    public event System.Action<int> OnHPChanged, OnShieldChanged;
    public event System.Action<StatusEffect> OnStatusEffectApplied;
    public event System.Action OnDeath;

    [System.Serializable]
    public class StatusEffectInstance
    {
        public StatusEffect effectType;
        public float timer;
    }

    void Start() => ResetHP();

    void Update()
    {
        if (!useInvulnerability ||
            (_isInvulnerable && (_invulnerabilityTimer -= Time.deltaTime) <= 0))
        {
            _isInvulnerable = false;
        }
        
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

        if (useCritHit && 
            Random.value <= critChance) dam *= critMultiplier;

        if (useArmor && 
            armor > 0 && damType == DamageType.Physical) dam = Mathf.Max(0, dam - armor);

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

        if (useInvulnerability)
            EnableInvulnerability(invulnerabilityTime);
    }
    
    public void RestoreShield(float amount)
    {
        if (!useShield) return;
        _currentShield = Mathf.Min(_currentShield + amount, MaxShieldEffective);
        OnShieldChanged?.Invoke(Mathf.FloorToInt(_currentShield));
    }

    public void ApplyStatusEffect(StatusEffect effect)
    {
        if (_isDead) return;
        _activeStatusEffects.Add(new() { effectType = effect, timer = statusEffectDuration });
        OnStatusEffectApplied?.Invoke(effect);
    }

    public void EnableInvulnerability(float duration)
    {
        _isInvulnerable = true;
        _invulnerabilityTimer = duration;
    }

    void UpdateStatusEffects()
    {
        for (int i = _activeStatusEffects.Count - 1; i >= 0; --i)
        {
            var effect = _activeStatusEffects[i];
            effect.timer -= Time.deltaTime;

            if (effect.effectType == StatusEffect.Poison)
                TakeDamage(poisonDamagePerSecond * Time.deltaTime, DamageType.Poison);
            else if (effect.effectType == StatusEffect.Burn)
                TakeDamage(burnDamagePerSecond * Time.deltaTime, DamageType.Fire);

            if (effect.timer <= 0) _activeStatusEffects.RemoveAt(i);
        }
    }
    
    float ApplyResistance(float dam, DamageType damType) => 
        resistances.TryGetValue(damType, out float resistance) ? dam * (1 - resistance) : dam;

    void Die()
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
        var newHP = Mathf.Clamp(_currentHP + amount, 0, MaxHPEffective);
        if (Mathf.Approximately(newHP, _currentHP)) return;
        _currentHP = newHP;
        OnHPChanged?.Invoke(Mathf.FloorToInt(_currentHP));
    }
    
    public void SetHP(float val) => 
        ChangeHP(Mathf.Clamp(val, 0, MaxHPEffective) - _currentHP);
}