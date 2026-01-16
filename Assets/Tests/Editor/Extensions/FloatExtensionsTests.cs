using NUnit.Framework;

public class FloatExtensionsTests : TestBase
{
    [Test] public void Remap01()
    {
        Approx(0f, 0f.Remap01(0f, 100f));
        Approx(0.5f, 50f.Remap01(0f, 100f));
        Approx(1f, 100f.Remap01(0f, 100f));
    }

    [Test] public void Remap()
    {
        Approx(50f, 0f.Remap(0f, 1f, 50f, 100f));
        Approx(75f, 0.5f.Remap(0f, 1f, 50f, 100f));
        Approx(100f, 1f.Remap(0f, 1f, 50f, 100f));
    }

    [Test] public void Abs()
    {
        Approx(5f, (-5f).Abs());
        Approx(5f, 5f.Abs());
        Approx(0f, 0f.Abs());
    }

    [Test] public void IsNaN()
    {
        True(float.NaN.IsNaN());
        False(1f.IsNaN());
        False(0f.IsNaN());
    }
}
