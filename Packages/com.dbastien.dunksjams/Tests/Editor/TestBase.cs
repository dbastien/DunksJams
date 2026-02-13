using System;
using NUnit.Framework;

public abstract class TestBase
{
    protected static void Throws<T>(Action a) where T : Exception
    {
        try
        {
            a();
            Assert.Fail($"Expected {typeof(T).Name}");
        }
        catch (T)
        {
        }
    }

    protected static void InRange(float v, float min, float max) =>
        Assert.IsTrue(v >= min && v <= max, $"{v} not in [{min},{max}]");

    protected static void InRange(int v, int min, int max) =>
        Assert.IsTrue(v >= min && v <= max, $"{v} not in [{min},{max}]");

    protected static void Approx(float a, float b, float eps = 0.0001f) =>
        Assert.IsTrue(MathF.Abs(a - b) < eps, $"{a} != {b} (eps={eps})");

    protected static void Eq<T>(T expected, T actual) => Assert.AreEqual(expected, actual);
    protected static void NotEq<T>(T a, T b) => Assert.AreNotEqual(a, b);
    protected static void True(bool v) => Assert.IsTrue(v);
    protected static void False(bool v) => Assert.IsFalse(v);
    protected static void NotNull(object o) => Assert.IsNotNull(o);
    protected static void Null(object o) => Assert.IsNull(o);
}