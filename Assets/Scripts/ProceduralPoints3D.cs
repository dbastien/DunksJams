using System;
using UnityEngine;

public class ProceduralPoints3D
{
    public static Vector3 InSphere(float radius)
    {
        float theta = Rand.Radian();
        float phi = Rand.Radian();
        float r = radius * MathF.Pow(Rand.Float(), 1f / 3f);
        float sinTheta = MathF.Sin(theta);
        return new(r * sinTheta * MathF.Cos(phi), r * sinTheta * MathF.Sin(phi), r * MathF.Cos(theta));
    }

    public static Vector3 OnSphere(float radius)
    {
        float theta = Rand.Radian();
        float phi = Rand.Radian();
        float sinTheta = MathF.Sin(theta);
        return new(radius * sinTheta * MathF.Cos(phi), radius * sinTheta * MathF.Sin(phi), radius * MathF.Cos(theta));
    }

    public static Vector3 InCube(float size)
    {
        float halfSize = size * 0.5f;
        return new(Rand.Float() * size - halfSize, Rand.Float() * size - halfSize, Rand.Float() * size - halfSize);
    }

    public static Vector3 OnCube(float size)
    {
        float edge = Rand.Float() * 3f;
        float offset = size * (Rand.Float() - 0.5f);
        return edge switch
        {
            < 1f => new(size * 0.5f, offset, offset),
            < 2f => new(offset, size * 0.5f, offset),
            _ => new(offset, offset, size * 0.5f)
        };
    }

    public static Vector3 InCuboid(float width, float height, float depth) => new(Rand.Float() * width - width * 0.5f, Rand.Float() * height - height * 0.5f, Rand.Float() * depth - depth * 0.5f);

    public static Vector3 OnCuboid(float width, float height, float depth)
    {
        float edge = Rand.Float() * (2f * width + 2f * height + 2f * depth);
        if (edge < width) return new(edge - width * 0.5f, height * 0.5f, depth * 0.5f);
        if (edge < width + height) return new(width * 0.5f, edge - width - height * 0.5f, depth * 0.5f);
        if (edge < 2f * width + height) return new(edge - 2f * width - width * 0.5f, -height * 0.5f, depth * 0.5f);
        if (edge < 2f * width + height + depth) return new(width * 0.5f, edge - 2f * width - height - depth * 0.5f, depth * 0.5f);
        if (edge < 2f * width + 2f * height + depth) return new(edge - 2f * width - 2f * height - depth * 0.5f, height * 0.5f, -depth * 0.5f);
        return new(-width * 0.5f, edge - 2f * width - 2f * height - depth * 0.5f, depth * 0.5f);
    }

    public static Vector3 InCylinder(float radius, float height)
    {
        float angle = Rand.Radian();
        float r = radius * MathF.Sqrt(Rand.Float());
        return new(r * MathF.Cos(angle), r * MathF.Sin(angle), Rand.Float() * height - height * 0.5f);
    }

    public static Vector3 OnCylinder(float radius, float height)
    {
        float angle = Rand.Radian();
        return new(radius * MathF.Cos(angle), radius * MathF.Sin(angle), Rand.Float() * height - height * 0.5f);
    }

    public static Vector3 InCone(float radians, float spread)
    {
        float angle = Rand.Radian();
        float r = MathF.Tan(spread * Rand.Float());
        return new(r * MathF.Cos(angle), r * MathF.Sin(angle), -radians * Rand.Float());
    }

    public static Vector3 InHollowCone(float innerRadians, float outerRadians)
    {
        float angle = Rand.Radian();
        float r = MathF.Tan(innerRadians + (outerRadians - innerRadians) * Rand.Float());
        return new(r * MathF.Cos(angle), r * MathF.Sin(angle), -outerRadians * Rand.Float());
    }

    public static Vector3 InPyramid(float width, float height)
    {
        float angle = Rand.Radian();
        float r = Rand.Float();
        return new(r * width * MathF.Cos(angle), r * width * MathF.Sin(angle), Rand.Float() * height - height * 0.5f);
    }

    public static Vector3 OnPyramid(float width, float height)
    {
        float angle = Rand.Radian();
        return new(width * MathF.Cos(angle), width * MathF.Sin(angle), Rand.Float() * height - height * 0.5f);
    }

    public static Vector3 InHemisphere(float radians)
    {
        float angle = Rand.Radian();
        return new(MathF.Sin(radians) * MathF.Cos(angle), MathF.Sin(radians) * MathF.Sin(angle), MathF.Cos(radians));
    }

    public static Vector3 OnHemisphere(float radians)
    {
        float angle = Rand.Radian();
        return new(MathF.Sin(radians) * MathF.Cos(angle), MathF.Sin(radians) * MathF.Sin(angle), MathF.Cos(radians));
    }

    public static Vector3 InCapsule(float radius, float height)
    {
        float angle = Rand.Radian();
        float r = radius * MathF.Sqrt(Rand.Float());
        float z = Rand.Float() * height - height * 0.5f;
        return new(r * MathF.Cos(angle), r * MathF.Sin(angle), z);
    }

    public static Vector3 OnCapsule(float radius, float height)
    {
        float angle = Rand.Radian();
        float z = Rand.Float() * height - height * 0.5f;
        return new(radius * MathF.Cos(angle), radius * MathF.Sin(angle), z);
    }

    public static Vector3 InTorus(float majorRadius, float minorRadius)
    {
        float theta = Rand.Radian();
        float phi = Rand.Radian();
        float r = minorRadius * MathF.Sqrt(Rand.Float());
        return new((majorRadius + r) * MathF.Cos(theta), (majorRadius + r) * MathF.Sin(theta), r * MathF.Sin(phi));
    }

    public static Vector3 OnTorus(float majorRadius, float minorRadius)
    {
        float theta = Rand.Radian();
        float phi = Rand.Radian();
        return new((majorRadius + minorRadius * MathF.Cos(phi)) * MathF.Cos(theta), (majorRadius + minorRadius * MathF.Cos(phi)) * MathF.Sin(theta), minorRadius * MathF.Sin(phi));
    }
}