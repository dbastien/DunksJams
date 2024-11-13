using System;
using UnityEngine;

public static class MathConsts
{
    public const float ZeroTolerance = 1e-6f;   // Float epsilon
    public const float Tau = Mathf.PI * 2.0f;   // 360º (2π)
    public const float τ = Tau;
    public const float TauDiv2 = Mathf.PI;      // 180º (π)
    public const float TauDiv3 = Tau / 3.0f;    // 120º
    public const float TauDiv4 = Tau / 4.0f;    // 90º (π/2)
    public const float TauDiv5 = Tau / 5.0f;    // 72º
    public const float TauDiv6 = Tau / 6.0f;    // 60º
    public const float TauDiv7 = Tau / 7.0f;    // ~51.43º
    public const float TauDiv8 = Tau / 8.0f;    // 45º
    public const float TauDiv12 = Tau / 12.0f;  // 30º
    public const float TauInv = 1.0f / Tau;
    public const float TauSqrt = 2.506628f;     // √τ (≈ 2.50663)
    public const float Deg2Rad = Tau / 360.0f;  // º to radians
    public const float º2Rad   = Deg2Rad;
    public const float Rad2Deg = 360.0f / Tau;  // Radians to º
    public const float Rad2º   = Rad2Deg;

    public const float E = (float)Math.E;       // Euler's number (e)
    public const float e = E;
    public const float Log2E = 1.44269504089f;  // Log base 2 of e
    public const float Log10E = 0.43429448190f; // Log base 10 of e

    public const float Sqrt2 = 1.41421356237f;
    public const float Sqrt3 = 1.73205080757f;
    
    public const float EulerGamma = 0.5772156649f;          // Euler-Mascheroni constant
    public const float GoldenRatio = 1.6180339887f;         // φ
    public const float φ = GoldenRatio;
    public const float SilverRatio = 2.41421356237f;        // δS
    public const float ApéryConstant = 1.20206f;            // ζ(3)
    public const float KhinchinConstant = 2.6854520010f;    // Related to continued fractions
    public const float GlaisherConstant = 1.2824271291f;    // Appears in series/zeta functions
    public const float PlasticNumber = 1.3247179572f;       // Root of x^3 = x + 1
    public const float LemniscateConstant = 2.62205755429f; // Elliptic integrals
    public const float FeigenbaumDelta = 4.6692016091f;     // Bifurcation ratio (chaos theory)
    public const float FeigenbaumAlpha = 2.5029078750f;     // Scaling ratio (chaos theory)
    public const float CatalanConstant = 0.9159655942f;     // Appears in combinatorics
}