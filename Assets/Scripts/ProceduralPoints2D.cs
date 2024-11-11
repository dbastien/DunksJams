using System;
using UnityEngine;

public static class ProceduralPoints2D
{
    public static Vector2 InCircle(float radius)
    {
        float angle = Rand.Radian();
        float r = radius * MathF.Sqrt(Rand.Float());
        return new(r * MathF.Sin(angle), r * MathF.Cos(angle));
    }
    
    public static Vector2 OnUnitCircle() => Vector2Extensions.SinCos(Rand.Radian());

    public static Vector2 OnCircle(float radius)
    {
        float angle = Rand.Radian();
        return new(radius * MathF.Sin(angle), radius * MathF.Cos(angle));
    }
    
    public static Vector2 InCone(float radians, float spread) => 
        Vector2Extensions.SinCos(radians - spread + 2f * spread * Rand.Float());

    public static Vector2 InHemisphere(float radians) => 
        Vector2Extensions.SinCos(radians - MathfConstants.TauDiv2 + Rand.Radian());

    public static Vector2 InRectangle(float width, float height) => 
        new(Rand.Float() * width - width * 0.5f,
            Rand.Float() * height - height * 0.5f);

    public static Vector2 OnRectangle(float width, float height)
    {
        float edge = Rand.Float() * (2f * width + 2f * height);
        if (edge < width) return new(edge - width * 0.5f, height * 0.5f);
        if (edge < width + height) return new(width * 0.5f, edge - width - height * 0.5f);
        if (edge < 2f * width + height) return new(edge - 2f * width - width * 0.5f, -height * 0.5f);
        return new(-width * 0.5f, edge - 2f * width - 2f * height - height * 0.5f);
    }

    public static Vector2 InEllipse(float width, float height)
    {
        float angle = Rand.Radian();
        float r = MathF.Sqrt(Rand.Float());
        return new(r * width * MathF.Cos(angle), r * height * MathF.Sin(angle));
    }

    public static Vector2 OnEllipse(float width, float height)
    {
        float angle = Rand.Radian();
        return new(width * MathF.Cos(angle), height * MathF.Sin(angle));
    }

    public static Vector2 InTriangle(Vector2 a, Vector2 b, Vector2 c)
    {
        float u = Rand.Float();
        float v = Rand.Float();
        if (u + v > 1f)
        {
            u = 1f - u;
            v = 1f - v;
        }
        return a + u * (b - a) + v * (c - a);
    }

    public static Vector2 InWedge(float radians, float wedgeAngle) => 
        Vector2Extensions.SinCos(radians - wedgeAngle * 0.5f + wedgeAngle * Rand.Float());

    public static Vector2 InAnnulus(float innerRadius, float outerRadius)
    {
        float angle = Rand.Radian();
        float radius = MathF.Sqrt(Rand.Float() * (outerRadius * outerRadius - innerRadius * innerRadius) + innerRadius * innerRadius);
        return new(radius * MathF.Sin(angle), radius * MathF.Cos(angle));
    }

    public static Vector2 InGaussian(float radians, float stddev) => 
        Vector2Extensions.SinCos(radians + stddev * Rand.Gaussian());

    public static Vector2 OnLine(Vector2 p1, Vector2 p2) => 
        Vector2.Lerp(p1, p2, Rand.Float());

    public static Vector2 InRegularPolygon(int sides, float radius)
    {
        float angle = Rand.Radian();
        float sectorAngle = MathfConstants.Tau / sides;
        float randomAngle = MathF.Floor(angle / sectorAngle) * sectorAngle;
        return new(radius * MathF.Cos(randomAngle), radius * MathF.Sin(randomAngle));
    }

    public static Vector2 OnParabola(float a, float b, float xMin, float xMax)
    {
        float x = Mathf.Lerp(xMin, xMax, Rand.Float());
        float y = a * x * x + b * x;
        return new(x, y);
    }
}