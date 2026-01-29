using System;
using UnityEngine;
using Random = System.Random;

public class WhiteNoiseGenerator : INoiseGenerator
{
    Random _random = new();

    public WhiteNoiseGenerator() { }
    public WhiteNoiseGenerator(int seed) => SetSeed(seed);

    public void SetSeed(int seed) => _random = new Random(seed);

    public float GetValue(float x) => NextUnit();
    public float GetValue(float x, float y) => NextUnit();
    public float GetValue(float x, float y, float z) => NextUnit();

    float NextUnit() => (float)_random.NextDouble();
}

public class PinkNoiseGenerator : INoiseGenerator
{
    readonly float[] _b = new float[7];
    Random _random = new();

    public PinkNoiseGenerator() { }
    public PinkNoiseGenerator(int seed) => SetSeed(seed);

    public void SetSeed(int seed)
    {
        _random = new Random(seed);
        Array.Clear(_b, 0, _b.Length);
    }

    public float GetValue(float x) => NextUnit();
    public float GetValue(float x, float y) => NextUnit();
    public float GetValue(float x, float y, float z) => NextUnit();

    float NextUnit()
    {
        float white = NextSigned();

        _b[0] = 0.99886f * _b[0] + white * 0.0555179f;
        _b[1] = 0.99332f * _b[1] + white * 0.0750759f;
        _b[2] = 0.96900f * _b[2] + white * 0.1538520f;
        _b[3] = 0.86650f * _b[3] + white * 0.3104856f;
        _b[4] = 0.55000f * _b[4] + white * 0.5329522f;
        _b[5] = -0.7616f * _b[5] - white * 0.0168980f;
        float pink = _b[0] + _b[1] + _b[2] + _b[3] + _b[4] + _b[5] + _b[6] + white * 0.5362f;
        _b[6] = white * 0.115926f;

        return ToUnitFloat(Mathf.Clamp(pink, -1f, 1f));
    }

    float NextSigned() => (float)_random.NextDouble() * 2f - 1f;
    static float ToUnitFloat(float value) => value * 0.5f + 0.5f;
}

public class BrownNoiseGenerator : INoiseGenerator
{
    float _lastValue;
    Random _random = new();

    public BrownNoiseGenerator() { }
    public BrownNoiseGenerator(int seed) => SetSeed(seed);

    public void SetSeed(int seed)
    {
        _random = new Random(seed);
        _lastValue = 0f;
    }

    public float GetValue(float x) => NextUnit();
    public float GetValue(float x, float y) => NextUnit();
    public float GetValue(float x, float y, float z) => NextUnit();

    float NextUnit()
    {
        float white = (float)_random.NextDouble() * 2f - 1f;
        _lastValue += white * 0.1f;
        _lastValue = Mathf.Clamp(_lastValue, -1f, 1f);
        return _lastValue * 0.5f + 0.5f;
    }
}
