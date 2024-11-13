public static class UintExtensions
{
    public static uint ReverseBits(this uint v)
    {
        v = ((v & 0xAAAAAAAA) >> 1) | ((v & 0x55555555) << 1);
        v = ((v & 0xCCCCCCCC) >> 2) | ((v & 0x33333333) << 2);
        v = ((v & 0xF0F0F0F0) >> 4) | ((v & 0x0F0F0F0F) << 4);
        v = ((v & 0xFF00FF00) >> 8) | ((v & 0x00FF00FF) << 8);
        return (v >> 16) | (v << 16);
    }

    public static uint ReverseBytes(this uint v)
    {
        return ((v & 0x000000FF) << 24) |
               ((v & 0x0000FF00) << 8) |
               ((v & 0x00FF0000) >> 8) |
               ((v & 0xFF000000) >> 24);
    }

    public static uint Shuffle(this uint v, uint mask)
    {
        uint result = 0;
        for (int i = 0, shift = 0; i < 32; ++i)
        {
            if ((mask & (1u << i)) == 0) continue;
            result |= ((v >> i) & 1u) << shift;
            ++shift;
        }
        return result;
    }
}