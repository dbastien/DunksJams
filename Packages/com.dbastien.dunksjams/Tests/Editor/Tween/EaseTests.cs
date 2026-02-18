using System;
using NUnit.Framework;

public class EaseTests : TestBase
{
    [Test]
    public void Linear_ReturnsInput()
    {
        Approx(0f, Ease.Linear(0f));
        Approx(0.5f, Ease.Linear(0.5f));
        Approx(1f, Ease.Linear(1f));
    }

    [Test]
    public void AllEase_StartAtZero()
    {
        var funcs = new Func<float, float>[]
        {
            Ease.Linear, Ease.QuadraticIn, Ease.QuadraticOut, Ease.QuadraticInOut,
            Ease.CubicIn, Ease.CubicOut, Ease.CubicInOut,
            Ease.SineIn, Ease.SineOut, Ease.SineInOut,
            Ease.SmoothStep, Ease.SmootherStep
        };
        foreach (Func<float, float> f in funcs)
            Approx(0f, f(0f), 0.001f);
    }

    [Test]
    public void AllEase_EndAtOne()
    {
        var funcs = new Func<float, float>[]
        {
            Ease.Linear, Ease.QuadraticIn, Ease.QuadraticOut, Ease.QuadraticInOut,
            Ease.CubicIn, Ease.CubicOut, Ease.CubicInOut,
            Ease.SineIn, Ease.SineOut, Ease.SineInOut,
            Ease.SmoothStep, Ease.SmootherStep
        };
        foreach (Func<float, float> f in funcs)
            Approx(1f, f(1f), 0.001f);
    }

    [Test]
    public void InOut_SymmetricAtMidpoint()
    {
        Approx(0.5f, Ease.QuadraticInOut(0.5f), 0.001f);
        Approx(0.5f, Ease.CubicInOut(0.5f), 0.001f);
        Approx(0.5f, Ease.SineInOut(0.5f), 0.001f);
    }

    [Test]
    public void SmoothStep_Properties()
    {
        // SmoothStep should be 0 at 0, 1 at 1, and have zero derivative at endpoints
        Approx(0f, Ease.SmoothStep(0f));
        Approx(1f, Ease.SmoothStep(1f));
        Approx(0.5f, Ease.SmoothStep(0.5f));
    }

    [Test]
    public void Bounce_Properties()
    {
        Approx(0f, Ease.BounceEaseIn(0f), 0.001f);
        Approx(1f, Ease.BounceEaseOut(1f), 0.001f);
    }

    [Test]
    public void Square_BinaryOutput()
    {
        Approx(0f, Ease.Square(0f));
        Approx(0f, Ease.Square(0.49f));
        Approx(1f, Ease.Square(0.5f));
        Approx(1f, Ease.Square(1f));
    }
}