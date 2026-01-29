using UnityEngine;

public static class IntExtensions
{
    public static void Swap(ref this int a, ref int b) => (a, b) = (b, a);
    public static bool IsEven(this int v) => (v & 1) == 0;
    public static bool IsOdd(this int v) => (v & 1) == 1;
    public static int LeadingZeroCount(this int v)
    {
        if (v == 0) return 32;
        int count = 1;
        if ((v & 0xFFFF0000) == 0) { count += 16; v <<= 16; }
        if ((v & 0xFF000000) == 0) { count += 8;  v <<= 8;  }
        if ((v & 0xF0000000) == 0) { count += 4;  v <<= 4;  }
        if ((v & 0xC0000000) == 0) { count += 2;  v <<= 2;  }
        return count - (v >> 31);
    }
    
    public static int NextPowerOfTwoAtLeast(this int v)
    {
        Debug.Assert(v > 0);
        return 1 << (31 - ((v - 1) | 7).LeadingZeroCount());
    }
    
    public static int MostSignificantBit(this int v)
    {
        v |= v >> 1;
        v |= v >> 2;
        v |= v >> 4;
        v |= v >> 8;
        v |= v >> 16;
        return v & ~(v >> 1);
    }
    
    public static int BitCount(this int v)
    {
        v -= (v >> 1) & 0x55555555;
        v = (v & 0x33333333) + ((v >> 2) & 0x33333333);
        v = (v + (v >> 4)) & 0x0F0F0F0F;
        return (v * 0x01010101) >> 24;
    }
    
    public static int Log2(this int v)
    {
        Debug.Assert(v > 0);
        return 31 - v.LeadingZeroCount();
    }
    
    public static int NextLog2(this int v)
    {
        if (v <= 1) return 0;
        return (v - 1).Log2() + 1;
    }
    
    public static int Pow(this int n, int p)
    {
        Debug.Assert(p >= 1);
        int result = 1;
        while (--p >= 0) result *= n;
        return result;
    }


    public static int Wrap(this int v, int min, int max)
    {
        int range = max - min + 1;
        return (v - min) % range + min;
    }


    public static int[] ToDigits(this int v)
    {
        int count = v == 0 ? 1 : 1 + (int)Mathf.Log10(v);
        int[] digits = new int[count];
        for (int i = count - 1; i >= 0; --i)
        {
            digits[i] = v % 10;
            v /= 10;
        }
        return digits;
    }

    public static int FromDigits(this int[] digits)
    {
        int result = 0;
        for (int i = 0; i < digits.Length; ++i)
        {
            result *= 10;
            result += digits[i];
        }
        return result;
    }

    public static int Reverse(this int v)
    {
        int result = 0;
        while (v > 0)
        {
            result = result * 10 + v % 10;
            v /= 10;
        }
        return result;
    }

    public static int RotateLeft(this int v, int n)
    {
        n %= 32;
        return (v << n) | (v >> (32 - n));
    }

    public static int RotateRight(this int v, int n)
    {
        n %= 32;
        return (v >> n) | (v << (32 - n));
    }

    public static int Fibonacci(this int n)
    {
        int a = 0, b = 1;
        while (--n > 0) { int temp = a + b; a = b; b = temp; }
        return b;
    }

    public static int Lerp(int a, int b, float t)
    {
        t = Mathf.Clamp01(t);
        return Mathf.RoundToInt(Mathf.Lerp(a, b, t));
    }
}