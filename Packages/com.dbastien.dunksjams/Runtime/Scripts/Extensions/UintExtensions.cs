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

    public static uint ReverseBytes(this uint v) =>
        ((v & 0x000000FF) << 24) |
        ((v & 0x0000FF00) << 8) |
        ((v & 0x00FF0000) >> 8) |
        ((v & 0xFF000000) >> 24);

    public static uint ReverseWords(this uint v) => ((v & 0x0000FFFF) << 16) | ((v & 0xFFFF0000) >> 16);

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

    public static uint IsolateLowestBit(this uint v) => v & ~(v - 1);

    public static uint IsolateHighestBit(this uint v)
    {
        if (v == 0) return 0;
        v |= v >> 1;
        v |= v >> 2;
        v |= v >> 4;
        v |= v >> 8;
        v |= v >> 16;
        return v & ~(v >> 1);
    }

    public static bool HasOddParity(this uint v)
    {
        v ^= v >> 1;
        v ^= v >> 2;
        v ^= v >> 4;
        v ^= v >> 8;
        v ^= v >> 16;
        return (v & 1) != 0;
    }

    public static uint BitMask(int start, int len) => ((1u << len) - 1) << start;
    public static uint ClearBit(this uint v, int pos) => v & ~(1u << pos);
    public static uint SetBit(this uint v, int pos) => v | (1u << pos);
    public static uint ToggleBit(this uint v, int pos) => v ^ (1u << pos);
    public static uint ExtractBits(this uint v, int start, int len) => (v >> start) & ((1u << len) - 1);
}