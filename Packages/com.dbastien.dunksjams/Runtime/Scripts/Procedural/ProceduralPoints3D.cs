using System;
using UnityEngine;

public class ProceduralPoints3D
{
    public enum FibonacciStructureType
    {
        Spiral,
        RadialStack,
        Grid,
        RandomOffset
    }

    public static Vector3 InSphere(float radius)
    {
        float theta = Rand.Rad();
        float phi = Rand.Rad();
        float r = radius * MathF.Pow(Rand.Float(), 1f / 3f);
        float sinTheta = MathF.Sin(theta);
        return new Vector3(r * sinTheta * MathF.Cos(phi), r * sinTheta * MathF.Sin(phi), r * MathF.Cos(theta));
    }

    public static Vector3 OnSphere(float r)
    {
        float theta = Rand.Rad();
        float phi = Rand.Rad();
        float sinTheta = MathF.Sin(theta);
        return new Vector3(r * sinTheta * MathF.Cos(phi), r * sinTheta * MathF.Sin(phi), r * MathF.Cos(theta));
    }

    public static Vector3 InCube(float size)
    {
        float halfSize = size * 0.5f;
        return new Vector3(Rand.Float() * size - halfSize, Rand.Float() * size - halfSize,
            Rand.Float() * size - halfSize);
    }

    public static Vector3 OnCube(float size)
    {
        float edge = Rand.Float() * 3f;
        float offset = size * (Rand.Float() - 0.5f);
        return edge switch
        {
            < 1f => new Vector3(size * 0.5f, offset, offset),
            < 2f => new Vector3(offset, size * 0.5f, offset),
            _ => new Vector3(offset, offset, size * 0.5f)
        };
    }

    public static Vector3 InCuboid(float width, float height, float depth) =>
        new(Rand.Float() * width - width * 0.5f, Rand.Float() * height - height * 0.5f,
            Rand.Float() * depth - depth * 0.5f);

    public static Vector3 OnCuboid(float width, float height, float depth)
    {
        float edge = Rand.Float() * (2f * width + 2f * height + 2f * depth);
        if (edge < width) return new Vector3(edge - width * 0.5f, height * 0.5f, depth * 0.5f);
        if (edge < width + height) return new Vector3(width * 0.5f, edge - width - height * 0.5f, depth * 0.5f);
        if (edge < 2f * width + height)
            return new Vector3(edge - 2f * width - width * 0.5f, -height * 0.5f, depth * 0.5f);
        if (edge < 2f * width + height + depth)
            return new Vector3(width * 0.5f, edge - 2f * width - height - depth * 0.5f, depth * 0.5f);
        if (edge < 2f * width + 2f * height + depth)
            return new Vector3(edge - 2f * width - 2f * height - depth * 0.5f, height * 0.5f, -depth * 0.5f);
        return new Vector3(-width * 0.5f, edge - 2f * width - 2f * height - depth * 0.5f, depth * 0.5f);
    }

    public static Vector3 InCylinder(float radius, float height)
    {
        float angle = Rand.Rad();
        float r = radius * Rand.Radial();
        return new Vector3(r * MathF.Cos(angle), r * MathF.Sin(angle), Rand.Float() * height - height * 0.5f);
    }

    public static Vector3 OnCylinder(float r, float height)
    {
        float angle = Rand.Rad();
        return new Vector3(r * MathF.Cos(angle), r * MathF.Sin(angle), Rand.Float() * height - height * 0.5f);
    }

    public static Vector3 InCone(float rad, float spread)
    {
        float angle = Rand.Rad();
        float cosSpread = MathF.Cos(spread * Rand.Float());
        float sinSpread = MathF.Sqrt(1f - cosSpread * cosSpread);
        return new Vector3(sinSpread * MathF.Cos(angle), sinSpread * MathF.Sin(angle), cosSpread * rad);
    }

    public static Vector3 OnCone(float rad, float spread)
    {
        float angle = Rand.Rad();
        float cosSpread = MathF.Cos(spread);
        float sinSpread = MathF.Sqrt(1f - cosSpread * cosSpread);
        return new Vector3(sinSpread * MathF.Cos(angle), sinSpread * MathF.Sin(angle), cosSpread * rad);
    }

    public static Vector3 InPyramid(float width, float height)
    {
        float angle = Rand.Rad();
        float r = Rand.Float();
        return new Vector3(r * width * MathF.Cos(angle), r * width * MathF.Sin(angle),
            Rand.Float() * height - height * 0.5f);
    }

    public static Vector3 OnPyramid(float width, float height)
    {
        float angle = Rand.Rad();
        return new Vector3(width * MathF.Cos(angle), width * MathF.Sin(angle), Rand.Float() * height - height * 0.5f);
    }

    public static Vector3 InHemisphere(float radius)
    {
        float theta = Rand.Rad();
        float phi = Rand.Rad() * 0.5f;
        float r = radius * MathF.Pow(Rand.Float(), 1f / 3f);
        float sinPhi = MathF.Sin(phi);
        return new Vector3(r * sinPhi * MathF.Cos(theta), r * sinPhi * MathF.Sin(theta), r * MathF.Cos(phi));
    }

    public static Vector3 OnHemisphere(float r)
    {
        float theta = Rand.Rad();
        float phi = MathF.Acos(Rand.Float());
        float sinPhi = MathF.Sin(phi);
        return new Vector3(r * sinPhi * MathF.Cos(theta), r * sinPhi * MathF.Sin(theta), r * MathF.Cos(phi));
    }

