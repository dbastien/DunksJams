public static class ArrayExtensions
{
    public static void Fill(this bool[] a, bool val)
    {
        for (int i = 0; i < a.Length; ++i) a[i] = val;
    }
}