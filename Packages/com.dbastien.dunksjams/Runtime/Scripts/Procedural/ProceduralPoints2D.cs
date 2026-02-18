using System;
using UnityEngine;

public static class ProceduralPoints2D
{
    public static Vector2 InCircle(float r)
    {
        float theta = Rand.Rad();
        float dist = r * Rand.Radial();
        return new Vector2(dist * MathF.Cos(theta), dist * MathF.Sin(theta));
    }

    public static Vector2 OnCircle(float r)
    {
        float angle = Rand.Rad();
        return new Vector2(r * MathF.Sin(angle), r * MathF.Cos(angle));
    }

    public static Vector2 OnUnitCircle() => Vector2Extensions.SinCos(Rand.Rad());

    public static Vector2 InSector(float rad, float spread) =>
        Vector2Extensions.SinCos(rad - spread * 0.5f + spread * Rand.Float());

    public static Vector2 InCone(float rad, float spread) =>
        Vector2Extensions.SinCos(rad - spread + 2f * spread * Rand.Float());

    public static Vector2 InHemisphere(float rad) =>
        Vector2Extensions.SinCos(rad - MathConsts.Tau_Div2 + Rand.Rad());

    public static Vector2 OnHemisphere(float rad) => Vector2Extensions.SinCos(rad - MathConsts.Tau_Div2);

    public static Vector2 InRectangle(float width, float height) =>
        new(Rand.Float() * width - width * 0.5f,
            Rand.Float() * height - height * 0.5f);

    public static Vector2 OnRectangle(float width, float height)
    {
        float edge = Rand.Float() * (2f * width + 2f * height);
        if (edge < width) return new Vector2(edge - width * 0.5f, height * 0.5f);
        if (edge < width + height) return new Vector2(width * 0.5f, edge - width - height * 0.5f);
        if (edge < 2f * width + height) return new Vector2(edge - 2f * width - width * 0.5f, -height * 0.5f);
        return new Vector2(-width * 0.5f, edge - 2f * width - 2f * height - height * 0.5f);
    }

    public static Vector2 InSquare(float size) =>
        new(Rand.Float() * size - size * 0.5f, Rand.Float() * size - size * 0.5f);

    public static Vector2 OnSquare(float size) =>
        new(Rand.Sign() * size * Rand.Float(), Rand.Sign() * size * Rand.Float());

    public static Vector2 InEllipse(float width, float height)
    {
        float angle = Rand.Rad();
        float r = Rand.Radial();
        return new Vector2(r * width * MathF.Cos(angle), r * height * MathF.Sin(angle));
    }

    public static Vector2 OnEllipse(float width, float height)
    {
        float angle = Rand.Rad();
        return new Vector2(width * MathF.Cos(angle), height * MathF.Sin(angle));
    }

    public static Vector2 InSuperEllipse(float width, float height, float n)
    {
        float angle = Rand.Rad();
        float r = MathF.Pow(Rand.Float(), 1f / n);
        return new Vector2(r * width * MathF.Cos(angle), r * height * MathF.Sin(angle));
    }

    public static Vector2 OnSuperEllipse(float width, float height, float n)
    {
        float angle = Rand.Rad();
        return new Vector2(width * MathF.Cos(angle), height * MathF.Sin(angle));
    }

    public static Vector2 InRing(float innerR, float outerR)
    {
        float angle = Rand.Rad();
        float r = MathF.Sqrt(Rand.Float() * (outerR * outerR - innerR * innerR) + innerR * innerR);
        return new Vector2(r * MathF.Cos(angle), r * MathF.Sin(angle));
    }

