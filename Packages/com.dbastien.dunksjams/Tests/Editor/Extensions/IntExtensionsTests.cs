using NUnit.Framework;

public class IntExtensionsTests : TestBase
{
    [Test]
    public void IsEven_IsOdd()
    {
        True(2.IsEven());
        True(0.IsEven());
        False(3.IsEven());
        True(3.IsOdd());
        False(2.IsOdd());
    }

    [Test]
    public void BitCount()
    {
        Eq(0, 0.BitCount());
        Eq(1, 1.BitCount());
        Eq(3, 7.BitCount());
        Eq(1, 8.BitCount());
        Eq(8, 255.BitCount());
    }

    [Test]
    public void Pow()
    {
        Eq(1, 5.Pow(1));
        Eq(8, 2.Pow(3));
        Eq(27, 3.Pow(3));
        Eq(1, 1.Pow(10));
    }

    [Test]
    public void ToDigits()
    {
        H.Seq(0.ToDigits(), 0);
        H.Seq(123.ToDigits(), 1, 2, 3);
        H.Seq(9.ToDigits(), 9);
    }

    [Test]
    public void FromDigits()
    {
        Eq(123, new[] { 1, 2, 3 }.FromDigits());
        Eq(0, new[] { 0 }.FromDigits());
    }

    [Test]
    public void Reverse()
    {
        Eq(321, 123.Reverse());
        Eq(1, 1.Reverse());
        Eq(21, 12.Reverse());
    }

    [Test]
    public void Fibonacci()
    {
        Eq(1, 1.Fibonacci());
        Eq(1, 2.Fibonacci());
        Eq(2, 3.Fibonacci());
        Eq(55, 10.Fibonacci());
    }

    [Test]
    public void Log2()
    {
        Eq(0, 1.Log2());
        Eq(1, 2.Log2());
        Eq(3, 8.Log2());
        Eq(4, 16.Log2());
    }

    [Test]
    public void Wrap()
    {
        Eq(0, 0.Wrap(0, 2));
        Eq(0, 3.Wrap(0, 2));
    }
}