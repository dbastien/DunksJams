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

    public static Vector2 OnCircle(float radius)
    {
        float angle = Rand.Radian();
        return new(radius * MathF.Sin(angle), radius * MathF.Cos(angle));
    }

    public static Vector2 OnUnitCircle() => Vector2Extensions.SinCos(Rand.Radian());

    public static Vector2 InSector(float radians, float spread) =>
        Vector2Extensions.SinCos(radians - spread * 0.5f + spread * Rand.Float());

    public static Vector2 InCone(float radians, float spread) => 
        Vector2Extensions.SinCos(radians - spread + 2f * spread * Rand.Float());

    public static Vector2 InHemisphere(float radians) => 
        Vector2Extensions.SinCos(radians - MathfConstants.TauDiv2 + Rand.Radian());

    public static Vector2 OnHemisphere(float radians) => Vector2Extensions.SinCos(radians - MathfConstants.TauDiv2);

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

    public static Vector2 InSquare(float size) =>
        new(Rand.Float() * size - size * 0.5f, Rand.Float() * size - size * 0.5f);

    public static Vector2 OnSquare(float size) =>
        new(Rand.Sign() * size * Rand.Float(), Rand.Sign() * size * Rand.Float());

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

    public static Vector2 InSuperEllipse(float width, float height, float n)
    {
        float angle = Rand.Radian();
        float r = MathF.Pow(Rand.Float(), 1f / n);
        return new(r * width * MathF.Cos(angle), r * height * MathF.Sin(angle));
    }

    public static Vector2 OnSuperEllipse(float width, float height, float n)
    {
        float angle = Rand.Radian();
        return new(width * MathF.Cos(angle), height * MathF.Sin(angle));
    }

    public static Vector2 InRing(float innerRadius, float outerRadius)
    {
        float angle = Rand.Radian();
        float r = MathF.Sqrt(Rand.Float());
        return new(r * (outerRadius - innerRadius) + innerRadius * MathF.Cos(angle), r * (outerRadius - innerRadius) + innerRadius * MathF.Sin(angle));
    }

    public static Vector2 OnRing(float innerRadius, float outerRadius)
    {
        float angle = Rand.Radian();
        return new(outerRadius * MathF.Cos(angle), outerRadius * MathF.Sin(angle));
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

    public static Vector2 OnAnnulus(float innerRadius, float outerRadius)
    {
        float angle = Rand.Radian();
        return new(outerRadius * MathF.Sin(angle), outerRadius * MathF.Cos(angle));
    }

    public static Vector2 InSpiral(float a, float b, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        float r = a + b * theta;
        return new(r * MathF.Cos(theta), r * MathF.Sin(theta));
    }

    public static Vector2 OnSpiral(float a, float b, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new(a + b * theta * MathF.Cos(theta), a + b * theta * MathF.Sin(theta));
    }

    public static Vector2 InOnCardioid(float a, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        float r = a * (1f + MathF.Cos(theta));
        return new(r * MathF.Cos(theta), r * MathF.Sin(theta));
    }

    public static Vector2 InOnRose(float k, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        float r = MathF.Cos(k * theta);
        return new(r * MathF.Cos(theta), r * MathF.Sin(theta));
    }

    public static Vector2 InOnLissajous(float a, float b, float delta, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new(a * MathF.Sin(theta), b * MathF.Sin(theta + delta));
    }


    public static Vector2 InOnEpicycloid(float a, float b, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        float r = a + b;
        return new(r * MathF.Cos(theta) - b * MathF.Cos((a / b + 1f) * theta), r * MathF.Sin(theta) - b * MathF.Sin((a / b + 1f) * theta));
    }

    public static Vector2 InLogNormal(float radians, float stddev) =>
        Vector2Extensions.SinCos(radians + Rand.LogNormal(0f, stddev));

    public static Vector2 InCauchy(float radians, float gamma) =>
        Vector2Extensions.SinCos(radians + Rand.Cauchy(0f, gamma));

    public static Vector2 InOnLine(Vector2 p1, Vector2 p2) =>
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

    public static Vector2 InHyperbola(float a, float b, float xMin, float xMax)
    {
        float x = Mathf.Lerp(xMin, xMax, Rand.Float());
        float y = a / x + b;
        return new(x, y);
    }

    public static Vector2 InOnHypocycloid(float a, float b, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        float r = a - b;
        return new(r * MathF.Cos(theta) + b * MathF.Cos((a / b - 1f) * theta), r * MathF.Sin(theta) - b * MathF.Sin((a / b - 1f) * theta));
    }

    public static Vector2 InOnAstroid(float a, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new(a * MathF.Cos(theta) * MathF.Cos(theta) * MathF.Cos(theta), a * MathF.Sin(theta) * MathF.Sin(theta) * MathF.Sin(theta));
    }

    public static Vector2 InOnCissoid(float a, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        float r = a * MathF.Sin(theta) * MathF.Sin(theta) / MathF.Cos(theta);
        return new(r * MathF.Cos(theta), r * MathF.Sin(theta));
    }

    public static Vector2 InOnFolium(float a, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        float r = 4f * a * MathF.Cos(theta) * MathF.Cos(theta) * MathF.Cos(theta);
        return new(r * MathF.Sin(theta), r * MathF.Cos(theta));
    }

    public static Vector2 InOnLemniscate(float a, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        float r = a * MathF.Cos(2f * theta);
        return new(r * MathF.Cos(theta), r * MathF.Sin(theta));
    }

    public static Vector2 InOnFloral(float amplitude, int petalCount, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        float r = amplitude * MathF.Cos(petalCount * theta);
        return new(r * MathF.Cos(theta), r * MathF.Sin(theta));
    }

    public static Vector2 InOnSineWave(float amplitude, float frequency, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new(theta, amplitude * MathF.Sin(frequency * theta));
    }

    public static Vector2 InOnCosineWave(float amplitude, float frequency, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new(theta, amplitude * MathF.Cos(frequency * theta));
    }

    public static Vector2 InOnTangentWave(float amplitude, float frequency, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new(theta, amplitude * MathF.Tan(frequency * theta));
    }

    public static Vector2 InOnCosecantWave(float amplitude, float frequency, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new(theta, amplitude / MathF.Sin(frequency * theta));
    }

    public static Vector2 InOnSecantWave(float amplitude, float frequency, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new(theta, amplitude / MathF.Cos(frequency * theta));
    }

    public static Vector2 InOnCotangentWave(float amplitude, float frequency, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new(theta, amplitude / MathF.Tan(frequency * theta));
    }

    public static Vector2 InOnHyperbolicSineWave(float amplitude, float frequency, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new(theta, amplitude * MathF.Sinh(frequency * theta));
    }

    public static Vector2 InOnHyperbolicCosineWave(float amplitude, float frequency, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new(theta, amplitude * MathF.Cosh(frequency * theta));
    }

    public static Vector2 InOnHyperbolicCosecantWave(float amplitude, float frequency, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new(theta, amplitude / MathF.Sinh(frequency * theta));
    }

    public static Vector2 InOnHyperbolicTangentWave(float amplitude, float frequency, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new(theta, amplitude * MathF.Tanh(frequency * theta));
    }

    public static Vector2 InOnHyperbolicSecantWave(float amplitude, float frequency, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new(theta, amplitude / MathF.Cosh(frequency * theta));
    }

    public static Vector2 InOnHyperbolicCotangentWave(float amplitude, float frequency, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new(theta, amplitude / MathF.Tanh(frequency * theta));
    }

    public static Vector2 InOnArchimedeanSpiral(float a, float b, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new(a * theta * MathF.Cos(theta), a * theta * MathF.Sin(theta));
    }

    public static Vector2 InOnFermatSpiral(float a, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        float r = a * MathF.Sqrt(theta);
        return new(r * MathF.Cos(theta), r * MathF.Sin(theta));
    }

    public static Vector2 InGaussian(float radians, float stddev) =>
        Vector2Extensions.SinCos(radians + stddev * Rand.Gaussian());
}