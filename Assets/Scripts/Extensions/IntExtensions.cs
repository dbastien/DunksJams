using UnityEngine;

public static class IntExtensions
{
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
        return 1 << (31 - LeadingZeroCount((v - 1) | 7));
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
        return 31 - LeadingZeroCount(v);
    }
    
    public static int NextLog2(this int v)
    {
        if (v <= 1) return 0;
        return Log2(v - 1) + 1;
    }
    
    public static int Pow(this int n, int p)
    {
        Debug.Assert(p >= 1);
        int result = 1;
        while (--p >= 0) result *= n;
        return result;
    }
}
