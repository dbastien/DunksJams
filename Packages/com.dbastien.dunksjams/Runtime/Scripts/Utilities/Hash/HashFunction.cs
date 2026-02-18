/// <summary>
/// Abstract base class providing a hash-based random number interface.
/// Derived classes implement GetHash to produce deterministic uint hashes
/// from integer inputs, enabling reproducible procedural generation.
///
/// Original code by Rune Skovbo Johansen.
/// This Source Code Form is subject to the terms of the Mozilla Public
/// License, v. 2.0. http://mozilla.org/MPL/2.0/
/// </summary>
public abstract class HashFunction
{
    public abstract uint GetHash(params int[] data);

    public virtual uint GetHash(int data) => GetHash(new[] { data });
    public virtual uint GetHash(int x, int y) => GetHash(new[] { x, y });
    public virtual uint GetHash(int x, int y, int z) => GetHash(new[] { x, y, z });

    public float Value(params int[] data) => GetHash(data) / (float)uint.MaxValue;
    public float Value(int data) => GetHash(data) / (float)uint.MaxValue;
    public float Value(int x, int y) => GetHash(x, y) / (float)uint.MaxValue;
    public float Value(int x, int y, int z) => GetHash(x, y, z) / (float)uint.MaxValue;

    public int Range(int min, int max, params int[] data) => min + (int)(GetHash(data) % (uint)(max - min));
    public int Range(int min, int max, int data) => min + (int)(GetHash(data) % (uint)(max - min));
    public int Range(int min, int max, int x, int y) => min + (int)(GetHash(x, y) % (uint)(max - min));

    public float Range(float min, float max, params int[] data) =>
        min + GetHash(data) * (max - min) / uint.MaxValue;

    public float Range(float min, float max, int data) =>
        min + GetHash(data) * (max - min) / uint.MaxValue;

    public float Range(float min, float max, int x, int y) =>
        min + GetHash(x, y) * (max - min) / uint.MaxValue;

    public float Range(float min, float max, int x, int y, int z) =>
        min + GetHash(x, y, z) * (max - min) / uint.MaxValue;
}