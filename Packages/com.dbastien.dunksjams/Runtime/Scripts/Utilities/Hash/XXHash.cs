using System;

/// <summary>
/// C# implementation of xxHash optimized for producing random numbers from integer inputs.
/// Useful for deterministic procedural generation (terrain seeding, voxels, spawning).
///
/// Copyright (C) 2015, Rune Skovbo Johansen. (https://bitbucket.org/runevision/random-numbers-testing/)
/// Based on C# implementation Copyright (C) 2014, Seok-Ju, Yun. (https://github.com/noricube/xxHashSharp)
/// Original C Implementation Copyright (C) 2012-2014, Yann Collet. (https://code.google.com/p/xxhash/)
///
/// BSD 2-Clause License (http://www.opensource.org/licenses/bsd-license.php)
/// </summary>
public class XXHash : HashFunction
{
    uint _seed;

    const uint Prime1 = 2654435761U;
    const uint Prime2 = 2246822519U;
    const uint Prime3 = 3266489917U;
    const uint Prime4 = 668265263U;
    const uint Prime5 = 374761393U;

    public XXHash(int seed) => _seed = (uint)seed;

    public uint GetHash(byte[] buf)
    {
        uint h32;
        int index = 0, len = buf.Length;

        if (len >= 16)
        {
            int limit = len - 16;
            uint v1 = _seed + Prime1 + Prime2;
            uint v2 = _seed + Prime2;
            uint v3 = _seed;
            uint v4 = _seed - Prime1;

            do
            {
                v1 = SubHash(v1, buf, index); index += 4;
                v2 = SubHash(v2, buf, index); index += 4;
                v3 = SubHash(v3, buf, index); index += 4;
                v4 = SubHash(v4, buf, index); index += 4;
            } while (index <= limit);

            h32 = RotateLeft(v1, 1) + RotateLeft(v2, 7) + RotateLeft(v3, 12) + RotateLeft(v4, 18);
        }
        else
        {
            h32 = _seed + Prime5;
        }

        h32 += (uint)len;

        while (index <= len - 4)
        {
            h32 += BitConverter.ToUInt32(buf, index) * Prime3;
            h32 = RotateLeft(h32, 17) * Prime4;
            index += 4;
        }

        while (index < len)
        {
            h32 += buf[index] * Prime5;
            h32 = RotateLeft(h32, 11) * Prime1;
            index++;
        }

        h32 ^= h32 >> 15; h32 *= Prime2;
        h32 ^= h32 >> 13; h32 *= Prime3;
        h32 ^= h32 >> 16;
        return h32;
    }

    public uint GetHash(params uint[] buf)
    {
        uint h32;
        int index = 0, len = buf.Length;

        if (len >= 4)
        {
            int limit = len - 4;
            uint v1 = _seed + Prime1 + Prime2;
            uint v2 = _seed + Prime2;
            uint v3 = _seed;
            uint v4 = _seed - Prime1;

            do
            {
                v1 = SubHash(v1, buf[index++]);
                v2 = SubHash(v2, buf[index++]);
                v3 = SubHash(v3, buf[index++]);
                v4 = SubHash(v4, buf[index++]);
            } while (index <= limit);

            h32 = RotateLeft(v1, 1) + RotateLeft(v2, 7) + RotateLeft(v3, 12) + RotateLeft(v4, 18);
        }
        else
        {
            h32 = _seed + Prime5;
        }

        h32 += (uint)len * 4;

        while (index < len)
        {
            h32 += buf[index++] * Prime3;
            h32 = RotateLeft(h32, 17) * Prime4;
        }

        h32 ^= h32 >> 15; h32 *= Prime2;
        h32 ^= h32 >> 13; h32 *= Prime3;
        h32 ^= h32 >> 16;
        return h32;
    }

    public override uint GetHash(params int[] buf)
    {
        uint h32;
        int index = 0, len = buf.Length;

        if (len >= 4)
        {
            int limit = len - 4;
            uint v1 = _seed + Prime1 + Prime2;
            uint v2 = _seed + Prime2;
            uint v3 = _seed;
            uint v4 = _seed - Prime1;

            do
            {
                v1 = SubHash(v1, (uint)buf[index++]);
                v2 = SubHash(v2, (uint)buf[index++]);
                v3 = SubHash(v3, (uint)buf[index++]);
                v4 = SubHash(v4, (uint)buf[index++]);
            } while (index <= limit);

            h32 = RotateLeft(v1, 1) + RotateLeft(v2, 7) + RotateLeft(v3, 12) + RotateLeft(v4, 18);
        }
        else
        {
            h32 = _seed + Prime5;
        }

        h32 += (uint)len * 4;

        while (index < len)
        {
            h32 += (uint)buf[index++] * Prime3;
            h32 = RotateLeft(h32, 17) * Prime4;
        }

        h32 ^= h32 >> 15; h32 *= Prime2;
        h32 ^= h32 >> 13; h32 *= Prime3;
        h32 ^= h32 >> 16;
        return h32;
    }

    public override uint GetHash(int buf)
    {
        uint h32 = _seed + Prime5 + 4U;
        h32 += (uint)buf * Prime3;
        h32 = RotateLeft(h32, 17) * Prime4;
        h32 ^= h32 >> 15; h32 *= Prime2;
        h32 ^= h32 >> 13; h32 *= Prime3;
        h32 ^= h32 >> 16;
        return h32;
    }

    static uint SubHash(uint value, byte[] buf, int index)
    {
        value += BitConverter.ToUInt32(buf, index) * Prime2;
        value = RotateLeft(value, 13);
        return value * Prime1;
    }

    static uint SubHash(uint value, uint readValue)
    {
        value += readValue * Prime2;
        value = RotateLeft(value, 13);
        return value * Prime1;
    }

    static uint RotateLeft(uint value, int count) => (value << count) | (value >> (32 - count));
}