    public static Vector2 OnRing(float innerR, float outerR)
    {
        float angle = Rand.Rad();
        float r = Mathf.Lerp(innerR, outerR, Rand.Float());
        return new Vector2(r * MathF.Cos(angle), r * MathF.Sin(angle));
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

    public static Vector2 InWedge(float rad, float wedgeAngle) =>
        Vector2Extensions.SinCos(rad - wedgeAngle * 0.5f + wedgeAngle * Rand.Float());

    public static Vector2 InAnnulus(float innerR, float outerR)
    {
        float angle = Rand.Rad();
        float radius = MathF.Sqrt(Rand.Float() * (outerR * outerR - innerR * innerR) + innerR * innerR);
        return new Vector2(radius * MathF.Sin(angle), radius * MathF.Cos(angle));
    }

    public static Vector2 OnAnnulus(float innerR, float r)
    {
        float angle = Rand.Rad();
        return new Vector2(r * MathF.Sin(angle), r * MathF.Cos(angle));
    }

    public static Vector2 InSpiral(float a, float b, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        float r = (a + b * theta) * Rand.Radial();
        return new Vector2(r * MathF.Cos(theta), r * MathF.Sin(theta));
    }

    public static Vector2 OnSpiral(float a, float b, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        float r = a + b * theta;
        return new Vector2(r * MathF.Cos(theta), r * MathF.Sin(theta));
    }

    public static Vector2 InOnCardioid(float a, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        float r = a * (1f + MathF.Cos(theta));
        return new Vector2(r * MathF.Cos(theta), r * MathF.Sin(theta));
    }

    public static Vector2 InOnRose(float k, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        float r = MathF.Cos(k * theta);
        return new Vector2(r * MathF.Cos(theta), r * MathF.Sin(theta));
    }

    public static Vector2 InOnLissajous(float a, float b, float delta, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new Vector2(a * MathF.Sin(theta), b * MathF.Sin(theta + delta));
    }

    public static Vector2 InOnEpicycloid(float a, float b, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        float r = a + b;
        return new Vector2(r * MathF.Cos(theta) - b * MathF.Cos((a / b + 1f) * theta),
            r * MathF.Sin(theta) - b * MathF.Sin((a / b + 1f) * theta));
    }

    public static Vector2 InLogNormal(float rad, float stddev) =>
        Vector2Extensions.SinCos(rad + Rand.LogNormal(0f, stddev));

    public static Vector2 InCauchy(float rad, float gamma) =>
        Vector2Extensions.SinCos(rad + Rand.Cauchy(0f, gamma));

    public static Vector2 InOnLine(Vector2 p1, Vector2 p2) =>
        Vector2.Lerp(p1, p2, Rand.Float());

    public static Vector2 InRegularPolygon(int sides, float r)
    {
        float angle = Rand.Rad();
        float sectorAngle = MathConsts.Tau / sides;
        float randomAngle = MathF.Floor(angle / sectorAngle) * sectorAngle;
        return new Vector2(r * MathF.Cos(randomAngle), r * MathF.Sin(randomAngle));
    }

    public static Vector2 OnParabola(float a, float b, float xMin, float xMax)
    {
        float x = Mathf.Lerp(xMin, xMax, Rand.Float());
        float y = a * x * x + b * x;
        return new Vector2(x, y);
    }

    public static Vector2 InHyperbola(float a, float b, float xMin, float xMax)
    {
        float x = Mathf.Lerp(xMin, xMax, Rand.Float());
        float y = a / x + b;
        return new Vector2(x, y);
    }

    public static Vector2 InOnHypocycloid(float a, float b, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        float r = a - b;
        return new Vector2(r * MathF.Cos(theta) + b * MathF.Cos((a / b - 1f) * theta),
            r * MathF.Sin(theta) - b * MathF.Sin((a / b - 1f) * theta));
    }

    public static Vector2 InOnAstroid(float a, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new Vector2(a * MathF.Cos(theta) * MathF.Cos(theta) * MathF.Cos(theta),
            a * MathF.Sin(theta) * MathF.Sin(theta) * MathF.Sin(theta));
    }

    public static Vector2 InOnCissoid(float a, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        float r = a * MathF.Sin(theta) * MathF.Sin(theta) / MathF.Cos(theta);
        return new Vector2(r * MathF.Cos(theta), r * MathF.Sin(theta));
    }

    public static Vector2 InOnFolium(float a, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        float r = 4f * a * MathF.Cos(theta) * MathF.Cos(theta) * MathF.Cos(theta);
        return new Vector2(r * MathF.Sin(theta), r * MathF.Cos(theta));
    }

    public static Vector2 InOnLemniscate(float a, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        float r = a * MathF.Cos(2f * theta);
        return new Vector2(r * MathF.Cos(theta), r * MathF.Sin(theta));
    }

    public static Vector2 InOnFloral(float amp, int petalCount, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        float r = amp * MathF.Cos(petalCount * theta);
        return new Vector2(r * MathF.Cos(theta), r * MathF.Sin(theta));
    }

    public static Vector2 InOnSineWave(float amp, float freq, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new Vector2(theta, amp * MathF.Sin(freq * theta));
    }

    public static Vector2 InOnCosineWave(float amp, float freq, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new Vector2(theta, amp * MathF.Cos(freq * theta));
    }

    public static Vector2 InOnTangentWave(float amp, float freq, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new Vector2(theta, amp * MathF.Tan(freq * theta));
    }

    public static Vector2 InOnCosecantWave(float amp, float freq, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new Vector2(theta, amp / MathF.Sin(freq * theta));
    }

    public static Vector2 InOnSecantWave(float amp, float freq, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new Vector2(theta, amp / MathF.Cos(freq * theta));
    }

    public static Vector2 InOnCotangentWave(float amp, float freq, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new Vector2(theta, amp / MathF.Tan(freq * theta));
    }

    public static Vector2 InOnHyperbolicSineWave(float amp, float freq, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new Vector2(theta, amp * MathF.Sinh(freq * theta));
    }

    public static Vector2 InOnHyperbolicCosineWave(float amp, float freq, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new Vector2(theta, amp * MathF.Cosh(freq * theta));
    }

    public static Vector2 InOnHyperbolicCosecantWave(float amp, float freq, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new Vector2(theta, amp / MathF.Sinh(freq * theta));
    }

    public static Vector2 InOnHyperbolicTangentWave(float amp, float freq, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new Vector2(theta, amp * MathF.Tanh(freq * theta));
    }

    public static Vector2 InOnHyperbolicSecantWave(float amp, float freq, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new Vector2(theta, amp / MathF.Cosh(freq * theta));
    }

    public static Vector2 InOnHyperbolicCotangentWave(float amplitude, float freq, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new Vector2(theta, amplitude / MathF.Tanh(freq * theta));
    }

    public static Vector2 InOnArchimedeanSpiral(float a, float b, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        return new Vector2(a * theta * MathF.Cos(theta), a * theta * MathF.Sin(theta));
    }

    public static Vector2 InOnFermatSpiral(float a, float thetaMin, float thetaMax)
    {
        float theta = Mathf.Lerp(thetaMin, thetaMax, Rand.Float());
        float r = a * MathF.Sqrt(theta);
        return new Vector2(r * MathF.Cos(theta), r * MathF.Sin(theta));
    }

    public static Vector2 InGaussian(float rad, float stddev) =>
        Vector2Extensions.SinCos(rad + stddev * Rand.Gaussian());
}