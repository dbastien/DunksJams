using System.Collections.Generic;
using NUnit.Framework;

public class RandTests : TestBase
{
    //[SetUp] public void Setup() => Rand.SetSeed(42);

    [Test] public void Shuffle_PreservesElements()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };
        Rand.Shuffle(list);
        H.Count(list, 5);
        for (int i = 1; i <= 5; i++) H.Contains(list, i);
    }

    [Test] public void Shuffle_ActuallyShuffles()
    {
        var list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var original = new List<int>(list);
        Rand.Shuffle(list);
        // Very unlikely to stay in order
        NotEq(original, list);
    }

    // [Test] public void SeededRand_IsDeterministic()
    // {
    //     Rand.SetSeed(123);
    //     float a = Rand.Float();
    //     Rand.SetSeed(123);
    //     float b = Rand.Float();
    //     Eq(a, b);
    // }

    [Test] public void FloatRanged_StaysInBounds()
    {
        for (int i = 0; i < 1000; i++)
            InRange(Rand.FloatRanged(5f, 10f), 5f, 10f);
    }

    [Test] public void IntRanged_StaysInBounds()
    {
        for (int i = 0; i < 1000; i++)
            InRange(Rand.IntRanged(0, 10), 0, 9);
    }

    [Test] public void Float_InZeroOne()
    {
        for (int i = 0; i < 1000; i++)
            InRange(Rand.Float(), 0f, 1f);
    }

    [Test] public void Bool_ReturnsBothValues()
    {
        bool seenTrue = false, seenFalse = false;
        for (int i = 0; i < 100; i++)
        {
            if (Rand.Bool()) seenTrue = true;
            else seenFalse = true;
        }
        True(seenTrue && seenFalse);
    }

    [Test] public void Element_ReturnsFromArray()
    {
        var arr = new[] { 10, 20, 30 };
        for (int i = 0; i < 50; i++)
            H.Contains(arr, Rand.Element(arr));
    }
}