    public static Vector3 InCapsule(float radius, float height)
    {
        float angle = Rand.Rad();
        float r = radius * Rand.Radial();
        float z = Rand.Float() * height - height * 0.5f;
        return new Vector3(r * MathF.Cos(angle), r * MathF.Sin(angle), z);
    }

    public static Vector3 OnCapsule(float r, float height)
    {
        float angle = Rand.Rad();
        float z = Rand.Float() * height - height * 0.5f;
        return new Vector3(r * MathF.Cos(angle), r * MathF.Sin(angle), z);
    }

    public static Vector3 InTorus(float majorR, float minorR)
    {
        float theta = Rand.Rad();
        float phi = Rand.Rad();
        float r = minorR * Rand.Radial();
        return new Vector3((majorR + r) * MathF.Cos(theta), (majorR + r) * MathF.Sin(theta), r * MathF.Sin(phi));
    }

    public static Vector3 OnTorus(float majorR, float minorR)
    {
        float theta = Rand.Rad();
        float phi = Rand.Rad();
        return new Vector3((majorR + minorR * MathF.Cos(phi)) * MathF.Cos(theta),
            (majorR + minorR * MathF.Cos(phi)) * MathF.Sin(theta), minorR * MathF.Sin(phi));
    }

    public static Vector3 InTorusKnot(float p, float q, float radius, float tubeR)
    {
        float theta = Rand.Rad();
        float r = tubeR * MathF.Sin(q * theta) + radius;
        return new Vector3(r * MathF.Cos(p * theta), r * MathF.Sin(p * theta), tubeR * MathF.Cos(q * theta));
    }

    public static Vector3 OnTorusKnot(float p, float q, float r, float tubeR)
    {
        float theta = Rand.Rad();
        return new Vector3((tubeR * MathF.Cos(q * theta) + r) * MathF.Cos(p * theta),
            (tubeR * MathF.Cos(q * theta) + r) * MathF.Sin(p * theta), tubeR * MathF.Sin(q * theta));
    }

    public static Vector3 InSpiral(float radius, float height, float revolutions)
    {
        float theta = Rand.Rad() * revolutions;
        float r = Rand.Float() * radius * theta / MathConsts.Tau;
        return new Vector3(r * MathF.Cos(theta), r * MathF.Sin(theta), Rand.Float() * height - height * 0.5f);
    }

    public static Vector3 OnSpiral(float radius, float height, float revolutions)
    {
        float theta = Rand.Rad() * revolutions;
        float r = radius * theta / MathConsts.Tau;
        return new Vector3(r * MathF.Cos(theta), r * MathF.Sin(theta), Rand.Float() * height - height * 0.5f);
    }

    public static Vector3 InHelix(float radius, float height, float revolutions, float pitch)
    {
        float theta = Rand.Rad() * revolutions;
        float r = radius * theta / MathConsts.Tau;
        return new Vector3(r * MathF.Cos(theta), r * MathF.Sin(theta), pitch * theta / MathConsts.Tau);
    }

    public static Vector3 OnHelix(float r, float height, float revolutions, float pitch)
    {
        float theta = Rand.Rad() * revolutions;
        return new Vector3(r * theta / MathConsts.Tau * MathF.Cos(theta), r * theta / MathConsts.Tau * MathF.Sin(theta),
            pitch * theta / MathConsts.Tau);
    }

    public static Vector3 InFibonacciSphere(float radius, int idx)
    {
        float phi = idx * MathConsts.GoldenRatio % MathConsts.Tau;
        float y = 1f - 2f * idx / (idx + 1).Fibonacci();
        float r = radius * MathF.Sqrt(1f - y * y) * MathF.Pow(Rand.Float(), 1f / 3f);
        return new Vector3(r * MathF.Cos(phi), y * radius, r * MathF.Sin(phi));
    }

    public static Vector3 OnFibonacciSphere(float radius, int idx)
    {
        float phi = idx * MathConsts.GoldenRatio % MathConsts.Tau;
        float y = 1f - 2f * idx / (idx + 1).Fibonacci();
        float r = radius * MathF.Sqrt(1f - y * y);
        return new Vector3(r * MathF.Cos(phi), y * radius, r * MathF.Sin(phi));
    }

    public static Vector3 InFibonacciStructure3D(float radius, int index, FibonacciStructureType type)
    {
        float phi = index * MathConsts.GoldenRatio % MathConsts.Tau;
        float y = 1f - 2f * index / (index + 1).Fibonacci();
        float r = radius * MathF.Sqrt(1f - y * y);

        return type switch
        {
            FibonacciStructureType.Spiral => new Vector3(r * MathF.Cos(phi), y * radius, r * MathF.Sin(phi)),
            FibonacciStructureType.RadialStack => new Vector3(r * MathF.Cos(phi), r * MathF.Sin(phi),
                index * 0.1f), // Stacks along z
            FibonacciStructureType.Grid => new Vector3(r * MathF.Cos(phi), y * radius,
                r * MathF.Sin(phi)), // Similar to lattice, but less aligned
            FibonacciStructureType.RandomOffset => new Vector3(r * MathF.Cos(phi + Rand.Float() * 0.1f),
                y * radius + Rand.Float() * 0.1f,
                r * MathF.Sin(phi + Rand.Float() * 0.1f)),

            _ => throw new ArgumentOutOfRangeException()
        };
    }
}