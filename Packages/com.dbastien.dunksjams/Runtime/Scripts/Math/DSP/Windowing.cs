using System;

public static class Windowing
{
    public static void Hann(ref float[] samples)
    {
        int N = samples.Length - 1;
        for (var n = 0; n < samples.Length; ++n)
        {
            float window = 0.5f * (1f - MathF.Cos(2f * MathF.PI * n / N));
            samples[n] *= window;
        }
    }

    public static void Hamming(ref float[] samples)
    {
        int N = samples.Length - 1;
        for (var n = 0; n < samples.Length; ++n)
        {
            float window = 0.54f - 0.46f * MathF.Cos(2f * MathF.PI * n / N);
            samples[n] *= window;
        }
    }

    public static void Blackman(ref float[] samples)
    {
        int N = samples.Length - 1;
        for (var n = 0; n < samples.Length; ++n)
        {
            var a0 = 0.42f;
            var a1 = 0.5f;
            var a2 = 0.08f;
            float window = a0 - a1 * MathF.Cos(2f * MathF.PI * n / N) + a2 * MathF.Cos(4f * MathF.PI * n / N);
            samples[n] *= window;
        }
    }

    public static float[] HannWindow(int length)
    {
        var window = new float[length];
        int N = length - 1;
        for (var n = 0; n < length; ++n) window[n] = 0.5f * (1f - MathF.Cos(2f * MathF.PI * n / N));
        return window;
    }
}