using System;
using System.Collections.Generic;
using UnityEngine;

//todo: largely untested
[Flags]
public enum DamageType
{
    None = 0x0,
    Physical = 0x1,
    Fire = 0x2,
    Poison = 0x4,
    Ice = 0x8,
    Electric = 0x10,
    Arcane = 0x20,
    Dark = 0x40,
    Light = 0x80,
    Explosive = 0x100,
    Sonic = 0x200,
    Radiation = 0x400,
    Healing = 0x800,
    True = 0x1000
}

[Flags]
public enum StatusEffect
{
    None = 0x0,
    Poison = 0x1,
    Burn = 0x2,
    Bleed = 0x4,
    Freeze = 0x8,
    Shock = 0x10,
    Silence = 0x20,
    Root = 0x40,
    Fear = 0x80,
    Slow = 0x100,
    Vulnerable = 0x200,
    Invulnerable = 0x400,
    Regeneration = 0x800,
    Curse = 0x1000,
    Sleep = 0x2000,
    Mute = 0x4000,
    Stagger = 0x8000,
    Shielded = 0x10000
}

[Serializable]
public class StatusEffectInstance
{
    public StatusEffect effectType;
    public float duration;
    public float intensity;
    public int stacks;
    public bool isStackable;

    public StatusEffectInstance(StatusEffect effectType, float duration, float intensity = 1f, bool isStackable = false)
    {
        this.effectType = effectType;
        this.duration = duration;
        this.intensity = intensity;
        stacks = 1;
        this.isStackable = isStackable;
    }

    public bool IsPermanent => duration == -1;

    public void RefreshDuration(float newDuration)
    {
        if (!IsPermanent)
            duration = Mathf.Max(duration, newDuration);
    }

    public void AddStack(float additionalIntensity = 0f)
    {
        if (isStackable)
        {
            stacks++;
            intensity += additionalIntensity;
        }
    }

    public void CombineWith(StatusEffectInstance other)
    {
        if (effectType != other.effectType) return;
        RefreshDuration(other.duration);
        AddStack(other.intensity);
    }

    public override string ToString() =>
        $"{effectType}: {duration:F1}s, Intensity: {intensity:F1}, Stacks: {stacks}";
}

public static class EffectRegistry
{
    public static readonly Dictionary<StatusEffect, Action<StatusEffectInstance, GameObject>> EffectBehaviors = new();
    public static readonly Dictionary<(StatusEffect, StatusEffect), StatusEffect> Synergies = new();
    public static readonly Dictionary<(StatusEffect, StatusEffect), bool> Counters = new();

    static EffectRegistry()
    {
        EffectBehaviors[StatusEffect.Poison] = (effect, target) =>
        {
            if (target.TryGetComponent<Health>(out var health))
                health.TakeDamage(effect.intensity * Time.deltaTime, DamageType.Poison);
        };

        EffectBehaviors[StatusEffect.Burn] = (effect, target) =>
        {
            if (target.TryGetComponent<Health>(out var health))
                health.TakeDamage(effect.intensity * Time.deltaTime, DamageType.Fire);
        };

        // EffectBehaviors[StatusEffect.Freeze] = (effect, target) =>
        // {
        //     if (target.TryGetComponent<MovementController>(out var movement))
        //         movement.ModifySpeed(-effect.intensity);
        // };

        Synergies[(StatusEffect.Burn, StatusEffect.Poison)] = StatusEffect.Curse;
        Counters[(StatusEffect.Burn, StatusEffect.Freeze)] = true;
    }

    public static void ApplyEffectLogic(StatusEffectInstance effect, GameObject target)
    {
        if (EffectBehaviors.TryGetValue(effect.effectType, out var logic))
            logic(effect, target);
    }

    public static bool IsCounter(StatusEffect effect, StatusEffect counter)
    {
        return Counters.TryGetValue((effect, counter), out var exists) && exists;
    }

    public static StatusEffect? GetSynergy(StatusEffect a, StatusEffect b)
    {
        return Synergies.TryGetValue((a, b), out var synergy) ? synergy : null;
    }
}

public class StatusEffectManager : MonoBehaviour
{
    private readonly List<StatusEffectInstance> _activeEffects = new();

    public void ApplyEffect(StatusEffectInstance newEffect)
    {
        var existingEffect = _activeEffects.Find(e => e.effectType == newEffect.effectType);

        if (existingEffect != null)
        {
            if (existingEffect.isStackable)
                existingEffect.CombineWith(newEffect);
            else
                existingEffect.RefreshDuration(newEffect.duration);
        }
        else
        {
            _activeEffects.Add(newEffect);
        }
    }

    public void UpdateEffects(float deltaTime)
    {
        for (int i = _activeEffects.Count - 1; i >= 0; --i)
        {
            var effect = _activeEffects[i];
            if (!effect.IsPermanent)
                effect.duration -= deltaTime;

            EffectRegistry.ApplyEffectLogic(effect, gameObject);

            if (effect.duration <= 0 && !effect.IsPermanent)
                _activeEffects.RemoveAt(i);
        }
    }

    public void RemoveEffect(StatusEffect effectType)
    {
        _activeEffects.RemoveAll(e => e.effectType == effectType);
    }

    public bool HasEffect(StatusEffect effectType) =>
        _activeEffects.Exists(e => e.effectType == effectType);

    private void Update() => UpdateEffects(Time.deltaTime);
}